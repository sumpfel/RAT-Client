using RAT_WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_WPF.Commands
{
    //KI start (Claude Opus 4.8, prompt 15): log the current user out (clear session + connection, back to login).
    public class LogoutCommand(TopologyViewModel topologyViewModel) : CommandBase
    {
        public override void Execute(object? parameter)
        {
            topologyViewModel.Logout();
        }
    }
    //KI end
}
