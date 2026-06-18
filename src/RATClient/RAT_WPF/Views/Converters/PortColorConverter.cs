using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RAT_WPF.Views.Converters
{
    //KI start (Claude Opus 4.8, prompt 27): colours a port label red when it is a configured login that nmap did
    // not see open (IsUnreachableLogin == true), otherwise the normal muted text colour. Used on the node's ports.
    public class PortColorConverter : IValueConverter
    {
        private static readonly SolidColorBrush UnreachableBrush =
            new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B)); // same red as the LoginFailed badge

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool unreachable = value is bool b && b;
            if (unreachable) { return UnreachableBrush; }

            // normal: pull the themed muted brush so it matches the previous look
            if (System.Windows.Application.Current?.TryFindResource("Brush.TextMuted") is Brush muted)
            {
                return muted;
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
    //KI end
}
