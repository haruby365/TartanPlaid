// © 2021 Jong-il Hong

using System;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace Haruby.TartanPlaid
{
    /// <summary>
    /// AboutWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            Version? version = typeof(MainWindow).Assembly.GetName().Version;
            if (version is not null)
            {
                VersionTextBlock.Text = $"{version.Major}.{version.Minor}";
            }

            LicenseTextBox.Text = Encoding.UTF8.GetString(Resource.ABOUT_LICENSE);
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Util.OpenSystemWebBrowser(e.Uri.ToString());
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            Hide();
        }
    }
}
