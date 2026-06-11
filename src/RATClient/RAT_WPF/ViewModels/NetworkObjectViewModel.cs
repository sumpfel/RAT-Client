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

        //KI start (Claude Opus 4.8, prompt 11): expose the model so create/delete can check access rights
        public NetworkObject Model => _networkObject;
        //KI end

        public String Type => _networkObject.Type.ToString();

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

        public NetworkObjectOpenSettings NetworkObjectOpenSettings { get; set; }


		public NetworkObjectViewModel(RAT_Logic.NetworkObject networkObject) 
		{
            _networkObject = networkObject;

            NetworkObjectOpenSettings = new NetworkObjectOpenSettings(_networkObject);
        }
	}
}
