using System.Linq;
using System.Windows;
using HomeCareManager.Core.Data;

namespace HomeCareManagerApp
{
    public partial class AssignTaskWindow : Window
    {
        private readonly Data database = new Data();

        public string SelectedUserId { get; private set; } = string.Empty;

        public AssignTaskWindow()
        {
            InitializeComponent();
        }

        public AssignTaskWindow(TaskRow task) : this()
        {
            TaskInfoText.Text = $"{task.Description} - {task.PatientName}";

            var users = database.GetEligibleWorkerSummaries(task.RequiredSkillId, task.Date, task.PatientZone)
                .Select(u => new { UserId = u.UserId, DisplayName = $"{u.Name} - {u.RoleName} - {u.Email}" })
                .ToList();

            WorkerComboBox.ItemsSource = users;

            if (users.Count == 0)
            {
                MessageBox.Show(
                    "No active worker matches this task skill, date, and zone.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Assign_Click(object sender, RoutedEventArgs e)
        {
            if (WorkerComboBox.SelectedValue is not string userId || string.IsNullOrWhiteSpace(userId))
            {
                MessageBox.Show("Select a worker.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedUserId = userId;
            DialogResult = true;
        }
    }
}
