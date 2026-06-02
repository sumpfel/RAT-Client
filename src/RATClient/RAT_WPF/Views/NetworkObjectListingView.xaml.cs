using RAT_WPF.Commands;
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
    /// Interaction logic for NetworkObjectListingView.xaml
    /// </summary>
    public partial class NetworkObjectListingView : UserControl
    {
        public NetworkObjectListingView()
        {
            InitializeComponent();
        }

        private void ListviewNetworkObject_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is FrameworkElement frameworkElement)
            {
                DragDrop.DoDragDrop(frameworkElement, new DataObject(DataFormats.Serializable, frameworkElement.DataContext), DragDropEffects.Move);
            }
        }
    }
}
