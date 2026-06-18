using System;
using System.Globalization;
using System.Windows.Data;
using RAT_Logic;
using RAT_WPF.Themes;

namespace RAT_WPF.Converters
{
    //KI start (Claude Opus 4.8, prompt 2): converts an icon key (or device-type string) to an ImageSource via IconProvider
    /// <summary>
    /// Binding converter that turns a logical icon key into the actual ImageSource.
    /// Pass the key directly (e.g. "Icon.PC") or a NetworkObjectType name
    /// ("PC", "Router", ...) which is mapped to the matching icon.
    /// </summary>
    public class IconKeyConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? key = value?.ToString();
            if (string.IsNullOrWhiteSpace(key)) { return null; }

            //KI start (Claude Opus 4.8, prompt 21): a device-type name is resolved to its icon via the abstract
            // DeviceDescriptor hierarchy (polymorphism) instead of a local switch; raw "Icon.*" keys pass through.
            if (!key.StartsWith("Icon.") && Enum.TryParse(key, out NetworkObjectType type))
            {
                key = DeviceDescriptor.For(type).IconKey;
            }
            //KI end

            return IconProvider.Get(key);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
    //KI end
}
