using RAT_Data;
using RAT_WPF.Commands;
using RAT_WPF.Stores;
using RAT_WPF.Themes;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace RAT_WPF.ViewModels
{
    //KI start (Claude Opus 4.8, prompt 2/20): MVVM viewmodel for the settings window (theme dropdown + zoom dropdown)
    public class SettingsViewModel : ViewModelBase
    {
        public IEnumerable<AppTheme> Themes { get; } = new[] { AppTheme.Light, AppTheme.Dark };

        private AppTheme _selectedTheme;
        public AppTheme SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                _selectedTheme = value;
                OnPropertyChanged(nameof(SelectedTheme));
                // live-apply so the dropdown previews instantly; also exposed via ApplyThemeCommand
                ThemeManager.Apply(value);
            }
        }

        public ICommand ApplyThemeCommand { get; }

        //KI start (Claude Opus 4.8, prompt 20): UI zoom. The dropdown lists "50%"…"300%"; selecting one scales the
        // whole app immediately (ZoomManager) and best-effort persists it to the backend user settings.
        public IEnumerable<string> ZoomLevels { get; } =
            ZoomManager.Levels.Select(z => z + "%").ToList();

        private string _selectedZoom = ZoomManager.Current + "%";
        public string SelectedZoom
        {
            get => _selectedZoom;
            set
            {
                if (_selectedZoom == value) { return; }
                _selectedZoom = value;
                OnPropertyChanged(nameof(SelectedZoom));

                int percent = ParsePercent(value);
                ZoomManager.Apply(percent);
                _ = PersistZoomAsync(percent);
            }
        }

        private static int ParsePercent(string text)
        {
            int.TryParse(text.Replace("%", "").Trim(), out int p);
            return p == 0 ? ZoomManager.Default : p;
        }

        //KI start (Claude Opus 4.8, prompt 20/22): read-modify-write so saving the zoom keeps the other settings
        // (showPorts / showInterfaces) instead of resetting them.
        private static async System.Threading.Tasks.Task PersistZoomAsync(int percent)
        {
            IDatabaseConnection? db = DatabaseConnectionStore.Current;
            if (db == null) { return; } // not connected (debug-skip login) -> zoom still applies, just isn't saved
            try
            {
                UserSettings current;
                try { current = await db.GetUserSettings(); }
                catch { current = new UserSettings(); }

                current.Zoom = percent;
                current.ShowInterfaces = DisplaySettings.ShowInterfaces; // keep the live toggle in sync
                await db.EditUserSettings(current);
            }
            catch
            {
                // saving the preference is best-effort; the zoom is already applied locally either way
            }
        }
        //KI end

        public SettingsViewModel()
        {
            _selectedTheme = ThemeManager.Current;
            ApplyThemeCommand = new ChangeThemeCommand(this);
        }
    }
    //KI end
}
