using RAT_Logic;
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

namespace RAT_WPF.NetworkObject
{
    /// <summary>
    /// Interaktionslogik für LoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl
    {
        public Login login;
        public RAT_Logic.NetworkObject networkObject;
        public LoginControl(Login login_, RAT_Logic.NetworkObject networkObject_)
        {
            InitializeComponent();
            login = login_;
            networkObject = networkObject_;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (login.Type)
            {
                case LoginType.SSH:
                    networkObject.OpenSSH(login);
                    break;
                case LoginType.SFTP:
                    networkObject.OpenSFTP(login);
                    break;
                case LoginType.SCP:
                    networkObject.OpenSCP(login);
                    break;
                case LoginType.Telnet:
                    throw new NotImplementedException();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            UpdateLoginWindow updateLoginWindow = new UpdateLoginWindow(login);
            if (updateLoginWindow.ShowDialog() == true)
            {

            }
        }
    }
}
