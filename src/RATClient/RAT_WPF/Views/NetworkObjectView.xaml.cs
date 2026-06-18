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

        public ICommand CommandLeftClickWithDeleteTool
        {
            get { return (ICommand)GetValue(CommandLeftClickWithDeleteToolProperty); }
            set { SetValue(CommandLeftClickWithDeleteToolProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandLeftClickWithDeleteTool.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandLeftClickWithDeleteToolProperty =
            DependencyProperty.Register(nameof(CommandLeftClickWithDeleteTool), typeof(ICommand), typeof(NetworkObjectView), new PropertyMetadata(null));



        public NetworkObjectView()
        {
            InitializeComponent();
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            //KI start (Claude Opus 4.8, prompt 22): only the Cursor tool drags. The Connection/Delete tools act on a
            // CLICK (MouseLeftButtonUp) instead of on move — see Grid_MouseLeftButtonUp — so a precise click on the
            // device (icon included) reliably triggers them.
            if (CurrentTool == EnumTool.Cursor
                && e.LeftButton == MouseButtonState.Pressed
                && DataContext is NetworkObjectViewModel)
            {
                DragDrop.DoDragDrop(grid, new DataObject(DataFormats.Serializable, grid), DragDropEffects.Move);
            }
            //KI end
        }

        //KI start (Claude Opus 4.8, prompt 22): Connection/Delete tools fire on a click of the whole node (the icon
        // is IsHitTestVisible=False and the node Grid is Transparent, so clicking anywhere on the device — including
        // over the icon — counts).
        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not NetworkObjectViewModel networkObjectViewModel) { return; }

            if (CurrentTool == EnumTool.Connector)
            {
                if (CommandLeftClickWithConnectionTool?.CanExecute(networkObjectViewModel) ?? false)
                {
                    CommandLeftClickWithConnectionTool.Execute(networkObjectViewModel);
                    e.Handled = true;
                }
            }
            else if (CurrentTool == EnumTool.Delete)
            {
                if (CommandLeftClickWithDeleteTool?.CanExecute(networkObjectViewModel) ?? false)
                {
                    CommandLeftClickWithDeleteTool.Execute(networkObjectViewModel);
                    e.Handled = true;
                }
            }
        }
        //KI end
    }
}
