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
    /// Interaction logic for TopologyView.xaml
    /// </summary>
    public partial class TopologyView : UserControl
    {

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
    }
}
