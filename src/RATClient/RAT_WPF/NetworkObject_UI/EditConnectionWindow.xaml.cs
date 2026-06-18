using RAT_Logic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RAT_WPF.NetworkObject_UI
{
    //KI start (Claude Opus 4.8, prompt 25): edit a cable's name / type / speed (preset dropdown per type) / note.
    public partial class EditConnectionWindow : Window
    {
        private readonly NetworkConnection _connection;

        /// <summary>True only when the user saved the changes.</summary>
        public bool Saved { get; private set; }

        public EditConnectionWindow(NetworkConnection connection)
        {
            InitializeComponent();
            _connection = connection;

            NameBox.Text = connection.Name ?? "";
            NoteBox.Text = connection.Note ?? "";
            TypeCombo.SelectedIndex = connection.Type == NetworkConnectionType.Wireless ? 1 : 0;
            // SelectionChanged on TypeCombo fills the speed presets; then pick the closest to the current value
            SelectClosestSpeed(connection.Speed);
        }

        private NetworkConnectionType SelectedType =>
            TypeCombo.SelectedIndex == 1 ? NetworkConnectionType.Wireless : NetworkConnectionType.Wired;

        private void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SpeedCombo == null) { return; } // fires once during InitializeComponent before SpeedCombo exists
            long current = SpeedCombo.SelectedItem is ConnectionSpeed cs ? cs.Bps : _connection.Speed;
            SpeedCombo.ItemsSource = ConnectionSpeed.For(SelectedType);
            SelectClosestSpeed(current);
        }

        private void SelectClosestSpeed(long bps)
        {
            var presets = ConnectionSpeed.For(SelectedType);
            SpeedCombo.ItemsSource = presets;
            // pick the exact match if present, else the nearest preset
            ConnectionSpeed best = presets.OrderBy(p => System.Math.Abs(p.Bps - bps)).First();
            SpeedCombo.SelectedItem = presets.FirstOrDefault(p => p.Bps == bps) ?? best;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _connection.Name = NameBox.Text.Trim();
            _connection.Note = NoteBox.Text.Trim();
            _connection.Type = SelectedType;
            if (SpeedCombo.SelectedItem is ConnectionSpeed speed)
            {
                _connection.Speed = (int)speed.Bps;
            }
            Saved = true;
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
