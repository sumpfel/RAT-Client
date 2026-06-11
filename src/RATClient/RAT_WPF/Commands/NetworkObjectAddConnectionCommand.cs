using RAT_Logic;
using RAT_WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_WPF.Commands
{
    public class NetworkObjectAddConnectionCommand : CommandBase
    {
        NetworkObjectViewModel? _previousnetworkObjectViewModel = null;

        public override void Execute(object? parameter)
        {
            // TODO: Select Interface which should be connected

            if (parameter is NetworkObjectViewModel currentNetworkObjectViewModel)
            {
                if (_previousnetworkObjectViewModel != null && _previousnetworkObjectViewModel != currentNetworkObjectViewModel)
                {

                }
                else
                {
                    _previousnetworkObjectViewModel = currentNetworkObjectViewModel;
                }
            }
        }
    }
}
