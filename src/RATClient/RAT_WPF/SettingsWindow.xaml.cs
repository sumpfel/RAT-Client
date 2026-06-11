using System.Windows;

namespace RAT_WPF
{
    //KI start (Claude Opus 4.8, prompt 2): settings window (theme switcher). DataContext is set in XAML (SettingsViewModel).
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
    //KI end
}
