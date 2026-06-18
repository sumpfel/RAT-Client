using System;

namespace RAT_WPF.Themes
{
    //KI start (Claude Opus 4.8, prompt 22): app-wide display toggles (mirrors ZoomManager). Currently holds
    // ShowInterfaces — when on, the canvas labels each cable with the two endpoint interface name + IP. The
    // value is loaded from the backend user settings at login and saved when toggled in the toolbar.
    public static class DisplaySettings
    {
        private static bool _showInterfaces;

        /// <summary>Whether cables show their endpoint interface name + IP on the canvas.</summary>
        public static bool ShowInterfaces
        {
            get => _showInterfaces;
            set
            {
                if (_showInterfaces == value) { return; }
                _showInterfaces = value;
                ShowInterfacesChanged?.Invoke(value);
            }
        }

        /// <summary>Raised when <see cref="ShowInterfaces"/> changes (canvas re-renders cable labels).</summary>
        public static event Action<bool>? ShowInterfacesChanged;
    }
    //KI end
}
