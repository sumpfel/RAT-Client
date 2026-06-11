using Lextm.SharpSnmpLib;
using RAT_Logic;
using RAT_WPF.NetworkObject_UI;
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

        //KI start (Claude Opus 4.8, prompt 1): true when this object represents the host PC (live, mostly read-only specs)
        private bool isOwnPc;
        //KI end

        public NetworkObjectSettingsWindow(NetworkObject networkObject_)
        {
            InitializeComponent();
            networkObject = networkObject_;

            isOwnPc = networkObject.Type == NetworkObjectType.PC;

            //KI start (Claude Opus 4.8, prompt 1/11): own PC gets owned by the logged-in user if it has no owner yet
            if (isOwnPc && Session.CurrentUser != null
                && !networkObject.AccessRights.Any(a => a.Rights == AccesRights.Owner))
            {
                networkObject.ApplyRight(Session.CurrentUser, AccesRights.Owner);
            }
            //KI end

            LoadOverview();
            LoadLogins();
            LoadInterfaces();
            LoadMibTree();
            LoadAccessControl();
        }

        //KI start (Claude Opus 4.8, prompt 2): load any logins already stored on the device + wire delete
        private void LoadLogins()
        {
            foreach (Login existing in networkObject.Settings.Logins)
            {
                AddLoginControl(existing);
            }
        }

        private void AddLoginControl(Login loginToAdd)
        {
            LoginControl control = new LoginControl(loginToAdd, networkObject);
            control.Deleted += OnLoginDeleted;
            LoginsStackPanel.Children.Add(control);
        }

        private void OnLoginDeleted(LoginControl control)
        {
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

        private void SaveOverview_Click(object sender, RoutedEventArgs e)
        {
            networkObject.Name = NameBox.Text;
            if (!isOwnPc)
            {
                networkObject.Os = OsBox.Text;
                networkObject.Cpu = CpuBox.Text;
                networkObject.Gpu = GpuBox.Text;
                networkObject.Ram = RamBox.Text;
            }
            networkObject.Specs = SpecsBox.Text;
            Title = $"Device Settings — {networkObject.Name}";
            MessageBox.Show("Saved (in software).");
        }

        private void LoadInterfaces()
        {
            InterfacesStackPanel.Children.Clear();
            if (isOwnPc)
            {
                //KI start (Claude Opus 4.8, prompt 2): show real host NICs only (Ethernet/WiFi/USB-Ethernet) with the
                // interface control + (i) details; you can't invent interfaces on your own PC, so hide the add button.
                NewInterfaceButton.Visibility = Visibility.Collapsed;
                InterfacesHint.Text = "Physical interfaces detected on this PC (Ethernet, Wi-Fi, USB-Ethernet). Click (i) for details.";

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
        }

        private void OnInterfaceEdit(InterfaceControl control)
        {
            if (control.ModelInterface == null) { return; }
            UpdateInterfaceWindow window = new UpdateInterfaceWindow(control.ModelInterface);
            if (window.ShowDialog() == true)
            {
                control.Refresh();
            }
        }

        private void OnInterfaceDelete(InterfaceControl control)
        {
            if (control.ModelInterface == null) { return; }
            networkObject.NetworkInterfaces.Remove(control.ModelInterface);
            LoadInterfaces();
        }
        //KI end

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateLoginWindow updateLoginWindow = new UpdateLoginWindow(null);
            if (updateLoginWindow.ShowDialog() == true)
            {
                networkObject.Settings.AddLogin(updateLoginWindow.login);
                //KI start (Claude Opus 4.8, prompt 2): wire new control through the delete-aware helper
                AddLoginControl(updateLoginWindow.login);
                //KI end
            }

        }

        //KI start (Claude Opus 4.8, prompt 2): shared "no connection" popup so every action fails clearly
        private static void ShowNoConnection(string protocol)
        {
            MessageBox.Show(
                $"There is no open {protocol} connection to this device.\n\n" +
                "Open a connection first from the Logins tab (add a login and press Connect).",
                "No connection", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private static void ShowActionFailed(string action, Exception ex)
        {
            MessageBox.Show($"{action} failed:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            //KI start (Claude Opus 4.8, prompt 7): add a new modelled interface, then rebuild the (editable) list
            UpdateInterfaceWindow window = new UpdateInterfaceWindow();
            if (window.ShowDialog() == true)
            {
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
                MessageBox.Show($"SNMP GET failed:\n{ex.Message}");
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
                    MessageBox.Show("Walk returned no variables.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SNMP WALK failed:\n{ex.Message}");
            }
        }

        private void SnmpSet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                networkObject.SetSnmp(BuildSnmpSettings(), SnmpOid.Text.Trim(), SnmpSetValue.Text, SelectedSnmpVersion());
                MessageBox.Show("SET sent. Use Get to confirm the new value.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SNMP SET failed:\n{ex.Message}");
            }
        }
        //KI end

        //KI start (Claude Opus 4.8, prompt 11): hierarchical per-user access control with privilege rules
        private sealed class AccessRow
        {
            public string UserName { get; set; } = "";
            public string Right { get; set; } = "";
        }

        private void LoadAccessControl()
        {
            AccesRights myRight = networkObject.GetRight(Session.CurrentUser);
            MyRoleLabel.Text = $"Your access on this device: {myRight}";

            // only Admin and Owner can change rights
            bool canManage = myRight >= AccesRights.Admin;
            GrantPanel.IsEnabled = canManage;
            AccessHint.Text = canManage
                ? "You can change other users' access below."
                : "You don't have permission to change access on this device (need Admin or Owner).";

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

        private void GrantPermission_Click(object sender, RoutedEventArgs e)
        {
            if (Session.CurrentUser == null)
            {
                MessageBox.Show("No current user.");
                return;
            }

            string userName = PermUserName.Text.Trim();
            if (string.IsNullOrWhiteSpace(userName))
            {
                MessageBox.Show("Enter the user name to set access for.");
                return;
            }
            if (RightLevel.SelectedItem is not AccesRights newRight)
            {
                MessageBox.Show("Select an access level.");
                return;
            }

            // ID is unknown without a database; derive a stable-ish id from the name for now.
            // TODO: resolve the real user (and ID) from IDatabaseConnection.GetAllUsers().
            NetworkUser target = new NetworkUser(userName, userName.GetHashCode());

            if (!networkObject.SetRight(Session.CurrentUser, target, newRight))
            {
                MessageBox.Show(
                    "You're not allowed to set that access for this user.\n\n" +
                    "Admins can only set Hidden/See/Edit on users below them; only an Owner can change Admins/Owners " +
                    "or grant Admin/Owner.",
                    "Not allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            RefreshPermissionsGrid();
            PermUserName.Clear();
        }
        //KI end
    }
}
