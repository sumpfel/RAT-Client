using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace RAT_WPF
{
    //KI start (Claude Opus 4.8, prompt 17): a small themed message dialog that shows a rat vector graphic next to
    // the text — used instead of the plain system MessageBox for database / connection errors so popups stay on-brand.
    public partial class RatDialog : Window
    {
        public RatDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Shows a themed rat dialog. <paramref name="iconKey"/> is a key from Themes/Icons.xaml
        /// (e.g. "Icon.DatabaseError", "Icon.NoConnection").
        /// </summary>
        public static void Show(string heading, string message, string iconKey = "Icon.DatabaseError", Window? owner = null)
        {
            RatDialog dialog = new RatDialog
            {
                HeadingTextValue = heading,
                MessageTextValue = message,
            };
            dialog.SetIcon(iconKey);

            owner ??= Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            if (owner != null && owner != dialog) { dialog.Owner = owner; }
            else { dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen; }

            dialog.ShowDialog();
        }

        private string HeadingTextValue { set => HeadingText.Text = value; }
        private string MessageTextValue { set => MessageText.Text = value; }

        private void SetIcon(string iconKey)
        {
            if (Application.Current?.TryFindResource(iconKey) is ImageSource img)
            {
                RatImage.Source = img;
            }
            else if (Application.Current?.TryFindResource("Icon.NoConnection") is ImageSource fallback)
            {
                RatImage.Source = fallback;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e) => Close();
    }
    //KI end
}
