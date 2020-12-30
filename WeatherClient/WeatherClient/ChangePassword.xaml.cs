using System.Windows;

namespace WeatherClient
{
    /// <summary>
    /// Logika interakcji dla klasy ChangePassword.xaml
    /// </summary>
    public partial class ChangePassword : Window
    {
        public ChangePassword()
        {
            InitializeComponent();
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }
    }
}
