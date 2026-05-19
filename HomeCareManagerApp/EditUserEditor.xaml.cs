using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HomeCareManager.Core.Models;

namespace HomeCareManagerApp
{
    public partial class EditUserEditor : Window
    {
        public ObservableCollection<LookupOption> Skills { get; } = new ObservableCollection<LookupOption>();

        public string UserName => NameTextBox.Text.Trim();
        public string Email => EmailTextBox.Text.Trim();
        public bool UserIsActive => IsActiveCheckBox.IsChecked == true;
        public string RoleId => (RoleComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "assistant";
        public string RoleName => (RoleComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Assistant";
        public string SkillId => (SkillComboBox.SelectedItem as LookupOption)?.Id ?? DefaultSkillForRole().Id;
        public string SkillName => (SkillComboBox.SelectedItem as LookupOption)?.Name ?? DefaultSkillForRole().Name;

        public EditUserEditor(User user, IEnumerable<LookupOption> skills)
        {
            InitializeComponent();
            DataContext = this;

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

            NameTextBox.Text = user.Name;
            EmailTextBox.Text = user.Email;
            IsActiveCheckBox.IsChecked = user.IsActive;
            SelectRole(user.RoleId);
            SkillComboBox.SelectedItem = Skills.FirstOrDefault(skill => skill.Id == user.SkillId) ?? Skills.FirstOrDefault();
        }

        private void SelectRole(string roleId)
        {
            foreach (ComboBoxItem item in RoleComboBox.Items)
            {
                if ((item.Tag?.ToString() ?? string.Empty).Equals(roleId, System.StringComparison.OrdinalIgnoreCase))
                {
                    RoleComboBox.SelectedItem = item;
                    return;
                }
            }

            RoleComboBox.SelectedIndex = 2;
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

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
