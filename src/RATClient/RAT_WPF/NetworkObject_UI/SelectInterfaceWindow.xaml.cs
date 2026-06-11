using RAT_Logic;
using System.Windows;

namespace RAT_WPF.NetworkObject_UI
{
    //KI start (Claude Opus 4.8, prompt 9): window to select an interface of a device.
    // For now it only returns the chosen interface (SelectedInterface) — nothing is done with it yet.
    public partial class SelectInterfaceWindow : Window
    {
        /// <summary>The interface the user picked, or null if cancelled.</summary>
        public NetworkObjectInterface? SelectedInterface { get; private set; }

        public SelectInterfaceWindow(NetworkObject networkObject)
        {
            InitializeComponent();

            SubtitleText.Text = $"Choose an interface of \"{networkObject.Name}\".";
            InterfaceList.ItemsSource = networkObject.NetworkInterfaces;

            if (networkObject.NetworkInterfaces.Count > 0)
            {
                InterfaceList.SelectedIndex = 0;
            }
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            Confirm();
        }

        private void InterfaceList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (InterfaceList.SelectedItem is NetworkObjectInterface)
            {
                Confirm();
            }
        }

        private void Confirm()
        {
            if (InterfaceList.SelectedItem is not NetworkObjectInterface chosen)
            {
                MessageBox.Show("Please select an interface.", "No selection",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
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
