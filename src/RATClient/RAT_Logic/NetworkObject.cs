using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace RAT_Logic
{
    public enum NetworkObjectType 
    {
        PC,
        Router,
        Switch,
        Server,
        Client
    }
    public class NetworkObject
    {
        public List<NetworkObjectInterface> NetworkInterfaces = new List<NetworkObjectInterface>();//new List<NetworkObjectInterface>() { new NetworkObjectInterface() { Name="eth0" ,IP = new IP() { IPv4 = "192.168.66.13", IPv4SubnetMask = "255.255.255.0", IPv4Gateway = "192.168.66.253", IPv6 = "ABC::ABC", IPv6PrefixLength=16 } } };

        public NetworkObjectType Type;

        //KI start (Claude Opus 4.8, prompt 21): polymorphic descriptor for this object's Type (icon, label,
        // whether it can mirror the host's specs). Uses the abstract DeviceDescriptor hierarchy.
        public DeviceDescriptor Descriptor => DeviceDescriptor.For(Type);
        //KI end

        public NetworkObjectSettings Settings = new NetworkObjectSettings();

        public string Name;

        public int ID;

        public int X;

        public int Y;

        //KI start (Claude Opus 4.8, prompt 1): stored, software-only specs editable for any device (not pushed to the real device)
        public string Os = "";
        public string Cpu = "";
        public string Gpu = "";
        public string Ram = "";
        public string Specs = ""; // free-form extra notes
        //KI end

        //KI start (Claude Opus 4.8, prompt 11): per-user access rights (one AccessRight per user).
        // A user with no entry is assumed to have AccesRights.Hidden (0). The DB already filters out objects a
        // user has no access to, so anything reaching the client is at least visible to the current user.
        public List<AccessRight> AccessRights = new List<AccessRight>();

        //KI start (Claude Opus 4.8, prompt 15): a global admin (account Privileges >= 100, set by the backend
        // is_admin flag) implicitly has Owner rights on EVERY object — the backend already enforces this, the
        // client must mirror it or admins can't manage/delete objects they have no explicit permission row on.
        private const int GlobalAdminPrivilege = 100;
        public static bool IsGlobalAdmin(NetworkUser? user) => user != null && user.Privileges >= GlobalAdminPrivilege;
        //KI end

        /// <summary>The right a user holds on this object; Hidden (0) if they have no entry.</summary>
        public AccesRights GetRight(NetworkUser? user)
        {
            if (user == null) { return AccesRights.Hidden; }
            if (IsGlobalAdmin(user)) { return AccesRights.Owner; } // KI (prompt 15): admins own everything
            AccessRight? entry = AccessRights.FirstOrDefault(a => a.User.ID == user.ID);
            return entry?.Rights ?? AccesRights.Hidden;
        }

        public bool HasAtLeast(NetworkUser? user, AccesRights minimum) => GetRight(user) >= minimum;

        /// <summary>
        /// Whether <paramref name="actor"/> may change <paramref name="target"/>'s right to
        /// <paramref name="newRight"/>, per the rules:
        ///  - Admins may set Hidden/See/Edit on users whose current right is lower than the admin's,
        ///    and may not grant Admin/Owner, nor touch other Admins/Owners.
        ///  - Owners may change anyone (including granting/removing Owner and Admin).
        /// </summary>
        public bool CanChangeRight(NetworkUser actor, NetworkUser target, AccesRights newRight)
        {
            if (actor.ID == target.ID) { return false; } // can't change your own rights here

            AccesRights actorRight = GetRight(actor);
            AccesRights targetRight = GetRight(target);

            if (actorRight == AccesRights.Owner)
            {
                return true; // owner can do anything, including grant/remove Owner and Admin
            }

            if (actorRight == AccesRights.Admin)
            {
                // admins can only manage users strictly below them...
                if (targetRight >= AccesRights.Admin) { return false; } // can't touch admins or owners
                // ...and only assign Hidden / See / Edit (never Admin or Owner)
                return newRight <= AccesRights.Edit;
            }

            return false; // See / Edit / Hidden users can't change anyone's rights
        }

        /// <summary>Sets a user's right if <paramref name="actor"/> is allowed to. Returns success.</summary>
        public bool SetRight(NetworkUser actor, NetworkUser target, AccesRights newRight)
        {
            if (!CanChangeRight(actor, target, newRight)) { return false; }
            ApplyRight(target, newRight);
            // TODO: persist to database once IDatabaseConnection AccessRights API exists
            return true;
        }

        /// <summary>Sets a user's right without permission checks (e.g. seeding the first owner).</summary>
        public void ApplyRight(NetworkUser user, AccesRights right)
        {
            AccessRight? existing = AccessRights.FirstOrDefault(a => a.User.ID == user.ID);
            if (right == AccesRights.Hidden)
            {
                if (existing != null) { AccessRights.Remove(existing); } // Hidden == no entry
                return;
            }
            if (existing != null) { existing.Rights = right; }
            else { AccessRights.Add(new AccessRight(user, right)); }
        }

        /// <summary>Only an Owner may delete the object.</summary>
        public bool CanBeDeletedBy(NetworkUser? user) => GetRight(user) == AccesRights.Owner;
        //KI end

        // ssh tutorial: https://deepwiki.com/sshnet/SSH.NET/2-getting-started
        private SshClient? sshClient = null;
        private List<ShellStream> sshShellStreams = new List<ShellStream>();
        private SftpClient? sftpClient = null;
        private ScpClient? scpClient = null;

        //KI start (Claude Opus 4.8, prompt 2): live connection status for the UI (connected / not connected)
        public bool IsSshConnected => sshClient != null && sshClient.IsConnected;
        public bool IsSftpConnected => sftpClient != null && sftpClient.IsConnected;
        public bool IsScpConnected => scpClient != null && scpClient.IsConnected;

        /// <summary>True if there is an open, connected session for the given login protocol.</summary>
        public bool IsConnected(LoginType type) => type switch
        {
            LoginType.SSH => IsSshConnected,
            LoginType.SFTP => IsSftpConnected,
            LoginType.SCP => IsScpConnected,
            _ => false
        };
        //KI end

        //KI start (Claude Opus 4.8, prompt 21): XML doc summaries on the public session/SNMP API
        /// <summary>Opens an SSH connection to this device using the given login. No-op if one is already open.</summary>
        /// <param name="login">SSH credentials (username/password/port).</param>
        /// <exception cref="EntryPointNotFoundException">No interface of this device is reachable from the host.</exception>
        public void OpenSSH(Login login) // TODO: See if by id is better than by login
        {
            if (sshClient != null) { return; }
            NetworkObjectInterface? networkObjectInterface = GetInterfaceInSameNetworkAsHost();
            if (networkObjectInterface == null || string.IsNullOrWhiteSpace(networkObjectInterface.IP.IPv4))
            {
                throw new EntryPointNotFoundException("No path to Remote Device Available");
            }

            SshClient sshClient_ = new SshClient(networkObjectInterface.IP.IPv4, login.Port, login.Username, login.Password);
            sshClient_.Connect();
            sshClient = sshClient_;

            //this.OpenSSHstream();
        }
        /// <summary>Opens a new interactive SSH shell stream and returns its id (index).</summary>
        /// <returns>The shell-stream id to pass to <see cref="SendCommand"/> / <see cref="StartReadingAsync"/>.</returns>
        /// <exception cref="EntryPointNotFoundException">No SSH connection is open yet.</exception>
        public int OpenSSHstream()
        {
            if (sshClient == null)
            {
                throw new EntryPointNotFoundException(
                    "No SSH connection to device to open shell stream.");
            }

            ShellStream shellStream = sshClient.CreateShellStream(
                "xterm",
                80,
                24,
                800,
                600,
                1024);

            sshShellStreams.Add(shellStream);

            return sshShellStreams.Count - 1;
        }

        //KI: ChatGPT prompt: how to change my executeSSH wpf input output textbox button setup to use my sshShellStreams for a real bash linux terminal feal
        /// <summary>Continuously reads output from a shell stream and reports it via the callback until the SSH connection closes.</summary>
        /// <param name="onDataReceived">Called with each chunk of received text.</param>
        /// <param name="shellId">The shell-stream id returned by <see cref="OpenSSHstream"/>.</param>
        public async Task StartReadingAsync(Action<string> onDataReceived, int shellId)
        {
            if (shellId < 0 || shellId >= sshShellStreams.Count)
                return;

            ShellStream shell = sshShellStreams[shellId];

            await Task.Run(() =>
            {
                while (sshClient != null && sshClient.IsConnected)
                {
                    if (shell.DataAvailable)
                    {
                        string data = shell.Read();

                        if (!string.IsNullOrEmpty(data))
                        {
                            onDataReceived(data);
                        }
                    }

                    Thread.Sleep(10);
                }
            });
        }
        

        /// <summary>Writes a command line into the given interactive shell stream.</summary>
        /// <param name="command">The command text (a newline is appended).</param>
        /// <param name="shellId">The shell-stream id returned by <see cref="OpenSSHstream"/>.</param>
        public void SendCommand(string command, int shellId)
        {
            if (shellId < 0 || shellId >= sshShellStreams.Count)
                return;

            sshShellStreams[shellId].WriteLine(command);
        }
        // KI END

        /// <summary>Runs a single SSH command and returns its captured output.</summary>
        /// <param name="command">The command to run.</param>
        /// <returns>The command's stdout.</returns>
        /// <exception cref="EntryPointNotFoundException">No SSH connection is open yet.</exception>
        public async Task<string> ExecuteSSH(string command)
        {
            if (sshClient == null) { throw new EntryPointNotFoundException("Open a ssh connection first!"); }

            var ssh_command = sshClient.RunCommand(command);
            var async_execute = ssh_command.ExecuteAsync();
            await async_execute;
            string result = ssh_command.Result;

            return result;
        }

        /// <summary>Opens an SFTP connection to this device. No-op if one is already open.</summary>
        /// <exception cref="EntryPointNotFoundException">No reachable interface for this device.</exception>
        public void OpenSFTP(Login login)
        {
            if (sftpClient != null) { return; }
            NetworkObjectInterface? networkObjectInterface = GetInterfaceInSameNetworkAsHost();
            if (networkObjectInterface == null || string.IsNullOrWhiteSpace(networkObjectInterface.IP.IPv4))
            {
                throw new EntryPointNotFoundException("No path to Remote Device Available");
            }

            SftpClient sftpClient_ = new SftpClient(networkObjectInterface.IP.IPv4, login.Port, login.Username, login.Password);
            sftpClient_.Connect();
            sftpClient = sftpClient_;
        }

        /// <summary>Uploads a local file to the device over SFTP. Requires an open SFTP connection.</summary>
        public void UploadSFTP(string localPath, string remotePath)
        {
            if (sftpClient == null) { throw new EntryPointNotFoundException("Open a sftp connection first!"); }
            using (var fileStream = File.OpenRead(localPath))
            {
                sftpClient.UploadFile(fileStream, remotePath);
            }
        }

        /// <summary>Downloads a remote file from the device over SFTP. Requires an open SFTP connection.</summary>
        public void DownloadSFTP(string localPath, string remotePath)
        {
            if (sftpClient == null) { throw new EntryPointNotFoundException("Open a sftp connection first!"); }
            using (var fileStream = File.Create(localPath))
            {
                sftpClient.DownloadFile(remotePath, fileStream);
            }
        }

        /// <summary>Lists the entries of a remote directory over SFTP. Requires an open SFTP connection.</summary>
        public List<string> ListDirSFTP(string remotePath)
        {
            List<string> files_ = new List<string>();

            if (sftpClient == null) { throw new EntryPointNotFoundException("Open a sftp connection first!"); }
            var files = sftpClient.ListDirectory("/remote/path");
            foreach (var file in files)
            {
                files_.Add(file.FullName);
            }
            return files_;
        }

        /// <summary>Opens an SCP connection to this device. No-op if one is already open.</summary>
        /// <exception cref="EntryPointNotFoundException">No reachable interface for this device.</exception>
        public void OpenSCP(Login login)
        {
            if (scpClient != null) { return; }
            NetworkObjectInterface? networkObjectInterface = GetInterfaceInSameNetworkAsHost();
            if (networkObjectInterface == null || string.IsNullOrWhiteSpace(networkObjectInterface.IP.IPv4))
            {
                throw new EntryPointNotFoundException("No path to Remote Device Available");
            }

            ScpClient scpClient_ = new ScpClient(networkObjectInterface.IP.IPv4, login.Port, login.Username, login.Password);
            scpClient_.Connect();
            scpClient = scpClient_;
        }

        /// <summary>Uploads a local file to the device over SCP. Requires an open SCP connection.</summary>
        public void UploadSCP(string localPath, string remotePath)
        {
            if (scpClient == null) { throw new EntryPointNotFoundException("Open a scp connection first!"); }
            scpClient.Upload(new FileInfo(localPath), remotePath);
        }

        /// <summary>Downloads a remote file from the device over SCP. Requires an open SCP connection.</summary>
        public void DownloadSCP(string localPath, string remotePath)
        {
            if (scpClient == null) { throw new EntryPointNotFoundException("Open a scp connection first!"); }
            scpClient.Download(remotePath, new FileInfo(localPath));
        }

        /// <summary>Opens a Telnet session to this device. Not implemented yet.</summary>
        public void OpenTelnet(Login login)
        {
            // TODO
            throw new NotImplementedException();
        }

        /// <summary>Writes a value to an SNMP OID on this device (SNMP SET) using the write community.</summary>
        /// <param name="snmpSettings">Community strings + port.</param>
        /// <param name="objectIdentifier">The numeric OID to set.</param>
        /// <param name="newValue">The new value (sent as an OctetString).</param>
        /// <param name="version">SNMP version (default v1).</param>
        public void SetSnmp(SnmpSettings snmpSettings, string objectIdentifier, string newValue, VersionCode version = VersionCode.V1)
        {
            NetworkObjectInterface? networkObjectInterface = GetInterfaceInSameNetworkAsHost();
            if (networkObjectInterface == null || string.IsNullOrWhiteSpace(networkObjectInterface.IP.IPv4))
            {
                throw new EntryPointNotFoundException("No path to Remote Device Available");
            }
            var result = Messenger.Set(version,
                           new IPEndPoint(IPAddress.Parse(networkObjectInterface.IP.IPv4), snmpSettings.Port),
                           new OctetString(snmpSettings.WriteCommunity),
                           new List<Variable> { new Variable(new ObjectIdentifier(objectIdentifier), new OctetString(newValue)) },
                           60000);
        }

        /// <summary>Reads a single SNMP OID from this device (SNMP GET) using the read community.</summary>
        /// <returns>The variable(s) returned by the agent.</returns>
        public IList<Lextm.SharpSnmpLib.Variable> GetSnmp(SnmpSettings snmpSettings, string objectIdentifier, VersionCode version = VersionCode.V1)
        {
            NetworkObjectInterface? networkObjectInterface = GetInterfaceInSameNetworkAsHost();
            if (networkObjectInterface == null || string.IsNullOrWhiteSpace(networkObjectInterface.IP.IPv4))
            {
                throw new EntryPointNotFoundException("No path to Remote Device Available");
            }
            var result = Messenger.Get(version,
                           new IPEndPoint(IPAddress.Parse(networkObjectInterface.IP.IPv4), snmpSettings.Port),
                           new OctetString(snmpSettings.ReadCommunity),
                           new List<Variable> { new Variable(new ObjectIdentifier(objectIdentifier))},
                           60000);
            return result;
        }

        //KI start (Claude Opus 4.8, prompt 1): SNMP walk for the MIB browser tree
        /// <summary>
        /// Walks the SNMP subtree under <paramref name="objectIdentifier"/> and returns
        /// every variable found. Used to populate the MIB browser.
        /// </summary>
        public IList<Lextm.SharpSnmpLib.Variable> WalkSnmp(SnmpSettings snmpSettings, string objectIdentifier, VersionCode version = VersionCode.V1)
        {
            NetworkObjectInterface? networkObjectInterface = GetInterfaceInSameNetworkAsHost();
            if (networkObjectInterface == null || string.IsNullOrWhiteSpace(networkObjectInterface.IP.IPv4))
            {
                throw new EntryPointNotFoundException("No path to Remote Device Available");
            }

            List<Variable> results = new List<Variable>();
            Messenger.Walk(version,
                           new IPEndPoint(IPAddress.Parse(networkObjectInterface.IP.IPv4), snmpSettings.Port),
                           new OctetString(snmpSettings.ReadCommunity),
                           new ObjectIdentifier(objectIdentifier),
                           results,
                           60000,
                           WalkMode.WithinSubtree);
            return results;
        }
        //KI end

        /// <summary>Reads this host machine's specs (name, OS, RAM, CPU, GPU) via WMI. Keys: name/os/ram/cpu/gpu.</summary>
        public static Dictionary<string, string> GetOwnDeviceInfos()
        {
            Dictionary<string, string> stats = new Dictionary<string, string>();
            //name
            stats.Add("name", Environment.MachineName);
            //OS
            stats.Add("os", System.Runtime.InteropServices.RuntimeInformation.OSDescription);

            //RAM
            var ramSearcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");

            ulong totalRamBytes = 0;

            foreach (ManagementObject obj in ramSearcher.Get())
            {
                totalRamBytes = (ulong)obj["TotalPhysicalMemory"];
            }

            stats.Add("ram", (totalRamBytes / 1024.0 / 1024 / 1024).ToString());

            //CPU
            var cpuSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");

            string cpuName = "";

            foreach (ManagementObject obj in cpuSearcher.Get())
            {
                cpuName = obj["Name"]?.ToString() ?? "";
            }

            stats.Add("cpu", cpuName);

            //GPU
            var gpuSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
            string gpuNmaes = "";
            foreach (ManagementObject obj in gpuSearcher.Get())
            {
                gpuNmaes += obj["Name"]?.ToString()+" " ?? "";
            }

            stats.Add("gpu", gpuNmaes);
            return stats;
        }

        /// <summary>Lists this host's network interfaces as name/status dictionaries (simple form).</summary>
        public static List<Dictionary<string,string>> GetOwnDeviceInterfaces()
        {
            List<Dictionary<string, string>> interfaces_list = new List<Dictionary<string, string>>();
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface interf in interfaces)
            {
                
                if (interf.OperationalStatus == OperationalStatus.Up)
                {
                    interfaces_list.Add(new Dictionary<string, string>() { ["name"] = $"{interf.Name}", ["status"] = "UP" });
                }
                else
                {
                    interfaces_list.Add(new Dictionary<string, string>() { ["name"] = $"{interf.Name}", ["status"] = "DOWN" });
                }
            }

            return interfaces_list;
        }

        //KI start (Claude Opus 4.8, prompt 2): real host interfaces only (Ethernet / WiFi / USB-Ethernet),
        // with full details, filtering out loopback / tunnels / virtual junk.
        public static List<HostInterfaceInfo> GetOwnDeviceInterfacesDetailed()
        {
            List<HostInterfaceInfo> result = new List<HostInterfaceInfo>();

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                // drop the obvious garbage
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                {
                    continue;
                }

                HostInterfaceKind kind;
                switch (nic.NetworkInterfaceType)
                {
                    case NetworkInterfaceType.Ethernet:
                    case NetworkInterfaceType.GigabitEthernet:
                    case NetworkInterfaceType.FastEthernetT:
                    case NetworkInterfaceType.FastEthernetFx:
                        kind = HostInterfaceKind.Ethernet;
                        break;
                    case NetworkInterfaceType.Wireless80211:
                        kind = HostInterfaceKind.Wifi;
                        break;
                    default:
                        // keep only USB-ethernet-style adapters among the rest; skip the noise
                        string blob = (nic.Description + " " + nic.Name).ToLowerInvariant();
                        if (blob.Contains("usb") && (blob.Contains("ethernet") || blob.Contains("lan") || blob.Contains("rndis")))
                        {
                            kind = HostInterfaceKind.UsbEthernet;
                        }
                        else
                        {
                            // not a real wired/wireless/usb NIC -> garbage (virtual switch, bluetooth PAN, etc.)
                            continue;
                        }
                        break;
                }

                HostInterfaceInfo info = new HostInterfaceInfo
                {
                    Name = nic.Name,
                    Description = nic.Description,
                    Kind = kind,
                    IsUp = nic.OperationalStatus == OperationalStatus.Up,
                    Mac = string.Join(":", Array.ConvertAll(nic.GetPhysicalAddress().GetAddressBytes(), b => b.ToString("X2")))
                };

                try { info.SpeedText = nic.Speed > 0 ? $"{nic.Speed / 1_000_000.0:0.#} Mbps" : "—"; }
                catch { info.SpeedText = "—"; }

                try
                {
                    foreach (var ua in nic.GetIPProperties().UnicastAddresses)
                    {
                        if (ua.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            info.IPv4.Add(ua.Address.ToString());
                        }
                        else if (ua.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        {
                            info.IPv6.Add(ua.Address.ToString());
                        }
                    }
                }
                catch { /* some adapters expose no IP props */ }

                result.Add(info);
            }

            return result;
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 12): real host NICs as NetworkObjectInterface objects, so the PC's
        // interfaces actually live on the NetworkObject and can be picked in the SelectInterfaceWindow.
        public static List<NetworkObjectInterface> GetOwnDeviceInterfacesAsModel()
        {
            List<NetworkObjectInterface> result = new List<NetworkObjectInterface>();
            foreach (HostInterfaceInfo info in GetOwnDeviceInterfacesDetailed())
            {
                result.Add(new NetworkObjectInterface
                {
                    Name = info.Name,
                    IsUp = info.IsUp,
                    IP = info.IPv4.Count > 0 || info.IPv6.Count > 0
                        ? new IP
                        {
                            IPv4 = info.IPv4.FirstOrDefault() ?? "",
                            IPv6 = info.IPv6.FirstOrDefault() ?? ""
                        }
                        : null
                });
            }
            return result;
        }

        /// <summary>Fills this object's interface list from the real host NICs (used for the "own PC" object).</summary>
        public void PopulateOwnDeviceInterfaces()
        {
            NetworkInterfaces = GetOwnDeviceInterfacesAsModel();
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 17): give a PC sensible default specs (this machine's real values) so a
        // newly created PC is never blank, and renaming it (which flips it out of the live "own PC" view) keeps real
        // stored values instead of empty strings. Only fills fields that are still empty — never clobbers user edits.
        public void ApplyDefaultPcSpecsIfEmpty()
        {
            Dictionary<string, string> stats;
            try { stats = GetOwnDeviceInfos(); }
            catch { return; } // WMI can fail on some hosts; just leave the fields empty then

            if (string.IsNullOrWhiteSpace(Os)) { Os = stats.GetValueOrDefault("os", ""); }
            if (string.IsNullOrWhiteSpace(Cpu)) { Cpu = stats.GetValueOrDefault("cpu", ""); }
            if (string.IsNullOrWhiteSpace(Gpu)) { Gpu = stats.GetValueOrDefault("gpu", ""); }
            if (string.IsNullOrWhiteSpace(Ram))
            {
                string ram = stats.GetValueOrDefault("ram", "");
                Ram = string.IsNullOrWhiteSpace(ram) ? "" : ram + " GB";
            }
        }
        //KI end

        private NetworkObjectInterface? GetInterfaceInSameNetworkAsHost()
        {

            //GET OWN SUBNETMASK
            List<string> ownIpv4subnet = new List<string>();
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface interf in interfaces)
            {
                if (interf.OperationalStatus == OperationalStatus.Up)
                {
                    var unicastIPAddresses = interf.GetIPProperties().UnicastAddresses;

                    foreach (var kp in unicastIPAddresses)
                    {
                        ownIpv4subnet.Add(kp.IPv4Mask.ToString());
                    }
                }
            }

            NetworkObjectInterface? interface_ = null;
            // Get remote machine interface with same subnetmask and physical connection
            // TODO: physical connection check
            foreach (string ipv4subnet in ownIpv4subnet)
            {
                foreach (NetworkObjectInterface networkObjectInterface in NetworkInterfaces)
                {
                    if (networkObjectInterface.IP.IPv4SubnetMask != ipv4subnet) { continue; }

                    //TODO: check if there is a wire connection aka: own machine interface -cable- switch -cable- remote machine interface (also wifi to AccessPoint)

                    interface_ = networkObjectInterface;
                }
            }

            return interface_;
            /*return new NetworkObjectInterface()
            {
                Name = "eth0",
                IP = new IP() { IPv4 = "192.168.66.13", IPv4SubnetMask = "255.255.255.0", IPv4Gateway = "192.168.66.253", IPv6 = "ABC::ABC", IPv6PrefixLength = 16 }
            };*/
        }
    }
}
