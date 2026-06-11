using System;
using System.Collections.Generic;

namespace RAT_Logic
{
    //KI start (Claude Opus 4.8, prompt 2): structured info about a real host network interface (for the PC interface list)
    /// <summary>The logical category of a host interface, used to pick an icon and to filter out garbage.</summary>
    public enum HostInterfaceKind
    {
        Ethernet,
        Wifi,
        UsbEthernet,
        Other
    }

    /// <summary>
    /// A real network interface on the host PC with the details worth showing in the UI.
    /// </summary>
    public class HostInterfaceInfo
    {
        public string Name = "";
        public string Description = "";
        public HostInterfaceKind Kind;
        public bool IsUp;
        public string Mac = "";
        public string SpeedText = "";
        public List<string> IPv4 = new List<string>();
        public List<string> IPv6 = new List<string>();

        public string StatusText => IsUp ? "UP" : "DOWN";
    }
    //KI end
}
