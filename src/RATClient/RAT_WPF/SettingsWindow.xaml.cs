using RAT_Logic;
using System.Windows;

namespace RAT_WPF
{
    //KI start (Claude Opus 4.8, prompt 2): settings window (theme switcher). DataContext is set in XAML (SettingsViewModel).
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            //KI start (Claude Opus 4.8, prompt 16): show who is logged in for the account section
            NetworkUser? me = Session.CurrentUser;
            AccountLabel.Text = me != null
                ? $"Signed in as {me.UserName}. You can change your username and password here."
                : "Not signed in.";
            //KI end
        }

        //KI start (Claude Opus 4.8, prompt 16): edit my own account. Admins may also change their admin/can-create
        // flags; normal users only see username + password (allowAdminFields follows the global-admin privilege).
        private void EditMyAccount_Click(object sender, RoutedEventArgs e)
        {
            NetworkUser? me = Session.CurrentUser;
            if (me == null)
            {
                MessageBox.Show("Not signed in.", "Account", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isAdmin = me.Privileges >= 100;
            EditUserWindow dialog = new EditUserWindow(
                me.ID, me.UserName, isAdmin, me.CanCreate, allowAdminFields: isAdmin)
            {
                Owner = this
            };
            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                // keep the in-memory session in sync with the change
                Session.CurrentUser = new NetworkUser(
                    dialog.Result.UserName, dialog.Result.ID,
                    canCreate: dialog.Result.CanCreate, privileges: dialog.Result.Privileges);
                AccountLabel.Text = $"Signed in as {dialog.Result.UserName}. You can change your username and password here.";
            }
        }
        //KI end

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
    //KI end
}
