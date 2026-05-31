using System.Windows;

namespace HomeCareManagerApp
{
    public partial class TaskDetailWindow : Window
    {
        public TaskDetailWindow(TaskRow task, PatientRow? patient)
        {
            InitializeComponent();

            TaskSubtitleText.Text = $"{task.PatientName} - {task.PatientZone}";
            DescriptionText.Text = task.Description;
            StatusText.Text = task.Status;
            PriorityText.Text = task.Priority;
            DateText.Text = task.Date.ToString("g");
            AssignedToText.Text = task.AssignmentDisplay;

            PatientNameText.Text = patient?.Name ?? task.PatientName;
            PatientZoneText.Text = patient?.Zone ?? task.PatientZone;
            PatientPhoneText.Text = patient?.Phone ?? "Not available";
            PatientPriorityText.Text = patient?.Priority ?? "Not available";
            PatientAddressText.Text = patient?.Address ?? "Not available";
            PatientNotesText.Text = patient?.Notes ?? "Not available";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
