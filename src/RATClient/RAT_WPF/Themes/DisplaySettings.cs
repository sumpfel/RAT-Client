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

        //KI start (Claude Opus 4.8, prompt 26): ShowPorts — when on, nmap discovery also scans open ports and the
        // device nodes show their open ports (with friendly names, e.g. "22 - SSH"). Backed by the same UserSettings
        // field (show_ports) so it loads/saves with the other display prefs.
        private static bool _showPorts;

        /// <summary>Whether device nodes show their discovered open ports, and discovery does a port scan.</summary>
        public static bool ShowPorts
        {
            get => _showPorts;
            set
            {
                if (_showPorts == value) { return; }
                _showPorts = value;
                ShowPortsChanged?.Invoke(value);
            }
        }

        /// <summary>Raised when <see cref="ShowPorts"/> changes.</summary>
        public static event Action<bool>? ShowPortsChanged;
        //KI end
    }
    //KI end
}
