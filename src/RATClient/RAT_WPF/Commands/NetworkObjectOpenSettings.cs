using RAT_Logic;
using RAT_WPF.NetworkObject_UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RAT_WPF.Commands
{
    public class NetworkObjectOpenSettings(NetworkObject networkObject) : CommandBase
    {
        public override void Execute(object? parameter)
        {
            NetworkObjectSettingsWindow window = new NetworkObjectSettingsWindow(networkObject);
            window.Show();
        }
    }
}
