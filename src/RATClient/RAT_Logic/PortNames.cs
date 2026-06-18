using System.Collections.Generic;

namespace RAT_Logic
{
    //KI start (Claude Opus 4.8, prompt 26): friendly names for common TCP/UDP ports, so the UI can show
    // "22 - SSH", "80 - HTTP" etc. for discovered open ports.
    public static class PortNames
    {
        private static readonly Dictionary<int, string> _common = new()
        {
            [20] = "FTP-Data",
            [21] = "FTP",
            [22] = "SSH",
            [23] = "Telnet",
            [25] = "SMTP",
            [53] = "DNS",
            [67] = "DHCP",
            [68] = "DHCP",
            [69] = "TFTP",
            [80] = "HTTP",
            [110] = "POP3",
            [123] = "NTP",
            [135] = "MS-RPC",
            [137] = "NetBIOS",
            [139] = "NetBIOS",
            [143] = "IMAP",
            [161] = "SNMP",
            [162] = "SNMP-Trap",
            [389] = "LDAP",
            [443] = "HTTPS",
            [445] = "SMB",
            [465] = "SMTPS",
            [587] = "SMTP",
            [636] = "LDAPS",
            [993] = "IMAPS",
            [995] = "POP3S",
            [1433] = "MSSQL",
            [1521] = "Oracle",
            [3306] = "MySQL",
            [3389] = "RDP",
            [5432] = "PostgreSQL",
            [5900] = "VNC",
            [6379] = "Redis",
            [8080] = "HTTP-Alt",
            [8443] = "HTTPS-Alt",
            [27017] = "MongoDB",
        };

        /// <summary>The service name for a well-known port, or null if unknown.</summary>
        public static string? ServiceName(int port) =>
            _common.TryGetValue(port, out string? name) ? name : null;

        /// <summary>"22 - SSH" for known ports, otherwise just "22".</summary>
        public static string Describe(int port)
        {
            string? name = ServiceName(port);
            return name == null ? port.ToString() : $"{port} - {name}";
        }
    }
    //KI end
}
