using RAT_WPF.Stores;
using RAT_WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public override void Execute(object? parameter)
        {
            // TODO: Do login, if successful nav to topologieview

            //KI start (Claude Opus 4.8, prompt 1): remember who logged in so PC network objects can be owned by them
            RAT_Logic.Session.CurrentUser = new RAT_Logic.NetworkUser(_loginViewModel.Username, 0);
            //KI end

            // navigation Part

            _navigationStore.CurrentViewModel = new TopologyViewModel();
        }

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
