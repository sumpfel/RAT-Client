using RAT_Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_WPF.ViewModels
{
    public class NetworkConnectionViewModel : ViewModelBase
    {
        private NetworkObjectViewModel _networkObjectViewModelSource;

        public NetworkObjectViewModel Source => _networkObjectViewModelSource;

        private NetworkObjectViewModel _networkObjectViewModelTarget;

        public NetworkObjectViewModel Target => _networkObjectViewModelTarget;

        private NetworkConnection _networkConnection;

        public NetworkConnection networkConnection => _networkConnection;

        public int xSource 
        {
            get => _networkObjectViewModelSource.X;
        }

        public int ySource 
        {
            get => _networkObjectViewModelSource.Y;
        }

        public int xTarget
        {
            get => _networkObjectViewModelTarget.X;
        }

        public int yTarget
        {
            get => _networkObjectViewModelTarget.Y;
        }

        public string interfaceNameSource 
        {
            get
            {
                // Sourcename is name of interface in networkConnection.networkObectInterfaces, which belongs to networkObjectViewModelSource

                foreach (NetworkObjectInterface networkObjectInterface in _networkConnection.networkObectInterfaces) 
                {
                    if (_networkObjectViewModelSource.networkObjectInterfaces.Contains(networkObjectInterface))
                    {
                        return networkObjectInterface.Name;
                    }
                }

                return "Something didn't work here";
            }
        }

        public string interfaceNameTarget
        {
            get
            {
                // Targetname is name of interface in networkConnection.networkObectInterfaces, which belongs to networkObjectViewModelTarget

                foreach (NetworkObjectInterface networkObjectInterface in _networkConnection.networkObectInterfaces)
                {
                    if (_networkObjectViewModelTarget.networkObjectInterfaces.Contains(networkObjectInterface))
                    {
                        return networkObjectInterface.Name;
                    }
                }

                return "Something didn't work here";
            }
        }


        // Making a viewmodel where currently only networkConnection and networkObjectViewModelSource are known, networkObjectViewModelTarget filled in later
        public NetworkConnectionViewModel(NetworkConnection networkConnection, NetworkObjectViewModel networkObjectViewModelSource)
        {
            _networkConnection = networkConnection;
            _networkObjectViewModelSource = networkObjectViewModelSource;
        }

        // Making a completed viewmodel where incomplete viewmodel networkObjectViewModelTarget are known
        public NetworkConnectionViewModel(NetworkConnectionViewModel incompleteNetworkConnectionViewModel, NetworkObjectViewModel networkObjectViewModelTarget) 
            : this(incompleteNetworkConnectionViewModel._networkConnection, incompleteNetworkConnectionViewModel._networkObjectViewModelSource, networkObjectViewModelTarget)
        {
        }

        // Making a complete viewmodel from the start
        public NetworkConnectionViewModel(NetworkConnection networkConnection, NetworkObjectViewModel networkObjectViewModelSource, NetworkObjectViewModel networkObjectViewModelTarget)
        {
            _networkObjectViewModelSource = networkObjectViewModelSource;
            _networkObjectViewModelTarget = networkObjectViewModelTarget;

            _networkConnection = networkConnection;

            _networkObjectViewModelSource.PropertyChanged += NetworkObjectViewModelSource_PropertyChanged;
            _networkObjectViewModelTarget.PropertyChanged += _networkObjectViewModelTarget_PropertyChanged;
        }

        private void _networkObjectViewModelTarget_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NetworkObjectViewModel.X) || e.PropertyName == nameof(NetworkObjectViewModel.Y))
            {
                OnPropertyChanged(nameof(xTarget));
                OnPropertyChanged(nameof(yTarget));
            }
        }

        private void NetworkObjectViewModelSource_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NetworkObjectViewModel.X) || e.PropertyName == nameof(NetworkObjectViewModel.Y))
            {
                OnPropertyChanged(nameof(xSource));
                OnPropertyChanged(nameof(ySource));
            }
        }
    }
}
