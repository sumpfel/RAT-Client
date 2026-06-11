using RAT_WPF.Themes;
using RAT_WPF.ViewModels;
using System;

namespace RAT_WPF.Commands
{
    //KI start (Claude Opus 4.8, prompt 2): command that applies the theme chosen in the SettingsViewModel
    public class ChangeThemeCommand : CommandBase
    {
        private readonly SettingsViewModel _settingsViewModel;

        public ChangeThemeCommand(SettingsViewModel settingsViewModel)
        {
            _settingsViewModel = settingsViewModel;
        }

        public override void Execute(object? parameter)
        {
            ThemeManager.Apply(_settingsViewModel.SelectedTheme);
        }
    }
    //KI end
}
