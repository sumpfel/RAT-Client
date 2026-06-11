using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RAT_Logic;
using RAT_WPF.Commands;
using RAT_WPF.Views;

namespace RAT_WPF.ViewModels
{
    public enum EnumTool
    {
        Cursor,
        Connector
    };

    public class TopologyViewModel : ViewModelBase
    {
        public NetworkObjectListingViewModel defaultItems { get; }

        public ICommand NetworkObjectAddedCommand { get; }

        public ICommand NetworkObjectAddConnectionCommand { get; }

        private readonly ObservableCollection<NetworkObjectViewModel> _networkObjects;

        public IEnumerable<NetworkObjectViewModel> NetworkObjects => _networkObjects;

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
            defaultItems.AddNetworkObject(new NetworkObject() { Type = NetworkObjectType.PC, Name = "New PC", Settings = new NetworkObjectSettings() });

            // NetworkObjectAddedCommand = new NetworkObjectAddedCommand(defaultItems.NetworkObjects);

            this.NetworkObjectAddConnectionCommand = new NetworkObjectAddConnectionCommand();

            _networkObjects = new ObservableCollection<NetworkObjectViewModel>()
            {
                // For testing
                
                new NetworkObjectViewModel(new NetworkObject() { Type = NetworkObjectType.Router , Name = "Test", Settings = new NetworkObjectSettings(), X = 50, Y=100}),
                new NetworkObjectViewModel(new NetworkObject() { Type = NetworkObjectType.Router , Name = "Test2", Settings = new NetworkObjectSettings(), X = 350, Y=250})  
            };
        }

        public void AddNetworkObjectViewModelToCanvas(NetworkObjectViewModel networkObject)
        {
            _networkObjects.Add(networkObject);
        }
    }
}
