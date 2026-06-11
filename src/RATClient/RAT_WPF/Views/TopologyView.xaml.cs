using RAT_WPF.Commands;
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
                        // TODO: Add to List in TopologyViewModel, and update x and y
                        // canvas.Children.Add(view);
                        Point dropPosition = e.GetPosition(canvas);
                        // Canvas.SetLeft(view, dropPosition.X);
                        // Canvas.SetTop(view, dropPosition.Y);
                        networkObject.X = (int)dropPosition.X;
                        networkObject.Y = (int)dropPosition.Y;
                        topologyViewModel.AddNetworkObjectViewModelToCanvas(networkObject);
                    }
                }

                else if (data is NetworkObjectView networkObjectView && networkObjectView.DataContext is NetworkObjectViewModel networkObjectViewModel)
                {
                    Point dropPosition = e.GetPosition(canvas);
                    
                    networkObjectViewModel.X = (int)dropPosition.X;
                    networkObjectViewModel.Y = (int)dropPosition.Y;
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
    }
}
