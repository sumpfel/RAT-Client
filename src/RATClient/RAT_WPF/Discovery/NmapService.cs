using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up) { continue; }
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback) { continue; }
                foreach (var ua in nic.GetIPProperties().UnicastAddresses)
                {
                    if (ua.Address.AddressFamily != AddressFamily.InterNetwork) { continue; }
                    string ip = ua.Address.ToString();
                    if (ip.StartsWith("169.254")) { continue; } // link-local, no real network
                    int lastDot = ip.LastIndexOf('.');
                    if (lastDot > 0) { return (ip, ip.Substring(0, lastDot + 1)); }
                }
            }
            return null;
        }

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
            // -sn = ping scan (hosts only); -F = fast scan of the ~100 most common ports (hosts + open ports)
            string args = scanPorts ? $"-F {target}" : $"-sn {target}";
            int timeoutMs = scanPorts ? 300000 : 120000;

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

            return ParseScan(output);
        }

        // "Nmap scan report for hostname (192.168.1.10)" or "Nmap scan report for 192.168.1.10"
        private static readonly Regex ReportLine =
            new(@"Nmap scan report for (?:(?<host>[^\s()]+) \()?(?<ip>\d{1,3}(?:\.\d{1,3}){3})\)?");
        // "22/tcp   open  ssh"
        private static readonly Regex PortLine =
            new(@"^(?<port>\d+)/tcp\s+open\b");

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
                }
            }
            return hosts;
        }
    }
    //KI end
}
