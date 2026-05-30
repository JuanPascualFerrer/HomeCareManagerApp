using HomeCareManager.Core.Data;
using System.Windows;
using System.Windows.Controls;

namespace HomeCareManagerApp
{
    public partial class IncidentDetailWindow : Window
    {
        public string SelectedStatus { get; private set; } = "Open";
        public string ResolutionNotes { get; private set; } = string.Empty;

        public IncidentDetailWindow(IncidentRow incident, ReportSummary? report)
        {
            InitializeComponent();

            SubtitleText.Text = $"{incident.Patient} - {incident.StatusLine}";
            SeverityText.Text = incident.Severity;
            DescriptionText.Text = incident.Description;
            PatientText.Text = incident.Patient;
            CreatedText.Text = incident.CreatedLine;
            TaskText.Text = incident.Task;
            ResolutionNotesTextBox.Text = incident.ResolutionNotes;
            SelectStatus(incident.Status);

            if (report == null)
            {
                ReportPanel.Visibility = Visibility.Collapsed;
                ReportEmptyText.Visibility = Visibility.Visible;
                return;
            }

            ReportPanel.Visibility = Visibility.Visible;
            ReportEmptyText.Visibility = Visibility.Collapsed;
            ReportIdText.Text = report.ReportId;
            ReportCreatedByText.Text = report.CreatedBy;
            ReportCreatedText.Text = report.CreatedAt.ToString("g");
            ReportDurationText.Text = report.Duration;
            ReportStatusText.Text = $"{report.StatusBefore} -> {report.StatusAfter}";
            ReportNotesText.Text = report.Notes;
        }

        private void SelectStatus(string status)
        {
            foreach (ComboBoxItem item in StatusComboBox.Items)
            {
                if ((item.Content?.ToString() ?? string.Empty).Equals(status, System.StringComparison.OrdinalIgnoreCase))
                {
                    StatusComboBox.SelectedItem = item;
                    return;
                }
            }

            StatusComboBox.SelectedIndex = 0;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SelectedStatus = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Open";
            ResolutionNotes = ResolutionNotesTextBox.Text.Trim();

            if ((SelectedStatus.Equals("Resolved", System.StringComparison.OrdinalIgnoreCase)
                || SelectedStatus.Equals("Closed", System.StringComparison.OrdinalIgnoreCase))
                && string.IsNullOrWhiteSpace(ResolutionNotes))
            {
                MessageBox.Show("Add resolution notes before resolving or closing the incident.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }
    }
}
