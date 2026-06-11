using RAT_Logic;
using RAT_WPF.Themes;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RAT_WPF.NetworkObject_UI
{
    //KI start (Claude Opus 4.8, prompt 2/7): interface row, two modes:
    //  - host PC interface (HostInterfaceInfo): read-only, just the (i) details popup
    //  - modelled device interface (NetworkObjectInterface): editable — edit / delete / up-down toggle
    public partial class InterfaceControl : UserControl
    {
        private readonly HostInterfaceInfo? _hostInfo;
        private readonly NetworkObjectInterface? _modelInterface;

        /// <summary>Raised (software mode) when the user clicks Edit; arg is this control.</summary>
        public event Action<InterfaceControl>? EditRequested;
        /// <summary>Raised (software mode) when the user clicks Delete; arg is this control.</summary>
        public event Action<InterfaceControl>? DeleteRequested;

        public NetworkObjectInterface? ModelInterface => _modelInterface;

        /// <summary>Read-only host interface.</summary>
        public InterfaceControl(HostInterfaceInfo info)
        {
            InitializeComponent();
            _hostInfo = info;

            string iconKey = info.Kind switch
            {
                HostInterfaceKind.Wifi => IconProvider.Wifi,
                HostInterfaceKind.UsbEthernet => IconProvider.Usb,
                _ => IconProvider.Ethernet
            };
            TypeIcon.Source = IconProvider.Get(iconKey);

            NameText.Text = info.Name;
            SubText.Text = string.IsNullOrWhiteSpace(info.Description) ? info.Kind.ToString() : info.Description;
            ApplyStatus(info.IsUp, info.StatusText);
            // host interfaces can't be edited / removed / toggled from here
        }

        /// <summary>
        /// Modelled device interface. When <paramref name="readOnly"/> is false the row shows
        /// Toggle/Edit/Delete actions; when true it only shows the (i) details button (e.g. for selection lists).
        /// </summary>
        public InterfaceControl(NetworkObjectInterface networkObjectInterface, bool readOnly = false)
        {
            InitializeComponent();
            _modelInterface = networkObjectInterface;

            TypeIcon.Source = IconProvider.Get(IconProvider.Ethernet);

            if (!readOnly)
            {
                ToggleButton.Visibility = Visibility.Visible;
                EditButton.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;
            }

            Refresh();
        }

        /// <summary>Re-reads the modelled interface into the UI (after an edit or toggle).</summary>
        public void Refresh()
        {
            if (_modelInterface == null) { return; }
            NameText.Text = string.IsNullOrWhiteSpace(_modelInterface.Name) ? "(unnamed)" : _modelInterface.Name;
            SubText.Text = _modelInterface.IP?.IPv4 is string ip && !string.IsNullOrWhiteSpace(ip)
                ? ip
                : "no IP set";
            ApplyStatus(_modelInterface.IsUp, _modelInterface.IsUp ? "UP" : "DOWN");
        }

        private void ApplyStatus(bool isUp, string text)
        {
            Brush ok = (Brush)Application.Current.Resources["Brush.Success"];
            Brush bad = (Brush)Application.Current.Resources["Brush.Danger"];
            StatusDot.Fill = isUp ? ok : bad;
            StatusText.Text = text;
        }

        private void Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (_modelInterface == null) { return; }
            _modelInterface.IsUp = !_modelInterface.IsUp;
            Refresh();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            EditRequested?.Invoke(this);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            DeleteRequested?.Invoke(this);
        }

        private void Info_Click(object sender, RoutedEventArgs e)
        {
            string details;
            string title;

            if (_hostInfo != null)
            {
                title = $"Interface — {_hostInfo.Name}";
                details =
                    $"Name: {_hostInfo.Name}\n" +
                    $"Description: {_hostInfo.Description}\n" +
                    $"Type: {_hostInfo.Kind}\n" +
                    $"Status: {_hostInfo.StatusText}\n" +
                    $"MAC: {_hostInfo.Mac}\n" +
                    $"Speed: {_hostInfo.SpeedText}\n" +
                    $"IPv4: {(_hostInfo.IPv4.Count > 0 ? string.Join(", ", _hostInfo.IPv4) : "—")}\n" +
                    $"IPv6: {(_hostInfo.IPv6.Count > 0 ? string.Join(", ", _hostInfo.IPv6) : "—")}";
            }
            else if (_modelInterface != null)
            {
                title = $"Interface — {_modelInterface.Name}";
                IP? ip = _modelInterface.IP;
                details =
                    $"Name: {_modelInterface.Name}\n" +
                    $"Status: {(_modelInterface.IsUp ? "UP" : "DOWN")}\n" +
                    $"IPv4: {ip?.IPv4 ?? "—"}\n" +
                    $"Subnet: {ip?.IPv4SubnetMask ?? "—"}\n" +
                    $"Gateway: {ip?.IPv4Gateway ?? "—"}\n" +
                    $"IPv6: {ip?.IPv6 ?? "—"}\n" +
                    $"IPv6 prefix: {(ip != null ? ip.IPv6PrefixLength.ToString() : "—")}";
            }
            else
            {
                return;
            }

            MessageBox.Show(details, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    //KI end
}
