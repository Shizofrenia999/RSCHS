using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using RSCHS.Helpers;
using RSCHS.Models;
using RSCHS.Services;
using RSCHS.Views;

namespace RSCHS.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IDatabaseService _databaseService;
        private Employee _currentUser;

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

        // Коллекции данных
        public ObservableCollection<Incident> Incidents { get; } = new ObservableCollection<Incident>();
        public ObservableCollection<Assignment> Assignments { get; } = new ObservableCollection<Assignment>();
        public ObservableCollection<Employee> Employees { get; } = new ObservableCollection<Employee>();
        public ObservableCollection<Transport> Transports { get; } = new ObservableCollection<Transport>();

        // Выбранные элементы
        private Incident _selectedIncident;
        private Assignment _selectedAssignment;

        public Incident SelectedIncident
        {
            get => _selectedIncident;
            set
            {
                _selectedIncident = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanDeleteIncident));
            }
        }

        public Assignment SelectedAssignment
        {
            get => _selectedAssignment;
            set
            {
                _selectedAssignment = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanDeleteAssignment));
            }
        }

        public bool CanDeleteIncident => SelectedIncident != null;
        public bool CanDeleteAssignment => SelectedAssignment != null;

        // Команды
        public ICommand LoadDataCommand { get; }
        public ICommand DeleteIncidentCommand { get; }
        public ICommand DeleteAssignmentCommand { get; }
        public ICommand CreateIncidentCommand { get; }
        public ICommand CreateAssignmentCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;

            // Инициализация команд
            LoadDataCommand = new RelayCommand(LoadAllData);
            DeleteIncidentCommand = new RelayCommand(DeleteIncident, () => CanDeleteIncident);
            DeleteAssignmentCommand = new RelayCommand(DeleteAssignment, () => CanDeleteAssignment);
            CreateIncidentCommand = new RelayCommand(CreateIncident);
            CreateAssignmentCommand = new RelayCommand(CreateAssignment);
            LogoutCommand = new RelayCommand(Logout);

            // Загружаем данные
            LoadAllData();
        }

        private async void LoadAllData()
        {
            try
            {
                // Загружаем все данные асинхронно
                var incidents = await Task.Run(() => _databaseService.GetIncidents());
                var assignments = await Task.Run(() => _databaseService.GetAssignments());
                var employees = await Task.Run(() => _databaseService.GetEmployees());
                var transports = await Task.Run(() => _databaseService.GetTransports());

                Application.Current.Dispatcher.Invoke(() =>
                {
                    UpdateCollection(Incidents, incidents);
                    UpdateCollection(Assignments, assignments);
                    UpdateCollection(Employees, employees);
                    UpdateCollection(Transports, transports);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCollection<T>(ObservableCollection<T> collection, IEnumerable<T> newItems)
        {
            collection.Clear();
            foreach (var item in newItems)
            {
                collection.Add(item);
            }
        }

        private void DeleteIncident()
        {
            // Реализация удаления
        }

        private void DeleteAssignment()
        {
            // Реализация удаления
        }

        private void CreateIncident()
        {
            // Реализация создания
        }

        private void CreateAssignment()
        {
            // Реализация создания
        }

        private void Logout()
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Application.Current.MainWindow?.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}