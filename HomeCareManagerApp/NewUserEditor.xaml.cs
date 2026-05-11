using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace HomeCareManagerApp
{
    public partial class NewUserEditor : Window
    {
        public ObservableCollection<LookupOption> Skills { get; } = new ObservableCollection<LookupOption>();

        public string UserName => NameTextBox.Text.Trim();
        public string Email => EmailTextBox.Text.Trim();
        public string Password => PasswordTextBox.Password;
        public bool UserIsActive => IsActiveCheckBox.IsChecked == true;
        public string RoleId => (RoleComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "assistant";
        public string RoleName => (RoleComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Assistant";
        public string SkillId => (SkillComboBox.SelectedItem as LookupOption)?.Id ?? DefaultSkillForRole().Id;
        public string SkillName => (SkillComboBox.SelectedItem as LookupOption)?.Name ?? DefaultSkillForRole().Name;

        public NewUserEditor()
        {
            InitializeComponent();
            DataContext = this;
        }

        public NewUserEditor(IEnumerable<LookupOption> skills)
            : this()
        {
            foreach (LookupOption skill in skills)
            {
                Skills.Add(skill);
            }

            if (Skills.Count == 0)
            {
                Skills.Add(new LookupOption("skill-admin", "Administration"));
                Skills.Add(new LookupOption("skill-doctor", "Medical care"));
                Skills.Add(new LookupOption("skill-assistant", "Home care assistance"));
            }

            SkillComboBox.SelectedIndex = 0;
        }

        private LookupOption DefaultSkillForRole()
        {
            return RoleId.ToLowerInvariant() switch
            {
                "admin" => new LookupOption("skill-admin", "Administration"),
                "doctor" => new LookupOption("skill-doctor", "Medical care"),
                _ => new LookupOption("skill-assistant", "Home care assistance")
            };
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                MessageBox.Show("Full name is required.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("Email is required.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Email.Contains('@'))
            {
                MessageBox.Show("Enter a valid email address.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Password != ConfirmPasswordTextBox.Password)
            {
                MessageBox.Show("Passwords do not match.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
