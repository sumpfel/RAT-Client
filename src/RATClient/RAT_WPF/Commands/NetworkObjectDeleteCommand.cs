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
                //KI start (Claude Opus 4.8, prompt 15): delete through the DB-persisting + owner-checked path,
                // not just the in-memory canvas removal (which was lost on the next load).
                topologyViewModel.DeleteNetworkObjectFromCanvasAndDatabase(networkObjectViewModel);
                //KI end
            }
            //KI start (Claude Opus 4.8, prompt 22): the Delete tool can also delete a cable (connection)
            else if (parameter is NetworkConnectionViewModel networkConnectionViewModel)
            {
                topologyViewModel.DeleteConnectionFromCanvasAndDatabase(networkConnectionViewModel);
            }
            //KI end
        }
    }
}
