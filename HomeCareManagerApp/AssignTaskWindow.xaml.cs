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
            TaskInfoText.Text = $"{task.Description} — {task.PatientName}";

            // Load assistants into the combo box
            var users = database.GetUserSummaries()
                .Where(u => string.Equals(u.RoleName, "Assistant", StringComparison.OrdinalIgnoreCase))
                .Select(u => new { UserId = u.UserId, DisplayName = $"{u.Name} — {u.Email}" })
                .ToList();

            WorkerComboBox.ItemsSource = users;
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
