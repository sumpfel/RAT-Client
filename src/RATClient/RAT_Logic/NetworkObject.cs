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

namespace RAT_Logic
{
    public enum NetworkObjectType 
    { 
        Router,
        Switch,
        Server,
        Client
    }
    public class NetworkObject
    {
        public List<NetworkObjectInterface> NetworkInterfaces = new List<NetworkObjectInterface>();

        public NetworkObjectType Type;

        public NetworkObjectSettings Settings;

        public string Name;

        public int ID;
        // ssh tutorial: https://deepwiki.com/sshnet/SSH.NET/2-getting-started
        private SshClient? sshClient = null;
        private SftpClient? sftpClient = null;
        private ScpClient? scpClient = null;

        public void OpenSSH(Login login) // TODO: See if by id is better than by login
        {
            if (sshClient != null) { return; }
            NetworkObjectInterface? networkObjectInterface = GetInterfaceInSameNetworkAsHost();
            if (networkObjectInterface == null || string.IsNullOrWhiteSpace(networkObjectInterface.IP.IPv4))
            {
                throw new EntryPointNotFoundException("No path to Remote Device Available");
            }

            SshClient sshClient_ = new SshClient($"{networkObjectInterface.IP.IPv4}:{login.Port}", login.Username, login.Password);
            sshClient_.Connect();
            sshClient = sshClient_;
        }

        public async Task<string> ExecuteSSH(string command)
        {
            if (sshClient == null) { throw new EntryPointNotFoundException("Open a ssh connection first!"); }

            var ssh_command = sshClient.RunCommand(command);
            var async_execute = ssh_command.ExecuteAsync();
            await async_execute;
            string result = ssh_command.Result;

            return result;
        }

        public void OpenSFTP(Login login)
        {
            if (sftpClient != null) { return; }
            NetworkObjectInterface? networkObjectInterface = GetInterfaceInSameNetworkAsHost();
            if (networkObjectInterface == null || string.IsNullOrWhiteSpace(networkObjectInterface.IP.IPv4))
            {
                throw new EntryPointNotFoundException("No path to Remote Device Available");
            }

            SftpClient sftpClient_ = new SftpClient($"{networkObjectInterface.IP.IPv4}:{login.Port}", login.Username, login.Password);
            sftpClient_.Connect();
            sftpClient = sftpClient_;
        }

        public void UploadSFTP(string localPath, string remotePath)
        {
            if (sftpClient == null) { throw new EntryPointNotFoundException("Open a sftp connection first!"); }
            using (var fileStream = File.OpenRead(localPath))
            {
                sftpClient.UploadFile(fileStream, remotePath);
            }
        }

        public void DownloadSFTP(string localPath, string remotePath)
        {
            if (sftpClient == null) { throw new EntryPointNotFoundException("Open a sftp connection first!"); }
            using (var fileStream = File.Create(localPath))
            {
                sftpClient.DownloadFile(remotePath, fileStream);
            }
        }

        public void OpenSCP(Login login)
        {
            if (scpClient != null) { return; }
            NetworkObjectInterface? networkObjectInterface = GetInterfaceInSameNetworkAsHost();
            if (networkObjectInterface == null || string.IsNullOrWhiteSpace(networkObjectInterface.IP.IPv4))
            {
                throw new EntryPointNotFoundException("No path to Remote Device Available");
            }

            ScpClient scpClient_ = new ScpClient($"{networkObjectInterface.IP.IPv4}:{login.Port}", login.Username, login.Password);
            scpClient_.Connect();
            scpClient = scpClient_;
        }

        public void UploadSCP(string localPath, string remotePath)
        {
            if (scpClient == null) { throw new EntryPointNotFoundException("Open a scp connection first!"); }
            scpClient.Upload(new FileInfo(localPath), remotePath);
        }

        public void DownloadSCP(string localPath, string remotePath)
        {
            if (scpClient == null) { throw new EntryPointNotFoundException("Open a scp connection first!"); }
            scpClient.Download(remotePath, new FileInfo(localPath));
        }

        public void OpenTelnet(Login login)
        {
            // TODO
            throw new NotImplementedException();
        }

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
                    if (networkObjectInterface.IP.IPv4SubnetMask != ipv4subnet){continue;}

                    //TODO: check if there is a wire connection aka: own machine interface -cable- switch -cable- remote machine interface (also wifi to AccessPoint)

                    interface_ = networkObjectInterface;
                }
            }

            return interface_;
        }
        
    }
}
