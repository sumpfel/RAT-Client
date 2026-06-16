using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RAT_WPF.Themes
{
    //KI start (Claude Opus 4.8, prompt 2): icon loader. Prefers a user-supplied PNG (anime art) in
    // Assets/Icons/<key>.png next to the exe; otherwise falls back to the built-in vector DrawingImage.
    public static class IconProvider
    {
        // Logical icon keys (must match resource keys in Themes/Icons.xaml and the PNG file names)
        public const string Pc = "Icon.PC";
        public const string NoConnection = "Icon.NoConnection";
        public const string ConnectionLost = "Icon.ConnectionLost";
        public const string Connected = "Icon.Connected";
        public const string Router = "Icon.Router";
        public const string Switch = "Icon.Switch";
        public const string Server = "Icon.Server";
        public const string Client = "Icon.Client";
        public const string Ethernet = "Icon.Ethernet";
        public const string Wifi = "Icon.Wifi";
        public const string Usb = "Icon.Usb";
        //KI start (Claude Opus 4.8, prompt 15): more rat status icons
        public const string LoginSuccess = "Icon.LoginSuccess";
        public const string LoginFailed = "Icon.LoginFailed";
        public const string Logout = "Icon.Logout";
        //KI end

        private static readonly string IconDir =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Icons");

        /// <summary>
        /// Returns the best available image for the given icon key:
        /// a dropped-in PNG override if present, else the built-in vector.
        /// </summary>
        public static ImageSource? Get(string key)
        {
            try
            {
                string png = Path.Combine(IconDir, key + ".png");
                if (File.Exists(png))
                {
                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad; // don't lock the file
                    bmp.UriSource = new Uri(png, UriKind.Absolute);
                    bmp.EndInit();
                    bmp.Freeze();
                    return bmp;
                }
            }
            catch
            {
                // fall through to the vector fallback
            }

            if (Application.Current != null &&
                Application.Current.TryFindResource(key) is ImageSource vector)
            {
                return vector;
            }
            return null;
        }
    }
    //KI end
}
