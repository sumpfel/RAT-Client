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
        private void NetworkObject_Drop(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);

            if (sender is Canvas canvas && this.DataContext is TopologyViewModel topologyViewModel)
            {
                if (data is NetworkObjectViewModel networkObject)
                {
                    //KI start (Claude Opus 4.8, prompt 11): only users with CanCreate may add network objects
                    if (RAT_Logic.Session.CurrentUser?.CanCreate != true)
                    {
                        MessageBox.Show("You don't have permission to create network objects.",
                            "Not allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                        //KI start (Claude Opus 4.8, prompt: link the C# frontend with the RAT-Backend database):
                        // persist the new device to the backend (which also makes the creator its Owner
                        // server-side and assigns the real id). Saving happens in the helper below.
                        PersistNewNetworkObject(networkObject);
                        //KI end

                        topologyViewModel.AddNetworkObjectViewModelToCanvas(networkObject);
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
        }

        //KI start (Claude Opus 4.8, prompt 2): open the MVVM settings window
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.Owner = Window.GetWindow(this);
            settings.ShowDialog();
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 4): delete tool — let a canvas node remove itself when the tool is active

        public void DeleteNode(NetworkObjectViewModel node)
        {
            //KI start (Claude Opus 4.8, prompt 11): only an Owner of the object may delete it
            if (!node.Model.CanBeDeletedBy(RAT_Logic.Session.CurrentUser))
            {
                MessageBox.Show("Only an owner of this device can delete it.",
                    "Not allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //KI end
            if (this.DataContext is TopologyViewModel topologyViewModel)
            {
                //KI start (Claude Opus 4.8, prompt: link the C# frontend with the RAT-Backend database):
                // remove from the backend first (only if it was ever saved, i.e. has a real id), then
                // from the canvas. If the server rejects it we keep the node and show the reason.
                PersistDeletedNetworkObject(node, topologyViewModel);
                //KI end
            }
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt: link the C# frontend with the RAT-Backend database):
        // Persistence helpers. All are async-void event-handler style (no Task is awaited by the
        // caller) and swallow nothing silently — failures are surfaced via a MessageBox. They are
        // no-ops when there is no active connection (e.g. the debug-skip-login path).

        private async void PersistNewNetworkObject(NetworkObjectViewModel node)
        {
            if (DatabaseConnectionStore.Current == null) { return; }
            try
            {
                await DatabaseConnectionStore.Current.AddNetworkObject(node.Model);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save the new device to the server: {ex.Message}",
                    "Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

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
                MessageBox.Show($"Could not save the device position to the server: {ex.Message}",
                    "Database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void PersistDeletedNetworkObject(NetworkObjectViewModel node, TopologyViewModel topologyViewModel)
        {
            if (DatabaseConnectionStore.Current != null && node.Model.ID > 0)
            {
                try
                {
                    await DatabaseConnectionStore.Current.DeleteNetworkObject(node.Model);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not delete the device on the server: {ex.Message}",
                        "Database", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // keep the node on the canvas if the server refused
                }
            }
            topologyViewModel.RemoveNetworkObjectViewModelFromCanvas(node);
        }
        //KI end
    }
}
