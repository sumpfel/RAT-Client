using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RAT_WPF.Themes;

namespace RAT_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //KI start (Claude Opus 4.8, prompt 20): apply the current zoom and follow future changes.
            ApplyZoom(ZoomManager.Current);
            ZoomManager.ZoomChanged += ApplyZoom;
            Unloaded += (_, _) => ZoomManager.ZoomChanged -= ApplyZoom;
            //KI end
        }

        //KI start (Claude Opus 4.8, prompt 20): scale the whole shell to the chosen zoom.
        private void ApplyZoom(int percent)
        {
            double scale = percent / 100.0;
            ZoomTransform.ScaleX = scale;
            ZoomTransform.ScaleY = scale;
        }
        //KI end
    }
}