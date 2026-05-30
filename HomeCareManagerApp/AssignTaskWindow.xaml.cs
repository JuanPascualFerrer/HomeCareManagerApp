using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HomeCareManagerApp
{
    /// <summary>
    /// Lógica de interacción para AssignTaskWindow.xaml
    /// </summary>
    public partial class AssignTaskWindow : Window
    {
        private readonly HomeCareManager.Core.Data.Data database = new HomeCareManager.Core.Data.Data();

        public string SelectedUserId { get; private set; } = string.Empty;

        public AssignTaskWindow()
        {
            InitializeComponent();
        }

        public AssignTaskWindow(TaskRow task) : this()
        {
            // Display task info
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
