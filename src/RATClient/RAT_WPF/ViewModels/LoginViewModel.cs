using RAT_WPF.Commands;
using RAT_WPF.Stores;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.Swift;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace RAT_WPF.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
		private string _username;

		public string Username
		{
			get
			{ 
				return _username; 
			}
			set 
			{ 
				_username = value; 
				OnPropertyChanged(nameof(Username));
			}
		}

        private string _password;

		public string Password
        {
			get 
			{ 
				return _password; 
			}
			set 
			{ 
				_password = value; 
				OnPropertyChanged(nameof(Password));
			}
		}

		private string _serverIp;

		public string ServerIp
		{
			get 
			{ 
				return _serverIp; 
			}
			set 
			{ 
				_serverIp = value;
				OnPropertyChanged(nameof(ServerIp));
			}
		}

		private int _serverPort;

		public int ServerPort
		{
			get 
			{ 
				return _serverPort; 
			}
			set 
			{ 
				_serverPort = value;
				OnPropertyChanged(nameof(ServerPort));
			}
		}

		public ICommand ConfirmCommand { get; }

		public LoginViewModel(NavigationStore navigationStore)
		{
			ConfirmCommand = new LoginCommand(this, navigationStore);

            this.PropertyChanged += OnLoginViewModelPropertyChanged;
		}

		private Brush _textBoxUsernameColor = Brushes.LightCoral;

		public Brush TextBoxUsernameColor
		{
			get
			{
				return _textBoxUsernameColor;
			}
			set
			{
				_textBoxUsernameColor = value;
				OnPropertyChanged(nameof(TextBoxUsernameColor));
			}
		}

        private Brush _textBoxPasswordColor = Brushes.LightCoral;

        public Brush TextBoxPasswordColor
        {
            get
            {
                return _textBoxPasswordColor;
            }
            set
            {
                _textBoxPasswordColor = value;
                OnPropertyChanged(nameof(TextBoxPasswordColor));
            }
        }

        private Brush _textBoxServerIpColor = Brushes.LightCoral;

        public Brush TextBoxServerIpColor
        {
            get
            {
                return _textBoxServerIpColor;
            }
            set
            {
                _textBoxServerIpColor = value;
                OnPropertyChanged(nameof(TextBoxServerIpColor));
            }
        }

        private Brush _textBoxServerPortColor = Brushes.LightCoral;

        public Brush TextBoxServerPortColor
        {
            get
            {
                return _textBoxServerPortColor;
            }
            set
            {
                _textBoxServerPortColor = value;
                OnPropertyChanged(nameof(TextBoxServerPortColor));
            }
        }

        public bool UsernameOk { get; private set; } = false;
        public bool PasswordOk { get; private set; } = false;
        public bool ServerIpOk { get; private set; } = false;
        public bool ServerPortOk { get; private set; } = false;

        private void OnLoginViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
			switch (e.PropertyName)
			{
				case nameof(Username):
					if (!string.IsNullOrEmpty(this.Username))
					{
						UsernameOk = true;
					}
					else
					{
						UsernameOk = false;
					}

					if (UsernameOk)
					{
						TextBoxUsernameColor = Brushes.Green;
					}
					else
					{
						TextBoxUsernameColor = Brushes.LightCoral;
					}

					break;
				case nameof(Password):
                    if (!string.IsNullOrEmpty(this.Password))
                    {
                        PasswordOk = true;
                    }
                    else
                    {
                        PasswordOk = false;
                    }

					if (PasswordOk)
					{
						TextBoxPasswordColor = Brushes.Green;
					}
					else
					{
                        TextBoxPasswordColor = Brushes.LightCoral;
					}

                    break;
				case nameof(ServerIp):
                    if (Regex.IsMatch(ServerIp, @"^(((?!25?[6-9])[12]\d|[1-9])?\d\.?\b){4}$")) // Regex from https://stackoverflow.com/questions/5284147/validating-ipv4-addresses-with-regexp
                    {
                        ServerIpOk = true;
                    }
                    else
                    {
                        ServerIpOk = false;
                    }

					if (ServerIpOk)
					{
						TextBoxServerIpColor = Brushes.Green;
					}
					else
					{
						TextBoxServerIpColor = Brushes.LightCoral;
					}

                    break;
				case nameof(ServerPort):
					if (ServerPort >= 0 && ServerPort <= 65535)
					{
						ServerPortOk = true;
					}
					else
					{
						ServerPortOk = false;
					}
					
					if (ServerPortOk)
					{
						TextBoxServerPortColor = Brushes.Green;
					}
					else
					{
						TextBoxServerPortColor = Brushes.LightCoral;
					}

					break;
                default:
					break;
			}
		}
    }
}
