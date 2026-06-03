using RAT_Logic;
using RAT_WPF.NetworkObject_UI;
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

namespace RAT_WPF.NetworkObject_UI
{
    /// <summary>
    /// Interaktionslogik für NetworkObjectSettingsWindow.xaml
    /// </summary>
    public partial class NetworkObjectSettingsWindow : Window
    {
        NetworkObject networkObject;
        public NetworkObjectSettingsWindow(NetworkObject networkObject)
        {
            InitializeComponent();
            if (networkObject.Type == NetworkObjectType.PC)
            {
                Dictionary<string, string> stats = NetworkObject.GetOwnDeviceInfos();
                name.Content = $"name: {stats["name"]} (Your PC)";
                os.Content = $"os: {stats["os"]}";
                ram.Content = $"ram: {stats["ram"]}";
                cpu.Content = $"cpu: {stats["cpu"]}";
                gpu.Content = $"gpu: {stats["gpu"]}";

                foreach (Dictionary<string,string> interface_ in NetworkObject.GetOwnDeviceInterfaces())
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

        private void TabItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TabItem tab = new TabItem() { Header = "ssh" };
            SshShellsTabControl.Items.Add(tab);
        }

        private async void Button_Click_6(object sender, RoutedEventArgs e)
        {
            try
            {
                string result = await networkObject.ExecuteSSH(sshInputBox.Text);
            }
            catch
            {
                MessageBox.Show("something went wrong? have you already opened a connection?");
            }
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            UpdateInterfaceWindow updateLoginWindow = new UpdateInterfaceWindow();
            if (updateLoginWindow.ShowDialog() == true)
            {
                networkObject.NetworkInterfaces.Add(updateLoginWindow.networkObjectInterface);
            }
            InterfacesStackPanel.Children.Clear();
            foreach (NetworkObjectInterface networkObjectInterface in networkObject.NetworkInterfaces)
            {
                InterfacesStackPanel.Children.Add(new Label() { Content = $"{networkObjectInterface.Name} [???]" });
            }
        }
    }
}
