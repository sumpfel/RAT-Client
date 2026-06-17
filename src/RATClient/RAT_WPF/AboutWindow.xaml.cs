using System.Windows;

namespace RAT_WPF
{
    //KI start (Claude Opus 4.8, prompt 17): About window — credits Tobias & Claude (helper Christof) + rat mascot.
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
    //KI end
}
