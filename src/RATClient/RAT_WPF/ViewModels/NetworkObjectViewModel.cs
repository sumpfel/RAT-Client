using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RAT_Logic;
using RAT_WPF.Commands;

namespace RAT_WPF.ViewModels
{
    public class NetworkObjectViewModel : ViewModelBase
    {
        private readonly NetworkObject _networkObject;

        public NetworkObject networkObject => _networkObject;

        //KI start (Claude Opus 4.8, prompt 11): expose the model so create/delete can check access rights
        public NetworkObject Model => _networkObject;
        //KI end

        public String Type => _networkObject.Type.ToString();

        public List<NetworkObjectInterface> networkObjectInterfaces => _networkObject.NetworkInterfaces;

		public string Name
		{
			get 
			{ 
				return _networkObject.Name; 
			}
			set 
			{ 
				_networkObject.Name = value;
				OnPropertyChanged(nameof(Name));
			}
		}

		public int X
		{
			get
            {
                return _networkObject.X;
            }
			set
            {
				if (value != _networkObject.X)
				{
                    _networkObject.X = value;
                    OnPropertyChanged(nameof(X));
                }

            }
		}

        public int Y
        {
            get
            {
                return _networkObject.Y;
            }
            set
            {
                if (value != _networkObject.Y)
                {
                    _networkObject.Y = value;
                    OnPropertyChanged(nameof(Y));
                }
            }
        }

        public NetworkObjectOpenSettingsCommand NetworkObjectOpenSettings { get; set; }

        //KI start (Claude Opus 4.8, prompt 26): open-ports label (friendly names, e.g. "22 - SSH"), shown on the
        // node when the global ShowPorts toggle is on. Aggregates the open ports across this device's interfaces.
        public bool ShowPorts => RAT_WPF.Themes.DisplaySettings.ShowPorts;

        public string PortsText
        {
            get
            {
                var ports = _networkObject.NetworkInterfaces
                    .SelectMany(i => i.OpenPorts)
                    .Distinct()
                    .OrderBy(p => p)
                    .Select(RAT_Logic.PortNames.Describe);
                return string.Join("\n", ports);
            }
        }
        //KI end


		public NetworkObjectViewModel(RAT_Logic.NetworkObject networkObject)
		{
            _networkObject = networkObject;

            NetworkObjectOpenSettings = new NetworkObjectOpenSettingsCommand(this);

            //KI start (Claude Opus 4.8, prompt 26): refresh the ports view when the global toggle flips
            RAT_WPF.Themes.DisplaySettings.ShowPortsChanged += _ =>
            {
                OnPropertyChanged(nameof(ShowPorts));
                OnPropertyChanged(nameof(PortsText));
            };
            //KI end
        }

        public void RefreshUI()
        {
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
        }
	}
}
