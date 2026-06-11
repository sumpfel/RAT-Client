using RAT_WPF.Commands;
using RAT_WPF.Themes;
using System.Collections.Generic;
using System.Windows.Input;

namespace RAT_WPF.ViewModels
{
    //KI start (Claude Opus 4.8, prompt 2): MVVM viewmodel for the settings window (theme dropdown + apply command)
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

        public SettingsViewModel()
        {
            _selectedTheme = ThemeManager.Current;
            ApplyThemeCommand = new ChangeThemeCommand(this);
        }
    }
    //KI end
}
