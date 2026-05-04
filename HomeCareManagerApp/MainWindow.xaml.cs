using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HomeCareManager.Core.Data;

namespace HomeCareManagerApp
{
    public partial class MainWindow : Window
    {
        private readonly Brush navDefaultBackground = Brushes.Transparent;
        private readonly Brush navDefaultForeground = new SolidColorBrush(Color.FromRgb(220, 235, 255));
        private readonly Brush navSelectedBackground = Brushes.White;
        private readonly Brush navSelectedForeground = new SolidColorBrush(Color.FromRgb(21, 101, 192));

        public ObservableCollection<PatientRow> Patients { get; } =
        [
            new PatientRow("Maria Lopez", "Zona Norte", "Alta", "600 112 233"),
            new PatientRow("Luis Martin", "Centro", "Media", "600 445 677"),
            new PatientRow("Ana Ruiz", "Zona Este", "Media", "600 889 100"),
            new PatientRow("Carlos Vega", "Zona Norte", "Baja", "600 224 118")
        ];

        public ObservableCollection<IncidentRow> Incidents { get; } =
        [
            new IncidentRow("Maria Lopez", "Administrar medicacion", "Abierta"),
            new IncidentRow("Ana Ruiz", "Apoyo en movilidad", "En revision")
        ];

        public ObservableCollection<UserRow> Users { get; } =
        [
            new UserRow("Carmen Diaz", "Cuidador", "Si"),
            new UserRow("Pablo Sanz", "Cuidador", "Si"),
            new UserRow("Laura Gil", "Admin", "Si"),
            new UserRow("Mario Perez", "Cuidador", "No")
        ];

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
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

        private void ShowPendingIntegration_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Esta pantalla ya esta preparada visualmente. El siguiente paso es conectar esta accion con la capa de datos compartida.",
                "HomeCare Manager",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
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

    public record PatientRow(string Name, string Zone, string Priority, string Phone);

    public record IncidentRow(string Patient, string Task, string Status);

    public record UserRow(string Name, string Role, string Active);
}
