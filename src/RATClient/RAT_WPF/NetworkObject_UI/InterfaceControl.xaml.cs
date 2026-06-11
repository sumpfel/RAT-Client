using RAT_Logic;
using RAT_WPF.Themes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RAT_WPF.NetworkObject_UI
{
    //KI start (Claude Opus 4.8, prompt 2): displays one real host interface with icon, status and an (i) details popup
    public partial class InterfaceControl : UserControl
    {
        private readonly HostInterfaceInfo _info;

        public InterfaceControl(HostInterfaceInfo info)
        {
            InitializeComponent();
            _info = info;

            NameText.Text = info.Name;
            SubText.Text = info.Description;
            StatusText.Text = info.StatusText;

            string iconKey = info.Kind switch
            {
                HostInterfaceKind.Wifi => IconProvider.Wifi,
                HostInterfaceKind.UsbEthernet => IconProvider.Usb,
                _ => IconProvider.Ethernet
            };
            TypeIcon.Source = IconProvider.Get(iconKey);

            StatusDot.Fill = info.IsUp
                ? (Brush)Application.Current.Resources["Brush.Success"]
                : (Brush)Application.Current.Resources["Brush.Danger"];
        }

        private void Info_Click(object sender, RoutedEventArgs e)
        {
            string details =
                $"Name: {_info.Name}\n" +
                $"Description: {_info.Description}\n" +
                $"Type: {_info.Kind}\n" +
                $"Status: {_info.StatusText}\n" +
                $"MAC: {_info.Mac}\n" +
                $"Speed: {_info.SpeedText}\n" +
                $"IPv4: {(_info.IPv4.Count > 0 ? string.Join(", ", _info.IPv4) : "—")}\n" +
                $"IPv6: {(_info.IPv6.Count > 0 ? string.Join(", ", _info.IPv6) : "—")}";

            MessageBox.Show(details, $"Interface — {_info.Name}",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    //KI end
}
