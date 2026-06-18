using RAT_Logic;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RAT_WPF.NetworkObject_UI
{
    /// <summary>
    /// Interaktionslogik für LoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl
    {
        public Login login;
        public RAT_Logic.NetworkObject networkObject;

        //KI start (Claude Opus 4.8, prompt 2): let the parent remove this control on delete
        public event Action<LoginControl>? Deleted;
        //KI end

        //KI start (Claude Opus 4.8, prompt 14): let the parent persist the edited login to the backend
        public event Action<LoginControl>? Edited;
        //KI end

        public LoginControl(Login login_, RAT_Logic.NetworkObject networkObject_)
        {
            InitializeComponent();
            login = login_;
            networkObject = networkObject_;

            //KI start (Claude Opus 4.8, prompt 1): show the real login details instead of static placeholders
            ProtocolText.Text = login.Type.ToString();
            UserText.Text = $"user: {login.Username}";
            PortText.Text = $"port: {login.Port}";
            //KI end

            UpdateStatus();
        }

        //KI start (Claude Opus 4.8, prompt 2): reflect connected / not connected state in the UI
        public void UpdateStatus()
        {
            bool connected = networkObject.IsConnected(login.Type);
            StatusDot.Fill = connected
                ? (Brush)Application.Current.Resources["Brush.Success"]
                : (Brush)Application.Current.Resources["Brush.Danger"];
            StatusText.Text = connected ? "connected" : "not connected";
            ConnectButton.Content = connected ? "Reconnect" : "Connect";
        }
        //KI end

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //KI start (Claude Opus 4.8, prompt 2/24): open the session; Telnet is now supported, and an SSH/SFTP
            // login opens that transport (it serves both). Errors show a popup.
            try
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
                        networkObject.OpenTelnet(login);
                        break;
                }

                UpdateStatus();
                if (networkObject.IsConnected(login.Type))
                {
                    string extra = (login.Type == LoginType.SSH || login.Type == LoginType.SFTP)
                        ? " (this login serves both SSH and SFTP)" : "";
                    RatDialog.Show("Connected", $"{login.Type} connected to {login.Username}.{extra}", "Icon.Connected");
                }
            }
            catch (EntryPointNotFoundException ex)
            {
                // no route / no interface in same network as host
                RatDialog.Show("No connection", ex.Message, "Icon.NoConnection");
            }
            catch (Exception ex)
            {
                RatDialog.Show("Connection failed", ex.Message, "Icon.ConnectionLost");
            }
            //KI end
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            UpdateLoginWindow updateLoginWindow = new UpdateLoginWindow(login);
            if (updateLoginWindow.ShowDialog() == true)
            {
                //KI start (Claude Opus 4.8, prompt 14): keep the db id across the edit and let the parent persist it
                updateLoginWindow.login.ID = login.ID;
                login = updateLoginWindow.login;
                //KI end
                ProtocolText.Text = login.Type.ToString();
                UserText.Text = $"user: {login.Username}";
                PortText.Text = $"port: {login.Port}";
                UpdateStatus();
                //KI start (Claude Opus 4.8, prompt 14): ask the parent to save the change to the backend
                Edited?.Invoke(this);
                //KI end
            }
        }

        //KI start (Claude Opus 4.8, prompt 2): delete this login
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Deleted?.Invoke(this);
        }
        //KI end
    }
}
