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
    /// <summary>A host found by an nmap ping scan.</summary>
    public class DiscoveredHost
    {
        public string Ip = "";
        public string Hostname = "";
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
        public static async Task<List<DiscoveredHost>> ScanLocalSubnetAsync()
        {
            string? nmap = FindNmap();
            if (nmap == null) { throw new InvalidOperationException("nmap is not installed."); }

            var subnet = GetLocalSubnet();
            if (subnet == null) { throw new InvalidOperationException("Could not determine the local network."); }

            string target = subnet.Value.subnetBase + "0/24";

            string output = await Task.Run(() =>
            {
                using Process p = new Process();
                p.StartInfo = new ProcessStartInfo(nmap, $"-sn {target}")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                p.Start();
                string o = p.StandardOutput.ReadToEnd();
                p.WaitForExit(120000);
                return o;
            });

            return ParseScan(output);
        }

        // "Nmap scan report for hostname (192.168.1.10)" or "Nmap scan report for 192.168.1.10"
        private static readonly Regex ReportLine =
            new(@"Nmap scan report for (?:(?<host>[^\s()]+) \()?(?<ip>\d{1,3}(?:\.\d{1,3}){3})\)?");

        public static List<DiscoveredHost> ParseScan(string nmapOutput)
        {
            List<DiscoveredHost> hosts = new List<DiscoveredHost>();
            foreach (Match m in ReportLine.Matches(nmapOutput))
            {
                hosts.Add(new DiscoveredHost
                {
                    Ip = m.Groups["ip"].Value,
                    Hostname = m.Groups["host"].Success ? m.Groups["host"].Value : ""
                });
            }
            return hosts;
        }
    }
    //KI end
}
