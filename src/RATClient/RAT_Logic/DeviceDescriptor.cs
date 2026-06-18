using System;

namespace RAT_Logic
{
    //KI start (Claude Opus 4.8, prompt 21): abstract inheritance — a small polymorphic hierarchy that describes
    // each NetworkObjectType (icon key, friendly label, whether it can carry real host specs/NICs). Concrete
    // subclasses override the abstract members; NetworkObject.Descriptor resolves the right one for its Type.
    // This keeps type-specific behaviour out of long switch statements and is easy to unit-test.

    /// <summary>
    /// Base description of a kind of network device. Abstract: every concrete device kind must supply its
    /// icon key and display label, and may override the spec/NIC capability flags.
    /// </summary>
    public abstract class DeviceDescriptor
    {
        /// <summary>The <see cref="NetworkObjectType"/> this descriptor describes.</summary>
        public abstract NetworkObjectType Type { get; }

        /// <summary>Resource key of the icon used for this device kind (see Themes/Icons.xaml).</summary>
        public abstract string IconKey { get; }

        /// <summary>Human-friendly label shown in the UI.</summary>
        public abstract string DisplayLabel { get; }

        /// <summary>
        /// Whether this device kind can be backed by the real host machine's specs/interfaces.
        /// Only a PC represents "this machine", so it is the only one that returns true by default.
        /// </summary>
        public virtual bool CanUseHostSpecs => false;

        /// <summary>Returns the descriptor for a given type (factory).</summary>
        public static DeviceDescriptor For(NetworkObjectType type) => type switch
        {
            NetworkObjectType.PC => new PcDescriptor(),
            NetworkObjectType.Router => new RouterDescriptor(),
            NetworkObjectType.Switch => new SwitchDescriptor(),
            NetworkObjectType.Server => new ServerDescriptor(),
            NetworkObjectType.Client => new ClientDescriptor(),
            NetworkObjectType.Hub => new HubDescriptor(), // KI (prompt 26)
            NetworkObjectType.Cloud => new CloudDescriptor(), // KI (prompt 27)
            NetworkObjectType.AccessPoint => new AccessPointDescriptor(), // KI (prompt 28)
            _ => new ClientDescriptor()
        };
    }

    /// <summary>A personal computer — the only kind that can mirror the real host's specs/NICs.</summary>
    public sealed class PcDescriptor : DeviceDescriptor
    {
        public override NetworkObjectType Type => NetworkObjectType.PC;
        public override string IconKey => "Icon.PC";
        public override string DisplayLabel => "PC";
        public override bool CanUseHostSpecs => true;
    }

    /// <summary>A router.</summary>
    public sealed class RouterDescriptor : DeviceDescriptor
    {
        public override NetworkObjectType Type => NetworkObjectType.Router;
        public override string IconKey => "Icon.Router";
        public override string DisplayLabel => "Router";
    }

    /// <summary>A switch.</summary>
    public sealed class SwitchDescriptor : DeviceDescriptor
    {
        public override NetworkObjectType Type => NetworkObjectType.Switch;
        public override string IconKey => "Icon.Switch";
        public override string DisplayLabel => "Switch";
    }

    /// <summary>A server.</summary>
    public sealed class ServerDescriptor : DeviceDescriptor
    {
        public override NetworkObjectType Type => NetworkObjectType.Server;
        public override string IconKey => "Icon.Server";
        public override string DisplayLabel => "Server";
    }

    /// <summary>A generic client device.</summary>
    public sealed class ClientDescriptor : DeviceDescriptor
    {
        public override NetworkObjectType Type => NetworkObjectType.Client;
        public override string IconKey => "Icon.Client";
        public override string DisplayLabel => "Client";
    }

    //KI start (Claude Opus 4.8, prompt 26): a hub (reuses the switch icon).
    /// <summary>A hub.</summary>
    public sealed class HubDescriptor : DeviceDescriptor
    {
        public override NetworkObjectType Type => NetworkObjectType.Hub;
        public override string IconKey => "Icon.Switch";
        public override string DisplayLabel => "Hub";
    }
    //KI end

    //KI start (Claude Opus 4.8, prompt 27): the internet, drawn as a cloud (added by tracert-based discovery).
    /// <summary>The internet / an external network, reached through the router.</summary>
    public sealed class CloudDescriptor : DeviceDescriptor
    {
        public override NetworkObjectType Type => NetworkObjectType.Cloud;
        public override string IconKey => "Icon.Cloud";
        public override string DisplayLabel => "Internet";
    }
    //KI end

    //KI start (Claude Opus 4.8, prompt 28): a Wi-Fi access point.
    /// <summary>A wireless access point.</summary>
    public sealed class AccessPointDescriptor : DeviceDescriptor
    {
        public override NetworkObjectType Type => NetworkObjectType.AccessPoint;
        public override string IconKey => "Icon.AccessPoint";
        public override string DisplayLabel => "Access Point";
    }
    //KI end
    //KI end
}
