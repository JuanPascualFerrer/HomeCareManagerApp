using System.Windows;
using System.Windows.Controls;

namespace HomeCareManagerApp
{
    public partial class PatientEditorWindow : Window
    {
        public string PatientName => NameTextBox.Text.Trim();
        public string Address => AddressTextBox.Text.Trim();
        public string Phone => PhoneTextBox.Text.Trim();
        public string Zone => ZoneTextBox.Text.Trim();
        public string Priority => (PriorityComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Medium";
        public string EmergencyContact => EmergencyContactTextBox.Text.Trim();
        public string Notes => NotesTextBox.Text.Trim();

        public PatientEditorWindow()
        {
            InitializeComponent();
        }
        public PatientEditorWindow(PatientRow patient) : this()
        {
            NameTextBox.Text = patient.Name;
            AddressTextBox.Text = patient.Address;
            PhoneTextBox.Text = patient.Phone;
            ZoneTextBox.Text = patient.Zone;
            NotesTextBox.Text = patient.Notes;

            foreach (ComboBoxItem item in PriorityComboBox.Items)
            {
                if (item.Content?.ToString() == patient.Priority)
                {
                    PriorityComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PatientName))
            {
                MessageBox.Show("Name is required.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Address))
            {
                MessageBox.Show("Address is required.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Phone))
            {
                MessageBox.Show("Phone is required.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Zone))
            {
                MessageBox.Show("Zone is required.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
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
