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
        public string Priority => (PriorityComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Media";
        public string EmergencyContact => EmergencyContactTextBox.Text.Trim();
        public string Notes => NotesTextBox.Text.Trim();

        public PatientEditorWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PatientName))
            {
                MessageBox.Show("El nombre es obligatorio.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Address))
            {
                MessageBox.Show("La direccion es obligatoria.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Phone))
            {
                MessageBox.Show("El telefono es obligatorio.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Zone))
            {
                MessageBox.Show("La zona es obligatoria.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
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
