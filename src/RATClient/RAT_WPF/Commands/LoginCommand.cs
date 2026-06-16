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
                MessageBox.Show($"Login failed: {ex.Message}", "Login",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DatabaseConnectionStore.Current = connection;
            RAT_Logic.Session.CurrentUser = new RAT_Logic.NetworkUser(
                loggedIn.UserName, loggedIn.ID, canCreate: loggedIn.CanCreate, privileges: loggedIn.Privileges);

            // navigation Part
            _navigationStore.CurrentViewModel = new TopologyViewModel();
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
