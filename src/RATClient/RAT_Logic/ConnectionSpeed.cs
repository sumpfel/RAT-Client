using System.Collections.Generic;

namespace RAT_Logic
{
    //KI start (Claude Opus 4.8, prompt 25): preset link speeds for the cable-edit dropdown. Stored value is bits/s.
    /// <summary>A named link-speed preset (label shown in the dropdown, value stored in bits/s).</summary>
    public class ConnectionSpeed
    {
        public string Label { get; }
        public long Bps { get; }

        public ConnectionSpeed(string label, long bps)
        {
            Label = label;
            Bps = bps;
        }

        public override string ToString() => Label;

        private const long M = 1_000_000L;
        private const long G = 1_000_000_000L;

        /// <summary>Standard Ethernet cable speeds.</summary>
        public static readonly IReadOnlyList<ConnectionSpeed> Ethernet = new List<ConnectionSpeed>
        {
            new ConnectionSpeed("10 Mbps (Ethernet)", 10 * M),
            new ConnectionSpeed("100 Mbps (Fast Ethernet)", 100 * M),
            new ConnectionSpeed("1 Gbps (Gigabit)", 1 * G),
            new ConnectionSpeed("2.5 Gbps", 2_500 * M),
            new ConnectionSpeed("5 Gbps", 5 * G),
            new ConnectionSpeed("10 Gbps", 10 * G),
            new ConnectionSpeed("40 Gbps", 40 * G),
        };

        /// <summary>Standard Wi-Fi link rates.</summary>
        public static readonly IReadOnlyList<ConnectionSpeed> Wifi = new List<ConnectionSpeed>
        {
            new ConnectionSpeed("11 Mbps (802.11b)", 11 * M),
            new ConnectionSpeed("54 Mbps (802.11g)", 54 * M),
            new ConnectionSpeed("600 Mbps (802.11n)", 600 * M),
            new ConnectionSpeed("1300 Mbps (802.11ac)", 1300 * M),
            new ConnectionSpeed("9600 Mbps (802.11ax)", 9600 * M),
        };

        /// <summary>The preset list for a connection type.</summary>
        public static IReadOnlyList<ConnectionSpeed> For(NetworkConnectionType type) =>
            type == NetworkConnectionType.Wireless ? Wifi : Ethernet;
    }
    //KI end
}
