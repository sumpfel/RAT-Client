using RAT_Logic;
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

                foreach (Dictionary<string,string> interface_ in RAT_Logic.NetworkObject.GetOwnDeviceInterfaces())
                {
                    InterfacesStackPanel.Children.Add(new Label() { Content = $"{interface_["name"]} [{interface_["status"]}]"});
                }
            }
            else
            {
                name.Content = networkObject.Name;
                foreach (NetworkObjectInterface networkObjectInterface in networkObject.NetworkInterfaces)
                {
                    InterfacesStackPanel.Children.Add(new Label() { Content = $"{networkObjectInterface.Name} [???]" });
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateLoginWindow updateLoginWindow = new UpdateLoginWindow(null);
            if (updateLoginWindow.ShowDialog() == true)
            {
                networkObject.Settings.AddLogin(updateLoginWindow.login);
            }
            LoginsStackPanel.Children.Add(new LoginControl(updateLoginWindow.login, networkObject));//TODO: use something smarter than stack pannel: listview
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                networkObject.DownloadSFTP(SftpLocalPath.Text, SftpRemotePath.Text);
            }catch
            {
                MessageBox.Show("something went wrong? have you already opened a connection?");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                networkObject.UploadSFTP(SftpLocalPath.Text, SftpRemotePath.Text);
            }
            catch
            {
                MessageBox.Show("something went wrong? have you already opened a connection?");
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                SftpDirList.Text = "";
                List<string> list = networkObject.ListDirSFTP(SftpRemotePath.Text);
                foreach (string s in list)
                {
                    SftpDirList.Text += s + ", ";
                }
            }
            catch
            {
                MessageBox.Show("something went wrong? have you already opened a connection?");
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                networkObject.DownloadSCP(ScpLocalPath.Text, ScpRemotePath.Text);
            }
            catch
            {
                MessageBox.Show("something went wrong? have you already opened a connection?");
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            try
            {
                networkObject.UploadSCP(ScpLocalPath.Text, ScpRemotePath.Text);
            }
            catch
            {
                MessageBox.Show("something went wrong? have you already opened a connection?");
            }
        }
    }
}
