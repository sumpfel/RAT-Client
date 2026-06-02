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
using System.Windows.Shapes;

namespace RAT_WPF
{
    /// <summary>
    /// Interaktionslogik für EnterPasswordWindow.xaml
    /// </summary>
    public partial class EnterPasswordWindow : Window
    {
        private string password_required;
        public EnterPasswordWindow(string password_)
        {
            InitializeComponent();
            password_required = password_;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (password.Password == password_required)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Wrong!");
            }
            

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
