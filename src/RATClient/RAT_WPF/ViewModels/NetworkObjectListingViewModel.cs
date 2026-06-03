using RAT_Logic;
using RAT_WPF.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RAT_WPF.ViewModels
{
    public class NetworkObjectListingViewModel
    {
        private readonly ObservableCollection<NetworkObjectViewModel> _networkObjects;

        public IEnumerable<NetworkObjectViewModel> NetworkObjects => _networkObjects;

        public NetworkObjectListingViewModel()
        {
            _networkObjects = new ObservableCollection<NetworkObjectViewModel>();
        }

        public void AddNetworkObject(RAT_Logic.NetworkObject networkObject)
        {
            _networkObjects.Add(new NetworkObjectViewModel(networkObject));
        }
    }
}
