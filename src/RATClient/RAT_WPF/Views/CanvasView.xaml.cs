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
    /// Interaction logic for CanvasView.xaml
    /// </summary>
    public partial class CanvasView : UserControl
    {

        // Property that gets called whenever NetworkObject is dropped on Canvas
        // TODO: Bind Command To Save on Database
        public ICommand NetworkObjectDropCommand
        {
            get { return (ICommand)GetValue(NetworkObjectDropCommandProperty); }
            set { SetValue(NetworkObjectDropCommandProperty, value); }
        }

        public static readonly DependencyProperty NetworkObjectDropCommandProperty =
            DependencyProperty.Register(nameof(NetworkObjectDropCommand), typeof(ICommand), typeof(CanvasView), new PropertyMetadata(null));


        public CanvasView()
        {
            InitializeComponent();
        }

        private void NetworkObject_Drop(object sender, DragEventArgs e)
        {
            if (NetworkObjectDropCommand?.CanExecute(null) ?? false)
            {
                NetworkObjectDropCommand?.Execute(null);
            }
        }

        private void NetworkObject_DragOver(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);

            if (data is NetworkObjectViewModel networkObject)
            {   
                NetworkObjectView view = new NetworkObjectView()
                {
                    DataContext = networkObject
                };

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
                    canvas.Children.Add(view);
                    Point dropPosition = e.GetPosition(view);
                    Canvas.SetLeft(view, dropPosition.X);
                    Canvas.SetTop(view, dropPosition.Y);
                }
            }

            else if (data is UIElement element)
            {
                Point dropPosition = e.GetPosition(canvas);

                Canvas.SetLeft(element, dropPosition.X);
                Canvas.SetTop(element, dropPosition.Y);
            }
        }
    }
}
