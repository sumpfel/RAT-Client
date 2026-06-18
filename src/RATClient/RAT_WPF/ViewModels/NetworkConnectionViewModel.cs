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

        //KI start (Claude Opus 4.8, prompt 22): cable end-labels (interface name + IP) shown when ShowInterfaces is on.
        private NetworkObjectInterface? InterfaceFor(NetworkObjectViewModel device) =>
            _networkConnection.networkObectInterfaces.FirstOrDefault(i => device.networkObjectInterfaces.Contains(i));

        private static string Label(NetworkObjectInterface? iface)
        {
            if (iface == null) { return ""; }
            string? ip = iface.IP?.IPv4;
            return string.IsNullOrWhiteSpace(ip) ? iface.Name : $"{iface.Name}  {ip}";
        }

        /// <summary>"name  ipv4" of the source endpoint's interface (ip omitted if unset).</summary>
        public string SourceLabel => Label(InterfaceFor(_networkObjectViewModelSource));

        /// <summary>"name  ipv4" of the target endpoint's interface.</summary>
        public string TargetLabel => Label(InterfaceFor(_networkObjectViewModelTarget));

        // a label anchored a bit in from each device toward the other end (so it sits on the cable near the device)
        public double SourceLabelX => xSource + (xTarget - xSource) * 0.25 + 20;
        public double SourceLabelY => ySource + (yTarget - ySource) * 0.25 + 10;
        public double TargetLabelX => xTarget + (xSource - xTarget) * 0.25 + 20;
        public double TargetLabelY => yTarget + (ySource - yTarget) * 0.25 + 10;

        /// <summary>Whether the cable labels are visible (follows the app-wide DisplaySettings toggle).</summary>
        public bool ShowInterfaceLabels => RAT_WPF.Themes.DisplaySettings.ShowInterfaces;
        //KI end

        //KI start (Claude Opus 4.8, prompt 28): wireless links are drawn dashed. StrokeDashArray binds straight onto
        // the cable Line; null = solid (wired). (4,3) = short dashes for a Wi-Fi "cable".
        public bool IsWireless => _networkConnection.Type == NetworkConnectionType.Wireless;

        public System.Windows.Media.DoubleCollection? StrokeDashArray =>
            IsWireless ? new System.Windows.Media.DoubleCollection { 4, 3 } : null;
        //KI end


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

            //KI start (Claude Opus 4.8, prompt 22): refresh the cable labels when the global toggle flips
            RAT_WPF.Themes.DisplaySettings.ShowInterfacesChanged += OnShowInterfacesChanged;
            //KI end
        }

        //KI start (Claude Opus 4.8, prompt 22): keep the label positions in sync with the endpoints
        private void RaiseLabelPositions()
        {
            OnPropertyChanged(nameof(SourceLabelX));
            OnPropertyChanged(nameof(SourceLabelY));
            OnPropertyChanged(nameof(TargetLabelX));
            OnPropertyChanged(nameof(TargetLabelY));
        }

        private void OnShowInterfacesChanged(bool _) => OnPropertyChanged(nameof(ShowInterfaceLabels));
        //KI end

        private void _networkObjectViewModelTarget_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NetworkObjectViewModel.X) || e.PropertyName == nameof(NetworkObjectViewModel.Y))
            {
                OnPropertyChanged(nameof(xTarget));
                OnPropertyChanged(nameof(yTarget));
                RaiseLabelPositions(); // KI (prompt 22)
            }
        }

        private void NetworkObjectViewModelSource_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NetworkObjectViewModel.X) || e.PropertyName == nameof(NetworkObjectViewModel.Y))
            {
                OnPropertyChanged(nameof(xSource));
                OnPropertyChanged(nameof(ySource));
                RaiseLabelPositions(); // KI (prompt 22)
            }
        }
    }
}
