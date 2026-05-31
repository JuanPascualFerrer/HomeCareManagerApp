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
        private const string PendingTaskStatusId = "pending";
        private const string AssignedTaskStatusId = "assigned";
        private const string AcceptedTaskStatusId = "accepted";
        private const string RejectedTaskStatusId = "rejected";
        private const string CompletedTaskStatusId = "completed";

        public ObservableCollection<PatientRow> Patients { get; } = new ObservableCollection<PatientRow>();
        public ObservableCollection<TaskRow> Tasks { get; } = new ObservableCollection<TaskRow>();
        public ObservableCollection<TaskRow> VisibleTasks { get; } = new ObservableCollection<TaskRow>();
        public ObservableCollection<TaskRow> DashboardTasks { get; } = new ObservableCollection<TaskRow>();
        public ObservableCollection<TaskRow> DashboardHistoryTasks { get; } = new ObservableCollection<TaskRow>();
        public ObservableCollection<DashboardAlertRow> DashboardAlerts { get; } = new ObservableCollection<DashboardAlertRow>();
        public ObservableCollection<IncidentRow> Incidents { get; } = new ObservableCollection<IncidentRow>();
        public ObservableCollection<IncidentRow> VisibleIncidents { get; } = new ObservableCollection<IncidentRow>();
        public ObservableCollection<UserRow> Users { get; } = new ObservableCollection<UserRow>();
        public ObservableCollection<PatientOption> PatientOptions { get; } = new ObservableCollection<PatientOption>();
        public ObservableCollection<PatientRow> VisiblePatients { get; } = new ObservableCollection<PatientRow>();
        public ObservableCollection<PatientOption> TaskFilterPatientOptions { get; } = new ObservableCollection<PatientOption>();
        public ObservableCollection<LookupOption> SkillOptions { get; } = new ObservableCollection<LookupOption>();
        public ObservableCollection<LookupOption> StatusOptions { get; } = new ObservableCollection<LookupOption>();

        private readonly User currentUser;
        private string editingTaskId = string.Empty;

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
            IncidentsNavButton.Visibility = canCreatePatientsAndTasks ? Visibility.Visible : Visibility.Collapsed;
            DashboardStatsGrid.Columns = canCreatePatientsAndTasks ? 4 : 2;
            AssistantsDashboardCard.Visibility = canCreatePatientsAndTasks ? Visibility.Visible : Visibility.Collapsed;
            IncidentsDashboardCard.Visibility = canCreatePatientsAndTasks ? Visibility.Visible : Visibility.Collapsed;

            NewTaskButton.Visibility = canCreatePatientsAndTasks ? Visibility.Visible : Visibility.Collapsed;
            NewPatientButton.Visibility = canCreatePatientsAndTasks ? Visibility.Visible : Visibility.Collapsed;

            if (!canCreatePatientsAndTasks)
            {
                HideCreateTaskPanel();
            }
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
            if (!CanCreatePatientsAndTasks())
            {
                MessageBox.Show(
                    "You do not have permission to create tasks.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            SelectSection(TasksNavButton);
            ResetCreateTaskForm();
            ShowCreateTaskPanel();
        }

        private void CancelCreateTask_Click(object sender, RoutedEventArgs e)
        {
            ResetCreateTaskForm();
            HideCreateTaskPanel();
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
                return;
            }

            ReloadData(showMessages: false);
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

            string description = TaskDescriptionTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(description))
            {
                MessageBox.Show("Enter a task description.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string priority = (TaskPriorityComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Medium";
            DateTime date = TaskDatePicker.SelectedDate ?? DateTime.Today;

            bool isEditing = !string.IsNullOrWhiteSpace(editingTaskId);
            LookupOption? status = TaskStatusComboBox.SelectedItem as LookupOption;
            if (isEditing && status == null)
            {
                MessageBox.Show("Select the task status.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string statusId = isEditing ? status!.Id : PendingTaskStatusId;
            bool saved = isEditing
                ? database.UpdateTask(
                    editingTaskId,
                    skill.Id,
                    patient.Id,
                    description,
                    date,
                    priority,
                    statusId)
                : database.InsertTask(
                    $"task-{Guid.NewGuid():N}".Substring(0, 18),
                    skill.Id,
                    patient.Id,
                    description,
                    date,
                    priority,
                    statusId);

            if (!saved)
            {
                MessageBox.Show(
                    isEditing
                        ? "The task could not be updated."
                        : "The task could not be saved. Check that MySQL is running and related records exist.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            MessageBox.Show(
                isEditing ? "Task updated successfully." : "Task saved successfully.",
                "HomeCare Manager",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            ReloadData(showMessages: false);
            ResetCreateTaskForm();
            HideCreateTaskPanel();
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not TaskRow task)
                return;

            if (!CanCreatePatientsAndTasks())
            {
                MessageBox.Show(
                    "You do not have permission to edit tasks.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            SelectSection(TasksNavButton);
            editingTaskId = task.Id;
            TaskEditorTitleText.Text = "Edit task";
            TaskStatusLabelText.Text = "Status";
            TaskStatusLabelText.Visibility = Visibility.Visible;
            TaskStatusComboBox.Visibility = Visibility.Visible;
            SaveTaskButton.Content = "Save changes";

            TaskDescriptionTextBox.Text = task.Description;
            TaskDatePicker.SelectedDate = task.Date;
            SelectComboItemById(TaskPatientComboBox, task.PatientId);
            SelectComboItemById(TaskSkillComboBox, task.RequiredSkillId);
            SelectComboItemById(TaskStatusComboBox, task.StatusId);
            SelectPriority(task.Priority);
            ShowCreateTaskPanel();
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

            if (task.IsFinal)
            {
                MessageBox.Show(
                    "Completed or cancelled tasks cannot be reassigned.",
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
            bool wasAssigned = task.IsAssigned;

            if (wasAssigned && !database.RejectOpenTaskAssignmentsForTask(task.Id))
            {
                MessageBox.Show(
                    "The current assignment could not be closed for reassignment.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

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

            if (!database.UpdateTaskItemStatus(task.Id, AssignedTaskStatusId))
            {
                MessageBox.Show(
                    "The task was assigned, but its status could not be updated to assigned.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            MessageBox.Show(
                wasAssigned ? "Task reassigned successfully." : "Task assigned successfully.",
                "HomeCare Manager",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            ReloadData(showMessages: false);
        }

        private void AcceptTask_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not TaskRow task)
                return;

            if (!task.CanWorkerAccept)
            {
                MessageBox.Show("This task cannot be accepted from its current state.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool saved;
            if (string.IsNullOrWhiteSpace(task.CurrentUserAssignmentId))
            {
                saved = database.InsertTaskAssignment(
                    $"assign-{Guid.NewGuid():N}".Substring(0, 20),
                    currentUser.UserId,
                    task.Id,
                    DateTime.Now,
                    AcceptedTaskStatusId);
            }
            else
            {
                saved = database.UpdateTaskAssignmentStatus(task.CurrentUserAssignmentId, AcceptedTaskStatusId);
            }

            if (!saved)
            {
                MessageBox.Show("The task could not be accepted.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!database.UpdateTaskItemStatus(task.Id, AcceptedTaskStatusId))
            {
                MessageBox.Show("The assignment was accepted, but the task status could not be updated.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            MessageBox.Show("Task accepted.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            ReloadData(showMessages: false);
        }

        private void RejectTask_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not TaskRow task)
                return;

            if (string.IsNullOrWhiteSpace(task.CurrentUserAssignmentId) || !task.CanWorkerReject)
            {
                MessageBox.Show("Only assigned tasks waiting for your response can be rejected.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!database.UpdateTaskAssignmentStatus(task.CurrentUserAssignmentId, RejectedTaskStatusId))
            {
                MessageBox.Show("The task could not be rejected.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!database.UpdateTaskItemStatus(task.Id, PendingTaskStatusId))
            {
                MessageBox.Show("The assignment was rejected, but the task could not be returned to pending.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            MessageBox.Show("Task rejected and returned for reassignment.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            ReloadData(showMessages: false);
        }

        private void CompleteTask_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not TaskRow task)
                return;

            if (string.IsNullOrWhiteSpace(task.CurrentUserAssignmentId) || !task.CanWorkerComplete)
            {
                MessageBox.Show("Only accepted tasks can be completed.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CompleteTaskWindow dialog = new CompleteTaskWindow(task)
            {
                Owner = this
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            if (!database.UpdateTaskAssignmentStatus(task.CurrentUserAssignmentId, CompletedTaskStatusId))
            {
                MessageBox.Show("The assignment could not be marked as completed.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!database.UpdateTaskItemStatus(task.Id, CompletedTaskStatusId))
            {
                MessageBox.Show("The assignment was completed, but the task status could not be updated.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            string reportId = $"report-{Guid.NewGuid():N}".Substring(0, 20);
            bool reportSaved = database.InsertReport(
                reportId,
                currentUser.UserId,
                dialog.Notes,
                DateTime.Now,
                task.Status,
                "Completed",
                dialog.Duration,
                task.Id);

            if (dialog.ShouldCreateIncident)
            {
                database.InsertIncident(
                    $"incident-{Guid.NewGuid():N}".Substring(0, 22),
                    currentUser.UserId,
                    task.Id,
                    dialog.IncidentDescription,
                    DateTime.Now,
                    "Open",
                    dialog.IncidentSeverity,
                    reportId: reportId);
            }

            MessageBox.Show(
                reportSaved
                    ? "Task completed and report saved."
                    : "Task completed, but the report could not be saved.",
                "HomeCare Manager",
                MessageBoxButton.OK,
                reportSaved ? MessageBoxImage.Information : MessageBoxImage.Warning);
            ReloadData(showMessages: false);
        }

        private void OpenTaskDetail_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not TaskRow task)
                return;

            PatientRow? patient = Patients.FirstOrDefault(
                candidate => candidate.Id.Equals(task.PatientId, StringComparison.OrdinalIgnoreCase));

            TaskDetailWindow dialog = new TaskDetailWindow(task, patient)
            {
                Owner = this
            };

            dialog.ShowDialog();
        }

        private void OpenIncidentDetail_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not IncidentRow incident)
                return;

            if (!CanCreatePatientsAndTasks())
            {
                MessageBox.Show("You do not have permission to manage incidents.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ReportSummary? report = database.GetReportForIncident(incident.Id);
            IncidentDetailWindow dialog = new IncidentDetailWindow(incident, report)
            {
                Owner = this
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            if (!database.UpdateIncidentFollowUp(incident.Id, dialog.SelectedStatus, dialog.ResolutionNotes))
            {
                MessageBox.Show("The incident could not be updated.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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

            string selectedTaskPatientFilterId = (TaskPatientFilterComboBox.SelectedItem as PatientOption)?.Id ?? string.Empty;
            string selectedAssignmentFilter = GetSelectedComboTag(TaskAssignmentFilterComboBox, "all");
            string selectedTaskStatusFilter = GetSelectedComboTag(TaskStatusFilterComboBox, "all");
            string selectedTaskPriorityFilter = GetSelectedComboTag(TaskPriorityFilterComboBox, "all");
            string selectedTaskSort = GetSelectedComboTag(TaskSortComboBox, "date-desc");
            string selectedIncidentFilter = GetSelectedComboTag(IncidentStatusFilterComboBox, "active");

            List<HomeCareManager.Core.Models.Patient> patients = database.GetPatients();
            List<TaskSummary> tasks = CanCreatePatientsAndTasks()
                ? database.GetTaskSummaries()
                : database.GetTaskSummariesForWorker(currentUser.UserId, currentUser.SkillId);
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
            ApplyPatientFilters();

            ReplaceItems(PatientOptions, patients.Select(patient => new PatientOption(patient.PatientId, patient.Name)));
            ReplaceItems(
                TaskFilterPatientOptions,
                new[] { new PatientOption(string.Empty, "All patients") }
                    .Concat(patients.Select(patient => new PatientOption(patient.PatientId, patient.Name))));
            SelectTaskFilterPatient(selectedTaskPatientFilterId);
            SelectComboItemByTag(TaskAssignmentFilterComboBox, selectedAssignmentFilter);
            SelectComboItemByTag(TaskStatusFilterComboBox, selectedTaskStatusFilter);
            SelectComboItemByTag(TaskPriorityFilterComboBox, selectedTaskPriorityFilter);
            SelectComboItemByTag(TaskSortComboBox, selectedTaskSort);

            ReplaceItems(Tasks, tasks.Select(task => new TaskRow(
                task.TaskId,
                task.PatientId,
                task.RequiredSkillId,
                task.StatusId,
                task.Description,
                task.PatientName,
                task.PatientZone,
                task.Priority,
                task.StatusName,
                task.Date,
                task.AssignmentCount,
                task.AssignedTo,
                task.CurrentUserAssignmentId,
                task.CurrentUserAssignmentStatusId,
                CanCreatePatientsAndTasks())));
            ApplyTaskFilters();

            ReplaceItems(Incidents, incidents.Select(incident => new IncidentRow(
                incident.IncidentId,
                incident.PatientName,
                incident.TaskDescription,
                incident.Description,
                incident.Severity,
                incident.Status,
                incident.CreatedBy,
                incident.CreatedAt,
                incident.ResolutionNotes,
                incident.ReportId,
                incident.ResolvedAt)));
            SelectComboItemByTag(IncidentStatusFilterComboBox, selectedIncidentFilter);
            ApplyIncidentFilters();

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
            UpdateDashboardTasks();

            if (showMessages)
            {
                MessageBox.Show("Data synced successfully.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void TaskFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyTaskFilters();
        }

        private void IncidentFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyIncidentFilters();
        }

        private void PatientSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyPatientFilters();
        }

        private void ApplyPatientFilters()
        {
            if (PatientSearchTextBox == null || PatientResultCountText == null)
            {
                return;
            }

            string searchText = PatientSearchTextBox.Text.Trim();
            IEnumerable<PatientRow> filteredPatients = Patients;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredPatients = filteredPatients.Where(patient =>
                    ContainsSearchText(patient.Name, searchText)
                    || ContainsSearchText(patient.Zone, searchText)
                    || ContainsSearchText(patient.Priority, searchText)
                    || ContainsSearchText(patient.Phone, searchText)
                    || ContainsSearchText(patient.Address, searchText)
                    || ContainsSearchText(patient.Notes, searchText));
            }

            List<PatientRow> visiblePatients = filteredPatients
                .OrderByDescending(patient => IsHighPriority(patient.Priority))
                .ThenBy(patient => patient.Name)
                .ToList();

            ReplaceItems(VisiblePatients, visiblePatients);
            PatientResultCountText.Text = visiblePatients.Count == 1 ? "1 patient" : $"{visiblePatients.Count} patients";
        }

        private void ApplyTaskFilters()
        {
            if (TaskPatientFilterComboBox == null
                || TaskAssignmentFilterComboBox == null
                || TaskStatusFilterComboBox == null
                || TaskPriorityFilterComboBox == null
                || TaskSortComboBox == null
                || TaskResultCountText == null)
            {
                return;
            }

            EnsureTaskFilterSelections();

            IEnumerable<TaskRow> filteredTasks = Tasks;

            if (TaskPatientFilterComboBox.SelectedItem is PatientOption selectedPatient
                && !string.IsNullOrWhiteSpace(selectedPatient.Id))
            {
                filteredTasks = filteredTasks.Where(task => task.PatientId.Equals(selectedPatient.Id, StringComparison.OrdinalIgnoreCase));
            }

            string statusFilter = (TaskStatusFilterComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "all";
            if (!statusFilter.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                filteredTasks = filteredTasks.Where(task => task.StatusId.Equals(statusFilter, StringComparison.OrdinalIgnoreCase));
            }

            string priorityFilter = (TaskPriorityFilterComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "all";
            if (!priorityFilter.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                filteredTasks = filteredTasks.Where(task => task.Priority.Equals(priorityFilter, StringComparison.OrdinalIgnoreCase));
            }

            string assignmentFilter = (TaskAssignmentFilterComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "all";
            filteredTasks = assignmentFilter switch
            {
                "assigned" => filteredTasks.Where(task => task.IsAssigned),
                "unassigned" => filteredTasks.Where(task => !task.IsAssigned),
                _ => filteredTasks
            };

            string sortMode = (TaskSortComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "date-desc";
            filteredTasks = sortMode switch
            {
                "date-asc" => filteredTasks.OrderBy(task => task.Date).ThenBy(task => PriorityRank(task.Priority)),
                "priority-desc" => filteredTasks.OrderByDescending(task => PriorityRank(task.Priority)).ThenBy(task => task.Date),
                "priority-asc" => filteredTasks.OrderBy(task => PriorityRank(task.Priority)).ThenBy(task => task.Date),
                _ => filteredTasks.OrderByDescending(task => task.Date).ThenByDescending(task => PriorityRank(task.Priority))
            };

            List<TaskRow> visibleTasks = filteredTasks.ToList();
            ReplaceItems(VisibleTasks, visibleTasks);
            TaskResultCountText.Text = visibleTasks.Count == 1 ? "1 task" : $"{visibleTasks.Count} tasks";
        }

        private void ApplyIncidentFilters()
        {
            if (IncidentStatusFilterComboBox == null || IncidentResultCountText == null)
            {
                return;
            }

            if (IncidentStatusFilterComboBox.SelectedIndex < 0)
            {
                IncidentStatusFilterComboBox.SelectedIndex = 0;
            }

            string statusFilter = (IncidentStatusFilterComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "active";
            IEnumerable<IncidentRow> filteredIncidents = statusFilter switch
            {
                "open" => Incidents.Where(incident => incident.Status.Equals("Open", StringComparison.OrdinalIgnoreCase)),
                "in-review" => Incidents.Where(incident => incident.Status.Equals("In review", StringComparison.OrdinalIgnoreCase)),
                "resolved" => Incidents.Where(incident => incident.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase)),
                "closed" => Incidents.Where(incident => incident.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase)),
                "active" => Incidents.Where(incident => incident.IsActive),
                _ => Incidents
            };

            List<IncidentRow> visibleIncidents = filteredIncidents
                .OrderByDescending(incident => incident.IsActive)
                .ThenByDescending(incident => SeverityRank(incident.Severity))
                .ThenByDescending(incident => incident.CreatedAt)
                .ToList();

            ReplaceItems(VisibleIncidents, visibleIncidents);
            IncidentResultCountText.Text = visibleIncidents.Count == 1 ? "1 incident" : $"{visibleIncidents.Count} incidents";
        }

        private void LoadEmptyData()
        {
            ReplaceItems(Patients, Array.Empty<PatientRow>());
            ReplaceItems(VisiblePatients, Array.Empty<PatientRow>());

            ReplaceItems(PatientOptions, Patients.Select(patient => new PatientOption(patient.Id, patient.Name)));

            ReplaceItems(Tasks, Array.Empty<TaskRow>());
            ReplaceItems(VisibleTasks, Array.Empty<TaskRow>());
            ReplaceItems(DashboardTasks, Array.Empty<TaskRow>());
            ReplaceItems(DashboardHistoryTasks, Array.Empty<TaskRow>());
            ReplaceItems(DashboardAlerts, Array.Empty<DashboardAlertRow>());
            ReplaceItems(TaskFilterPatientOptions, new[] { new PatientOption(string.Empty, "All patients") });

            ReplaceItems(Incidents, Array.Empty<IncidentRow>());
            ReplaceItems(VisibleIncidents, Array.Empty<IncidentRow>());

            ReplaceItems(Users, Array.Empty<UserRow>());

            ReplaceItems(SkillOptions, Array.Empty<LookupOption>());

            ReplaceItems(StatusOptions, Array.Empty<LookupOption>());

            SelectFirstOptions();
            EnsureTaskFilterSelections();
            ApplyPatientFilters();
            ApplyTaskFilters();
            ApplyIncidentFilters();
            ConnectionStatusText.Text = "Disconnected";
            UpdateDashboardNumbers();
            UpdateDashboardAlerts();
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

        private void EnsureTaskFilterSelections()
        {
            if (TaskPatientFilterComboBox.SelectedIndex < 0 && TaskFilterPatientOptions.Count > 0)
            {
                TaskPatientFilterComboBox.SelectedIndex = 0;
            }

            if (TaskAssignmentFilterComboBox.SelectedIndex < 0)
            {
                TaskAssignmentFilterComboBox.SelectedIndex = 0;
            }

            if (TaskStatusFilterComboBox.SelectedIndex < 0)
            {
                TaskStatusFilterComboBox.SelectedIndex = 0;
            }

            if (TaskPriorityFilterComboBox.SelectedIndex < 0)
            {
                TaskPriorityFilterComboBox.SelectedIndex = 0;
            }

            if (TaskSortComboBox.SelectedIndex < 0)
            {
                TaskSortComboBox.SelectedIndex = 0;
            }
        }

        private static int PriorityRank(string priority)
        {
            return priority.ToLowerInvariant() switch
            {
                "high" => 3,
                "medium" => 2,
                "low" => 1,
                _ => 0
            };
        }

        private static int SeverityRank(string severity)
        {
            return severity.ToLowerInvariant() switch
            {
                "critical" => 4,
                "high" => 3,
                "medium" => 2,
                "low" => 1,
                _ => 0
            };
        }

        private static void SelectComboItemById(ComboBox comboBox, string id)
        {
            foreach (object item in comboBox.Items)
            {
                string itemId = item switch
                {
                    PatientOption patient => patient.Id,
                    LookupOption lookup => lookup.Id,
                    _ => string.Empty
                };

                if (itemId.Equals(id, StringComparison.OrdinalIgnoreCase))
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }
        }

        private void SelectTaskFilterPatient(string patientId)
        {
            PatientOption? selectedPatient = TaskFilterPatientOptions.FirstOrDefault(
                patient => patient.Id.Equals(patientId, StringComparison.OrdinalIgnoreCase));

            TaskPatientFilterComboBox.SelectedItem = selectedPatient ?? TaskFilterPatientOptions.FirstOrDefault();
        }

        private static string GetSelectedComboTag(ComboBox comboBox, string fallback)
        {
            return (comboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? fallback;
        }

        private static void SelectComboItemByTag(ComboBox comboBox, string tag)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if ((item.Tag?.ToString() ?? string.Empty).Equals(tag, StringComparison.OrdinalIgnoreCase))
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }

            comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
        }

        private void SelectPriority(string priority)
        {
            foreach (ComboBoxItem item in TaskPriorityComboBox.Items)
            {
                if ((item.Content?.ToString() ?? string.Empty).Equals(priority, StringComparison.OrdinalIgnoreCase))
                {
                    TaskPriorityComboBox.SelectedItem = item;
                    return;
                }
            }

            TaskPriorityComboBox.SelectedIndex = 1;
        }

        private void ShowCreateTaskPanel()
        {
            CreateTaskPanel.Visibility = Visibility.Visible;
        }

        private void HideCreateTaskPanel()
        {
            CreateTaskPanel.Visibility = Visibility.Collapsed;
        }

        private void ResetCreateTaskForm()
        {
            editingTaskId = string.Empty;
            TaskEditorTitleText.Text = "Create task";
            TaskStatusLabelText.Text = "Initial status";
            TaskStatusLabelText.Visibility = Visibility.Collapsed;
            TaskStatusComboBox.Visibility = Visibility.Collapsed;
            SaveTaskButton.Content = "Save task";
            TaskDescriptionTextBox.Clear();
            TaskDatePicker.SelectedDate = DateTime.Today;
            TaskPriorityComboBox.SelectedIndex = 0;
            SelectFirstOptions();
            SelectComboItemById(TaskStatusComboBox, PendingTaskStatusId);
        }

        private void UpdateDashboardNumbers()
        {
            int highPriorityPatients = Patients.Count(patient => IsHighPriority(patient.Priority));
            int pendingTasks = Tasks.Count(task => task.IsPending);
            int activeTasks = Tasks.Count(task => !task.IsFinal);
            int activeUsers = Users.Count(user => user.Active.Equals("Yes", StringComparison.OrdinalIgnoreCase));
            int activeIncidents = Incidents.Count(incident => incident.IsActive);

            PatientCountText.Text = Patients.Count.ToString();
            PatientMetaText.Text = $"{highPriorityPatients} high priority";
            TaskCountText.Text = pendingTasks.ToString();
            TaskMetaText.Text = $"{activeTasks} active total";
            UserCountText.Text = activeUsers.ToString();
            UserMetaText.Text = $"{Users.Count} registered users";
            IncidentCountText.Text = activeIncidents.ToString();
        }

        private void UpdateDashboardTasks()
        {
            IEnumerable<TaskRow> dashboardTasks = Tasks
                .Where(task => !task.IsFinal)
                .OrderByDescending(task => task.IsOverdue)
                .ThenByDescending(task => PriorityRank(task.Priority))
                .ThenBy(task => task.Date)
                .Take(8);

            ReplaceItems(DashboardTasks, dashboardTasks);

            IEnumerable<TaskRow> historyTasks = Tasks
                .Where(task => task.IsCompleted)
                .OrderByDescending(task => task.Date)
                .Take(8);

            ReplaceItems(DashboardHistoryTasks, historyTasks);
            UpdateDashboardAlerts();
        }

        private void UpdateDashboardAlerts()
        {
            List<DashboardAlertRow> alerts = new List<DashboardAlertRow>();

            IEnumerable<TaskRow> overdueTasks = Tasks
                .Where(task => task.IsOverdue)
                .OrderBy(task => task.Date)
                .Take(3);

            foreach (TaskRow task in overdueTasks)
            {
                alerts.Add(new DashboardAlertRow(
                    "Overdue",
                    task.Description,
                    $"{task.PatientName} - {task.Date:g}",
                    "High"));
            }

            IEnumerable<TaskRow> highPriorityUnassignedTasks = Tasks
                .Where(task => task.IsPending && !task.IsAssigned && task.IsHighPriority)
                .OrderBy(task => task.Date)
                .Take(3);

            foreach (TaskRow task in highPriorityUnassignedTasks)
            {
                alerts.Add(new DashboardAlertRow(
                    "Unassigned",
                    task.Description,
                    $"{task.PatientName} - high priority",
                    "Medium"));
            }

            IEnumerable<IncidentRow> urgentIncidents = Incidents
                .Where(incident => incident.IsActive && SeverityRank(incident.Severity) >= 3)
                .OrderByDescending(incident => SeverityRank(incident.Severity))
                .ThenByDescending(incident => incident.CreatedAt)
                .Take(3);

            foreach (IncidentRow incident in urgentIncidents)
            {
                alerts.Add(new DashboardAlertRow(
                    incident.Severity,
                    incident.Description,
                    $"{incident.Patient} - {incident.CreatedAt:g}",
                    incident.SeverityRank >= 4 ? "High" : "Medium"));
            }

            if (alerts.Count == 0)
            {
                alerts.Add(new DashboardAlertRow(
                    "Clear",
                    "No urgent alerts",
                    "Tasks and incidents are within expected status.",
                    "Low"));
            }

            ReplaceItems(DashboardAlerts, alerts);
        }

        private static bool IsHighPriority(string priority)
        {
            return priority.Equals("High", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsSearchText(string value, string searchText)
        {
            return value.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void SelectSection(Button selectedButton)
        {
            HideCreateTaskPanel();

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

    public record TaskRow(
        string Id,
        string PatientId,
        string RequiredSkillId,
        string StatusId,
        string Description,
        string PatientName,
        string PatientZone,
        string Priority,
        string Status,
        DateTime Date,
        int AssignmentCount,
        string AssignedTo,
        string CurrentUserAssignmentId,
        string CurrentUserAssignmentStatusId,
        bool CanManageTasks)
    {
        public string Summary => $"Patient: {PatientName} - {PatientZone}";
        public bool IsAssigned => AssignmentCount > 0;
        public bool HasCurrentUserAssignment => !string.IsNullOrWhiteSpace(CurrentUserAssignmentId);
        public bool IsPending => StatusId.Equals("pending", StringComparison.OrdinalIgnoreCase);
        public bool IsCompleted => StatusId.Equals("completed", StringComparison.OrdinalIgnoreCase);
        public bool IsHighPriority => Priority.Equals("High", StringComparison.OrdinalIgnoreCase);
        public bool IsOverdue => !IsFinal && Date.Date < DateTime.Today;
        public bool IsAvailableForCurrentUser => !CanManageTasks
            && !HasCurrentUserAssignment
            && StatusId.Equals("pending", StringComparison.OrdinalIgnoreCase);
        public bool IsFinal => IsCompleted
            || StatusId.Equals("cancelled", StringComparison.OrdinalIgnoreCase);
        public bool CanWorkerAccept => !CanManageTasks
            && !IsFinal
            && (IsAvailableForCurrentUser || CurrentUserAssignmentStatusId.Equals("assigned", StringComparison.OrdinalIgnoreCase));
        public bool CanWorkerReject => !CanManageTasks
            && CurrentUserAssignmentStatusId.Equals("assigned", StringComparison.OrdinalIgnoreCase);
        public bool CanWorkerComplete => !CanManageTasks
            && (CurrentUserAssignmentStatusId.Equals("accepted", StringComparison.OrdinalIgnoreCase)
                || CurrentUserAssignmentStatusId.Equals("in-progress", StringComparison.OrdinalIgnoreCase));
        public string AssignmentDisplay => IsAvailableForCurrentUser ? "Available" : IsAssigned ? AssignedTo : "Unassigned";
        public string AssignAction => IsAssigned ? "Reassign" : "Assign";
        public string DashboardStatusLabel => IsOverdue ? "Overdue" : Status;
        public string DueLine => $"{Date:g} - {Priority} priority";
        public string HistoryLine => $"{PatientName} - {Date:g}";
        public Brush DashboardStatusBackground => DashboardStatusLabel.ToLowerInvariant() switch
        {
            "completed" => new SolidColorBrush(Color.FromRgb(220, 252, 231)),
            "accepted" => new SolidColorBrush(Color.FromRgb(227, 242, 253)),
            "assigned" => new SolidColorBrush(Color.FromRgb(254, 243, 199)),
            "overdue" => new SolidColorBrush(Color.FromRgb(254, 226, 226)),
            _ => new SolidColorBrush(Color.FromRgb(239, 246, 255))
        };
        public Brush DashboardStatusForeground => DashboardStatusLabel.ToLowerInvariant() switch
        {
            "completed" => new SolidColorBrush(Color.FromRgb(22, 101, 52)),
            "accepted" => new SolidColorBrush(Color.FromRgb(21, 101, 192)),
            "assigned" => new SolidColorBrush(Color.FromRgb(146, 64, 14)),
            "overdue" => new SolidColorBrush(Color.FromRgb(185, 28, 28)),
            _ => new SolidColorBrush(Color.FromRgb(21, 101, 192))
        };
        public Visibility EditActionVisibility => CanManageTasks ? Visibility.Visible : Visibility.Collapsed;
        public Visibility AssignActionVisibility => CanManageTasks && !IsFinal ? Visibility.Visible : Visibility.Collapsed;
        public Visibility AcceptActionVisibility => CanWorkerAccept ? Visibility.Visible : Visibility.Collapsed;
        public Visibility RejectActionVisibility => CanWorkerReject ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CompleteActionVisibility => CanWorkerComplete ? Visibility.Visible : Visibility.Collapsed;
    }

    public record IncidentRow(
        string Id,
        string Patient,
        string Task,
        string Description,
        string Severity,
        string Status,
        string CreatedBy,
        DateTime CreatedAt,
        string ResolutionNotes,
        string ReportId,
        DateTime? ResolvedAt)
    {
        public bool IsActive => !Status.Equals("Closed", StringComparison.OrdinalIgnoreCase)
            && !Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase);
        public int SeverityRank => Severity.ToLowerInvariant() switch
        {
            "critical" => 4,
            "high" => 3,
            "medium" => 2,
            "low" => 1,
            _ => 0
        };
        public string CreatedLine => $"By {CreatedBy} - {CreatedAt:g}";
        public string StatusLine => ResolvedAt.HasValue ? $"{Status} - {ResolvedAt:g}" : Status;
        public string ReportLabel => string.IsNullOrWhiteSpace(ReportId) ? "Report: task history" : $"Report: {ReportId}";
        public Brush SeverityBackground => Severity.ToLowerInvariant() switch
        {
            "critical" => new SolidColorBrush(Color.FromRgb(127, 29, 29)),
            "high" => new SolidColorBrush(Color.FromRgb(254, 226, 226)),
            "medium" => new SolidColorBrush(Color.FromRgb(254, 243, 199)),
            "low" => new SolidColorBrush(Color.FromRgb(220, 252, 231)),
            _ => new SolidColorBrush(Color.FromRgb(227, 242, 253))
        };
        public Brush SeverityForeground => Severity.ToLowerInvariant() switch
        {
            "critical" => Brushes.White,
            "high" => new SolidColorBrush(Color.FromRgb(185, 28, 28)),
            "medium" => new SolidColorBrush(Color.FromRgb(146, 64, 14)),
            "low" => new SolidColorBrush(Color.FromRgb(22, 101, 52)),
            _ => new SolidColorBrush(Color.FromRgb(21, 101, 192))
        };
        public Brush StatusBackground => IsActive
            ? new SolidColorBrush(Color.FromRgb(227, 242, 253))
            : new SolidColorBrush(Color.FromRgb(220, 252, 231));
        public Brush StatusForeground => IsActive
            ? new SolidColorBrush(Color.FromRgb(21, 101, 192))
            : new SolidColorBrush(Color.FromRgb(22, 101, 52));
    }

    public record UserRow(string Id, string Name, string Email, string Role, string Active);

    public record DashboardAlertRow(string Type, string Message, string Detail, string Severity)
    {
        public Brush SeverityBackground => Severity.ToLowerInvariant() switch
        {
            "high" => new SolidColorBrush(Color.FromRgb(254, 226, 226)),
            "medium" => new SolidColorBrush(Color.FromRgb(254, 243, 199)),
            _ => new SolidColorBrush(Color.FromRgb(220, 252, 231))
        };

        public Brush SeverityForeground => Severity.ToLowerInvariant() switch
        {
            "high" => new SolidColorBrush(Color.FromRgb(185, 28, 28)),
            "medium" => new SolidColorBrush(Color.FromRgb(146, 64, 14)),
            _ => new SolidColorBrush(Color.FromRgb(22, 101, 52))
        };
    }

    public record PatientOption(string Id, string Name);

    public record LookupOption(string Id, string Name);
}
