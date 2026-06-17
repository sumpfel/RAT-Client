using System;
using System.Collections.Generic;

namespace RAT_WPF.Themes
{
    //KI start (Claude Opus 4.8, prompt 20): app-wide UI zoom (scales everything via a LayoutTransform on the
    // main window root). Mirrors the ThemeManager pattern: a static holder + a ZoomChanged event the shell
    // subscribes to. 100% is normal, 50% min (zoomed out), 300% max (3x).
    public static class ZoomManager
    {
        public const int Min = 50;
        public const int Default = 100;
        public const int Max = 300;

        /// <summary>The selectable zoom levels shown in the settings dropdown.</summary>
        public static readonly IReadOnlyList<int> Levels = new[] { 50, 75, 100, 125, 150, 200, 250, 300 };

        public static int Current { get; private set; } = Default;

        /// <summary>The current zoom as a scale factor (1.0 == 100%).</summary>
        public static double Scale => Current / 100.0;

        /// <summary>Raised after the zoom changes; the shell uses it to update its scale transform.</summary>
        public static event Action<int>? ZoomChanged;

        public static void Apply(int percent)
        {
            // clamp into the supported range so an odd value can never break the layout
            if (percent < Min) { percent = Min; }
            if (percent > Max) { percent = Max; }

            Current = percent;
            ZoomChanged?.Invoke(percent);
        }
    }
    //KI end
}
