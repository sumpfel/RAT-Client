using RAT_WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_WPF.Commands
{
    public class NetworkObjectAddedCommand(List<NetworkObjectViewModel> _networkObjectViewModels) : CommandBase
    {

        public override void Execute(object? parameter)
        {
        }
    }
}
