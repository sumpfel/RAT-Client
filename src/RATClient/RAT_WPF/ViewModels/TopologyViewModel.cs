using RAT_Logic;
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
                NetworkObjectGraph graph = await DatabaseConnectionStore.Current.GetNetworkGraph();
                if (graph?.networkObjects == null) { return; }

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
                MessageBox.Show($"Could not load the topology from the server: {ex.Message}",
                    "Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        //KI end

        public void AddNetworkObjectViewModelToCanvas(NetworkObjectViewModel networkObject)
        {
            _networkObjects.Add(networkObject);
            OnPropertyChanged(nameof(NetworkConnectionViewModels));
        }

        //KI start (Claude Opus 4.8, prompt 15): delete a device for real — check ownership, delete it on the
        // backend, then remove it (and its connections) from the canvas. This is what the Delete tool calls now;
        // previously the tool only removed the node in memory so the deletion was lost on the next load.
        public async void DeleteNetworkObjectFromCanvasAndDatabase(NetworkObjectViewModel node)
        {
            if (!node.Model.CanBeDeletedBy(Session.CurrentUser))
            {
                MessageBox.Show("Only an owner of this device (or a global admin) can delete it.",
                    "Not allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    MessageBox.Show($"Could not delete the device on the server: {ex.Message}",
                        "Database", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // keep the node if the server refused
                }
            }

            RemoveNetworkObjectViewModelFromCanvas(node);
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
                MessageBox.Show(
                    "Both interfaces must be saved on the server before they can be connected.\n" +
                    "Open each device's settings and add/save the interface first.",
                    "Database", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                await DatabaseConnectionStore.Current.AddConnection(connection, a, b);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save the connection to the server: {ex.Message}",
                    "Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        //KI end
    }
}
