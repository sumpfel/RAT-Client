using RAT_Logic;
using RAT_WPF.NetworkObject_UI;
using RAT_WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_WPF.Commands
{
    public class NetworkObjectAddConnectionCommand(TopologyViewModel topologyViewModel) : CommandBase
    {
        NetworkObjectViewModel? _previousnetworkObjectViewModel = null;

        NetworkObjectInterface? _previousnetworkObjectInterface = null;

        public override void Execute(object? parameter)
        {
            if (parameter is NetworkObjectViewModel currentNetworkObjectViewModel)
            {
                SelectInterfaceWindow window = new SelectInterfaceWindow(currentNetworkObjectViewModel.networkObject);

                NetworkObjectInterface? currentNetworkObjectInterface = null;

                if (window.ShowDialog() == true)
                {
                    currentNetworkObjectInterface = window.SelectedInterface;
                }

                if (_previousnetworkObjectInterface != null && currentNetworkObjectInterface != null && _previousnetworkObjectInterface.Connection == null && currentNetworkObjectInterface.Connection == null && _previousnetworkObjectInterface != currentNetworkObjectInterface && _previousnetworkObjectViewModel != currentNetworkObjectViewModel && _previousnetworkObjectViewModel != null)
                {
                    topologyViewModel.AddNetworkObjectConnectionViewModelToCanvas(new NetworkObject[2] {_previousnetworkObjectViewModel.networkObject, currentNetworkObjectViewModel.networkObject},new NetworkObjectInterface[2] { _previousnetworkObjectInterface, currentNetworkObjectInterface });

                    _previousnetworkObjectViewModel = null;
                    _previousnetworkObjectInterface = null;
                }
                else
                {
                    _previousnetworkObjectViewModel = currentNetworkObjectViewModel;
                    _previousnetworkObjectInterface = currentNetworkObjectInterface;
                }
            }
        }
    }
}
