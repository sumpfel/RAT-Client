using RAT_Logic;
using RAT_WPF.NetworkObject_UI;
using RAT_WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RAT_WPF.Commands
{
    public class NetworkObjectOpenSettingsCommand(NetworkObjectViewModel networkObjectVM) : CommandBase
    {
        public override void Execute(object? parameter)
        {
            NetworkObjectSettingsWindow window = new NetworkObjectSettingsWindow(networkObjectVM.networkObject);
            window.Show();

            window.Closed += Window_Closed; // Bad solution better to just give VM instead
            // TODO: Make window use VM instead
            window.NetworkObjectViewNeedsUpdate += Window_NetworkObjectViewNeedsUpdate;
        }

        private void Window_NetworkObjectViewNeedsUpdate(object? sender, EventArgs e)
        {
            networkObjectVM.RefreshUI();
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            networkObjectVM.RefreshUI();
        }
    }
}
