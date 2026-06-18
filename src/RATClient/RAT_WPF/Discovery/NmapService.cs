using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RAT_WPF.Discovery
{
    //KI start (Claude Opus 4.8, prompt 25): nmap integration — detect / install nmap and scan the local subnet.
    /// <summary>A host found by an nmap scan (ports filled only when a port scan was requested).</summary>
    public class DiscoveredHost
    {
        public string Ip = "";
        public string Hostname = "";
        //KI (prompt 26): open TCP ports found (when scanWithPorts was used)
        public List<int> OpenPorts = new List<int>();
        //KI start (Claude Opus 4.8, prompt 27): extra info nmap can sometimes give us (best-effort, may be empty).
        public string Os = "";          // OS guess from "OS details:" / "Running:" (nmap -O)
        public string SubnetMask = "";  // /24 by default for the local scan; refined from our own NIC where known
        //KI end
    }

    public static class NmapService
    {
        // official nmap setup the user asked to use
        public const string NmapInstallerUrl = "https://nmap.org/dist/nmap-7.99-setup.exe";

        /// <summary>Common install locations + PATH; returns the nmap.exe path or null if not found.</summary>
        public static string? FindNmap()
        {
            string[] candidates =
            {
                @"C:\Program Files (x86)\Nmap\nmap.exe",
                @"C:\Program Files\Nmap\nmap.exe",
            };
            foreach (string c in candidates)
            {
                if (File.Exists(c)) { return c; }
            }
            // on PATH?
            try
            {
                using Process p = new Process();
                p.StartInfo = new ProcessStartInfo("nmap", "--version")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                p.Start();
                p.WaitForExit(4000);
                if (p.ExitCode == 0) { return "nmap"; }
            }
            catch { /* not on PATH */ }
            return null;
        }

        public static bool IsInstalled() => FindNmap() != null;

        /// <summary>
        /// Downloads the nmap setup and runs it. Returns true if nmap is present afterwards. The setup itself is
        /// interactive (UAC + its own wizard); we wait for it to finish, then re-check.
        /// </summary>
        public static async Task<bool> InstallAsync()
        {
            string tmp = Path.Combine(Path.GetTempPath(), "nmap-setup.exe");
            using (HttpClient http = new HttpClient())
            {
                http.Timeout = TimeSpan.FromMinutes(5);
                byte[] bytes = await http.GetByteArrayAsync(NmapInstallerUrl);
                await File.WriteAllBytesAsync(tmp, bytes);
            }

            ProcessStartInfo psi = new ProcessStartInfo(tmp)
            {
                UseShellExecute = true // allows the UAC elevation prompt the installer needs
            };
            using Process? p = Process.Start(psi);
            if (p != null)
            {
                await Task.Run(() => p.WaitForExit());
            }

            return IsInstalled();
        }

        /// <summary>The host's primary IPv4 + its /24 base (e.g. 192.168.1.) for a local scan.</summary>
        public static (string ip, string subnetBase)? GetLocalSubnet()
        {
            var info = GetLocalSubnetInfo();
            return info == null ? null : (info.Value.ip, info.Value.subnetBase);
        }

        //KI start (Claude Opus 4.8, prompt 27): richer local-network info — also returns the real subnet mask and
        // the default gateway (our router) from the active NIC. We still scan the /24, but we can now stamp the
        // host's true subnet mask onto discovered devices and know which IP is the router.
        public static (string ip, string subnetBase, string subnetMask, string gateway)? GetLocalSubnetInfo()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up) { continue; }
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback) { continue; }

                IPInterfaceProperties props = nic.GetIPProperties();
                foreach (var ua in props.UnicastAddresses)
                {
                    if (ua.Address.AddressFamily != AddressFamily.InterNetwork) { continue; }
                    string ip = ua.Address.ToString();
                    if (ip.StartsWith("169.254")) { continue; } // link-local, no real network
                    int lastDot = ip.LastIndexOf('.');
                    if (lastDot <= 0) { continue; }

                    string mask = ua.IPv4Mask?.ToString() ?? "255.255.255.0";
                    string gateway = props.GatewayAddresses
                        .Select(g => g.Address)
                        .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)?.ToString() ?? "";

                    return (ip, ip.Substring(0, lastDot + 1), mask, gateway);
                }
            }
            return null;
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 28): is the active (IPv4, has-gateway) connection a Wi-Fi NIC? Used so
        // discovery can model PC -(wifi)- AccessPoint - Switch - devices instead of a direct wired uplink.
        public static bool IsActiveConnectionWireless()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up) { continue; }
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback) { continue; }

                IPInterfaceProperties props = nic.GetIPProperties();
                bool hasIpv4 = props.UnicastAddresses.Any(ua =>
                    ua.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !ua.Address.ToString().StartsWith("169.254"));
                if (!hasIpv4) { continue; }

                // this is the NIC GetLocalSubnetInfo would have picked; report whether it's wireless
                return nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211;
            }
            return false;
        }
        //KI end

        /// <summary>
        /// Runs an nmap ping scan over the host's local /24 and returns the live hosts (IP + hostname).
        /// </summary>
        /// <summary>
        /// Scans the host's local /24. With <paramref name="scanPorts"/> a fast top-ports scan is run so each host's
        /// open TCP ports are filled (slower); otherwise a quick ping scan (-sn) is used.
        /// </summary>
        public static async Task<List<DiscoveredHost>> ScanLocalSubnetAsync(bool scanPorts = false)
        {
            string? nmap = FindNmap();
            if (nmap == null) { throw new InvalidOperationException("nmap is not installed."); }

            var subnet = GetLocalSubnet();
            if (subnet == null) { throw new InvalidOperationException("Could not determine the local network."); }

            string target = subnet.Value.subnetBase + "0/24";
            //KI (prompt 27): when probing ports also attempt OS detection (-O) so we can fill the device OS.
            // -O needs admin/raw sockets; if it isn't available nmap just omits the OS lines (harmless).
            // -sn = ping scan (hosts only); -F = fast scan of the ~100 most common ports (hosts + open ports)
            string args = scanPorts ? $"-F -O {target}" : $"-sn {target}";
            int timeoutMs = scanPorts ? 420000 : 120000;

            string output = await Task.Run(() =>
            {
                using Process p = new Process();
                p.StartInfo = new ProcessStartInfo(nmap, args)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                p.Start();
                string o = p.StandardOutput.ReadToEnd();
                p.WaitForExit(timeoutMs);
                return o;
            });

            List<DiscoveredHost> hosts = ParseScan(output);

            //KI start (Claude Opus 4.8, prompt 27): stamp the real local subnet mask onto every discovered host
            // (they share our /24, so they share our mask).
            string ourMask = GetLocalSubnetInfo()?.subnetMask ?? "255.255.255.0";
            foreach (DiscoveredHost h in hosts) { h.SubnetMask = ourMask; }
            //KI end

            return hosts;
        }

        //KI start (Claude Opus 4.8, prompt 31): scan ONE host's open ports — FAST. -F (top ~100 ports) with -T4
        // (aggressive timing), -n (no reverse DNS) and a hard --host-timeout so it can never hang for a minute.
        // This is the pass whose result is shown on the node, so it must be quick and must NOT depend on -O.
        public static async Task<List<int>> ScanHostPortsFastAsync(string ip)
        {
            string? nmap = FindNmap();
            if (nmap == null) { throw new InvalidOperationException("nmap is not installed."); }
            if (string.IsNullOrWhiteSpace(ip)) { return new List<int>(); }

            string output = await RunNmap(nmap, $"-F -T4 -n --host-timeout 20s {ip}", 25000);
            return ParseScan(output).FirstOrDefault(h => h.Ip == ip)?.OpenPorts ?? new List<int>();
        }

        /// <summary>Best-effort OS guess for one host (needs admin; returns "" if unavailable). Slow — run last.</summary>
        public static async Task<string> ScanHostOsAsync(string ip)
        {
            string? nmap = FindNmap();
            if (nmap == null || string.IsNullOrWhiteSpace(ip)) { return ""; }
            try
            {
                string output = await RunNmap(nmap, $"-O --osscan-guess -T4 -n --host-timeout 30s {ip}", 35000);
                return ParseScan(output).FirstOrDefault(h => h.Ip == ip)?.Os ?? "";
            }
            catch { return ""; }
        }

        // kept for compatibility: fast ports + best-effort OS (ports first, OS does not gate them)
        public static async Task<(List<int> openPorts, string os)> ScanHostPortsAsync(string ip)
        {
            List<int> ports = await ScanHostPortsFastAsync(ip);
            string os = await ScanHostOsAsync(ip);
            return (ports, os);
        }

        // run nmap with the given args and return stdout (helper for the single-host scans). KI (prompt 31): if the
        // process overruns the timeout it is killed so it can never block the per-device scan queue indefinitely.
        private static async Task<string> RunNmap(string nmap, string args, int timeoutMs)
        {
            return await Task.Run(() =>
            {
                using Process p = new Process();
                p.StartInfo = new ProcessStartInfo(nmap, args)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                p.Start();
                string o = p.StandardOutput.ReadToEnd();
                if (!p.WaitForExit(timeoutMs))
                {
                    try { p.Kill(true); } catch { /* already gone */ }
                }
                return o;
            });
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 27): run tracert to the internet and return the first hop (the local
        // router/gateway). Falls back to the NIC's configured default gateway when tracert can't resolve a hop.
        // The trailing reachability flag says whether the trace got past the first hop (i.e. the internet is up).
        public static async Task<(string routerIp, bool reachedInternet)> FindRouterAsync(string probeTarget = "8.8.8.8")
        {
            string gateway = GetLocalSubnetInfo()?.gateway ?? "";

            string output = await Task.Run(() =>
            {
                try
                {
                    using Process p = new Process();
                    // -d skip DNS, -h 5 only the first few hops, -w 800ms per hop so we don't hang for ~30s
                    p.StartInfo = new ProcessStartInfo("tracert", $"-d -h 5 -w 800 {probeTarget}")
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    p.Start();
                    string o = p.StandardOutput.ReadToEnd();
                    p.WaitForExit(30000);
                    return o;
                }
                catch { return ""; }
            });

            return ParseTracert(output, gateway);
        }

        // a tracert hop line carries one or more IPs; the first hop's last IP is the local router
        private static readonly Regex HopIp = new(@"(?<ip>\d{1,3}(?:\.\d{1,3}){3})");

        public static (string routerIp, bool reachedInternet) ParseTracert(string tracertOutput, string fallbackGateway)
        {
            string firstHop = "";
            int hopsSeen = 0;
            foreach (string raw in (tracertOutput ?? "").Split('\n'))
            {
                string line = raw.Trim();
                // hop lines start with the hop number, e.g. "  1    1 ms    1 ms    1 ms  192.168.1.1"
                if (line.Length == 0 || !char.IsDigit(line[0])) { continue; }
                Match m = HopIp.Match(line);
                if (!m.Success) { continue; } // "* * * Request timed out." -> still counts as a hop attempt below
                hopsSeen++;
                if (firstHop.Length == 0) { firstHop = m.Groups["ip"].Value; }
            }

            string router = firstHop.Length > 0 ? firstHop : fallbackGateway;
            bool reachedInternet = hopsSeen >= 2; // got past the local router
            return (router, reachedInternet);
        }
        //KI end

        // "Nmap scan report for hostname (192.168.1.10)" or "Nmap scan report for 192.168.1.10"
        private static readonly Regex ReportLine =
            new(@"Nmap scan report for (?:(?<host>[^\s()]+) \()?(?<ip>\d{1,3}(?:\.\d{1,3}){3})\)?");
        // "22/tcp   open  ssh"
        private static readonly Regex PortLine =
            new(@"^(?<port>\d+)/tcp\s+open\b");
        //KI (prompt 27): nmap -O OS lines, in order of preference. "OS details:" is the most specific.
        private static readonly Regex OsDetails = new(@"^OS details:\s*(?<os>.+)$");
        private static readonly Regex OsRunning = new(@"^Running:\s*(?<os>.+)$");
        private static readonly Regex OsGuess = new(@"^Aggressive OS guesses:\s*(?<os>.+)$");

        public static List<DiscoveredHost> ParseScan(string nmapOutput)
        {
            //KI (prompt 26): line-based so each "x/tcp open" attaches to the host of the preceding report block.
            List<DiscoveredHost> hosts = new List<DiscoveredHost>();
            DiscoveredHost? current = null;

            foreach (string raw in nmapOutput.Split('\n'))
            {
                string line = raw.Trim();
                Match report = ReportLine.Match(line);
                if (report.Success)
                {
                    current = new DiscoveredHost
                    {
                        Ip = report.Groups["ip"].Value,
                        Hostname = report.Groups["host"].Success ? report.Groups["host"].Value : ""
                    };
                    hosts.Add(current);
                    continue;
                }

                Match port = PortLine.Match(line);
                if (port.Success && current != null && int.TryParse(port.Groups["port"].Value, out int p))
                {
                    current.OpenPorts.Add(p);
                    continue;
                }

                //KI start (Claude Opus 4.8, prompt 27): capture nmap's OS guess (only set if not already set, so
                // the more specific "OS details:" wins over "Running:"/"Aggressive OS guesses:").
                if (current != null && string.IsNullOrEmpty(current.Os))
                {
                    Match os = OsDetails.Match(line);
                    if (!os.Success) { os = OsRunning.Match(line); }
                    if (!os.Success) { os = OsGuess.Match(line); }
                    if (os.Success) { current.Os = os.Groups["os"].Value.Trim(); }
                }
                //KI end
            }
            return hosts;
        }
    }
    //KI end
}
