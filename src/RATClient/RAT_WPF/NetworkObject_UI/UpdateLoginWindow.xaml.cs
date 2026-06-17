using RAT_Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RAT_WPF.NetworkObject_UI
{
    /// <summary>
    /// Interaktionslogik für UpdateLoginWindow.xaml
    /// </summary>
    public partial class UpdateLoginWindow : Window
    {
        public Login login;
        public UpdateLoginWindow(RAT_Logic.Login? login_)
        {
            
            InitializeComponent();
            if (login_ != null)
            {
                login = login_;
                username.Text = login.Username;
                port.Text = login.Port.ToString();
                password.Password = login.Password;
                protocoll.Text = login.Type.ToString();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                login = new Login(username.Text, password.Password, Convert.ToInt32(port.Text), Enum.Parse<RAT_Logic.LoginType>(protocoll.Text, ignoreCase: true));

                DialogResult = true;
                Close();
            }
            catch
            {
                RatDialog.Show("Check your input", "Please check your input.", "Icon.LoginFailed");
            }
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
