using RAT_Logic;
using System.Windows;
using System.Windows.Controls;

namespace RAT_WPF.NetworkObject_UI
{
    //KI start (Claude Opus 4.8, prompt 9/12): window to select an interface of a device.
    // Rows reuse the read-only InterfaceControl so the list matches the rest of the app.
    // For now it only returns the chosen interface (SelectedInterface) — nothing is done with it yet.
    public partial class SelectInterfaceWindow : Window
    {
        /// <summary>The interface the user picked, or null if cancelled.</summary>
        public NetworkObjectInterface? SelectedInterface { get; private set; }

        public SelectInterfaceWindow(NetworkObject networkObject)
        {
            InitializeComponent();

            SubtitleText.Text = $"Choose an interface of \"{networkObject.Name}\".";

            foreach (NetworkObjectInterface iface in networkObject.NetworkInterfaces)
            {
                // Only display interfaces where there is no connectiond
                if (iface.Connection == null) 
                {
                    InterfaceList.Items.Add(new ListBoxItem
                    {
                        Content = new InterfaceControl(iface, readOnly: true),
                        Tag = iface
                    });
                }
            }

            if (InterfaceList.Items.Count > 0)
            {
                InterfaceList.SelectedIndex = 0;
            }
            else
            {
                EmptyText.Visibility = Visibility.Visible;
            }
        }

        private void Select_Click(object sender, RoutedEventArgs e) => Confirm();

        private void InterfaceList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (InterfaceList.SelectedItem is ListBoxItem { Tag: NetworkObjectInterface })
            {
                Confirm();
            }
        }

        private void Confirm()
        {
            if (InterfaceList.SelectedItem is not ListBoxItem { Tag: NetworkObjectInterface chosen })
            {
                RatDialog.Show("No selection", "Please select an interface.", "Icon.Ethernet");
                return;
            }

            SelectedInterface = chosen;
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
