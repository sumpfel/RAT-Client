using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_Logic
{
    //KI start (Claude Opus 4.8, prompt 1): built-in catalog of common SNMP OIDs for the MIB browser
    /// <summary>
    /// A single named node in the MIB tree (a friendly name mapped to a numeric OID).
    /// </summary>
    public class MibNode
    {
        public string Name;
        public string Oid;
        public string Description;

        public MibNode(string name, string oid, string description)
        {
            Name = name;
            Oid = oid;
            Description = description;
        }

        public override string ToString() => $"{Name}  ({Oid})";
    }

    /// <summary>
    /// A small built-in set of well-known OIDs so the browser is useful without
    /// the user having to supply external .mib files. Users can still type any
    /// raw numeric OID to Get/Walk it.
    /// </summary>
    public static class MibCatalog
    {
        public static readonly IReadOnlyList<MibNode> CommonNodes = new List<MibNode>
        {
            new MibNode("system",            "1.3.6.1.2.1.1",     "System group (walk for all system info)"),
            new MibNode("sysDescr",          "1.3.6.1.2.1.1.1.0", "Textual description of the device"),
            new MibNode("sysObjectID",       "1.3.6.1.2.1.1.2.0", "Vendor authoritative identification"),
            new MibNode("sysUpTime",         "1.3.6.1.2.1.1.3.0", "Time since the network mgmt portion re-initialised"),
            new MibNode("sysContact",        "1.3.6.1.2.1.1.4.0", "Contact person for the node"),
            new MibNode("sysName",           "1.3.6.1.2.1.1.5.0", "Administratively assigned name"),
            new MibNode("sysLocation",       "1.3.6.1.2.1.1.6.0", "Physical location of the node"),
            new MibNode("sysServices",       "1.3.6.1.2.1.1.7.0", "Set of services offered"),
            new MibNode("interfaces",        "1.3.6.1.2.1.2",     "Interfaces group (walk for all interface info)"),
            new MibNode("ifNumber",          "1.3.6.1.2.1.2.1.0", "Number of network interfaces"),
            new MibNode("ifTable",           "1.3.6.1.2.1.2.2",   "Interface table (walk)"),
            new MibNode("ifDescr",           "1.3.6.1.2.1.2.2.1.2", "Interface descriptions (walk)"),
            new MibNode("ifType",            "1.3.6.1.2.1.2.2.1.3", "Interface types (walk)"),
            new MibNode("ifSpeed",           "1.3.6.1.2.1.2.2.1.5", "Interface speeds (walk)"),
            new MibNode("ifPhysAddress",     "1.3.6.1.2.1.2.2.1.6", "Interface MAC addresses (walk)"),
            new MibNode("ifOperStatus",      "1.3.6.1.2.1.2.2.1.8", "Interface operational status (walk)"),
            new MibNode("ip",                "1.3.6.1.2.1.4",     "IP group (walk)"),
            new MibNode("ipAddrTable",       "1.3.6.1.2.1.4.20",  "IP address table (walk)"),
            new MibNode("ipRouteTable",      "1.3.6.1.2.1.4.21",  "IP routing table (walk)"),
            new MibNode("tcpConnTable",      "1.3.6.1.2.1.6.13",  "TCP connection table (walk)"),
            new MibNode("hrStorageTable",    "1.3.6.1.2.1.25.2.3", "Host storage / disk table (walk)"),
            new MibNode("hrProcessorTable",  "1.3.6.1.2.1.25.3.3", "Host CPU load table (walk)"),
        };
    }
    //KI end
}
