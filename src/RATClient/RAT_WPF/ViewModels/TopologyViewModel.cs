using RAT_Logic;
using RAT_WPF.Commands;
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

        public TopologyViewModel()
        {
            defaultItems = new NetworkObjectListingViewModel();

            defaultItems.AddNetworkObject(new NetworkObject() { Type = NetworkObjectType.Router , Name = "New Router", Settings = new NetworkObjectSettings()});
            defaultItems.AddNetworkObject(new NetworkObject() { Type = NetworkObjectType.Switch , Name = "New Switch", Settings = new NetworkObjectSettings()});
            defaultItems.AddNetworkObject(new NetworkObject() { Type = NetworkObjectType.Server , Name = "New Server", Settings = new NetworkObjectSettings()});
            defaultItems.AddNetworkObject(new NetworkObject() { Type = NetworkObjectType.Client , Name = "New Client", Settings = new NetworkObjectSettings()});
            //KI start (Claude Opus 4.8, prompt 4/12): default PC name = this machine's name (still changeable);
            // and the PC carries this machine's real interfaces so they can be selected in the SelectInterfaceWindow.
            NetworkObject ownPc = new NetworkObject() { Type = NetworkObjectType.PC, Name = Environment.MachineName, Settings = new NetworkObjectSettings() };
            ownPc.PopulateOwnDeviceInterfaces();
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
        }

        public void AddNetworkObjectViewModelToCanvas(NetworkObjectViewModel networkObject)
        {
            _networkObjects.Add(networkObject);
            OnPropertyChanged(nameof(NetworkConnectionViewModels));
        }

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
            }
        }
    }
}
