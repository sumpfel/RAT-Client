using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RAT_Logic;

namespace RAT_WPF.ViewModels
{
    public class NetworkObjectViewModel : ViewModelBase
    {
        private readonly RAT_Logic.NetworkObject _networkObject;

        public String Type => _networkObject.Type.ToString();

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

		private bool _displayed = false;

		public bool Displayed
		{
			get { return _displayed; }
			set { _displayed = value; }
		}


		public NetworkObjectViewModel(RAT_Logic.NetworkObject networkObject) 
		{
            _networkObject = networkObject;
        }
	}
}
