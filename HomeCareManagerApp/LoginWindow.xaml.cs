using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HomeCareManager.Core.Models;
using HomeCareManager.Core.Services;


namespace HomeCareManagerApp
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService authService = new AuthService();


        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            TryLogin();
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TryLogin();
            }
        }

        private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorBorder.Visibility = Visibility.Collapsed;
        }

        private void TryLogin()
        {
            ErrorBorder.Visibility = Visibility.Collapsed;

            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;

            User? user = authService.Login(email, password);

            if (user == null)
            {
                ErrorText.Text = "Invalid email or password.";
                ErrorBorder.Visibility = Visibility.Visible;
                return;
            }

            MainWindow mainWindow = new MainWindow(user);
            mainWindow.Show();
            Close();
        }
    }
}
