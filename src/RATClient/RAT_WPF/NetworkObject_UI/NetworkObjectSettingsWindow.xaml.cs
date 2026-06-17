using Lextm.SharpSnmpLib;
using RAT_Data;
using RAT_Logic;
using RAT_WPF.NetworkObject_UI;
using RAT_WPF.Stores;
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

namespace RAT_WPF.NetworkObject_UI
{
    /// <summary>
    /// Interaktionslogik für NetworkObjectSettingsWindow.xaml
    /// </summary>
    public partial class NetworkObjectSettingsWindow : Window
    {
        NetworkObject networkObject;

        //KI start (Claude Opus 4.8, prompt 1/14): true only when this object is THIS machine AND the current user
        // owns it. A PC owned by someone else (or not matching this host's name) is treated as a normal device:
        // the stored DB fields are shown, never this machine's live specs/NICs. See ComputeIsOwnPc().
        private bool isOwnPc;
        //KI end

        //KI start (Claude Opus 4.8, prompt 14): shortcut to the active backend connection (may be null in dev).
        private IDatabaseConnection? Db => DatabaseConnectionStore.Current;
        //KI end

        public event EventHandler NetworkObjectViewNeedsUpdate;

        public NetworkObjectSettingsWindow(NetworkObject networkObject_)
        {
            InitializeComponent();
            networkObject = networkObject_;

            isOwnPc = ComputeIsOwnPc();

            LoadOverview();
            LoadLogins();
            LoadInterfaces();
            LoadMibTree();
            LoadAccessControl();
        }

        //KI start (Claude Opus 4.8, prompt 14): "own PC" = a PC device that represents this machine and that the
        // current user owns. Only then do we show live host specs/interfaces; otherwise it's a normal stored device.
        private bool ComputeIsOwnPc()
        {
            if (networkObject.Type != NetworkObjectType.PC) { return false; }
            if (Session.CurrentUser == null) { return false; }
            bool isOwner = networkObject.GetRight(Session.CurrentUser) == AccesRights.Owner;
            bool isThisMachine = string.Equals(networkObject.Name, Environment.MachineName, StringComparison.OrdinalIgnoreCase)
                                 || string.IsNullOrWhiteSpace(networkObject.Name);
            return isOwner && isThisMachine;
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 2/14): load the logins for this device. If a backend connection is
        // available they are read from the DB (per-user logins); otherwise fall back to whatever is in memory.
        private async void LoadLogins()
        {
            networkObject.Settings.Logins.Clear();
            LoginsStackPanel.Children.Clear();

            if (Db != null && networkObject.ID > 0)
            {
                try
                {
                    List<Login> logins = await Db.GetUserDeviceLogin(networkObject);
                    foreach (Login login in logins)
                    {
                        networkObject.Settings.Logins.Add(login);
                    }
                }
                catch (Exception ex) { ShowDbError("load the logins", ex); }
            }

            foreach (Login existing in networkObject.Settings.Logins)
            {
                AddLoginControl(existing);
            }
        }

        private void AddLoginControl(Login loginToAdd)
        {
            LoginControl control = new LoginControl(loginToAdd, networkObject);
            control.Deleted += OnLoginDeleted;
            control.Edited += OnLoginEdited; // KI (prompt 14): persist edits
            LoginsStackPanel.Children.Add(control);
        }

        //KI start (Claude Opus 4.8, prompt 14): persist an edited login to the backend.
        private async void OnLoginEdited(LoginControl control)
        {
            if (Db != null && control.login.ID > 0)
            {
                try { await Db.EditUserDeviceLogin(control.login); }
                catch (Exception ex) { ShowDbError("save the login", ex); }
            }
        }
        //KI end

        private async void OnLoginDeleted(LoginControl control)
        {
            //KI start (Claude Opus 4.8, prompt 14): delete the login on the backend before dropping it locally.
            if (Db != null && control.login.ID > 0)
            {
                try { await Db.DeletetUserDeviceLogin(control.login); }
                catch (Exception ex) { ShowDbError("delete the login", ex); return; }
            }
            //KI end
            networkObject.Settings.Logins.Remove(control.login);
            LoginsStackPanel.Children.Remove(control);
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 1): editable overview. For the host PC live specs are shown and only the
        // display name is editable; for every other device the name and all specs are editable in-software only.
        private void LoadOverview()
        {
            if (isOwnPc)
            {
                Dictionary<string, string> stats = NetworkObject.GetOwnDeviceInfos();
                OverviewSubtitle.Text = "This PC — specs are read live from the host. Only the name is editable.";

                NameBox.Text = string.IsNullOrWhiteSpace(networkObject.Name) ? stats["name"] : networkObject.Name;
                OsBox.Text = stats["os"];
                CpuBox.Text = stats["cpu"];
                GpuBox.Text = stats["gpu"];
                RamBox.Text = stats["ram"] + " GB";

                // live specs are read-only
                OsBox.IsReadOnly = CpuBox.IsReadOnly = GpuBox.IsReadOnly = RamBox.IsReadOnly = true;
                SpecsBox.Text = networkObject.Specs;
                SpecsHint.Text = "Note: changes to specs are not applied here for your own PC — they are read from the system.";
            }
            else
            {
                //KI start (Claude Opus 4.8, prompt 17): a PC that isn't the live "own PC" (e.g. renamed, or a 2nd PC)
                // still gets sensible default specs so the fields are never blank. Only fills empties — keeps edits.
                if (networkObject.Type == NetworkObjectType.PC)
                {
                    networkObject.ApplyDefaultPcSpecsIfEmpty();
                }
                //KI end
                OverviewSubtitle.Text = $"{networkObject.Type} — changes are stored in the software only and are NOT pushed to the device.";
                NameBox.Text = networkObject.Name;
                OsBox.Text = networkObject.Os;
                CpuBox.Text = networkObject.Cpu;
                GpuBox.Text = networkObject.Gpu;
                RamBox.Text = networkObject.Ram;
                SpecsBox.Text = networkObject.Specs;
                SpecsHint.Text = "These fields describe the device inside this app only; nothing is written to the real hardware.";
            }
        }

        private async void SaveOverview_Click(object sender, RoutedEventArgs e)
        {
            networkObject.Name = NameBox.Text;
            //KI start (Claude Opus 4.8, prompt 17): always persist the spec values shown in the boxes (for the live
            // own-PC these are the read-only host specs) so they're stored on the object and survive a later rename
            // that flips it out of the live "own PC" view — otherwise the stats would go blank.
            networkObject.Os = OsBox.Text;
            networkObject.Cpu = CpuBox.Text;
            networkObject.Gpu = GpuBox.Text;
            networkObject.Ram = RamBox.Text;
            //KI end
            networkObject.Specs = SpecsBox.Text;
            Title = $"Device Settings — {networkObject.Name}";

            //KI start (Claude Opus 4.8, prompt 14): persist the overview to the backend.
            // Edit if it already has a db id, otherwise create it (and own it).
            if (Db != null)
            {
                try
                {
                    if (networkObject.ID > 0)
                    {
                        await Db.EditNetworkObject(networkObject);
                    }
                    else
                    {
                        await Db.AddNetworkObject(networkObject);
                    }
                }
                catch (Exception ex) { ShowDbError("save the device", ex); }
            }
            //KI end

            NetworkObjectViewNeedsUpdate?.Invoke(this, EventArgs.Empty);
        }

        private void LoadInterfaces()
        {
            InterfacesStackPanel.Children.Clear();
            if (isOwnPc)
            {
                //KI start (Claude Opus 4.8, prompt 2/12): show real host NICs only (Ethernet/WiFi/USB-Ethernet) with the
                // interface control + (i) details; you can't invent interfaces on your own PC, so hide the add button.
                // Also make sure the PC's model interface list is populated (so they can be selected elsewhere).
                NewInterfaceButton.Visibility = Visibility.Collapsed;
                InterfacesHint.Text = "Physical interfaces detected on this PC (Ethernet, Wi-Fi, USB-Ethernet). Click (i) for details.";

                if (networkObject.NetworkInterfaces.Count == 0)
                {
                    networkObject.PopulateOwnDeviceInterfaces();
                }

                foreach (HostInterfaceInfo info in NetworkObject.GetOwnDeviceInterfacesDetailed())
                {
                    InterfacesStackPanel.Children.Add(new InterfaceControl(info));
                }

                if (InterfacesStackPanel.Children.Count == 0)
                {
                    InterfacesStackPanel.Children.Add(new TextBlock
                    {
                        Text = "No physical network interfaces found.",
                        Foreground = (System.Windows.Media.Brush)Application.Current.Resources["Brush.TextMuted"]
                    });
                }
                //KI end
            }
            else
            {
                //KI start (Claude Opus 4.8, prompt 7): software interfaces are editable — control with edit/delete/toggle
                NewInterfaceButton.Visibility = Visibility.Visible;
                InterfacesHint.Text = "Interfaces stored for this device (software model). Add, edit, toggle up/down or delete.";
                foreach (NetworkObjectInterface networkObjectInterface in networkObject.NetworkInterfaces)
                {
                    AddInterfaceControl(networkObjectInterface);
                }

                if (networkObject.NetworkInterfaces.Count == 0)
                {
                    InterfacesStackPanel.Children.Add(new TextBlock
                    {
                        Text = "No interfaces yet — add one above.",
                        Foreground = (System.Windows.Media.Brush)Application.Current.Resources["Brush.TextMuted"],
                        Margin = new Thickness(0, 6, 0, 0)
                    });
                }
                //KI end
            }
        }

        //KI start (Claude Opus 4.8, prompt 7): build an editable interface control wired to edit/delete
        private void AddInterfaceControl(NetworkObjectInterface networkObjectInterface)
        {
            InterfaceControl control = new InterfaceControl(networkObjectInterface);
            control.EditRequested += OnInterfaceEdit;
            control.DeleteRequested += OnInterfaceDelete;
            InterfacesStackPanel.Children.Add(control);
            // TODO: Save to Database
        }

        private async void OnInterfaceEdit(InterfaceControl control)
        {
            if (control.ModelInterface == null) { return; }
            UpdateInterfaceWindow window = new UpdateInterfaceWindow(control.ModelInterface);
            if (window.ShowDialog() == true)
            {
                control.Refresh();
                //KI start (Claude Opus 4.8, prompt 14): persist the edited interface to the backend
                if (Db != null && control.ModelInterface.ID > 0)
                {
                    try { await Db.EditInterface(control.ModelInterface); }
                    catch (Exception ex) { ShowDbError("save the interface", ex); }
                }
                //KI end
            }
        }

        private async void OnInterfaceDelete(InterfaceControl control)
        {
            if (control.ModelInterface == null) { return; }
            //KI start (Claude Opus 4.8, prompt 14): delete the interface on the backend before removing it locally
            if (Db != null && control.ModelInterface.ID > 0)
            {
                try { await Db.DeleteInterface(control.ModelInterface); }
                catch (Exception ex) { ShowDbError("delete the interface", ex); return; }
            }
            //KI end
            networkObject.NetworkInterfaces.Remove(control.ModelInterface);
            LoadInterfaces();
        }
        //KI end

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateLoginWindow updateLoginWindow = new UpdateLoginWindow(null);
            if (updateLoginWindow.ShowDialog() == true)
            {
                //KI start (Claude Opus 4.8, prompt 14): persist the new login to the backend (it is stored against
                // the current user's permission row for this device). Only add it to the UI once it saved.
                if (Db != null && networkObject.ID > 0)
                {
                    try { await Db.AddUserDeviceLogin(updateLoginWindow.login, networkObject); }
                    catch (Exception ex) { ShowDbError("add the login", ex); return; }
                }
                //KI end
                networkObject.Settings.Logins.Add(updateLoginWindow.login);
                //KI start (Claude Opus 4.8, prompt 2): wire new control through the delete-aware helper
                AddLoginControl(updateLoginWindow.login);
                //KI end
            }

        }

        //KI start (Claude Opus 4.8, prompt 2/17): shared rat-themed "no connection" / "action failed" popups.
        private static void ShowNoConnection(string protocol)
        {
            RatDialog.Show(
                "No connection",
                $"There is no open {protocol} connection to this device.\n\n" +
                "Open a connection first from the Logins tab (add a login and press Connect).",
                "Icon.NoConnection");
        }

        private static void ShowActionFailed(string action, Exception ex)
        {
            RatDialog.Show($"{action} failed", ex.Message, "Icon.ConnectionLost");
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 14/17): one consistent, rat-themed popup for backend errors.
        private static void ShowDbError(string action, Exception ex)
        {
            RatDialog.Show(
                "Database hiccup",
                $"The rat couldn't {action} on the server.\n\n{ex.Message}",
                "Icon.DatabaseError");
        }
        //KI end

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!networkObject.IsSftpConnected) { ShowNoConnection("SFTP"); return; }
            try { networkObject.DownloadSFTP(SftpLocalPath.Text, SftpRemotePath.Text); }
            catch (Exception ex) { ShowActionFailed("SFTP download", ex); }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (!networkObject.IsSftpConnected) { ShowNoConnection("SFTP"); return; }
            try { networkObject.UploadSFTP(SftpLocalPath.Text, SftpRemotePath.Text); }
            catch (Exception ex) { ShowActionFailed("SFTP upload", ex); }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (!networkObject.IsSftpConnected) { ShowNoConnection("SFTP"); return; }
            try
            {
                SftpDirList.Text = "";
                List<string> list = networkObject.ListDirSFTP(SftpRemotePath.Text);
                foreach (string s in list)
                {
                    SftpDirList.Text += s + ", ";
                }
            }
            catch (Exception ex) { ShowActionFailed("SFTP list", ex); }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (!networkObject.IsScpConnected) { ShowNoConnection("SCP"); return; }
            try { networkObject.DownloadSCP(ScpLocalPath.Text, ScpRemotePath.Text); }
            catch (Exception ex) { ShowActionFailed("SCP download", ex); }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (!networkObject.IsScpConnected) { ShowNoConnection("SCP"); return; }
            try { networkObject.UploadSCP(ScpLocalPath.Text, ScpRemotePath.Text); }
            catch (Exception ex) { ShowActionFailed("SCP upload", ex); }
        }

        private async void Button_Click_6(object sender, RoutedEventArgs e)
        {
            //KI start (Claude Opus 4.8, prompt 2): pop up "no connection" instead of a generic message
            if (!networkObject.IsSshConnected) { ShowNoConnection("SSH"); return; }
            try
            {
                string result = await networkObject.ExecuteSSH(sshInputBox.Text);
                sshOutputBlock.Text = result;
            }
            catch (Exception ex) { ShowActionFailed("SSH command", ex); }
            //KI end
        }

        private async void Button_Click_7(object sender, RoutedEventArgs e)
        {
            //KI start (Claude Opus 4.8, prompt 7/14): add a new modelled interface, persist it, then rebuild the list
            UpdateInterfaceWindow window = new UpdateInterfaceWindow();
            if (window.ShowDialog() == true)
            {
                if (Db != null && networkObject.ID > 0)
                {
                    try { await Db.AddInterface(window.networkObjectInterface, networkObject); }
                    catch (Exception ex) { ShowDbError("add the interface", ex); return; }
                }
                networkObject.NetworkInterfaces.Add(window.networkObjectInterface);
                LoadInterfaces();
            }
            //KI end
        }

        private void SshShellsTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SshShellsTabControl.SelectedIndex != SshShellsTabControl.Items.Count - 1)
            {
                return;
            }

            //KI start (Claude Opus 4.8, prompt 2): don't try to open a shell stream with no SSH connection
            if (!networkObject.IsSshConnected)
            {
                ShowNoConnection("SSH");
                SshShellsTabControl.SelectedIndex = 0; // bounce back off the "+" tab
                return;
            }
            //KI end

            //KI
            // open new shell stream
            int shellId = networkObject.OpenSSHstream();

            // create terminal control
            sshTermainalControl terminal = new sshTermainalControl(
                networkObject,
                shellId);

            // create tab
            TabItem tab = new TabItem()
            {
                Header = $"ssh-{shellId}",
                Content = terminal
            };
            SshShellsTabControl.SelectedIndex = 0; //das es koa zwo tes mol des event triggered wegs selected tab changed (also es goht halt beim if hops)
            // insert BEFORE the + tab
            SshShellsTabControl.Items.Insert(
                SshShellsTabControl.Items.Count - 1,
                tab);

            // select new tab
            SshShellsTabControl.SelectedItem = tab;
            //KI END
        }

        //KI start (Claude Opus 4.8, prompt 1): MIB browser (built-in common OIDs + raw Get/Walk/Set)
        private sealed class SnmpRow
        {
            public string Oid { get; set; } = "";
            public string Value { get; set; } = "";
        }

        private void LoadMibTree()
        {
            foreach (MibNode node in MibCatalog.CommonNodes)
            {
                MibTree.Items.Add(new ListBoxItem
                {
                    Content = node.Name,
                    Tag = node,
                    ToolTip = $"{node.Oid}\n{node.Description}",
                    Foreground = (Brush)Application.Current.Resources["Brush.Text"]
                });
            }
        }

        private void MibTree_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MibTree.SelectedItem is ListBoxItem item && item.Tag is MibNode node)
            {
                SnmpOid.Text = node.Oid;
            }
        }

        private SnmpSettings BuildSnmpSettings()
        {
            int port = 161;
            int.TryParse(SnmpPort.Text, out port);
            if (port == 0) { port = 161; }
            return new SnmpSettings(
                string.IsNullOrWhiteSpace(SnmpReadCommunity.Text) ? "public" : SnmpReadCommunity.Text,
                string.IsNullOrWhiteSpace(SnmpWriteCommunity.Text) ? "private" : SnmpWriteCommunity.Text,
                port,
                networkObject.Settings.Snmp?.ID ?? 0);
        }

        private VersionCode SelectedSnmpVersion()
            => SnmpVersion.SelectedIndex == 1 ? VersionCode.V2 : VersionCode.V1;

        private void ShowSnmpResults(IEnumerable<Variable> variables)
        {
            List<SnmpRow> rows = variables
                .Select(v => new SnmpRow { Oid = v.Id.ToString(), Value = v.Data.ToString() })
                .ToList();
            SnmpResults.ItemsSource = rows;
        }

        private void SnmpGet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = networkObject.GetSnmp(BuildSnmpSettings(), SnmpOid.Text.Trim(), SelectedSnmpVersion());
                ShowSnmpResults(result);
            }
            catch (Exception ex)
            {
                RatDialog.Show("SNMP GET failed", ex.Message, "Icon.ConnectionLost");
            }
        }

        private void SnmpWalk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = networkObject.WalkSnmp(BuildSnmpSettings(), SnmpOid.Text.Trim(), SelectedSnmpVersion());
                ShowSnmpResults(result);
                if (result.Count == 0)
                {
                    RatDialog.Show("SNMP Walk", "Walk returned no variables.", "Icon.NoConnection");
                }
            }
            catch (Exception ex)
            {
                RatDialog.Show("SNMP WALK failed", ex.Message, "Icon.ConnectionLost");
            }
        }

        private void SnmpSet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                networkObject.SetSnmp(BuildSnmpSettings(), SnmpOid.Text.Trim(), SnmpSetValue.Text, SelectedSnmpVersion());
                RatDialog.Show("SNMP", "SET sent. Use Get to confirm the new value.", "Icon.LoginSuccess");
            }
            catch (Exception ex)
            {
                RatDialog.Show("SNMP SET failed", ex.Message, "Icon.ConnectionLost");
            }
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 11/14): hierarchical per-user access control, now backed by the database.
        // Users come from IDatabaseConnection.GetAllUsers() (real IDs!), permissions are read/written via the
        // permission endpoints, and the local client-side rules (SetRight) still gate what the UI lets you try.
        private sealed class AccessRow
        {
            public string UserName { get; set; } = "";
            public string Right { get; set; } = "";
        }

        private List<NetworkUser> _allUsers = new List<NetworkUser>();

        private async void LoadAccessControl()
        {
            // refresh the access rights from the backend so the grid is accurate
            if (Db != null && networkObject.ID > 0)
            {
                try
                {
                    networkObject.AccessRights = await Db.GetNetworkObjectPermissions(networkObject);
                    // map RAT_Data.User -> logic-layer NetworkUser (what the rights logic + UI use)
                    _allUsers = (await Db.GetAllUsers())
                        .Select(u => new NetworkUser(u.UserName, u.ID, canCreate: u.CanCreate, privileges: u.Privileges))
                        .ToList();
                }
                catch (Exception ex) { ShowDbError("load the access rights", ex); }
            }

            AccesRights myRight = networkObject.GetRight(Session.CurrentUser);
            MyRoleLabel.Text = $"Your access on this device: {myRight}";

            // only Admin and Owner can change rights
            bool canManage = myRight >= AccesRights.Admin;
            GrantPanel.IsEnabled = canManage;
            AccessHint.Text = canManage
                ? "You can change other users' access below."
                : "You don't have permission to change access on this device (need Admin or Owner).";

            // user dropdown (everyone except yourself — you can't change your own rights here)
            PermUserCombo.ItemsSource = _allUsers
                .Where(u => u.ID != Session.CurrentUser?.ID)
                .OrderBy(u => u.UserName)
                .ToList();
            // KI (prompt 16): names are rendered by the ComboBox's ItemTemplate (Brush.Text) so they stay readable
            if (PermUserCombo.Items.Count > 0) { PermUserCombo.SelectedIndex = 0; }

            // which levels the current user may assign
            RightLevel.Items.Clear();
            foreach (AccesRights level in Enum.GetValues<AccesRights>())
            {
                // an Admin can only assign up to Edit; an Owner can assign anything
                if (myRight == AccesRights.Admin && level > AccesRights.Edit) { continue; }
                RightLevel.Items.Add(level);
            }
            if (RightLevel.Items.Count > 0) { RightLevel.SelectedIndex = 0; }

            RefreshPermissionsGrid();
        }

        private void RefreshPermissionsGrid()
        {
            PermissionsGrid.ItemsSource = networkObject.AccessRights
                .OrderByDescending(a => a.Rights)
                .Select(a => new AccessRow { UserName = a.User.ToString(), Right = a.Rights.ToString() })
                .ToList();
        }

        private async void GrantPermission_Click(object sender, RoutedEventArgs e)
        {
            if (Session.CurrentUser == null)
            {
                RatDialog.Show("Access Control", "No current user.", "Icon.LoginFailed");
                return;
            }
            if (PermUserCombo.SelectedItem is not NetworkUser target)
            {
                RatDialog.Show("Access Control", "Select a user to set access for.", "Icon.LoginFailed");
                return;
            }
            if (RightLevel.SelectedItem is not AccesRights newRight)
            {
                RatDialog.Show("Access Control", "Select an access level.", "Icon.LoginFailed");
                return;
            }

            // client-side rule check first (mirrors the backend's rules; gives an instant, clear message)
            if (!networkObject.CanChangeRight(Session.CurrentUser, target, newRight))
            {
                RatDialog.Show("Not allowed",
                    "You're not allowed to set that access for this user.\n\n" +
                    "Admins can only set Hidden/See/Edit on users below them; only an Owner can change Admins/Owners " +
                    "or grant Admin/Owner.",
                    "Icon.LoginFailed");
                return;
            }

            //KI start (Claude Opus 4.8, prompt 14): persist the new right to the backend, then mirror it locally.
            if (Db != null && networkObject.ID > 0)
            {
                try { await Db.SetPermission(networkObject, target, newRight); }
                catch (Exception ex) { ShowDbError("set the access right", ex); return; }
            }
            //KI end

            networkObject.ApplyRight(target, newRight);
            RefreshPermissionsGrid();
        }
        //KI end
    }
}
