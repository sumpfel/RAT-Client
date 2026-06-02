using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RAT_Logic;
using RAT_WPF.Commands;

namespace RAT_WPF.ViewModels
{
    public class TopologyViewModel : ViewModelBase
    {
        public NetworkObjectListingViewModel defaultItems { get; }

        public TopologyViewModel()
        {
            defaultItems = new NetworkObjectListingViewModel();

            defaultItems.AddNetworkObject(new NetworkObject() { Type = NetworkObjectType.Router , Name = "New Router"});
            defaultItems.AddNetworkObject(new NetworkObject() { Type = NetworkObjectType.Switch , Name = "New Switch"});
            defaultItems.AddNetworkObject(new NetworkObject() { Type = NetworkObjectType.Server , Name = "New Server"});
            defaultItems.AddNetworkObject(new NetworkObject() { Type = NetworkObjectType.Client , Name = "New Client"});

        }
    }
}
