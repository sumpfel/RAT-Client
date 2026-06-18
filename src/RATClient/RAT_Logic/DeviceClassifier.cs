using System.Collections.Generic;
using System.Linq;

namespace RAT_Logic
{
    //KI start (Claude Opus 4.8, prompt 27): heuristics for naming switch interfaces like a real Cisco switch and
    // for guessing a discovered host's device type from the set of open TCP ports nmap found. Kept in RAT_Logic
    // (no UI/nmap dependency) so it is easy to unit-test.
    public static class DeviceClassifier
    {
        /// <summary>
        /// A Cisco-style switchport name for the (zero-based) port index, e.g. 0 -> "GigabitEthernet0/1".
        /// Real switches number ports from 1, so we add 1.
        /// </summary>
        public static string CiscoInterfaceName(int zeroBasedPort) => $"GigabitEthernet0/{zeroBasedPort + 1}";

        // Ports that strongly indicate a "real" server (databases, mail, directory, app stacks). A host exposing
        // several of these — or a database/app port at all — is almost certainly a server, not a workstation/switch.
        private static readonly HashSet<int> ServerServicePorts = new()
        {
            25, 110, 143, 389, 443, 465, 587, 636, 993, 995,   // mail / directory / https
            1433, 1521, 3306, 5432, 6379, 27017,               // databases
            8080, 8443                                          // app servers
        };

        // Database ports on their own are a very strong server signal (a workstation rarely exposes MySQL/Postgres).
        private static readonly HashSet<int> DatabasePorts = new() { 1433, 1521, 3306, 5432, 6379, 27017 };

        /// <summary>
        /// Guesses a device type from the open ports nmap found.
        /// Rules (from the user's brief):
        ///  - SSH (22) alone must NOT make it a Server (likely a managed switch / appliance) -> Switch.
        ///  - HTTP (80) alone might just be a device's web UI -> Switch.
        ///  - A database port, or several "server" service ports (e.g. 80 + 443 + 3306), -> Server.
        ///  - Otherwise a plain host -> Client.
        /// </summary>
        public static NetworkObjectType ClassifyByPorts(IEnumerable<int> openPorts)
        {
            HashSet<int> ports = new HashSet<int>(openPorts ?? Enumerable.Empty<int>());

            if (ports.Count == 0) { return NetworkObjectType.Client; }

            // any database port -> definitely a server
            if (ports.Any(DatabasePorts.Contains)) { return NetworkObjectType.Server; }

            int serverSignals = ports.Count(ServerServicePorts.Contains);
            // two or more distinct "server" service ports (e.g. http + https, https + mail) -> server
            if (serverSignals >= 2) { return NetworkObjectType.Server; }

            // exactly SSH and nothing else (or SSH + the device's own web UI) -> likely a managed switch/appliance
            bool onlySsh = ports.SetEquals(new[] { 22 });
            bool onlyHttp = ports.SetEquals(new[] { 80 }) || ports.SetEquals(new[] { 443 });
            bool sshPlusWebUi = ports.All(p => p == 22 || p == 80 || p == 443);
            if (onlySsh || onlyHttp || sshPlusWebUi) { return NetworkObjectType.Switch; }

            // a single server-ish service (just https, or just a mail port) -> lean server, else a generic client
            return serverSignals >= 1 ? NetworkObjectType.Server : NetworkObjectType.Client;
        }
    }
    //KI end
}
