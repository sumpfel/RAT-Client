using RAT_Data;
using RAT_WPF.Stores;
using System;
using System.Windows;

namespace RAT_WPF
{
    //KI start (Claude Opus 4.8, prompt 16): reusable edit-user dialog. Used by the admin (Manage Users, can edit
    // anyone + all fields) and by a normal user editing only themselves (admin fields hidden). It persists the
    // change via IDatabaseConnection.EditUser and exposes the resulting user.
    public partial class EditUserWindow : Window
    {
        private IDatabaseConnection? Db => DatabaseConnectionStore.Current;

        private readonly int _userId;

        /// <summary>The user as saved (only valid when the dialog returned true).</summary>
        public User? Result { get; private set; }

        /// <param name="allowAdminFields">true if the caller may change Admin / Can create (i.e. is a global admin).</param>
        public EditUserWindow(int userId, string username, bool isAdmin, bool canCreate, bool allowAdminFields)
        {
            InitializeComponent();
            _userId = userId;

            UserNameBox.Text = username;
            IsAdminBox.IsChecked = isAdmin;
            CanCreateBox.IsChecked = canCreate;

            // a normal user editing themselves can't touch the admin/can-create flags
            if (!allowAdminFields)
            {
                AdminFieldsPanel.Visibility = Visibility.Collapsed;
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (Db == null)
            {
                RatDialog.Show("Edit user", "Not connected to a server.", "Icon.NoConnection");
                return;
            }

            string username = UserNameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(username))
            {
                RatDialog.Show("Edit user", "Enter a username.", "Icon.LoginFailed");
                return;
            }

            //KI start (Claude Opus 4.8, prompt 22): a non-empty new password must satisfy the policy
            // (empty means "leave unchanged", so it's only checked when one was typed).
            if (!string.IsNullOrEmpty(PasswordBox.Password)
                && !RAT_Logic.PasswordPolicy.Validate(PasswordBox.Password, out string pwError))
            {
                RatDialog.Show("Weak password", pwError, "Icon.LoginFailed");
                return;
            }
            //KI end

            // empty password => keep the current one (EditUser treats "" as unchanged);
            // Privileges >= 100 encodes admin for the connection layer.
            User edited = new User(
                username,
                PasswordBox.Password,
                _userId,
                IsAdminBox.IsChecked == true ? 100 : 10,
                CanCreateBox.IsChecked == true);

            try
            {
                await Db.EditUser(edited);
            }
            catch (Exception ex)
            {
                RatDialog.Show("Database hiccup", $"The rat couldn't save the user on the server.\n\n{ex.Message}", "Icon.DatabaseError");
                return;
            }

            Result = edited;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
    //KI end
}
