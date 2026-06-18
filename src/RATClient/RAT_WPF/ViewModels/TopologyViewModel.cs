using RAT_Logic;
using RAT_WPF;
using RAT_WPF.Commands;
using RAT_WPF.Stores;
using RAT_WPF.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace RAT_WPF.ViewModels
{
    public enum EnumTool
    {
        Cursor,
        Connector,
        Delete
    };

    public class TopologyViewModel : ViewModelBase
    {
        public NetworkObjectListingViewModel defaultItems { get; }

        public ICommand NetworkObjectAddedCommand { get; }

        public ICommand NetworkObjectAddConnectionCommand { get; }

        public ICommand NetworkObjectDeleteCommand { get; }

        //KI start (Claude Opus 4.8, prompt 15): navigation store + logout command so the user can return to login.
        private readonly Stores.NavigationStore? _navigationStore;
        public ICommand LogoutCommand { get; }
        //KI end

        private readonly ObservableCollection<NetworkObjectViewModel> _networkObjects;

        public IEnumerable<NetworkObjectViewModel> NetworkObjects => _networkObjects;

        private readonly ObservableCollection<NetworkConnectionViewModel> _networkConnectionViewModels = new ObservableCollection<NetworkConnectionViewModel>();
        public IEnumerable<NetworkConnectionViewModel> NetworkConnectionViewModels => _networkConnectionViewModels;

        private EnumTool _toolEnum = EnumTool.Cursor;
        public EnumTool ToolEnum
        {
            get => _toolEnum;

            set
            {
                _toolEnum = value;
                OnPropertyChanged(nameof(ToolEnum));
            }
        }

        //KI start (Claude Opus 4.8, prompt 15): keep the parameterless ctor working (debug-skip-login path) by
        // chaining to the new one with no navigation store.
        public TopologyViewModel() : this(null) { }
        //KI end

        public TopologyViewModel(Stores.NavigationStore? navigationStore)
        {
            _navigationStore = navigationStore; // KI (prompt 15)
            LogoutCommand = new Commands.LogoutCommand(this); // KI (prompt 15)

            defaultItems = new NetworkObjectListingViewModel();

            defaultItems.AddNetworkObject(new NetworkObject() { Type = NetworkObjectType.Router , Name = "New Router", Settings = new NetworkObjectSettings()});
            defaultItems.AddNetworkObject(new NetworkObject() { Type = NetworkObjectType.Switch , Name = "New Switch", Settings = new NetworkObjectSettings()});
            defaultItems.AddNetworkObject(new NetworkObject() { Type = NetworkObjectType.Server , Name = "New Server", Settings = new NetworkObjectSettings()});
            defaultItems.AddNetworkObject(new NetworkObject() { Type = NetworkObjectType.Client , Name = "New Client", Settings = new NetworkObjectSettings()});
            //KI start (Claude Opus 4.8, prompt 28): the internet Cloud and the Wi-Fi Access Point are now draggable
            // sidebar objects too (like PC/Client).
            defaultItems.AddNetworkObject(new NetworkObject() { Type = NetworkObjectType.AccessPoint, Name = "New Access Point", Settings = new NetworkObjectSettings()});
            defaultItems.AddNetworkObject(new NetworkObject() { Type = NetworkObjectType.Cloud, Name = "Internet", Settings = new NetworkObjectSettings()});
            //KI end
            //KI start (Claude Opus 4.8, prompt 4/12): default PC name = this machine's name (still changeable);
            // and the PC carries this machine's real interfaces so they can be selected in the SelectInterfaceWindow.
            NetworkObject ownPc = new NetworkObject() { Type = NetworkObjectType.PC, Name = Environment.MachineName, Settings = new NetworkObjectSettings() };
            ownPc.PopulateOwnDeviceInterfaces();
            ownPc.ApplyDefaultPcSpecsIfEmpty(); // KI (prompt 17): a PC starts with this machine's real specs, not blanks
            defaultItems.AddNetworkObject(ownPc);
            //KI end

            // NetworkObjectAddedCommand = new NetworkObjectAddedCommand(defaultItems.NetworkObjects);

            this.NetworkObjectAddConnectionCommand = new NetworkObjectAddConnectionCommand(this);

            this.NetworkObjectDeleteCommand = new NetworkObjectDeleteCommand(this);

            _networkObjects = new ObservableCollection<NetworkObjectViewModel>()
            {
                /* For testing

                new NetworkObjectViewModel(new NetworkObject() { Type = NetworkObjectType.Router , Name = "Test", Settings = new NetworkObjectSettings(), X = 50, Y=100}),
                new NetworkObjectViewModel(new NetworkObject() { Type = NetworkObjectType.Router , Name = "Test2", Settings = new NetworkObjectSettings(), X = 350, Y=250})
                */
            };

            //KI start (Claude Opus 4.8, prompt: link the C# frontend with the RAT-Backend database):
            // load the saved topology from the backend once we are logged in. Fire-and-forget so the
            // constructor stays synchronous (matching the existing code); errors are shown but don't
            // crash the app. When there is no connection (debug-skip login) the canvas just starts empty.
            _ = LoadFromDatabaseAsync();
            //KI end
        }

        //KI start (Claude Opus 4.8, prompt 15): log out — clear the session + connection and go back to the
        // login screen. The server IP/port stay remembered in DatabaseConnectionStore so the login form only
        // needs the username + password again.
        public void Logout()
        {
            RAT_WPF.Logging.AppLogger.Info($"Logout: user '{Session.CurrentUser?.UserName}'"); // KI (prompt 28)
            DatabaseConnectionStore.Current = null;
            Session.CurrentUser = null;
            if (_navigationStore != null)
            {
                _navigationStore.CurrentViewModel = new LoginViewModel(_navigationStore);
            }
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt: link the C# frontend with the RAT-Backend database):
        /// <summary>Loads the network graph from the database and puts every device on the canvas.</summary>
        private async Task LoadFromDatabaseAsync()
        {
            if (DatabaseConnectionStore.Current == null) { return; }

            try
            {
                RAT_WPF.Logging.AppLogger.Info("Topology: loading graph from database"); // KI (prompt 28)
                NetworkObjectGraph graph = await DatabaseConnectionStore.Current.GetNetworkGraph();
                if (graph?.networkObjects == null) { RAT_WPF.Logging.AppLogger.Info("Topology: empty graph"); return; }
                RAT_WPF.Logging.AppLogger.Info($"Topology: loaded {graph.networkObjects.Count} device(s)"); // KI (prompt 28)

                // first the devices; keep a model -> view-model map so connections can find their endpoints
                Dictionary<NetworkObject, NetworkObjectViewModel> vmByModel = new Dictionary<NetworkObject, NetworkObjectViewModel>();
                foreach (NetworkObject networkObject in graph.networkObjects)
                {
                    NetworkObjectViewModel vm = new NetworkObjectViewModel(networkObject);
                    vmByModel[networkObject] = vm;
                    AddNetworkObjectViewModelToCanvas(vm);
                }

                //KI start (Claude Opus 4.8, prompt 15): also draw the saved connections. GetNetworkGraph() already
                // rebuilt each NetworkConnection and set it on both endpoint interfaces; here we turn every distinct
                // connection into a NetworkConnectionViewModel between the two devices it joins.
                HashSet<NetworkConnection> seen = new HashSet<NetworkConnection>();
                foreach (NetworkObject networkObject in graph.networkObjects)
                {
                    foreach (NetworkObjectInterface iface in networkObject.NetworkInterfaces)
                    {
                        NetworkConnection? conn = iface.Connection;
                        if (conn == null || !seen.Add(conn)) { continue; } // each connection only once

                        NetworkObjectViewModel? sourceVm = vmByModel.Values.FirstOrDefault(
                            v => v.networkObjectInterfaces.Contains(conn.networkObectInterfaces[0]));
                        NetworkObjectViewModel? targetVm = vmByModel.Values.FirstOrDefault(
                            v => v.networkObjectInterfaces.Contains(conn.networkObectInterfaces[1]));

                        if (sourceVm != null && targetVm != null)
                        {
                            _networkConnectionViewModels.Add(new NetworkConnectionViewModel(conn, sourceVm, targetVm));
                        }
                    }
                }
                OnPropertyChanged(nameof(NetworkConnectionViewModels));
                //KI end
            }
            catch (Exception ex)
            {
                RatDialog.Show("Database hiccup", $"The rat couldn't load the topology from the server.\n\n{ex.Message}", "Icon.DatabaseError");
            }
        }
        //KI end

        public void AddNetworkObjectViewModelToCanvas(NetworkObjectViewModel networkObject)
        {
            _networkObjects.Add(networkObject);
            OnPropertyChanged(nameof(NetworkConnectionViewModels));
        }

        //KI start (Claude Opus 4.8, prompt 25/26): nmap discovery.
        // 1) make sure the user's own PC is on the canvas (with its real interfaces);
        // 2) scan the local subnet (with open ports if ShowPorts is on);
        // 3) topology:
        //      - existing devices that aren't cabled to the PC -> cable them (directly or via the switch, see below);
        //      - brand-new hosts become Clients;
        //      - ONE new client and nothing else to wire -> a single 1:1 cable PC<->client;
        //      - MULTIPLE devices to wire -> create a Switch with enough interfaces and wire the PC + every device to it.
        // Everything created is persisted through the active connection (mock = local-only).
        public async Task DiscoverDevicesAsync()
        {
            RAT_WPF.Logging.AppLogger.Info("Discovery: starting"); // KI (prompt 28)

            //KI start (Claude Opus 4.8, prompt 29): only run discovery when nmap is actually installed.
            if (!Discovery.NmapService.IsInstalled())
            {
                RAT_WPF.Logging.AppLogger.Warn("Discovery: nmap is not installed — aborting");
                RatDialog.Show("nmap not installed",
                    "Network discovery needs nmap, which isn't installed. Install it from first-run setup to enable the Discover button.",
                    "Icon.NoConnection");
                return;
            }
            //KI end

            NetworkObjectViewModel pcVm = await EnsureOwnPcOnCanvasAsync();

            //KI (prompt 29): draw the topology from a FAST ping-only sweep (-sn). Port + OS probing is slow, so it's
            // done per-device in the background afterwards (see ScanPortsInBackground), and only when "Show ports"
            // is on — so the canvas appears quickly instead of waiting for a full -F -O scan of the whole subnet.
            List<Discovery.DiscoveredHost> hosts = await Discovery.NmapService.ScanLocalSubnetAsync(scanPorts: false);

            var subnetInfo = Discovery.NmapService.GetLocalSubnetInfo();
            string? ownIp = subnetInfo?.ip;
            //KI (prompt 28): log what the scan saw
            RAT_WPF.Logging.AppLogger.Info(
                $"Discovery: own ip={ownIp ?? "?"} mask={subnetInfo?.subnetMask ?? "?"} gateway={subnetInfo?.gateway ?? "?"} " +
                $"wireless={Discovery.NmapService.IsActiveConnectionWireless()}; nmap found {hosts.Count} host(s)");
            foreach (var h in hosts)
            {
                RAT_WPF.Logging.AppLogger.Debug($"Discovery: host {h.Ip} '{h.Hostname}' os='{h.Os}' ports=[{string.Join(",", h.OpenPorts)}]");
            }

            //KI start (Claude Opus 4.8, prompt 27): use tracert to find which discovered IP is the local router
            // (first hop towards the internet). That host becomes a Router, and — if the trace reached past it —
            // we also add an "internet" Cloud behind the router.
            (string routerIp, bool reachedInternet) route;
            try { route = await Discovery.NmapService.FindRouterAsync(); }
            catch (Exception ex) { RAT_WPF.Logging.AppLogger.Warn($"Discovery: tracert failed: {ex.Message}"); route = ("", false); }
            RAT_WPF.Logging.AppLogger.Info($"Discovery: router={route.routerIp} reachedInternet={route.reachedInternet}"); // KI (prompt 28)
            //KI end

            // split the scan results into "new hosts to add" and "already-modelled but not yet cabled to the PC"
            List<Discovery.DiscoveredHost> newHosts = new List<Discovery.DiscoveredHost>();
            List<NetworkObjectViewModel> existingToWire = new List<NetworkObjectViewModel>();

            foreach (Discovery.DiscoveredHost host in hosts)
            {
                if (string.IsNullOrWhiteSpace(host.Ip)) { continue; }
                if (ownIp != null && host.Ip == ownIp) { continue; } // our own PC

                NetworkObjectViewModel? existing = FindDeviceByIp(host.Ip);
                if (existing == null)
                {
                    newHosts.Add(host);
                }
                else
                {
                    // annotate the existing interface's ports/OS/mask too, and wire it if needed
                    AnnotateFromHost(existing, host);
                    if (!IsCabledTo(pcVm, existing)) { existingToWire.Add(existing); }
                }
            }

            // create the new devices first (not cabled yet); the router IP becomes a Router, the rest a typed device.
            List<NetworkObjectViewModel> newDevices = new List<NetworkObjectViewModel>();
            NetworkObjectViewModel? routerVm = null;
            foreach (Discovery.DiscoveredHost host in newHosts)
            {
                bool isRouter = !string.IsNullOrEmpty(route.routerIp) && host.Ip == route.routerIp;
                NetworkObjectViewModel vm = await AddDiscoveredDeviceAsync(host, isRouter ? NetworkObjectType.Router : (NetworkObjectType?)null);
                if (isRouter) { routerVm = vm; }
                newDevices.Add(vm);
            }

            // an already-modelled device might be the router too
            if (routerVm == null && !string.IsNullOrEmpty(route.routerIp))
            {
                routerVm = FindDeviceByIp(route.routerIp);
            }

            // everything that still needs a wire to the PC
            List<NetworkObjectViewModel> toWire = new List<NetworkObjectViewModel>();
            toWire.AddRange(newDevices);
            toWire.AddRange(existingToWire);

            //KI start (Claude Opus 4.8, prompt 28): if the host reaches the network over Wi-Fi, the PC's uplink is
            // wireless: model PC -(wifi)- AccessPoint, then the AccessPoint is what plugs into the switch (or the
            // single device). So the "uplink" node the rest of the topology hangs off is the AP, not the PC directly.
            bool wireless = Discovery.NmapService.IsActiveConnectionWireless();
            NetworkObjectViewModel uplink = pcVm;
            if (wireless && toWire.Count >= 1)
            {
                NetworkObjectViewModel apVm = await AddAccessPointAsync();
                apVm.X = pcVm.X + 160;
                apVm.Y = pcVm.Y;
                PersistMovedPosition(apVm);
                await CableWirelessAsync(pcVm, apVm); // dashed Wi-Fi link PC <-> AP
                uplink = apVm;
            }
            //KI end

            if (toWire.Count == 0)
            {
                // nothing new to connect
            }
            else if (toWire.Count == 1)
            {
                // exactly one device -> a direct 1:1 cable from the uplink (PC, or the AP on Wi-Fi)
                PlaceAround(uplink, new[] { toWire[0] });
                await CableAsync(uplink, toWire[0]);
            }
            else
            {
                // multiple devices -> a switch with enough interfaces, uplink + every device in a circle around it
                NetworkObjectViewModel switchVm = await AddSwitchAsync(toWire.Count + 1); // +1 for the uplink
                PlaceAround(switchVm, new[] { uplink }.Concat(toWire).ToList());

                int portIndex = 0;
                await CableViaSwitchAsync(uplink, switchVm, portIndex++);
                foreach (NetworkObjectViewModel device in toWire)
                {
                    await CableViaSwitchAsync(device, switchVm, portIndex++);
                }
            }

            //KI start (Claude Opus 4.8, prompt 27): if we found a router and the trace reached the internet, add an
            // "internet" Cloud behind the router and cable it to a spare router interface.
            if (routerVm != null && route.reachedInternet)
            {
                await EnsureCloudBehindRouterAsync(routerVm);
            }
            //KI end

            RAT_WPF.Logging.AppLogger.Info( // KI (prompt 28)
                $"Discovery: done — {newDevices.Count} new device(s), {existingToWire.Count} existing wired, " +
                $"{toWire.Count} total wired, wireless={wireless}");
            OnPropertyChanged(nameof(NetworkConnectionViewModels));

            //KI start (Claude Opus 4.8, prompt 29): the topology is drawn now — probe each discovered device's ports
            // (and OS) in the background, ONE at a time, only when "Show ports" is on. Each device's node updates as
            // its scan finishes, so the user isn't blocked and the slow per-host nmap work happens after the draw.
            if (RAT_WPF.Themes.DisplaySettings.ShowPorts)
            {
                List<NetworkObjectViewModel> scanTargets = new List<NetworkObjectViewModel>(newDevices);
                scanTargets.AddRange(existingToWire);
                _ = ScanPortsInBackgroundAsync(scanTargets);
            }
            //KI end
        }

        //KI start (Claude Opus 4.8, prompt 29): scan each device's ports/OS one at a time and annotate its node as
        // results arrive. Best-effort: a failure on one host is logged and skipped, the rest continue.
        private async Task ScanPortsInBackgroundAsync(IList<NetworkObjectViewModel> devices)
        {
            foreach (NetworkObjectViewModel vm in devices)
            {
                string? ip = vm.Model.NetworkInterfaces.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.IP?.IPv4))?.IP?.IPv4;
                if (string.IsNullOrWhiteSpace(ip)) { continue; }

                try
                {
                    RAT_WPF.Logging.AppLogger.Debug($"Discovery: background port scan of {ip} ('{vm.Model.Name}')");
                    (List<int> openPorts, string os) = await Discovery.NmapService.ScanHostPortsAsync(ip);

                    NetworkObjectInterface? iface = vm.Model.NetworkInterfaces.FirstOrDefault(i => i.IP?.IPv4 == ip);
                    if (iface != null && openPorts.Count > 0) { iface.OpenPorts = new List<int>(openPorts); }
                    if (!string.IsNullOrEmpty(os) && string.IsNullOrEmpty(vm.Model.Os)) { vm.Model.Os = os; }
                    vm.RefreshPorts();
                    RAT_WPF.Logging.AppLogger.Info($"Discovery: {ip} open ports=[{string.Join(",", openPorts)}] os='{os}'");
                }
                catch (Exception ex)
                {
                    RAT_WPF.Logging.AppLogger.Warn($"Discovery: background port scan of {ip} failed: {ex.Message}");
                }
            }
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 27): arrange devices evenly on a circle around a centre node.
        private void PlaceAround(NetworkObjectViewModel center, IList<NetworkObjectViewModel> around)
        {
            const int radius = 220;
            int cx = center.X, cy = center.Y;
            int n = around.Count;
            for (int i = 0; i < n; i++)
            {
                double angle = (2 * Math.PI * i) / Math.Max(1, n) - Math.PI / 2; // start at the top
                // KI (prompt 28): clamp onto the canvas so circle nodes never land in unreachable void
                around[i].X = Views.CanvasLayout.ClampX(cx + radius * Math.Cos(angle));
                around[i].Y = Views.CanvasLayout.ClampY(cy + radius * Math.Sin(angle));
                PersistMovedPosition(around[i]);
            }
        }

        // a position update for an already-saved device (fire-and-forget; no-op without a connection / unsaved)
        private async void PersistMovedPosition(NetworkObjectViewModel vm)
        {
            if (DatabaseConnectionStore.Current == null || vm.Model.ID <= 0) { return; }
            try { await DatabaseConnectionStore.Current.EditNetworkObject(vm.Model); } catch { /* best-effort */ }
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 26/27): create a Switch with the given number of interfaces, named
        // Cisco-style (GigabitEthernet0/1, 0/2, ...).
        private async Task<NetworkObjectViewModel> AddSwitchAsync(int interfaceCount)
        {
            NetworkObject model = new NetworkObject
            {
                Type = NetworkObjectType.Switch,
                Name = "Switch",
                X = 360,
                Y = 240
            };
            for (int i = 0; i < interfaceCount; i++)
            {
                model.NetworkInterfaces.Add(new NetworkObjectInterface
                {
                    Name = DeviceClassifier.CiscoInterfaceName(i), // KI (prompt 27): GigabitEthernet0/<n>
                    IsUp = true
                });
            }
            if (Session.CurrentUser != null) { model.ApplyRight(Session.CurrentUser, AccesRights.Owner); }

            await PersistObjectWithInterfacesAsync(model);

            NetworkObjectViewModel vm = new NetworkObjectViewModel(model);
            AddNetworkObjectViewModelToCanvas(vm);
            return vm;
        }

        //KI start (Claude Opus 4.8, prompt 28): add a Wi-Fi AccessPoint (a wlan radio + a wired uplink interface).
        private async Task<NetworkObjectViewModel> AddAccessPointAsync()
        {
            NetworkObject model = new NetworkObject
            {
                Type = NetworkObjectType.AccessPoint,
                Name = "Access Point",
                X = 200,
                Y = 120
            };
            model.NetworkInterfaces.Add(new NetworkObjectInterface { Name = "wlan0", IsUp = true });
            model.NetworkInterfaces.Add(new NetworkObjectInterface { Name = "uplink", IsUp = true });
            if (Session.CurrentUser != null) { model.ApplyRight(Session.CurrentUser, AccesRights.Owner); }

            await PersistObjectWithInterfacesAsync(model);

            NetworkObjectViewModel vm = new NetworkObjectViewModel(model);
            AddNetworkObjectViewModelToCanvas(vm);
            return vm;
        }

        // cable two devices with a WIRELESS (dashed) link on their first free interfaces
        private async Task CableWirelessAsync(NetworkObjectViewModel a, NetworkObjectViewModel b)
        {
            NetworkObjectInterface? ifaceA = a.Model.NetworkInterfaces.FirstOrDefault(i => i.Connection == null)
                                             ?? a.Model.NetworkInterfaces.FirstOrDefault();
            NetworkObjectInterface? ifaceB = b.Model.NetworkInterfaces.FirstOrDefault(i => i.Connection == null)
                                             ?? b.Model.NetworkInterfaces.FirstOrDefault();
            if (ifaceA == null || ifaceB == null) { return; }

            NetworkConnection connection = new NetworkConnection(
                ifaceA, ifaceB, 866_000_000, NetworkConnectionType.Wireless, "auto-discovered Wi-Fi", "Wi-Fi");
            ifaceA.Connection = connection;
            ifaceB.Connection = connection;
            _networkConnectionViewModels.Add(new NetworkConnectionViewModel(connection, a, b));

            if (DatabaseConnectionStore.Current != null && ifaceA.ID > 0 && ifaceB.ID > 0)
            {
                try { await DatabaseConnectionStore.Current.AddConnection(connection, ifaceA, ifaceB); }
                catch (Exception ex) { RatDialog.Show("Database hiccup", $"The rat couldn't save the Wi-Fi link.\n\n{ex.Message}", "Icon.DatabaseError"); }
            }
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 27): add the "internet" Cloud behind the router (once), cabled to a
        // free router interface (adding one if the router has none spare).
        private async Task EnsureCloudBehindRouterAsync(NetworkObjectViewModel routerVm)
        {
            if (_networkObjects.Any(n => n.Model.Type == NetworkObjectType.Cloud)) { return; }

            NetworkObject cloudModel = new NetworkObject
            {
                Type = NetworkObjectType.Cloud,
                Name = "Internet",
                X = routerVm.X,
                Y = routerVm.Y - 200
            };
            cloudModel.NetworkInterfaces.Add(new NetworkObjectInterface { Name = "wan", IsUp = true });
            if (Session.CurrentUser != null) { cloudModel.ApplyRight(Session.CurrentUser, AccesRights.Owner); }
            await PersistObjectWithInterfacesAsync(cloudModel);

            NetworkObjectViewModel cloudVm = new NetworkObjectViewModel(cloudModel);
            AddNetworkObjectViewModelToCanvas(cloudVm);

            // make sure the router has a spare interface (its WAN uplink)
            NetworkObjectInterface? routerFree = routerVm.Model.NetworkInterfaces.FirstOrDefault(i => i.Connection == null);
            if (routerFree == null)
            {
                routerFree = new NetworkObjectInterface { Name = "wan", IsUp = true };
                routerVm.Model.NetworkInterfaces.Add(routerFree);
                if (DatabaseConnectionStore.Current != null && routerVm.Model.ID > 0)
                {
                    try { await DatabaseConnectionStore.Current.AddInterface(routerFree, routerVm.Model); } catch { }
                }
            }

            await CableAsync(cloudVm, routerVm);
        }
        //KI end

        // cable a device's first interface to a specific switch port
        private async Task CableViaSwitchAsync(NetworkObjectViewModel device, NetworkObjectViewModel switchVm, int switchPort)
        {
            NetworkObjectInterface? deviceIface = device.Model.NetworkInterfaces.FirstOrDefault();
            if (deviceIface == null || switchPort >= switchVm.Model.NetworkInterfaces.Count) { return; }
            NetworkObjectInterface switchIface = switchVm.Model.NetworkInterfaces[switchPort];

            NetworkConnection connection = new NetworkConnection(
                deviceIface, switchIface, 1_000_000_000, NetworkConnectionType.Wired, "auto-discovered", "Kablex");
            deviceIface.Connection = connection;
            switchIface.Connection = connection;
            _networkConnectionViewModels.Add(new NetworkConnectionViewModel(connection, device, switchVm));

            if (DatabaseConnectionStore.Current != null && deviceIface.ID > 0 && switchIface.ID > 0)
            {
                try { await DatabaseConnectionStore.Current.AddConnection(connection, deviceIface, switchIface); }
                catch (Exception ex) { RatDialog.Show("Database hiccup", $"The rat couldn't save a discovered cable.\n\n{ex.Message}", "Icon.DatabaseError"); }
            }
        }

        // copy the host's open ports / OS / subnet mask onto its (first) interface + device so the views can show them
        private static void AnnotateFromHost(NetworkObjectViewModel vm, Discovery.DiscoveredHost host)
        {
            NetworkObjectInterface? iface = vm.Model.NetworkInterfaces.FirstOrDefault();
            if (iface != null)
            {
                if (host.OpenPorts.Count > 0) { iface.OpenPorts = new List<int>(host.OpenPorts); }
                if (!string.IsNullOrEmpty(host.SubnetMask))
                {
                    iface.IP ??= new IP { IPv4 = host.Ip };
                    if (string.IsNullOrEmpty(iface.IP.IPv4SubnetMask)) { iface.IP.IPv4SubnetMask = host.SubnetMask; }
                }
            }
            if (!string.IsNullOrEmpty(host.Os) && string.IsNullOrEmpty(vm.Model.Os)) { vm.Model.Os = host.Os; }
            vm.RefreshPorts(); // KI (prompt 27): refresh the ShowPorts label for a device already on the canvas
        }
        //KI end

        private async Task<NetworkObjectViewModel> EnsureOwnPcOnCanvasAsync()
        {
            string machine = Environment.MachineName;
            NetworkObjectViewModel? pc = _networkObjects.FirstOrDefault(
                n => n.Model.Type == NetworkObjectType.PC &&
                     string.Equals(n.Model.Name, machine, StringComparison.OrdinalIgnoreCase));
            if (pc != null) { return pc; }

            NetworkObject model = new NetworkObject { Type = NetworkObjectType.PC, Name = machine, X = 60, Y = 60 };
            model.PopulateOwnDeviceInterfaces();
            model.ApplyDefaultPcSpecsIfEmpty();
            if (Session.CurrentUser != null) { model.ApplyRight(Session.CurrentUser, AccesRights.Owner); }

            await PersistObjectWithInterfacesAsync(model);

            NetworkObjectViewModel vm = new NetworkObjectViewModel(model);
            AddNetworkObjectViewModelToCanvas(vm);
            return vm;
        }

        //KI (prompt 25/26/27): create a device for a discovered host. Type is forced when the caller knows it (e.g.
        // the router), otherwise guessed from the open ports. Fills name/OS/subnet mask/ports where nmap gave them.
        private async Task<NetworkObjectViewModel> AddDiscoveredDeviceAsync(Discovery.DiscoveredHost host, NetworkObjectType? forcedType)
        {
            string name = string.IsNullOrWhiteSpace(host.Hostname) ? host.Ip : host.Hostname;
            NetworkObjectType type = forcedType ?? DeviceClassifier.ClassifyByPorts(host.OpenPorts);

            NetworkObject model = new NetworkObject
            {
                Type = type,
                Name = name,
                Os = host.Os, // KI (prompt 27): nmap OS guess (may be empty)
                X = 200 + _networkObjects.Count * 30 % 600,
                Y = 200 + _networkObjects.Count * 25 % 300
            };
            model.NetworkInterfaces.Add(new NetworkObjectInterface
            {
                Name = "eth0",
                IsUp = true,
                IP = new IP { IPv4 = host.Ip, IPv4SubnetMask = host.SubnetMask }, // KI (prompt 27): fill the mask
                OpenPorts = new List<int>(host.OpenPorts) // KI (prompt 26): keep the scanned ports for the ShowPorts view
            });
            if (Session.CurrentUser != null) { model.ApplyRight(Session.CurrentUser, AccesRights.Owner); }

            await PersistObjectWithInterfacesAsync(model);

            RAT_WPF.Logging.AppLogger.Info($"Discovery: added {type} '{name}' ({host.Ip})"); // KI (prompt 28)
            NetworkObjectViewModel vm = new NetworkObjectViewModel(model);
            AddNetworkObjectViewModelToCanvas(vm);
            return vm;
        }

        private NetworkObjectViewModel? FindDeviceByIp(string ip) =>
            _networkObjects.FirstOrDefault(n => n.Model.NetworkInterfaces.Any(
                i => i.IP != null && i.IP.IPv4 == ip));

        private bool IsCabledTo(NetworkObjectViewModel a, NetworkObjectViewModel b) =>
            _networkConnectionViewModels.Any(c =>
                (c.Source == a && c.Target == b) || (c.Source == b && c.Target == a));

        private async Task CableAsync(NetworkObjectViewModel a, NetworkObjectViewModel b)
        {
            //KI (prompt 27): prefer a free interface on each side so an already-cabled device (e.g. the router,
            // wired to the switch) gets its NEXT interface used for this cable instead of overwriting the first one.
            NetworkObjectInterface? ifaceA = a.Model.NetworkInterfaces.FirstOrDefault(i => i.Connection == null)
                                             ?? a.Model.NetworkInterfaces.FirstOrDefault();
            NetworkObjectInterface? ifaceB = b.Model.NetworkInterfaces.FirstOrDefault(i => i.Connection == null)
                                             ?? b.Model.NetworkInterfaces.FirstOrDefault();
            if (ifaceA == null || ifaceB == null) { return; } // need an interface on each side

            NetworkConnection connection = new NetworkConnection(
                ifaceA, ifaceB, 1_000_000_000, NetworkConnectionType.Wired, "auto-discovered", "Kablex");
            ifaceA.Connection = connection;
            ifaceB.Connection = connection;
            _networkConnectionViewModels.Add(new NetworkConnectionViewModel(connection, a, b));

            if (DatabaseConnectionStore.Current != null && ifaceA.ID > 0 && ifaceB.ID > 0)
            {
                try { await DatabaseConnectionStore.Current.AddConnection(connection, ifaceA, ifaceB); }
                catch (Exception ex) { RatDialog.Show("Database hiccup", $"The rat couldn't save a discovered cable.\n\n{ex.Message}", "Icon.DatabaseError"); }
            }
        }

        // persist an object + its interfaces (assigns real ids); no-op without a connection
        private async Task PersistObjectWithInterfacesAsync(NetworkObject model)
        {
            if (DatabaseConnectionStore.Current == null) { return; }
            try
            {
                await DatabaseConnectionStore.Current.AddNetworkObject(model);
                foreach (NetworkObjectInterface iface in model.NetworkInterfaces)
                {
                    if (iface.ID > 0) { continue; }
                    await DatabaseConnectionStore.Current.AddInterface(iface, model);
                }
            }
            catch (Exception ex)
            {
                RatDialog.Show("Database hiccup", $"The rat couldn't save a discovered device.\n\n{ex.Message}", "Icon.DatabaseError");
            }
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 15): delete a device for real — check ownership, delete it on the
        // backend, then remove it (and its connections) from the canvas. This is what the Delete tool calls now;
        // previously the tool only removed the node in memory so the deletion was lost on the next load.
        public async void DeleteNetworkObjectFromCanvasAndDatabase(NetworkObjectViewModel node)
        {
            RAT_WPF.Logging.AppLogger.Info($"Delete device '{node.Model.Name}' ({node.Model.Type}, id {node.Model.ID})"); // KI (prompt 28)
            if (!node.Model.CanBeDeletedBy(Session.CurrentUser))
            {
                RAT_WPF.Logging.AppLogger.Warn($"Delete device '{node.Model.Name}' denied (insufficient rights)"); // KI (prompt 28)
                RatDialog.Show("Not allowed", "Only an owner of this device (or a global admin) can delete it.", "Icon.LoginFailed");
                return;
            }

            if (DatabaseConnectionStore.Current != null && node.Model.ID > 0)
            {
                try
                {
                    await DatabaseConnectionStore.Current.DeleteNetworkObject(node.Model);
                }
                catch (Exception ex)
                {
                    RatDialog.Show("Database hiccup", $"The rat couldn't delete the device on the server.\n\n{ex.Message}", "Icon.DatabaseError");
                    return; // keep the node if the server refused
                }
            }

            RemoveNetworkObjectViewModelFromCanvas(node);
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 25): persist an edited cable to the backend (name/speed/type/note).
        public async void PersistEditedConnection(NetworkConnectionViewModel connection)
        {
            if (DatabaseConnectionStore.Current == null || connection.networkConnection.ID <= 0) { return; }
            try
            {
                await DatabaseConnectionStore.Current.EditConnection(connection.networkConnection);
            }
            catch (Exception ex)
            {
                RatDialog.Show("Database hiccup", $"The rat couldn't save the cable on the server.\n\n{ex.Message}", "Icon.DatabaseError");
            }
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 22): delete a cable (NetworkObjectConnection) with the Delete tool —
        // remove it on the backend, clear it off both endpoint interfaces, then drop it from the canvas.
        public async void DeleteConnectionFromCanvasAndDatabase(NetworkConnectionViewModel connection)
        {
            RAT_WPF.Logging.AppLogger.Info($"Delete cable '{connection.networkConnection.Name}' (id {connection.networkConnection.ID})"); // KI (prompt 28)
            if (DatabaseConnectionStore.Current != null && connection.networkConnection.ID > 0)
            {
                try
                {
                    await DatabaseConnectionStore.Current.DeleteConnection(connection.networkConnection);
                }
                catch (Exception ex)
                {
                    RatDialog.Show("Database hiccup", $"The rat couldn't delete the connection on the server.\n\n{ex.Message}", "Icon.DatabaseError");
                    return; // keep the cable if the server refused
                }
            }

            foreach (NetworkObjectInterface interf in connection.networkConnection.networkObectInterfaces)
            {
                interf.Connection = null;
            }
            _networkConnectionViewModels.Remove(connection);
            OnPropertyChanged(nameof(NetworkConnectionViewModels));
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 4): remove a device from the canvas (used by the Delete tool)
        public void RemoveNetworkObjectViewModelFromCanvas(NetworkObjectViewModel networkObject)
        {
            _networkObjects.Remove(networkObject);
            OnPropertyChanged(nameof(NetworkConnectionViewModels));

            // Also Removes any Connections attached to the deleted networkObject

            // Help from AI for removing Connections

            var connectionsToRemove = _networkConnectionViewModels
                .Where(c => c.Source == networkObject || c.Target == networkObject)
                .ToList();

            foreach (var connection in connectionsToRemove)
            {
                foreach(var interf in connection.networkConnection.networkObectInterfaces) 
                {
                    interf.Connection = null;
                }

                _networkConnectionViewModels.Remove(connection);
            }

            // No more Help from AI for removing Connections
        }
        //KI end

        public void AddNetworkObjectConnectionViewModelToCanvas(NetworkObject[] networkObjects,NetworkObjectInterface[] networkObjectInterfaces)
        {
            //TODO: Add NetworkObjectConnectionViewModel To Canvas

            if (networkObjects.Length == 2 && networkObjectInterfaces.Length == 2)
            {
                // TODO: Make speed and type and note and name adjustable
                NetworkConnection networkConnection = new NetworkConnection(networkObjectInterfaces[0], networkObjectInterfaces[1], 1000000000, NetworkConnectionType.Wired, "test", "Kablex");

                networkObjectInterfaces[0].Connection = networkConnection;
                networkObjectInterfaces[1].Connection = networkConnection;

                // Help from AI
                NetworkObjectViewModel? sourceVM = _networkObjects.FirstOrDefault(n => n.networkObjectInterfaces.Contains(networkObjectInterfaces[0]));
                NetworkObjectViewModel? targetVM = _networkObjects.FirstOrDefault(n => n.networkObjectInterfaces.Contains(networkObjectInterfaces[1]));

                if (sourceVM != null && targetVM != null)
                {
                    _networkConnectionViewModels.Add(new NetworkConnectionViewModel(networkConnection, sourceVM, targetVM));
                }
                // No more Help from AI

                //KI start (Claude Opus 4.8, prompt 14): persist the new connection to the backend (needs both
                // interfaces to be saved already, i.e. have a db id). Fire-and-forget with an error popup.
                PersistNewConnection(networkConnection, networkObjectInterfaces[0], networkObjectInterfaces[1]);
                //KI end
            }
        }

        //KI start (Claude Opus 4.8, prompt 14): connection persistence helpers.
        private async void PersistNewConnection(NetworkConnection connection, NetworkObjectInterface a, NetworkObjectInterface b)
        {
            if (DatabaseConnectionStore.Current == null) { return; }
            if (a.ID <= 0 || b.ID <= 0)
            {
                RatDialog.Show("Database hiccup",
                    "Both interfaces must be saved on the server before they can be connected.\n" +
                    "Open each device's settings and add/save the interface first.",
                    "Icon.DatabaseError");
                return;
            }
            try
            {
                await DatabaseConnectionStore.Current.AddConnection(connection, a, b);
            }
            catch (Exception ex)
            {
                RatDialog.Show("Database hiccup", $"The rat couldn't save the connection on the server.\n\n{ex.Message}", "Icon.DatabaseError");
            }
        }
        //KI end
    }
}
