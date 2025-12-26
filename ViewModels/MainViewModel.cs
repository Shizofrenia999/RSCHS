using RSCHS.Helpers;
using RSCHS.Models;
using RSCHS.Services;
using RSCHS.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RSCHS.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private Employee _currentUser;
        private Incident _selectedIncident;
        private Assignment _selectedAssignment;
        private Report _selectedReport;
        private string _newIncidentLocation;
        private string _newIncidentType;
        private string _newIncidentDescription;
        private string _newIncidentPriority = "Средний";
        private string _newIncidentPhone;
        private Incident _selectedIncidentForAssignment;
        private Employee _selectedEmployeeForAssignment;
        private Transport _selectedTransportForAssignment;
        private string _newReportContent;
        private Assignment _selectedAssignmentForReport;

        // Фильтры
        private string _incidentFilter = "";
        private string _assignmentFilter = "";
        private string _reportFilter = "";

        public Employee CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDispatcher));
                OnPropertyChanged(nameof(UserDisplayName));
            }
        }

        public string UserDisplayName => CurrentUser?.FullName ?? "Гость";
        public bool IsDispatcher => CurrentUser?.Position == "Диспетчер";

        // Свойства фильтров
        public string IncidentFilter
        {
            get => _incidentFilter;
            set
            {
                if (_incidentFilter != value)
                {
                    _incidentFilter = value;
                    OnPropertyChanged();
                    FilterIncidents();
                }
            }
        }

        public string AssignmentFilter
        {
            get => _assignmentFilter;
            set
            {
                if (_assignmentFilter != value)
                {
                    _assignmentFilter = value;
                    OnPropertyChanged();
                    FilterAssignments();
                }
            }
        }

        public string ReportFilter
        {
            get => _reportFilter;
            set
            {
                if (_reportFilter != value)
                {
                    _reportFilter = value;
                    OnPropertyChanged();
                    FilterReports();
                }
            }
        }

        // Основные коллекции данных
        public ObservableCollection<Incident> Incidents { get; } = new ObservableCollection<Incident>();
        public ObservableCollection<Assignment> Assignments { get; } = new ObservableCollection<Assignment>();
        public ObservableCollection<Employee> Employees { get; } = new ObservableCollection<Employee>();
        public ObservableCollection<Transport> Transports { get; } = new ObservableCollection<Transport>();
        public ObservableCollection<Report> Reports { get; } = new ObservableCollection<Report>();
        public ObservableCollection<Assignment> AssignmentsForReport { get; } = new ObservableCollection<Assignment>();

        // Отфильтрованные коллекции для отображения
        public ObservableCollection<Incident> FilteredIncidents { get; } = new ObservableCollection<Incident>();
        public ObservableCollection<Assignment> FilteredAssignments { get; } = new ObservableCollection<Assignment>();
        public ObservableCollection<Report> FilteredReports { get; } = new ObservableCollection<Report>();

        // Выбранные элементы для списков
        public Incident SelectedIncident
        {
            get => _selectedIncident;
            set
            {
                _selectedIncident = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedIncident));
            }
        }

        public Assignment SelectedAssignment
        {
            get => _selectedAssignment;
            set
            {
                _selectedAssignment = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedAssignment));
            }
        }

        public Report SelectedReport
        {
            get => _selectedReport;
            set
            {
                _selectedReport = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedReport));
            }
        }

        // Вспомогательные свойства для команд
        public bool HasSelectedIncident => SelectedIncident != null;
        public bool HasSelectedAssignment => SelectedAssignment != null;
        public bool HasSelectedReport => SelectedReport != null;

        // Свойства для формы создания происшествия
        public string NewIncidentLocation
        {
            get => _newIncidentLocation;
            set { _newIncidentLocation = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanCreateIncident)); }
        }

        public string NewIncidentType
        {
            get => _newIncidentType;
            set { _newIncidentType = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanCreateIncident)); }
        }

        public string NewIncidentDescription
        {
            get => _newIncidentDescription;
            set { _newIncidentDescription = value; OnPropertyChanged(); }
        }

        public string NewIncidentPriority
        {
            get => _newIncidentPriority;
            set { _newIncidentPriority = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanCreateIncident)); }
        }

        public string NewIncidentPhone
        {
            get => _newIncidentPhone;
            set { _newIncidentPhone = value; OnPropertyChanged(); }
        }

        // Свойства для формы назначения
        public Incident SelectedIncidentForAssignment
        {
            get => _selectedIncidentForAssignment;
            set
            {
                _selectedIncidentForAssignment = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanCreateAssignment));
            }
        }

        public Employee SelectedEmployeeForAssignment
        {
            get => _selectedEmployeeForAssignment;
            set
            {
                _selectedEmployeeForAssignment = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanCreateAssignment));
            }
        }

        public Transport SelectedTransportForAssignment
        {
            get => _selectedTransportForAssignment;
            set
            {
                _selectedTransportForAssignment = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanCreateAssignment));
            }
        }

        // Свойства для формы создания отчёта
        public string NewReportContent
        {
            get => _newReportContent;
            set
            {
                _newReportContent = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanCreateReport));
            }
        }

        public Assignment SelectedAssignmentForReport
        {
            get => _selectedAssignmentForReport;
            set
            {
                _selectedAssignmentForReport = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanCreateReport));
            }
        }

        // Коллекции для ComboBox
        public List<string> IncidentTypes { get; } = new List<string>
        {
            "Пожар", "ДТП", "Авария", "Спасение"
        };

        public List<string> PriorityLevels { get; } = new List<string>
        {
            "Низкий", "Средний", "Высокий"
        };

        // Валидация форм
        public bool CanCreateIncident =>
            !string.IsNullOrWhiteSpace(NewIncidentLocation) &&
            !string.IsNullOrWhiteSpace(NewIncidentType) &&
            !string.IsNullOrWhiteSpace(NewIncidentPriority) &&
            ValidatePhone(NewIncidentPhone);

        public bool CanCreateAssignment =>
            SelectedIncidentForAssignment != null &&
            SelectedEmployeeForAssignment != null &&
            SelectedTransportForAssignment != null;

        public bool CanCreateReport =>
            SelectedAssignmentForReport != null &&
            !string.IsNullOrWhiteSpace(NewReportContent) &&
            NewReportContent.Length >= 10;

        // Команды
        public ICommand LoadDataCommand { get; }
        public ICommand DeleteIncidentCommand { get; }
        public ICommand DeleteAssignmentCommand { get; }
        public ICommand CreateIncidentCommand { get; }
        public ICommand CreateAssignmentCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand LoadReportsCommand { get; }
        public ICommand CreateReportCommand { get; }
        public ICommand ExportReportCommand { get; }
        public ICommand ExportAllReportsCommand { get; }
        public ICommand ClearIncidentFilterCommand { get; }
        public ICommand ClearAssignmentFilterCommand { get; }
        public ICommand ClearReportFilterCommand { get; }

        public MainViewModel(Employee user)
        {
            CurrentUser = user;

            // Устанавливаем начальные значения для выпадающих списков
            if (string.IsNullOrEmpty(NewIncidentType) && IncidentTypes.Count > 0)
            {
                NewIncidentType = IncidentTypes[0];
            }

            if (string.IsNullOrEmpty(NewIncidentPriority) && PriorityLevels.Count > 0)
            {
                NewIncidentPriority = PriorityLevels[1]; // "Средний"
            }

            // Инициализация команд
            LoadDataCommand = new RelayCommand(async () => await LoadAllDataAsync());
            DeleteIncidentCommand = new RelayCommand(DeleteIncident, () => HasSelectedIncident);
            DeleteAssignmentCommand = new RelayCommand(DeleteAssignment, () => HasSelectedAssignment);
            CreateIncidentCommand = new RelayCommand(CreateIncident, () => CanCreateIncident);
            CreateAssignmentCommand = new RelayCommand(CreateAssignment, () => CanCreateAssignment);
            LogoutCommand = new RelayCommand(Logout);

            // Команды для отчётов
            LoadReportsCommand = new RelayCommand(async () => await LoadReportsAsync());
            CreateReportCommand = new RelayCommand(CreateReport, () => CanCreateReport);
            ExportReportCommand = new RelayCommand(ExportReport, () => HasSelectedReport);
            ExportAllReportsCommand = new RelayCommand(ExportAllReports, () => Reports.Count > 0);

            // Команды для очистки фильтров
            ClearIncidentFilterCommand = new RelayCommand(() => IncidentFilter = "");
            ClearAssignmentFilterCommand = new RelayCommand(() => AssignmentFilter = "");
            ClearReportFilterCommand = new RelayCommand(() => ReportFilter = "");

            // Загружаем данные
            _ = LoadAllDataAsync();
            _ = LoadReportsAsync();
            LoadAssignmentsForReport();
        }

        // Загрузка всех данных (асинхронная)
        private async Task LoadAllDataAsync()
        {
            try
            {
                // Загружаем параллельно для скорости
                var incidentsTask = Task.Run(() => DatabaseHelper.GetIncidents());
                var employeesTask = Task.Run(() => DatabaseHelper.GetEmployees());
                var transportsTask = Task.Run(() => DatabaseHelper.GetTransports());
                var assignmentsTask = Task.Run(() => DatabaseHelper.GetAssignments());

                await Task.WhenAll(incidentsTask, employeesTask, transportsTask, assignmentsTask);

                // Обрабатываем результаты в UI потоке
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Происшествия
                    var incidents = incidentsTask.Result;
                    Incidents.Clear();
                    foreach (var incident in incidents)
                    {
                        Incidents.Add(incident);
                    }
                    FilterIncidents();

                    // Сотрудники
                    var employees = employeesTask.Result;
                    Employees.Clear();
                    foreach (var employee in employees)
                    {
                        Employees.Add(employee);
                    }

                    // Транспорт
                    var transports = transportsTask.Result;
                    Transports.Clear();
                    foreach (var transport in transports)
                    {
                        Transports.Add(transport);
                    }

                    // Назначения
                    var assignments = assignmentsTask.Result;
                    Assignments.Clear();
                    foreach (var assignment in assignments)
                    {
                        Assignments.Add(assignment);
                    }
                    FilterAssignments();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAssignmentsForReport()
        {
            try
            {
                var assignments = DatabaseHelper.GetAssignments();
                AssignmentsForReport.Clear();

                foreach (var assignment in assignments)
                {
                    AssignmentsForReport.Add(assignment);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки назначений для отчётов: {ex.Message}");
            }
        }

        // Методы фильтрации
        private void FilterIncidents()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                FilteredIncidents.Clear();

                if (string.IsNullOrWhiteSpace(IncidentFilter))
                {
                    foreach (var incident in Incidents)
                        FilteredIncidents.Add(incident);
                }
                else
                {
                    string filter = IncidentFilter.ToLower();
                    var filtered = Incidents.Where(i =>
                        (i.Location?.ToLower().Contains(filter) ?? false) ||
                        (i.Type?.ToLower().Contains(filter) ?? false) ||
                        (i.Status?.ToLower().Contains(filter) ?? false) ||
                        (i.Priority?.ToLower().Contains(filter) ?? false) ||
                        (i.CallPhone?.Contains(filter) ?? false) ||
                        (i.Description?.ToLower().Contains(filter) ?? false) ||
                        (i.ID.ToString().Contains(filter)));

                    foreach (var incident in filtered)
                        FilteredIncidents.Add(incident);
                }
            });
        }

        private void FilterAssignments()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                FilteredAssignments.Clear();

                if (string.IsNullOrWhiteSpace(AssignmentFilter))
                {
                    foreach (var assignment in Assignments)
                        FilteredAssignments.Add(assignment);
                }
                else
                {
                    string filter = AssignmentFilter.ToLower();
                    var filtered = Assignments.Where(a =>
                        (a.IncidentLocation?.ToLower().Contains(filter) ?? false) ||
                        (a.EmployeeName?.ToLower().Contains(filter) ?? false) ||
                        (a.TransportInfo?.ToLower().Contains(filter) ?? false) ||
                        (a.DisplayInfo?.ToLower().Contains(filter) ?? false) ||
                        (a.ID.ToString().Contains(filter)));

                    foreach (var assignment in filtered)
                        FilteredAssignments.Add(assignment);
                }
            });
        }

        private void FilterReports()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                FilteredReports.Clear();

                if (string.IsNullOrWhiteSpace(ReportFilter))
                {
                    foreach (var report in Reports)
                        FilteredReports.Add(report);
                }
                else
                {
                    string filter = ReportFilter.ToLower();
                    var filtered = Reports.Where(r =>
                        (r.IncidentLocation?.ToLower().Contains(filter) ?? false) ||
                        (r.EmployeeName?.ToLower().Contains(filter) ?? false) ||
                        (r.TransportInfo?.ToLower().Contains(filter) ?? false) ||
                        (r.Content?.ToLower().Contains(filter) ?? false) ||
                        (r.CreationDate.ToString("dd.MM.yyyy HH:mm").Contains(filter)) ||
                        (r.ID.ToString().Contains(filter)));

                    foreach (var report in filtered)
                        FilteredReports.Add(report);
                }
            });
        }

        // Валидация телефона
        private bool ValidatePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true; // телефон не обязателен

            // Убираем все символы кроме цифр и +
            string cleanPhone = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());

            // Проверяем количество цифр
            int digitCount = cleanPhone.Count(char.IsDigit);
            return digitCount >= 10 && digitCount <= 15;
        }

        private void CreateIncident()
        {
            try
            {
                if (!ValidatePhone(NewIncidentPhone))
                {
                    MessageBox.Show("Введите корректный номер телефона (от 10 до 15 цифр)!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool success = DatabaseHelper.CreateIncident(
                    NewIncidentLocation.Trim(),
                    NewIncidentType.Trim(),
                    NewIncidentDescription?.Trim() ?? "",
                    NewIncidentPriority.Trim(),
                    NewIncidentPhone?.Trim() ?? ""
                );

                if (success)
                {
                    MessageBox.Show("Происшествие успешно создано!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    ClearIncidentForm();
                    _ = LoadAllDataAsync();
                }
                else
                {
                    MessageBox.Show("Не удалось создать происшествие", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearIncidentForm()
        {
            NewIncidentLocation = "";
            NewIncidentType = IncidentTypes.FirstOrDefault();
            NewIncidentDescription = "";
            NewIncidentPriority = "Средний";
            NewIncidentPhone = "";
        }

        private void DeleteIncident()
        {
            if (SelectedIncident == null) return;

            var result = MessageBox.Show(
                $"Удалить происшествие?\n\n" +
                $"ID: {SelectedIncident.ID}\n" +
                $"Место: {SelectedIncident.Location}\n" +
                $"Тип: {SelectedIncident.Type}\n" +
                $"Статус: {SelectedIncident.Status}",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (DatabaseHelper.DeleteIncident(SelectedIncident.ID))
                {
                    Incidents.Remove(SelectedIncident);
                    FilterIncidents();
                    MessageBox.Show("Происшествие удалено", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Обновляем списки
                    _ = LoadAllDataAsync();
                    LoadAssignmentsForReport();
                }
            }
        }

        private void CreateAssignment()
        {
            try
            {
                bool success = DatabaseHelper.CreateAssignment(
                    SelectedIncidentForAssignment.ID,
                    SelectedEmployeeForAssignment.ID,
                    SelectedTransportForAssignment.ID
                );

                if (success)
                {
                    // Обновляем статус происшествия
                    DatabaseHelper.UpdateIncidentStatus(SelectedIncidentForAssignment.ID, "В работе");

                    MessageBox.Show("Назначение успешно создано!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    ClearAssignmentForm();
                    _ = LoadAllDataAsync();
                    LoadAssignmentsForReport();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearAssignmentForm()
        {
            SelectedIncidentForAssignment = null;
            SelectedEmployeeForAssignment = null;
            SelectedTransportForAssignment = null;
        }

        private void DeleteAssignment()
        {
            if (SelectedAssignment == null) return;

            var result = MessageBox.Show(
                $"Удалить назначение?\n\n" +
                $"ID: {SelectedAssignment.ID}\n" +
                $"Сотрудник: {SelectedAssignment.EmployeeName}\n" +
                $"Транспорт: {SelectedAssignment.TransportInfo}",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (DatabaseHelper.DeleteAssignment(SelectedAssignment.ID))
                {
                    // Находим связанное происшествие и меняем статус
                    var relatedIncident = Incidents.FirstOrDefault(i => i.ID == SelectedAssignment.IncidentID);
                    if (relatedIncident != null)
                    {
                        DatabaseHelper.UpdateIncidentStatus(relatedIncident.ID, "Новый");
                    }

                    Assignments.Remove(SelectedAssignment);
                    FilterAssignments();
                    MessageBox.Show("Назначение удалено", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    _ = LoadAllDataAsync();
                    LoadAssignmentsForReport();
                }
            }
        }

        // Методы для отчётов
        private async Task LoadReportsAsync()
        {
            try
            {
                var reports = await Task.Run(() => DatabaseHelper.GetReports());
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Reports.Clear();
                    foreach (var report in reports)
                    {
                        Reports.Add(report);
                    }
                    FilterReports();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отчётов: {ex.Message}");
            }
        }

        private void CreateReport()
        {
            try
            {
                if (SelectedAssignmentForReport == null)
                {
                    MessageBox.Show("Выберите назначение для отчёта");
                    return;
                }

                if (string.IsNullOrWhiteSpace(NewReportContent) || NewReportContent.Length < 10)
                {
                    MessageBox.Show("Введите содержание отчёта (минимум 10 символов)");
                    return;
                }

                bool success = DatabaseHelper.CreateReport(
                    SelectedAssignmentForReport.ID,
                    NewReportContent.Trim()
                );

                if (success)
                {
                    MessageBox.Show("Отчёт успешно создан!\nСтатус происшествия изменён на 'Завершено'.", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Очищаем форму
                    NewReportContent = "";
                    SelectedAssignmentForReport = null;

                    // Обновляем все данные
                    _ = LoadAllDataAsync();
                    _ = LoadReportsAsync();
                    LoadAssignmentsForReport();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания отчёта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportReport()
        {
            if (SelectedReport == null) return;

            try
            {
                ReportGenerator.GenerateTextReport(SelectedReport);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта отчёта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportAllReports()
        {
            if (Reports.Count == 0)
            {
                MessageBox.Show("Нет отчётов для экспорта");
                return;
            }

            try
            {
                ReportGenerator.GenerateAllReportsExcel(Reports.ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта всех отчётов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Logout()
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();

            Application.Current.MainWindow?.Close();
            Application.Current.MainWindow = loginWindow;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}