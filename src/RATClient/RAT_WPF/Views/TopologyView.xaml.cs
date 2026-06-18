using RAT_WPF.Commands;
using RAT_WPF.Stores;
using RAT_WPF.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RAT_WPF.Views
{
    /// <summary>
    /// Interaction logic for TopologyView.xaml
    /// </summary>
    public partial class TopologyView : UserControl
    {
        private async void NetworkObject_Drop(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);

            if (sender is Canvas canvas && this.DataContext is TopologyViewModel topologyViewModel)
            {
                if (data is NetworkObjectViewModel networkObject)
                {
                    //KI start (Claude Opus 4.8, prompt 11): only users with CanCreate may add network objects
                    if (RAT_Logic.Session.CurrentUser?.CanCreate != true)
                    {
                        RatDialog.Show("Not allowed", "You don't have permission to create network objects.", "Icon.LoginFailed");
                        return;
                    }
                    //KI end
                    /*
                    NetworkObjectView view = new NetworkObjectView()
                    {
                        DataContext = networkObject,
                        CurrentTool = topologyViewModel.ToolEnum,
                        CommandLeftClickWithConnectionTool = topologyViewModel.NetworkObjectAddConnectionCommand
                    };
                    */
                    bool exists = false;

                    // TODO: Improve so that multiple new Elements can be added, without having to rename them
                    foreach (FrameworkElement uiElement in canvas.Children)
                    {
                        if (uiElement.DataContext == networkObject)
                        {
                            exists = true;
                        }
                    }

                    if (!exists)
                    {
                        // canvas.Children.Add(view);
                        Point dropPosition = e.GetPosition(canvas);
                        // Canvas.SetLeft(view, dropPosition.X);
                        // Canvas.SetTop(view, dropPosition.Y);
                        //KI (prompt 28): clamp inside the canvas so a node can't land in unreachable void
                        networkObject.X = CanvasLayout.ClampX(dropPosition.X);
                        networkObject.Y = CanvasLayout.ClampY(dropPosition.Y);

                        //KI start (Claude Opus 4.8, prompt 11): the creator owns the object they just created
                        if (RAT_Logic.Session.CurrentUser != null
                            && !networkObject.Model.AccessRights.Any(a => a.Rights == RAT_Logic.AccesRights.Owner))
                        {
                            networkObject.Model.ApplyRight(RAT_Logic.Session.CurrentUser, RAT_Logic.AccesRights.Owner);
                        }
                        //KI end

                        //KI start (Claude Opus 4.8, prompt 16): persist FIRST and only put the device on the canvas
                        // if the backend accepted it. On a 500 (or any error) we don't add it, so the canvas never
                        // shows a device that wasn't saved. (AddNetworkObject also makes the creator Owner + sets the id.)
                        bool saved = await PersistNewNetworkObject(networkObject);
                        if (!saved) { return; }

                        topologyViewModel.AddNetworkObjectViewModelToCanvas(networkObject);
                        //KI end
                    }
                }

                else if (data is NetworkObjectView networkObjectView && networkObjectView.DataContext is NetworkObjectViewModel networkObjectViewModel)
                {
                    Point dropPosition = e.GetPosition(canvas);

                    //KI (prompt 28): clamp inside the canvas so a moved node can't be dragged into unreachable void
                    networkObjectViewModel.X = CanvasLayout.ClampX(dropPosition.X);
                    networkObjectViewModel.Y = CanvasLayout.ClampY(dropPosition.Y);

                    //KI start (Claude Opus 4.8, prompt: link the C# frontend with the RAT-Backend database):
                    // a moved device already exists in the DB -> persist its new X/Y.
                    PersistMovedNetworkObject(networkObjectViewModel);
                    //KI end
                }
            }
        }

        private void NetworkObject_DragOver(object sender, DragEventArgs e)
        {

        }

        //KI start (Claude Opus 4.8, prompt 27): drag-to-pan the canvas left/right (and up/down). We only start a pan
        // when the press lands on empty canvas — if it lands on a device node (NetworkObjectView) or a clickable
        // cable line we leave it alone so dragging devices / clicking cables keeps working. The horizontal scroll
        // bar comes from the ScrollViewer in XAML.
        private bool _panning;
        private Point _panStart;
        private double _panStartH, _panStartV;

        private void PanCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsOnInteractiveElement(e.OriginalSource as DependencyObject)) { return; }

            _panning = true;
            _panStart = e.GetPosition(CanvasScroll);
            _panStartH = CanvasScroll.HorizontalOffset;
            _panStartV = CanvasScroll.VerticalOffset;
            CanvasScroll.Cursor = Cursors.ScrollAll;
            CanvasScroll.CaptureMouse();
        }

        private void PanCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_panning) { return; }
            Point now = e.GetPosition(CanvasScroll);
            CanvasScroll.ScrollToHorizontalOffset(_panStartH - (now.X - _panStart.X));
            CanvasScroll.ScrollToVerticalOffset(_panStartV - (now.Y - _panStart.Y));
        }

        private void PanCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_panning) { return; }
            _panning = false;
            CanvasScroll.Cursor = Cursors.Arrow;
            CanvasScroll.ReleaseMouseCapture();
        }

        // walk up the visual tree: a press inside a device node or on a clickable cable line is NOT a pan
        private static bool IsOnInteractiveElement(DependencyObject? source)
        {
            while (source != null)
            {
                if (source is NetworkObjectView) { return true; }
                if (source is System.Windows.Shapes.Line) { return true; } // the clickable cable hit line
                source = System.Windows.Media.VisualTreeHelper.GetParent(source);
            }
            return false;
        }
        //KI end
        public TopologyView()
        {
            InitializeComponent();

            //KI start (Claude Opus 4.8, prompt 14): show the Users button only to admins (Privileges >= 100).
            if (RAT_Logic.Session.CurrentUser?.Privileges >= 100)
            {
                UsersButton.Visibility = Visibility.Visible;
            }
            //KI end

            //KI start (Claude Opus 4.8, prompt 22): reflect the loaded showInterfaces setting in the toggle
            ShowInterfacesToggle.IsChecked = RAT_WPF.Themes.DisplaySettings.ShowInterfaces;
            //KI end
            //KI start (Claude Opus 4.8, prompt 26): reflect the loaded showPorts setting in the toggle
            ShowPortsToggle.IsChecked = RAT_WPF.Themes.DisplaySettings.ShowPorts;
            //KI end

            //KI start (Claude Opus 4.8, prompt 25): the Discover button needs nmap. Grey it out (with a hint) when
            // nmap isn't installed or the user declined installing it during first-run setup.
            bool canDiscover = !Setup.SetupService.NmapDeclined && Discovery.NmapService.IsInstalled();
            DiscoverButton.IsEnabled = canDiscover;
            DiscoverButton.ToolTip = canDiscover
                ? "Scan the local network with nmap and add the devices it finds"
                : "nmap is not installed — install it to enable network discovery";
            //KI end
        }

        //KI start (Claude Opus 4.8, prompt 25): run an nmap discovery and add/cable the found devices
        private async void DiscoverButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is not TopologyViewModel topologyViewModel) { return; }
            DiscoverButton.IsEnabled = false;
            try
            {
                await topologyViewModel.DiscoverDevicesAsync();
            }
            catch (Exception ex)
            {
                RatDialog.Show("Discovery failed", $"The rat couldn't scan the network.\n\n{ex.Message}", "Icon.NoConnection");
            }
            finally
            {
                DiscoverButton.IsEnabled = !Setup.SetupService.NmapDeclined && Discovery.NmapService.IsInstalled();
            }
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 22): toggle the cable interface labels + persist the choice
        private async void ShowInterfacesToggle_Click(object sender, RoutedEventArgs e)
        {
            bool on = ShowInterfacesToggle.IsChecked == true;
            RAT_WPF.Themes.DisplaySettings.ShowInterfaces = on;

            if (DatabaseConnectionStore.Current == null) { return; } // dev mode / not connected
            try
            {
                RAT_Data.UserSettings settings;
                try { settings = await DatabaseConnectionStore.Current.GetUserSettings(); }
                catch { settings = new RAT_Data.UserSettings(); }

                settings.ShowInterfaces = on;
                settings.Zoom = RAT_WPF.Themes.ZoomManager.Current; // keep the zoom in sync
                await DatabaseConnectionStore.Current.EditUserSettings(settings);
            }
            catch
            {
                // saving the preference is best-effort
            }
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 26): toggle the open-ports view + persist the choice
        private async void ShowPortsToggle_Click(object sender, RoutedEventArgs e)
        {
            bool on = ShowPortsToggle.IsChecked == true;
            RAT_WPF.Themes.DisplaySettings.ShowPorts = on;

            //KI (prompt 30): turning ports ON should populate them for devices already on the canvas that have an IP
            // but no scanned ports yet (otherwise the toggle showed nothing unless you re-ran discovery).
            if (on && this.DataContext is TopologyViewModel vm) { vm.EnsurePortsScannedAsync(); }

            if (DatabaseConnectionStore.Current == null) { return; }
            try
            {
                RAT_Data.UserSettings settings;
                try { settings = await DatabaseConnectionStore.Current.GetUserSettings(); }
                catch { settings = new RAT_Data.UserSettings(); }

                settings.ShowPorts = on;
                settings.ShowInterfaces = RAT_WPF.Themes.DisplaySettings.ShowInterfaces; // keep the others in sync
                settings.Zoom = RAT_WPF.Themes.ZoomManager.Current;
                await DatabaseConnectionStore.Current.EditUserSettings(settings);
            }
            catch
            {
                // best-effort
            }
        }
        //KI end

        //KI start (ported to MVVM by AI, prompt 30): click a cable — Delete tool removes it (single click);
        // Cursor tool opens the editor on DOUBLE-click. Right-click always opens the editor (any tool). All actions
        // route through the TopologyViewModel (delete = command; edit = OpenEditConnection which raises an event the
        // view turns into the dialog, keeping the VM free of WPF window types).
        private void Connection_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext is not TopologyViewModel topologyViewModel) { return; }
            if (sender is not FrameworkElement fe || fe.DataContext is not NetworkConnectionViewModel connection) { return; }

            if (topologyViewModel.ToolEnum == EnumTool.Delete)
            {
                topologyViewModel.NetworkObjectDeleteCommand.Execute(connection);
                e.Handled = true;
            }
            else if (topologyViewModel.ToolEnum == EnumTool.Cursor && e.ClickCount >= 2)
            {
                EditConnection(connection);
                e.Handled = true;
            }
        }

        private void Connection_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is NetworkConnectionViewModel connection)
            {
                EditConnection(connection);
                e.Handled = true;
            }
        }

        private void EditConnection(NetworkConnectionViewModel connection)
        {
            if (this.DataContext is not TopologyViewModel topologyViewModel) { return; }
            NetworkObject_UI.EditConnectionWindow window =
                new NetworkObject_UI.EditConnectionWindow(connection.networkConnection) { Owner = Window.GetWindow(this) };
            if (window.ShowDialog() == true)
            {
                connection.RefreshAfterEdit();              // KI (prompt 30): re-render the cable (e.g. dashed <-> solid)
                topologyViewModel.PersistEditedConnection(connection);
            }
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 2): open the MVVM settings window
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.Owner = Window.GetWindow(this);
            settings.ShowDialog();
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 14): open the admin-only user management window
        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            ManageUsersWindow window = new ManageUsersWindow();
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 14/15): create/move persistence helpers. async-void event-handler
        // style (the caller does not await); failures surface via a MessageBox; no-ops without a connection.
        // (Delete now lives in TopologyViewModel.DeleteNetworkObjectFromCanvasAndDatabase, called by the Delete tool.)

        //KI start (Claude Opus 4.8, prompt 16): now returns whether the save succeeded, so the caller can avoid
        // putting a device on the canvas that the backend rejected (e.g. a 500). No connection (dev mode) == ok.
        private async Task<bool> PersistNewNetworkObject(NetworkObjectViewModel node)
        {
            RAT_WPF.Logging.AppLogger.Info($"Create device '{node.Model.Name}' ({node.Model.Type}) at {node.X},{node.Y}"); // KI (prompt 28)
            if (DatabaseConnectionStore.Current == null) { return true; }
            try
            {
                await DatabaseConnectionStore.Current.AddNetworkObject(node.Model);

                //KI start (Claude Opus 4.8, prompt 23): a PC auto-populates this machine's real interfaces, but
                // those only lived on the client — persist them now that the object has a real backend id, so the
                // interfaces (and their IPs/specs) exist on the server too. Each AddInterface fills the iface id.
                foreach (RAT_Logic.NetworkObjectInterface iface in node.Model.NetworkInterfaces)
                {
                    if (iface.ID > 0) { continue; } // already saved
                    await DatabaseConnectionStore.Current.AddInterface(iface, node.Model);
                }
                //KI end

                return true;
            }
            catch (Exception ex)
            {
                RAT_WPF.Logging.AppLogger.Error($"Create device '{node.Model.Name}' failed", ex); // KI (prompt 28)
                RatDialog.Show("Database hiccup", $"The rat couldn't save the new device on the server.\n\n{ex.Message}", "Icon.DatabaseError");
                return false;
            }
        }
        //KI end

        private async void PersistMovedNetworkObject(NetworkObjectViewModel node)
        {
            if (DatabaseConnectionStore.Current == null) { return; }
            if (node.Model.ID <= 0) { return; } // never saved yet -> nothing to update
            try
            {
                await DatabaseConnectionStore.Current.EditNetworkObject(node.Model);
            }
            catch (Exception ex)
            {
                RatDialog.Show("Database hiccup", $"The rat couldn't save the device position on the server.\n\n{ex.Message}", "Icon.DatabaseError");
            }
        }

        //KI end
    }
}
