using RAT_WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_WPF.Commands
{
    public class NetworkObjectDeleteCommand(TopologyViewModel topologyViewModel) : CommandBase
    {
        public override void Execute(object? parameter)
        {
            if (parameter is NetworkObjectViewModel networkObjectViewModel)
            {
                topologyViewModel.RemoveNetworkObjectViewModelFromCanvas(networkObjectViewModel);
            }
        }
    }
}
