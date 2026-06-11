using System;
using System.Linq;
using System.Windows;

namespace RAT_WPF.Themes
{
    //KI start (Claude Opus 4.8, prompt 2): runtime theme switching (light/dark brown)
    public enum AppTheme
    {
        Light,
        Dark
    }

    /// <summary>
    /// Swaps the active palette dictionary (merged-dictionary slot 0 in App.xaml).
    /// Because every control style uses DynamicResource for its brushes, replacing
    /// this dictionary recolors the whole running app instantly.
    /// </summary>
    public static class ThemeManager
    {
        public static AppTheme Current { get; private set; } = AppTheme.Light;

        public static event Action<AppTheme>? ThemeChanged;

        public static void Apply(AppTheme theme)
        {
            string source = theme == AppTheme.Dark
                ? "Themes/DarkTheme.xaml"
                : "Themes/LightTheme.xaml";

            ResourceDictionary newPalette = new ResourceDictionary
            {
                Source = new Uri(source, UriKind.Relative)
            };

            var merged = Application.Current.Resources.MergedDictionaries;
            // slot 0 is always the palette (see App.xaml ordering)
            if (merged.Count > 0)
            {
                merged[0] = newPalette;
            }
            else
            {
                merged.Add(newPalette);
            }

            Current = theme;
            ThemeChanged?.Invoke(theme);
        }
    }
    //KI end
}
