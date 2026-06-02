using RAT_WPF.NetworkObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RAT_WPF
{
    /// <summary>
    /// Interaktionslogik für NetworkObjectSettingsWindow.xaml
    /// </summary>
    public partial class NetworkObjectSettingsWindow : Window
    {
        RAT_Logic.NetworkObject networkObject;
        public NetworkObjectSettingsWindow(RAT_Logic.NetworkObject networkObject)
        {
            InitializeComponent();
            if (networkObject.Type == RAT_Logic.NetworkObjectType.PC)
            {
                Dictionary<string, string> stats = RAT_Logic.NetworkObject.GetOwnDeviceInfos();
                name.Content = $"name: {stats["name"]} (Your PC)";
                os.Content = $"os: {stats["os"]}";
                ram.Content = $"ram: {stats["ram"]}";
                cpu.Content = $"cpu: {stats["cpu"]}";
                gpu.Content = $"gpu: {stats["gpu"]}";
            }
            else
            {
                name.Content = networkObject.Name;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateLoginWindow updateLoginWindow = new UpdateLoginWindow(null);
            if (updateLoginWindow.ShowDialog() == true)
            {
                networkObject.Settings.AddLogin(updateLoginWindow.login);
            }
        }
    }
}
