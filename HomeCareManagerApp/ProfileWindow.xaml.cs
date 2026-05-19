using System.Windows;
using HomeCareManager.Core.Data;
using HomeCareManager.Core.Models;
using HomeCareManager.Core.Services;

namespace HomeCareManagerApp
{
    public partial class ProfileWindow : Window
    {
        private readonly User user;
        private readonly Data database;

        public bool LogoutRequested { get; private set; }

        public ProfileWindow(User user, Data database)
        {
            this.user = user;
            this.database = database;

            InitializeComponent();

            NameTextBlock.Text = user.Name;
            EmailTextBlock.Text = user.Email;
            RoleTextBlock.Text = user.RoleId;
            PasswordWarningBorder.Visibility = user.PasswordChanged ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string currentPassword = CurrentPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                MessageBox.Show("Current password is required.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!PasswordHasher.VerifyPassword(currentPassword, user.PasswordHash))
            {
                MessageBox.Show("Current password is not correct.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword.Length < 6)
            {
                MessageBox.Show("New password must be at least 6 characters.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("New passwords do not match.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PasswordHasher.VerifyPassword(newPassword, user.PasswordHash))
            {
                MessageBox.Show("New password must be different from the current password.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string newPasswordHash = PasswordHasher.HashPassword(newPassword);
            bool saved = database.UpdateUserPassword(user.UserId, newPasswordHash);

            if (!saved)
            {
                MessageBox.Show("The password could not be updated.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            user.PasswordHash = newPasswordHash;
            user.PasswordChanged = true;
            PasswordWarningBorder.Visibility = Visibility.Collapsed;
            CurrentPasswordBox.Clear();
            NewPasswordBox.Clear();
            ConfirmPasswordBox.Clear();

            MessageBox.Show("Password updated successfully.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            LogoutRequested = true;
            Close();
        }
    }
}
