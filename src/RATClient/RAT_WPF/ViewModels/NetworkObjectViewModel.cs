using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RAT_Logic;
using RAT_WPF.Commands;

namespace RAT_WPF.ViewModels
{
    public class NetworkObjectViewModel : ViewModelBase
    {
        private readonly NetworkObject _networkObject;

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


		private int _x;

		public int X
		{
			get { return _x; }
			set { _x = value; }
		}

		private int _y;

		public int Y
		{
			get { return _y; }
			set { _y = value; }
		}

		public NetworkObjectOpenSettings NetworkObjectOpenSettings { get; set; }


		public NetworkObjectViewModel(RAT_Logic.NetworkObject networkObject) 
		{
            _networkObject = networkObject;

            NetworkObjectOpenSettings = new NetworkObjectOpenSettings(_networkObject);
        }
	}
}
