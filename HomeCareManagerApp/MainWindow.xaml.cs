using HomeCareManager.Core.Data;
using HomeCareManager.Core.Models;
using HomeCareManager.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HomeCareManagerApp
{
    public partial class MainWindow : Window
    {
        private readonly Data database = new Data();
        private readonly Brush navDefaultBackground = Brushes.Transparent;
        private readonly Brush navDefaultForeground = new SolidColorBrush(Color.FromRgb(220, 235, 255));
        private readonly Brush navSelectedBackground = Brushes.White;
        private readonly Brush navSelectedForeground = new SolidColorBrush(Color.FromRgb(21, 101, 192));

        public ObservableCollection<PatientRow> Patients { get; } = new ObservableCollection<PatientRow>();
        public ObservableCollection<TaskRow> Tasks { get; } = new ObservableCollection<TaskRow>();
        public ObservableCollection<IncidentRow> Incidents { get; } = new ObservableCollection<IncidentRow>();
        public ObservableCollection<UserRow> Users { get; } = new ObservableCollection<UserRow>();
        public ObservableCollection<PatientOption> PatientOptions { get; } = new ObservableCollection<PatientOption>();
        public ObservableCollection<LookupOption> SkillOptions { get; } = new ObservableCollection<LookupOption>();
        public ObservableCollection<LookupOption> StatusOptions { get; } = new ObservableCollection<LookupOption>();

        private readonly User currentUser;

        public MainWindow(User user)
        {

            currentUser = user;

            InitializeComponent();
            Loaded += MainWindow_Loaded;
            DataContext = this;
            TaskDatePicker.SelectedDate = DateTime.Today;
            CurrentUserNameText.Text = currentUser.Name;
            CurrentUserEmailText.Text = currentUser.Email;

            LoadEmptyData();
            ReloadData(showMessages: false);
            SelectSection(DashboardNavButton);
            ApplyPermissions();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!currentUser.PasswordChanged)
            {
                MessageBox.Show(
                    "You are using an initial password. Please open your profile and change it.",
                    "Password change required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private bool IsAdmin()
        {
            return currentUser.RoleId.Equals("admin", StringComparison.OrdinalIgnoreCase);
        }

        private bool CanCreatePatientsAndTasks()
        {
            return currentUser.RoleId.Equals("admin", StringComparison.OrdinalIgnoreCase)
                || currentUser.RoleId.Equals("doctor", StringComparison.OrdinalIgnoreCase);
        }

        private void ApplyPermissions()
        {
            bool isAdmin = IsAdmin();
            bool canCreatePatientsAndTasks = CanCreatePatientsAndTasks();

            AdminNavButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

            NewTaskButton.Visibility = canCreatePatientsAndTasks ? Visibility.Visible : Visibility.Collapsed;
            NewPatientButton.Visibility = canCreatePatientsAndTasks ? Visibility.Visible : Visibility.Collapsed;
            CreateTaskPanel.Visibility = canCreatePatientsAndTasks ? Visibility.Visible : Visibility.Collapsed;
        }


        private void Navigate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                SelectSection(button);
            }
        }

        private void OpenCreateTask_Click(object sender, RoutedEventArgs e)
        {
            SelectSection(TasksNavButton);
        }

        private void ReloadData_Click(object sender, RoutedEventArgs e)
        {
            ReloadData(showMessages: true);
        }

        private void OpenProfile_Click(object sender, MouseButtonEventArgs e)
        {
            ProfileWindow dialog = new ProfileWindow(currentUser, database)
            {
                Owner = this
            };

            dialog.ShowDialog();

            if (dialog.LogoutRequested)
            {
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
        }

        private void SaveTask_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCreatePatientsAndTasks())
            {
                MessageBox.Show(
                    "You do not have permission to create tasks.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (TaskPatientComboBox.SelectedItem is not PatientOption patient)
            {
                MessageBox.Show("Select a patient.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TaskSkillComboBox.SelectedItem is not LookupOption skill)
            {
                MessageBox.Show("Select the required skill.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TaskStatusComboBox.SelectedItem is not LookupOption status)
            {
                MessageBox.Show("Select the initial status.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string description = TaskDescriptionTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(description))
            {
                MessageBox.Show("Enter a task description.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string priority = (TaskPriorityComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Medium";
            DateTime date = TaskDatePicker.SelectedDate ?? DateTime.Today;
            string taskId = $"task-{Guid.NewGuid():N}".Substring(0, 18);

            bool saved = database.InsertTask(
                taskId,
                skill.Id,
                patient.Id,
                description,
                date,
                priority,
                status.Id);

            if (!saved)
            {
                MessageBox.Show(
                    "The task could not be saved. Check that MySQL is running and related records exist.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Task saved successfully.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            ReloadData(showMessages: false);
        }

        private void AssignTask_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not TaskRow task)
                return;

            if (!CanCreatePatientsAndTasks())
            {
                MessageBox.Show(
                    "You do not have permission to assign tasks.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            AssignTaskWindow dialog = new AssignTaskWindow(task)
            {
                Owner = this
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            string assignmentId = $"assign-{Guid.NewGuid():N}".Substring(0, 20);

            bool saved = database.InsertTaskAssignment(
                assignmentId,
                dialog.SelectedUserId,
                task.Id,
                DateTime.Now,
                "assigned");

            if (!saved)
            {
                MessageBox.Show(
                    "The assignment could not be saved. Check that MySQL is running and related records exist.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Task assigned successfully.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            ReloadData(showMessages: false);
        }

        private void NewPatient_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCreatePatientsAndTasks())
            {
                MessageBox.Show(
                    "You do not have permission to create patients.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            PatientEditorWindow dialog = new PatientEditorWindow
            {
                Owner = this
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            string patientId = $"patient-{Guid.NewGuid():N}".Substring(0, 21);
            bool saved = database.InsertPatient(
                patientId,
                dialog.PatientName,
                dialog.Address,
                dialog.Phone,
                string.IsNullOrWhiteSpace(dialog.Notes) ? "No notes" : dialog.Notes,
                dialog.Priority,
                string.IsNullOrWhiteSpace(dialog.EmergencyContact) ? "Not specified" : dialog.EmergencyContact,
                dialog.Zone);

            if (!saved)
            {
                MessageBox.Show(
                    "The patient could not be saved. Check that MySQL is running and the patients table exists.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Patient saved successfully.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            ReloadData(showMessages: false);
            SelectSection(PatientsNavButton);
        }
        private void EditPatient_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not PatientRow row)
                return;

            PatientEditorWindow dialog = new PatientEditorWindow(row)
            {
                Owner = this
            };

            if (dialog.ShowDialog() != true)
                return;

            bool saved = database.UpdatePatient(
                row.Id,
                dialog.PatientName,
                dialog.Address,
                dialog.Phone,
                string.IsNullOrWhiteSpace(dialog.Notes) ? "No notes" : dialog.Notes,
                dialog.Priority,
                string.IsNullOrWhiteSpace(dialog.EmergencyContact) ? "Not specified" : dialog.EmergencyContact,
                dialog.Zone);

            if (!saved)
            {
                MessageBox.Show("The patient could not be updated.", "HomeCare Manager",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Patient updated successfully.", "HomeCare Manager",
                MessageBoxButton.OK, MessageBoxImage.Information);
            ReloadData(showMessages: false);
        }

        private void DeletePatient_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not PatientRow row)
                return;

            MessageBoxResult result = MessageBox.Show(
                $"Delete patient {row.Name}?",
                "HomeCare Manager",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            bool deleted = database.DeletePatient(row.Id);

            if (!deleted)
            {
                MessageBox.Show("The patient could not be deleted.", "HomeCare Manager",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ReloadData(showMessages: false);
        }

        private void CreateUser_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdmin())
            {
                MessageBox.Show(
                    "You do not have permission to create users.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            NewUserEditor dialog = new NewUserEditor(SkillOptions)
            {
                Owner = this
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            if (database.GetUserByEmail(dialog.Email) != null)
            {
                MessageBox.Show(
                    "A user with this email already exists.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            database.InsertRole(dialog.RoleId, dialog.RoleName);
            database.InsertSkill(dialog.SkillId, dialog.SkillName);

            User user = new User
            {
                UserId = $"user-{Guid.NewGuid():N}".Substring(0, 18),
                Name = dialog.UserName,
                Email = dialog.Email,
                PasswordHash = PasswordHasher.HashPassword(dialog.Password),
                PasswordChanged = false,
                IsActive = dialog.UserIsActive,
                CreatedAt = DateTime.Now,
                RoleId = dialog.RoleId,
                SkillId = dialog.SkillId
            };

            bool saved = database.InsertUser(user);

            if (!saved)
            {
                MessageBox.Show(
                    "The user could not be saved. Check MySQL and the users table.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("User created successfully.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            ReloadData(showMessages: false);
            SelectSection(AdminNavButton);
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not UserRow user)
            {
                return;
            }

            User? existingUser = database.GetUserById(user.Id);

            if (existingUser == null)
            {
                MessageBox.Show(
                    "The selected user could not be loaded.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            EditUserEditor dialog = new EditUserEditor(existingUser, SkillOptions)
            {
                Owner = this
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            User? userWithEmail = database.GetUserByEmail(dialog.Email);
            if (userWithEmail != null && !userWithEmail.UserId.Equals(existingUser.UserId, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "A user with this email already exists.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            database.InsertRole(dialog.RoleId, dialog.RoleName);
            database.InsertSkill(dialog.SkillId, dialog.SkillName);

            existingUser.Name = dialog.UserName;
            existingUser.Email = dialog.Email;
            existingUser.RoleId = dialog.RoleId;
            existingUser.SkillId = dialog.SkillId;
            existingUser.IsActive = dialog.UserIsActive;

            bool saved = database.UpdateUser(existingUser);

            if (!saved)
            {
                MessageBox.Show(
                    "The user could not be updated.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (existingUser.UserId.Equals(currentUser.UserId, StringComparison.OrdinalIgnoreCase))
            {
                currentUser.Name = existingUser.Name;
                currentUser.Email = existingUser.Email;
                currentUser.RoleId = existingUser.RoleId;
                currentUser.SkillId = existingUser.SkillId;
                currentUser.IsActive = existingUser.IsActive;
                CurrentUserNameText.Text = currentUser.Name;
                CurrentUserEmailText.Text = currentUser.Email;
                ApplyPermissions();
            }

            ReloadData(showMessages: false);
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not UserRow user)
            {
                return;
            }

            if (user.Id.Equals(currentUser.UserId, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "You cannot delete the user that is currently signed in.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Delete user {user.Name}?",
                "HomeCare Manager",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            bool deleted = database.DeleteUser(user.Id);

            if (!deleted)
            {
                MessageBox.Show(
                    "The user could not be deleted.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            ReloadData(showMessages: false);
        }

        private void ReloadData(bool showMessages)
        {
            if (!database.CanConnect())
            {
                LoadEmptyData();
                ConnectionStatusText.Text = "MySQL disconnected";
                UpdateDashboardNumbers();

                if (showMessages)
                {
                    MessageBox.Show(
                        "Could not connect to MySQL. Data is unavailable until the database connection is restored.",
                        "HomeCare Manager",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }

                return;
            }

            List<HomeCareManager.Core.Models.Patient> patients = database.GetPatients();
            List<TaskSummary> tasks = CanCreatePatientsAndTasks()
                ? database.GetTaskSummaries()
                : database.GetTaskSummariesForUser(currentUser.UserId);
            List<IncidentSummary> incidents = database.GetIncidentSummaries();
            List<UserSummary> users = database.GetUserSummaries();
            List<HomeCareManager.Core.Models.Skill> skills = database.GetSkills();
            List<HomeCareManager.Core.Models.TaskStatus> statuses = database.GetTaskStatuses();

            ReplaceItems(Patients, patients.Select(patient => new PatientRow(
                patient.PatientId,
                patient.Name,
                patient.Zone,
                patient.Priority,
                patient.Phone,
                patient.Address,
                patient.Notes)));

            ReplaceItems(PatientOptions, patients.Select(patient => new PatientOption(patient.PatientId, patient.Name)));

            ReplaceItems(Tasks, tasks.Select(task => new TaskRow(
                task.TaskId,
                task.Description,
                task.PatientName,
                task.PatientZone,
                task.Priority,
                task.StatusName,
                task.Date)));

            ReplaceItems(Incidents, incidents.Select(incident => new IncidentRow(
                incident.PatientName,
                incident.TaskDescription,
                incident.Status,
                incident.CreatedAt)));

            ReplaceItems(Users, users.Select(user => new UserRow(
                user.UserId,
                user.Name,
                user.Email,
                user.RoleName,
                user.IsActive ? "Yes" : "No")));

            ReplaceItems(SkillOptions, skills.Select(skill => new LookupOption(skill.SkillId, skill.Name)));
            ReplaceItems(StatusOptions, statuses.Select(status => new LookupOption(status.StatusId, status.Name)));

            SelectFirstOptions();
            ConnectionStatusText.Text = "Connected to MySQL";
            UpdateDashboardNumbers();

            if (showMessages)
            {
                MessageBox.Show("Data synced successfully.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadEmptyData()
        {
            ReplaceItems(Patients, Array.Empty<PatientRow>());

            ReplaceItems(PatientOptions, Patients.Select(patient => new PatientOption(patient.Id, patient.Name)));

            ReplaceItems(Tasks, Array.Empty<TaskRow>());

            ReplaceItems(Incidents, Array.Empty<IncidentRow>());

            ReplaceItems(Users, Array.Empty<UserRow>());

            ReplaceItems(SkillOptions, Array.Empty<LookupOption>());

            ReplaceItems(StatusOptions, Array.Empty<LookupOption>());

            SelectFirstOptions();
            ConnectionStatusText.Text = "Disconnected";
            UpdateDashboardNumbers();
        }

        private static void ReplaceItems<T>(ObservableCollection<T> target, IEnumerable<T> items)
        {
            target.Clear();
            foreach (T item in items)
            {
                target.Add(item);
            }
        }

        private void SelectFirstOptions()
        {
            TaskPatientComboBox.SelectedIndex = PatientOptions.Count > 0 ? 0 : -1;
            TaskSkillComboBox.SelectedIndex = SkillOptions.Count > 0 ? 0 : -1;
            TaskStatusComboBox.SelectedIndex = StatusOptions.Count > 0 ? 0 : -1;
            TaskPriorityComboBox.SelectedIndex = TaskPriorityComboBox.SelectedIndex < 0 ? 0 : TaskPriorityComboBox.SelectedIndex;
            TaskDatePicker.SelectedDate ??= DateTime.Today;
        }

        private void UpdateDashboardNumbers()
        {
            int highPriorityPatients = Patients.Count(patient => IsHighPriority(patient.Priority));
            int pendingTasks = Tasks.Count(task => IsPending(task.Status));
            int activeUsers = Users.Count(user => user.Active.Equals("Yes", StringComparison.OrdinalIgnoreCase));

            PatientCountText.Text = Patients.Count.ToString();
            PatientMetaText.Text = $"{highPriorityPatients} high priority";
            TaskCountText.Text = Tasks.Count.ToString();
            TaskMetaText.Text = $"{pendingTasks} pending";
            UserCountText.Text = activeUsers.ToString();
            UserMetaText.Text = $"{Users.Count} registered users";
            IncidentCountText.Text = Incidents.Count.ToString();
        }

        private static bool IsHighPriority(string priority)
        {
            return priority.Equals("High", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPending(string status)
        {
            return status.Equals("Pending", StringComparison.OrdinalIgnoreCase);
        }

        private void SelectSection(Button selectedButton)
        {
            DashboardView.Visibility = Visibility.Collapsed;
            PatientsView.Visibility = Visibility.Collapsed;
            TasksView.Visibility = Visibility.Collapsed;
            IncidentsView.Visibility = Visibility.Collapsed;
            AdminView.Visibility = Visibility.Collapsed;

            ResetNavButton(DashboardNavButton);
            ResetNavButton(PatientsNavButton);
            ResetNavButton(TasksNavButton);
            ResetNavButton(IncidentsNavButton);
            ResetNavButton(AdminNavButton);

            selectedButton.Background = navSelectedBackground;
            selectedButton.Foreground = navSelectedForeground;

            string title = selectedButton.Tag?.ToString() ?? "Dashboard";
            PageTitleText.Text = title;

            switch (selectedButton.Name)
            {
                case nameof(PatientsNavButton):
                    PatientsView.Visibility = Visibility.Visible;
                    PageSubtitleText.Text = "Patient registration and tracking";
                    break;
                case nameof(TasksNavButton):
                    TasksView.Visibility = Visibility.Visible;
                    PageSubtitleText.Text = "Task creation, assignment, and monitoring";
                    break;
                case nameof(IncidentsNavButton):
                    IncidentsView.Visibility = Visibility.Visible;
                    PageSubtitleText.Text = "Incident management and traceability";
                    break;
                case nameof(AdminNavButton):
                    AdminView.Visibility = Visibility.Visible;
                    PageSubtitleText.Text = "Users and roles";
                    break;
                default:
                    DashboardView.Visibility = Visibility.Visible;
                    PageSubtitleText.Text = "Operational overview for home care services";
                    break;
            }
        }

        private void ResetNavButton(Button button)
        {
            button.Background = navDefaultBackground;
            button.Foreground = navDefaultForeground;
        }
    }

    public record PatientRow(string Id, string Name, string Zone, string Priority, string Phone, string Address, string Notes);

    public record TaskRow(string Id, string Description, string PatientName, string PatientZone, string Priority, string Status, DateTime Date)
    {
        public string Summary => $"Patient: {PatientName} - {PatientZone}";
        public string StatusLine => $"{Status} - {Description}";
    }

    public record IncidentRow(string Patient, string Task, string Status, DateTime CreatedAt);

    public record UserRow(string Id, string Name, string Email, string Role, string Active);

    public record PatientOption(string Id, string Name);

    public record LookupOption(string Id, string Name);
}
