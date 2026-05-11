using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HomeCareManager.Core.Data;

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

        public MainWindow()
        {

            InitializeComponent();
            DataContext = this;
            TaskDatePicker.SelectedDate = DateTime.Today;
            LoadSampleData();
            ReloadData(showMessages: false);
            SelectSection(DashboardNavButton);
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

        private void SaveTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskPatientComboBox.SelectedItem is not PatientOption patient)
            {
                MessageBox.Show("Selecciona un paciente.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TaskSkillComboBox.SelectedItem is not LookupOption skill)
            {
                MessageBox.Show("Selecciona la habilidad requerida.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TaskStatusComboBox.SelectedItem is not LookupOption status)
            {
                MessageBox.Show("Selecciona el estado inicial.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string description = TaskDescriptionTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(description))
            {
                MessageBox.Show("Escribe una descripcion para la tarea.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string priority = (TaskPriorityComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Media";
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
                    "No se ha podido guardar la tarea. Revisa que MySQL este abierto y que existan las claves relacionadas.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Tarea guardada correctamente.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            ReloadData(showMessages: false);
        }

        private void NewPatient_Click(object sender, RoutedEventArgs e)
        {
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
                string.IsNullOrWhiteSpace(dialog.Notes) ? "Sin notas" : dialog.Notes,
                dialog.Priority,
                string.IsNullOrWhiteSpace(dialog.EmergencyContact) ? "No indicado" : dialog.EmergencyContact,
                dialog.Zone);

            if (!saved)
            {
                MessageBox.Show(
                    "No se ha podido guardar el paciente. Revisa que MySQL este abierto y que la tabla patients exista.",
                    "HomeCare Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Paciente guardado correctamente.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            ReloadData(showMessages: false);
            SelectSection(PatientsNavButton);
        }

        private void ShowPendingIntegration_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Esta accion sera la siguiente en conectarse a formularios completos de crear/editar.",
                "HomeCare Manager",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ReloadData(bool showMessages)
        {
            if (!database.CanConnect())
            {
                LoadSampleData();
                ConnectionStatusText.Text = "MySQL no conectado - datos de ejemplo";
                UpdateDashboardNumbers();

                if (showMessages)
                {
                    MessageBox.Show(
                        "No se ha podido conectar a MySQL. La app mantiene datos de ejemplo para poder seguir trabajando en la interfaz.",
                        "HomeCare Manager",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }

                return;
            }

            List<HomeCareManager.Core.Models.Patient> patients = database.GetPatients();
            List<TaskSummary> tasks = database.GetTaskSummaries();
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
                user.Name,
                user.RoleName,
                user.IsActive ? "Si" : "No")));

            ReplaceItems(SkillOptions, skills.Select(skill => new LookupOption(skill.SkillId, skill.Name)));
            ReplaceItems(StatusOptions, statuses.Select(status => new LookupOption(status.StatusId, status.Name)));

            EnsureFallbackOptions();
            SelectFirstOptions();
            ConnectionStatusText.Text = "Conectado a MySQL";
            UpdateDashboardNumbers();

            if (showMessages)
            {
                MessageBox.Show("Datos sincronizados correctamente.", "HomeCare Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadSampleData()
        {
            ReplaceItems(Patients, new[]
            {
                new PatientRow("patient-sample-1", "Maria Lopez", "Zona Norte", "Alta", "600 112 233", "Calle Mayor 18", "Seguimiento diario de medicacion"),
                new PatientRow("patient-sample-2", "Luis Martin", "Centro", "Media", "600 445 677", "Plaza Nueva 4", "Revision semanal de constantes"),
                new PatientRow("patient-sample-3", "Ana Ruiz", "Zona Este", "Media", "600 889 100", "Avenida del Parque 22", "Apoyo en movilidad"),
                new PatientRow("patient-sample-4", "Carlos Vega", "Zona Norte", "Baja", "600 224 118", "Calle Luna 7", "Visitas puntuales")
            });

            ReplaceItems(PatientOptions, Patients.Select(patient => new PatientOption(patient.Id, patient.Name)));

            ReplaceItems(Tasks, new[]
            {
                new TaskRow("task-sample-1", "Administrar medicacion", "Maria Lopez", "Zona Norte", "Alta", "Pendiente", DateTime.Today),
                new TaskRow("task-sample-2", "Revision de constantes", "Luis Martin", "Centro", "Media", "Aceptada", DateTime.Today),
                new TaskRow("task-sample-3", "Apoyo en movilidad", "Ana Ruiz", "Zona Este", "Media", "Disponible", DateTime.Today.AddDays(1))
            });

            ReplaceItems(Incidents, new[]
            {
                new IncidentRow("Maria Lopez", "Administrar medicacion", "Abierta", DateTime.Today.AddHours(9)),
                new IncidentRow("Ana Ruiz", "Apoyo en movilidad", "En revision", DateTime.Today.AddHours(11))
            });

            ReplaceItems(Users, new[]
            {
                new UserRow("Carmen Diaz", "Cuidador", "Si"),
                new UserRow("Pablo Sanz", "Cuidador", "Si"),
                new UserRow("Laura Gil", "Admin", "Si"),
                new UserRow("Mario Perez", "Cuidador", "No")
            });

            ReplaceItems(SkillOptions, new[]
            {
                new LookupOption("skill-basic", "Cuidados basicos"),
                new LookupOption("skill-medication", "Medicacion"),
                new LookupOption("skill-mobility", "Movilidad")
            });

            ReplaceItems(StatusOptions, new[]
            {
                new LookupOption("pending", "Pendiente"),
                new LookupOption("available", "Disponible"),
                new LookupOption("accepted", "Aceptada")
            });

            SelectFirstOptions();
            ConnectionStatusText.Text = "Datos de ejemplo";
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

        private void EnsureFallbackOptions()
        {
            if (PatientOptions.Count == 0)
            {
                ReplaceItems(PatientOptions, Patients.Select(patient => new PatientOption(patient.Id, patient.Name)));
            }

            if (SkillOptions.Count == 0)
            {
                SkillOptions.Add(new LookupOption("skill-basic", "Cuidados basicos"));
            }

            if (StatusOptions.Count == 0)
            {
                StatusOptions.Add(new LookupOption("pending", "Pendiente"));
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
            int activeUsers = Users.Count(user => user.Active.Equals("Si", StringComparison.OrdinalIgnoreCase));

            PatientCountText.Text = Patients.Count.ToString();
            PatientMetaText.Text = $"{highPriorityPatients} con prioridad alta";
            TaskCountText.Text = Tasks.Count.ToString();
            TaskMetaText.Text = $"{pendingTasks} pendientes";
            UserCountText.Text = activeUsers.ToString();
            UserMetaText.Text = $"{Users.Count} usuarios registrados";
            IncidentCountText.Text = Incidents.Count.ToString();
        }

        private static bool IsHighPriority(string priority)
        {
            return priority.Equals("Alta", StringComparison.OrdinalIgnoreCase)
                || priority.Equals("High", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPending(string status)
        {
            return status.Equals("Pendiente", StringComparison.OrdinalIgnoreCase)
                || status.Equals("Pending", StringComparison.OrdinalIgnoreCase);
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
                    PageSubtitleText.Text = "Busqueda, registro y seguimiento de pacientes";
                    break;
                case nameof(TasksNavButton):
                    TasksView.Visibility = Visibility.Visible;
                    PageSubtitleText.Text = "Creacion, asignacion y control de tareas";
                    break;
                case nameof(IncidentsNavButton):
                    IncidentsView.Visibility = Visibility.Visible;
                    PageSubtitleText.Text = "Gestion de incidencias y trazabilidad";
                    break;
                case nameof(AdminNavButton):
                    AdminView.Visibility = Visibility.Visible;
                    PageSubtitleText.Text = "Usuarios, roles y preparacion de integracion";
                    break;
                default:
                    DashboardView.Visibility = Visibility.Visible;
                    PageSubtitleText.Text = "Resumen operativo del servicio de atencion domiciliaria";
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
        public string Summary => $"Paciente: {PatientName} - {PatientZone}";
        public string StatusLine => $"{Status} - {Description}";
    }

    public record IncidentRow(string Patient, string Task, string Status, DateTime CreatedAt);

    public record UserRow(string Name, string Role, string Active);

    public record PatientOption(string Id, string Name);

    public record LookupOption(string Id, string Name);
}
