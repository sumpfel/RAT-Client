using RAT_Data;
using RAT_WPF.Stores;
using RAT_WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RAT_WPF.Commands
{
    public class LoginCommand : CommandBase
    {
        private LoginViewModel _loginViewModel;

        private readonly NavigationStore _navigationStore;

        public LoginCommand(LoginViewModel loginViewModel, NavigationStore navigationStore)
        {
            _navigationStore = navigationStore;

            _loginViewModel = loginViewModel;

            _loginViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }

        //KI start (Claude Opus 4.8, prompt: link the C# frontend with the RAT-Backend database):
        // real login against the backend. Build a DatabaseConnection from the entered server
        // IP/port + credentials, authenticate, then map the returned RAT_Data.User onto the
        // logic-layer NetworkUser (real ID / CanCreate). On success the connection is stored
        // app-wide (DatabaseConnectionStore) and we navigate to the topology; on failure we
        // show the error and stay on the login screen.
        public override async void Execute(object? parameter)
        {
            User user = new User(_loginViewModel.Username, _loginViewModel.Password, 0, 10, false);
            DatabaseConnection connection = new DatabaseConnection(user, _loginViewModel.ServerIp, _loginViewModel.ServerPort);

            User loggedIn;
            try
            {
                loggedIn = await connection.Login();
            }
            catch (Exception ex)
            {
                RAT_WPF.Logging.AppLogger.Warn($"Login failed for '{_loginViewModel.Username}': {ex.Message}"); // KI (prompt 22)
                //KI start (Claude Opus 4.8, prompt 15): show the sad-rat status on the login screen instead of a popup
                _loginViewModel.ShowStatus(false, $"Login failed: {ex.Message}");
                //KI end
                return;
            }

            RAT_WPF.Logging.AppLogger.Info($"User '{loggedIn.UserName}' logged in (id {loggedIn.ID})."); // KI (prompt 22)

            //KI start (Claude Opus 4.8, prompt 15): happy-rat success status (briefly visible before navigation)
            _loginViewModel.ShowStatus(true, $"Welcome, {loggedIn.UserName}!");
            //KI end

            DatabaseConnectionStore.Current = connection;
            //KI start (Claude Opus 4.8, prompt 15): remember the server so a re-login only needs username/password
            DatabaseConnectionStore.LastServerIp = _loginViewModel.ServerIp;
            DatabaseConnectionStore.LastServerPort = _loginViewModel.ServerPort;
            //KI end
            RAT_Logic.Session.CurrentUser = new RAT_Logic.NetworkUser(
                loggedIn.UserName, loggedIn.ID, canCreate: loggedIn.CanCreate, privileges: loggedIn.Privileges);

            //KI start (Claude Opus 4.8, prompt 22): restore the saved zoom + showInterfaces from the backend.
            try
            {
                UserSettings settings = await connection.GetUserSettings();
                RAT_WPF.Themes.ZoomManager.Apply(settings.Zoom);
                RAT_WPF.Themes.DisplaySettings.ShowInterfaces = settings.ShowInterfaces;
                RAT_WPF.Themes.DisplaySettings.ShowPorts = settings.ShowPorts; // KI (prompt 26)
            }
            catch
            {
                // settings are a nice-to-have; a failure here must not block login
            }
            //KI end

            // navigation Part
            // KI (prompt 15): pass the navigation store so the topology can navigate back to login on logout
            _navigationStore.CurrentViewModel = new TopologyViewModel(_navigationStore);
        }
        //KI end

        public override bool CanExecute(object? parameter)
        {
            return _loginViewModel.UsernameOk
                && _loginViewModel.PasswordOk
                && _loginViewModel.ServerIpOk
                && _loginViewModel.ServerPortOk
                && base.CanExecute(parameter);
        }
    }
}
