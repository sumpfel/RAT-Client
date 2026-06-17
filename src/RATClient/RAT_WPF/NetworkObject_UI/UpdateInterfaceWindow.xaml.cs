using RAT_Logic;
using System;
using System.Windows;

namespace RAT_WPF.NetworkObject_UI
{
    /// <summary>
    /// Interaktionslogik für UpdateInterfaceWindow.xaml
    /// </summary>
    public partial class UpdateInterfaceWindow : Window
    {
        public NetworkObjectInterface networkObjectInterface;

        //KI start (Claude Opus 4.8, prompt 7): only the name is required; supports editing an existing interface
        public UpdateInterfaceWindow(NetworkObjectInterface? existing = null)
        {
            InitializeComponent();

            networkObjectInterface = existing ?? new NetworkObjectInterface();

            if (existing != null)
            {
                Title = "Edit interface";
                InterfaceNameTextBox.Text = existing.Name;
                IP? ip = existing.IP;
                Ipv4TextBox.Text = ip?.IPv4 ?? "";
                Ipv4MaskTextBox.Text = ip?.IPv4SubnetMask ?? "";
                Ipv4GatewayTextBox.Text = ip?.IPv4Gateway ?? "";
                Ipv6TextBox.Text = ip?.IPv6 ?? "";
                Ipv6PrefixTextBox.Text = (ip != null && ip.IPv6PrefixLength != 0) ? ip.IPv6PrefixLength.ToString() : "";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string name = InterfaceNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                RatDialog.Show("Name required", "Please enter an interface name.", "Icon.Ethernet");
                return;
            }

            networkObjectInterface.Name = name;

            // IP is fully optional — only build an IP object when at least one field is filled in
            bool hasAnyIp =
                !string.IsNullOrWhiteSpace(Ipv4TextBox.Text) ||
                !string.IsNullOrWhiteSpace(Ipv4MaskTextBox.Text) ||
                !string.IsNullOrWhiteSpace(Ipv4GatewayTextBox.Text) ||
                !string.IsNullOrWhiteSpace(Ipv6TextBox.Text) ||
                !string.IsNullOrWhiteSpace(Ipv6PrefixTextBox.Text);

            if (hasAnyIp)
            {
                IP ip = networkObjectInterface.IP ?? new IP();
                ip.IPv4 = Ipv4TextBox.Text.Trim();
                ip.IPv4SubnetMask = Ipv4MaskTextBox.Text.Trim();
                ip.IPv4Gateway = Ipv4GatewayTextBox.Text.Trim();
                ip.IPv6 = Ipv6TextBox.Text.Trim();
                int.TryParse(Ipv6PrefixTextBox.Text.Trim(), out int prefix);
                ip.IPv6PrefixLength = prefix;
                networkObjectInterface.IP = ip;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        //KI end
    }
}
