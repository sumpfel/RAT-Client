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
                        networkObject.X = (int)dropPosition.X;
                        networkObject.Y = (int)dropPosition.Y;

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

                    networkObjectViewModel.X = (int)dropPosition.X;
                    networkObjectViewModel.Y = (int)dropPosition.Y;

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

        //KI start (Claude Opus 4.8, prompt 22/25): click a cable — Delete tool removes it, Cursor tool edits it.
        private void Connection_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext is not TopologyViewModel topologyViewModel) { return; }
            if (sender is not FrameworkElement fe || fe.DataContext is not NetworkConnectionViewModel connection) { return; }

            if (topologyViewModel.ToolEnum == EnumTool.Delete)
            {
                topologyViewModel.NetworkObjectDeleteCommand.Execute(connection);
                e.Handled = true;
            }
            else if (topologyViewModel.ToolEnum == EnumTool.Cursor)
            {
                NetworkObject_UI.EditConnectionWindow window =
                    new NetworkObject_UI.EditConnectionWindow(connection.networkConnection) { Owner = Window.GetWindow(this) };
                if (window.ShowDialog() == true)
                {
                    topologyViewModel.PersistEditedConnection(connection);
                }
                e.Handled = true;
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
