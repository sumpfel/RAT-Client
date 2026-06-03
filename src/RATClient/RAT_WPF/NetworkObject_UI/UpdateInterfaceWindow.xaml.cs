using RAT_Logic;
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
    /// Interaktionslogik für UpdateInterfaceWindow.xaml
    /// </summary>
    public partial class UpdateInterfaceWindow : Window
    {
        public NetworkObjectInterface networkObjectInterface = new NetworkObjectInterface();
        public UpdateInterfaceWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                networkObjectInterface.Name = InterfaceNameTextBox.Text;
                networkObjectInterface.IP.IPv4 = Ipv4TextBox.Text;
                networkObjectInterface.IP.IPv4SubnetMask = Ipv4MaskTextBox.Text;
                networkObjectInterface.IP.IPv4Gateway = Ipv4GatewayTextBox.Text;
                networkObjectInterface.IP.IPv6 = Ipv6TextBox.Text;
                networkObjectInterface.IP.IPv6PrefixLength = Convert.ToInt32(Ipv6PrefixTextBox.Text);
                DialogResult = true;
                this.Close();
            }
            catch
            {
                MessageBox.Show("something went wron check input");
            }
        }
    }
}
