using System;
using System.Collections.ObjectModel;
using System.Linq;
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

        public ObservableCollection<AvailabilitySlotRow> AvailabilitySlots { get; } = new ObservableCollection<AvailabilitySlotRow>();
        public bool LogoutRequested { get; private set; }

        public ProfileWindow(User user, Data database)
        {
            this.user = user;
            this.database = database;

            InitializeComponent();
            DataContext = this;

            NameTextBlock.Text = user.Name;
            EmailTextBlock.Text = user.Email;
            RoleTextBlock.Text = user.RoleId;
            SkillTextBlock.Text = user.SkillId;
            PasswordWarningBorder.Visibility = user.PasswordChanged ? Visibility.Collapsed : Visibility.Visible;
            AvailabilityDatePicker.SelectedDate = DateTime.Today;
            LoadAvailability();
        }

        private void LoadAvailability()
        {
            AvailabilitySlots.Clear();

            foreach (AvailabilitySummary availability in database.GetAvailabilityForUser(user.UserId)
                .Where(availability => availability.EndTime >= DateTime.Today.AddDays(-7)))
            {
                AvailabilitySlots.Add(new AvailabilitySlotRow(
                    availability.AvailabilityId,
                    availability.StartTime,
                    availability.EndTime,
                    availability.Zone));
            }
        }

        private void AddAvailability_Click(object sender, RoutedEventArgs e)
        {
            DateTime selectedDate = AvailabilityDatePicker.SelectedDate ?? DateTime.Today;
            if (!TryBuildAvailabilityRange(selectedDate, StartTimeTextBox.Text, EndTimeTextBox.Text, out DateTime startTime, out DateTime endTime))
            {
                MessageBox.Show("Enter valid start and end times. Example: 09:00 and 17:00.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string zone = AvailabilityZoneTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(zone))
            {
                MessageBox.Show("Zone is required.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool saved = database.InsertAvailability(
                $"avail-{Guid.NewGuid():N}".Substring(0, 20),
                startTime,
                zone,
                endTime,
                user.UserId);

            if (!saved)
            {
                MessageBox.Show("The availability slot could not be saved.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AvailabilityZoneTextBox.Clear();
            LoadAvailability();
        }

        private void DeleteAvailability_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not AvailabilitySlotRow availability)
            {
                return;
            }

            bool deleted = database.DeleteAvailability(availability.Id);
            if (!deleted)
            {
                MessageBox.Show("The availability slot could not be deleted.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            LoadAvailability();
        }

        private static bool TryBuildAvailabilityRange(
            DateTime selectedDate,
            string startValue,
            string endValue,
            out DateTime startTime,
            out DateTime endTime)
        {
            startTime = selectedDate.Date;
            endTime = selectedDate.Date;

            if (!TimeSpan.TryParse(startValue.Trim(), out TimeSpan start)
                || !TimeSpan.TryParse(endValue.Trim(), out TimeSpan end))
            {
                return false;
            }

            startTime = selectedDate.Date.Add(start);
            endTime = selectedDate.Date.Add(end);

            return endTime > startTime;
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

    public record AvailabilitySlotRow(string Id, DateTime StartTime, DateTime EndTime, string Zone)
    {
        public string DateDisplay => StartTime.ToString("d");
        public string StartDisplay => StartTime.ToString("HH:mm");
        public string EndDisplay => EndTime.ToString("HH:mm");
    }
}
