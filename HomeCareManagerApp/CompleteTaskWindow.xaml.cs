using System.Windows;
using System.Windows.Controls;

namespace HomeCareManagerApp
{
    public partial class CompleteTaskWindow : Window
    {
        public string Notes { get; private set; } = string.Empty;
        public string Duration { get; private set; } = "Not recorded";
        public bool ShouldCreateIncident { get; private set; }
        public string IncidentDescription { get; private set; } = string.Empty;
        public string IncidentSeverity { get; private set; } = "Medium";

        public CompleteTaskWindow(TaskRow task)
        {
            InitializeComponent();
            TaskInfoText.Text = $"{task.Description} - {task.PatientName}";
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ReportIncident_Changed(object sender, RoutedEventArgs e)
        {
            IncidentPanel.Visibility = ReportIncidentCheckBox.IsChecked == true
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void Complete_Click(object sender, RoutedEventArgs e)
        {
            string notes = NotesTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(notes))
            {
                MessageBox.Show("Add completion notes before closing the task.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Notes = notes;
            Duration = string.IsNullOrWhiteSpace(DurationTextBox.Text)
                ? "Not recorded"
                : DurationTextBox.Text.Trim();

            ShouldCreateIncident = ReportIncidentCheckBox.IsChecked == true;
            if (ShouldCreateIncident)
            {
                IncidentDescription = IncidentDescriptionTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(IncidentDescription))
                {
                    MessageBox.Show("Describe the incident before closing the task.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                IncidentSeverity = (IncidentSeverityComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Medium";
            }

            DialogResult = true;
        }
    }
}
