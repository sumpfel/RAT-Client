using RAT_WPF.Logging;
using RAT_WPF.Stores;
using RAT_WPF.ViewModels;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace RAT_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly NavigationStore _navigationStore;
        //KI start (Claude Opus 4.8, prompt: link the C# frontend with the RAT-Backend database):
        // default to the real login screen now that it authenticates against the backend.
        // Set this back to true to skip login during development (no DB connection then, so the
        // canvas starts empty and saving is a no-op).
        private bool _debugging_ignore_login = false;
        //KI end
        public App()
        {
            _navigationStore = new NavigationStore();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //KI start (Claude Opus 4.8, prompt 22): start a fresh log file for this run (keeps the newest 3),
            // and record any unhandled UI exception before the app dies.
            AppLogger.Start();
            DispatcherUnhandledException += OnUnhandledException;
            //KI end

            // TODO: If already logged in, start topologyviewmodel instead
            if (_debugging_ignore_login)
            {
                //KI start (Claude Opus 4.8, prompt 11): debug-login skips the login screen, so seed a dev user
                // (CanCreate=true) — otherwise Session.CurrentUser is null and creation/ownership won't work.
                RAT_Logic.Session.CurrentUser ??= new RAT_Logic.NetworkUser("debug", 0, canCreate: true);
                //KI end
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
            AppLogger.Info("Main window shown."); // KI (prompt 22)

            //KI start (Claude Opus 4.8, prompt 25): first-run setup — offer a desktop shortcut + nmap install once.
            RunFirstRunSetup();
            //KI end

            base.OnStartup(e);
        }

        //KI start (Claude Opus 4.8, prompt 25): on the very first start, ask about a desktop shortcut and installing
        // nmap (for the Discover feature). Choices are remembered; declining nmap greys out the Discover button.
        private async void RunFirstRunSetup()
        {
            if (Setup.SetupService.HasRunSetup) { return; }

            MessageBoxResult shortcut = MessageBox.Show(
                "Create a desktop shortcut for RAT?",
                "RAT setup", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (shortcut == MessageBoxResult.Yes)
            {
                bool ok = Setup.SetupService.CreateDesktopShortcut();
                AppLogger.Info($"Desktop shortcut created: {ok}");
            }

            if (!Discovery.NmapService.IsInstalled())
            {
                MessageBoxResult installNmap = MessageBox.Show(
                    "RAT can scan your network for devices using nmap, which isn't installed.\n\n" +
                    "Install nmap now? (You can use RAT without it — the 'Discover devices' button will just stay disabled.)",
                    "Install nmap?", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (installNmap == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool installed = await Discovery.NmapService.InstallAsync();
                        Setup.SetupService.NmapDeclined = !installed;
                        AppLogger.Info($"nmap install finished, installed={installed}");
                    }
                    catch (System.Exception ex)
                    {
                        Setup.SetupService.NmapDeclined = true;
                        AppLogger.Error("nmap install failed", ex);
                        RatDialog.Show("nmap", $"Couldn't install nmap automatically.\n\n{ex.Message}", "Icon.NoConnection");
                    }
                }
                else
                {
                    Setup.SetupService.NmapDeclined = true; // user said no -> Discover stays greyed out
                }
            }

            Setup.SetupService.MarkSetupDone();
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 22): log unhandled exceptions to the run's log file.
        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            AppLogger.Error("Unhandled UI exception", e.Exception);
        }
        //KI end
    }

}
