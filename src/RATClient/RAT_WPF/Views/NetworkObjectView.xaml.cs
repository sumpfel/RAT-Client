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
    /// Interaction logic for NetworkObjectView.xaml
    /// </summary>
    public partial class NetworkObjectView : UserControl
    {
        public EnumTool CurrentTool
        {
            get { return (EnumTool)GetValue(CurrentToolProperty); }
            set { SetValue(CurrentToolProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Tool.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentToolProperty =
            DependencyProperty.Register(nameof(CurrentTool), typeof(EnumTool), typeof(NetworkObjectView), new PropertyMetadata(EnumTool.Cursor));



        public ICommand CommandLeftClickWithConnectionTool
        {
            get { return (ICommand)GetValue(CommandLeftClickWithConnectionToolProperty); }
            set { SetValue(CommandLeftClickWithConnectionToolProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandLeftClickWithConnectionTool.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandLeftClickWithConnectionToolProperty =
            DependencyProperty.Register(nameof(CommandLeftClickWithConnectionTool), typeof(ICommand), typeof(NetworkObjectView), new PropertyMetadata(null));


        public NetworkObjectView()
        {
            InitializeComponent();
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            //KI start (Claude Opus 4.8, prompt 4): don't drag while the Delete tool is active
            if (FindTopologyView()?.IsDeleteToolActive == true)
            {
                return;
            }
            //KI end
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (CurrentTool == EnumTool.Cursor)
                {
                    DragDrop.DoDragDrop(grid, new DataObject(DataFormats.Serializable, grid), DragDropEffects.Move);
                }
                else if (CurrentTool == EnumTool.Connector && DataContext is NetworkObjectViewModel networkObjectViewModel)
                { 
                    if (CommandLeftClickWithConnectionTool?.CanExecute(null) ?? false)
                    {
                        CommandLeftClickWithConnectionTool.Execute(networkObjectViewModel);
                    }
                }
            }
        }

        //KI start (Claude Opus 4.8, prompt 4): when the Delete tool is active, clicking this device removes it
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TopologyView? topology = FindTopologyView();
            if (topology?.IsDeleteToolActive == true
                && DataContext is ViewModels.NetworkObjectViewModel networkObjectViewModel)
            {
                topology.DeleteNode(networkObjectViewModel);
                e.Handled = true;
            }
        }

        private TopologyView? FindTopologyView()
        {
            DependencyObject? current = this;
            while (current != null)
            {
                if (current is TopologyView topologyView)
                {
                    return topologyView;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
        //KI end
    }
}
