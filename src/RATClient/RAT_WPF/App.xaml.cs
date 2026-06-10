using RAT_WPF.Stores;
using RAT_WPF.ViewModels;
using System.Configuration;
using System.Data;
using System.Windows;

namespace RAT_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly NavigationStore _navigationStore;
        private bool _debugging_ignore_login=true;
        public App()
        {
            _navigationStore = new NavigationStore();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // TODO: If already logged in, start topologyviewmodel instead
            if (_debugging_ignore_login)
            {
                _navigationStore.CurrentViewModel = new TopologyViewModel();
            }
            else
            {
                _navigationStore.CurrentViewModel = new LoginViewModel(_navigationStore);
            }
            

            MainWindow = new MainWindow()
            {
                DataContext = new MainViewModel(_navigationStore)
            };
            MainWindow.Show();

            base.OnStartup(e);
        }
    }

}
