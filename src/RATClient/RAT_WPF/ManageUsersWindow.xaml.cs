using RAT_Data;
using RAT_WPF.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace RAT_WPF
{
    //KI start (Claude Opus 4.8, prompt 14): admin-only window to list and create users via IDatabaseConnection.
    /// <summary>
    /// Lists all users (GET /user/) and lets an admin create new ones (POST /user/register).
    /// Opened from the topology top bar; the button there is only shown to admins.
    /// </summary>
    public partial class ManageUsersWindow : Window
    {
        private IDatabaseConnection? Db => DatabaseConnectionStore.Current;

        // small row type so the grid shows friendly columns
        private sealed class UserRow
        {
            public int Id { get; set; }
            public string Username { get; set; } = "";
            public bool IsAdmin { get; set; }
            public bool CanCreate { get; set; }
        }

        public ManageUsersWindow()
        {
            InitializeComponent();
            LoadUsers();
        }

        private async void LoadUsers()
        {
            if (Db == null) { return; }
            try
            {
                List<RAT_Data.User> users = await Db.GetAllUsers();
                UsersGrid.ItemsSource = users
                    .Select(u => new UserRow
                    {
                        Id = u.ID,
                        Username = u.UserName,
                        // Privileges >= 100 is how the connection encodes an admin account
                        IsAdmin = u.Privileges >= 100,
                        CanCreate = u.CanCreate
                    })
                    .OrderBy(u => u.Id)
                    .ToList();
            }
            catch (Exception ex)
            {
                RatDialog.Show("Database hiccup", $"The rat couldn't load the users from the server.\n\n{ex.Message}", "Icon.DatabaseError");
            }
        }

        private async void CreateUser_Click(object sender, RoutedEventArgs e)
        {
            if (Db == null)
            {
                RatDialog.Show("Manage Users", "Not connected to a server.", "Icon.NoConnection");
                return;
            }

            string username = NewUserName.Text.Trim();
            string password = NewUserPassword.Password;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                RatDialog.Show("Manage Users", "Enter a username and a password.", "Icon.LoginFailed");
                return;
            }

            //KI start (Claude Opus 4.8, prompt 22): enforce the password policy before hitting the backend
            if (!RAT_Logic.PasswordPolicy.Validate(password, out string pwError))
            {
                RatDialog.Show("Weak password", pwError, "Icon.LoginFailed");
                return;
            }
            //KI end

            // Privileges >= 100 marks an admin for AddUser(); CanCreate is the create-devices flag.
            RAT_Data.User newUser = new RAT_Data.User(
                username, password, 0,
                NewUserIsAdmin.IsChecked == true ? 100 : 10,
                NewUserCanCreate.IsChecked == true);

            try
            {
                await Db.AddUser(newUser);
            }
            catch (Exception ex)
            {
                RatDialog.Show("Database hiccup", $"The rat couldn't create the user on the server.\n\n{ex.Message}", "Icon.DatabaseError");
                return;
            }

            // reset the form and refresh the list
            NewUserName.Clear();
            NewUserPassword.Clear();
            NewUserIsAdmin.IsChecked = false;
            NewUserCanCreate.IsChecked = false;
            LoadUsers();
        }

        //KI start (Claude Opus 4.8, prompt 16): edit a user (admin -> all fields allowed). Refresh on save.
        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement fe || fe.Tag is not UserRow row) { return; }

            EditUserWindow dialog = new EditUserWindow(
                row.Id, row.Username, row.IsAdmin, row.CanCreate, allowAdminFields: true)
            {
                Owner = this
            };
            if (dialog.ShowDialog() == true)
            {
                LoadUsers();
            }
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 25): delete a user (admin only). Confirm first; the backend refuses
        // deleting your own account. Refresh the list afterwards.
        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement fe || fe.Tag is not UserRow row) { return; }
            if (Db == null)
            {
                RatDialog.Show("Manage Users", "Not connected to a server.", "Icon.NoConnection");
                return;
            }
            if (RAT_Logic.Session.CurrentUser != null && RAT_Logic.Session.CurrentUser.ID == row.Id)
            {
                RatDialog.Show("Manage Users", "You can't delete your own account.", "Icon.LoginFailed");
                return;
            }

            MessageBoxResult confirm = MessageBox.Show(
                $"Delete user \"{row.Username}\"? This also removes their permissions and saved logins. This cannot be undone.",
                "Delete user", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) { return; }

            try
            {
                await Db.DeleteUser(new RAT_Data.User(row.Username, "", row.Id, row.IsAdmin ? 100 : 10, row.CanCreate));
            }
            catch (Exception ex)
            {
                RatDialog.Show("Database hiccup", $"The rat couldn't delete the user on the server.\n\n{ex.Message}", "Icon.DatabaseError");
                return;
            }
            LoadUsers();
        }
        //KI end

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
    //KI end
}
