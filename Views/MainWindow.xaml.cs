using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using RSCHS.Models;
using RSCHS.Services;

namespace RSCHS.Views
{
    public partial class MainWindow : Window
    {
        private Employee currentUser;

        public MainWindow(Employee user)
        {
            InitializeComponent();
            currentUser = user;
            InitializeInterface();
            LoadData();
        }

        public MainWindow()
        {
            InitializeComponent();
            currentUser = new Employee { FullName = "Тестовый Диспетчер", Position = "Диспетчер" };
            InitializeInterface();
            LoadData();
        }

        private void InitializeInterface()
        {
            txtDispatcherName.Text = currentUser.FullName;

            if (currentUser.Position != "Диспетчер")
            {
                foreach (TabItem tab in MainTabControl.Items)
                {
                    if (tab.Header.ToString() == "Новое происшествие" ||
                        tab.Header.ToString() == "Назначения" ||
                        tab.Header.ToString() == "Назначить вызов")
                    {
                        tab.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void LoadData()
        {
            LoadIncidents();
            AddContentToTabs();
        }

        private void AddContentToTabs()
        {
            foreach (TabItem tab in MainTabControl.Items)
            {
                switch (tab.Header.ToString())
                {
                    case "Происшествия":
                        break;

                    case "Новое происшествие":
                        tab.Content = CreateNewIncidentForm();
                        break;

                    case "Назначить вызов":
                        tab.Content = CreateAssignmentForm();
                        break;

                    case "Назначения":
                        tab.Content = CreateAssignmentsTab();
                        break;

                    case "Отчеты":
                        tab.Content = CreateReportsTab();
                        break;
                }
            }
        }

        private void LoadIncidents()
        {
            try
            {
                Console.WriteLine("Начало загрузки происшествий...");

                var incidents = DatabaseHelper.GetIncidents();
                Console.WriteLine($"Загружено происшествий: {incidents?.Count ?? 0}");

                var dataGrid = new DataGrid
                {
                    AutoGenerateColumns = true,
                    ItemsSource = incidents,
                    IsReadOnly = true,
                    Margin = new Thickness(10),
                    SelectionMode = DataGridSelectionMode.Single,
                    SelectionUnit = DataGridSelectionUnit.FullRow
                };

                dataGrid.ContextMenu = CreateIncidentsContextMenu(dataGrid);

                foreach (TabItem tab in MainTabControl.Items)
                {
                    if (tab.Header.ToString() == "Происшествия")
                    {
                        var stackPanel = new StackPanel();

                        var buttonPanel = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(10, 0, 10, 10)
                        };

                        var refreshButton = new Button
                        {
                            Content = "Обновить список",
                            Margin = new Thickness(0, 0, 10, 0),
                            Width = 120
                        };
                        refreshButton.Click += (s, e) => RefreshIncidents(dataGrid);

                        var deleteButton = new Button
                        {
                            Content = "Удалить выбранное",
                            Background = System.Windows.Media.Brushes.Red,
                            Foreground = System.Windows.Media.Brushes.White,
                            Width = 120
                        };
                        deleteButton.Click += (s, e) => DeleteSelectedIncident(dataGrid);

                        buttonPanel.Children.Add(refreshButton);
                        buttonPanel.Children.Add(deleteButton);

                        stackPanel.Children.Add(buttonPanel);
                        stackPanel.Children.Add(new ScrollViewer
                        {
                            Content = dataGrid,
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                        });

                        tab.Content = stackPanel;
                        Console.WriteLine("Вкладка 'Происшествия' заполнена");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА в LoadIncidents: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                MessageBox.Show($"Ошибка загрузки происшествий: {ex.Message}");
            }
        }

        private void RefreshIncidents(DataGrid dataGrid)
        {
            try
            {
                var incidents = DatabaseHelper.GetIncidents();
                dataGrid.ItemsSource = null;
                dataGrid.ItemsSource = incidents;

                Console.WriteLine($"Список происшествий обновлен. Записей: {incidents.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}");
            }
        }

        private void ShowIncidentDetails(DataGrid dataGrid)
        {
            try
            {
                if (dataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Выберите происшествие!");
                    return;
                }

                var incident = dataGrid.SelectedItem as Incident;
                if (incident == null) return;

                string details =
                    $"ДЕТАЛИ ПРОИСШЕСТВИЯ\n\n" +
                    $"ID: {incident.ID}\n" +
                    $"Место: {incident.Location}\n" +
                    $"Тип: {incident.Type}\n" +
                    $"Приоритет: {incident.Priority}\n" +
                    $"Статус: {incident.Status}\n" +
                    $"Дата: {incident.Date}\n" +
                    $"Телефон вызова: {incident.CallPhone}\n\n" +
                    $"Описание:\n{incident.Description}";

                MessageBox.Show(details, "Детали происшествия");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void SwitchToAssignmentTab(DataGrid dataGrid)
        {
            try
            {
                if (dataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Выберите происшествие для назначения!");
                    return;
                }

                var incident = dataGrid.SelectedItem as Incident;
                if (incident == null) return;

                if (incident.Status != "Новый")
                {
                    MessageBox.Show("Можно назначать только новые происшествия!");
                    return;
                }

                foreach (TabItem tab in MainTabControl.Items)
                {
                    if (tab.Header.ToString() == "Назначить вызов")
                    {
                        MainTabControl.SelectedItem = tab;

                        var stackPanel = tab.Content as StackPanel;
                        if (stackPanel != null && stackPanel.Children.Count > 0)
                        {
                            var incidentCombo = stackPanel.Children[0] as ComboBox;
                            if (incidentCombo != null)
                            {
                                foreach (var item in incidentCombo.Items)
                                {
                                    var rowView = item as DataRowView;
                                    if (rowView != null && (int)rowView["ID"] == incident.ID)
                                    {
                                        incidentCombo.SelectedItem = item;
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private ContextMenu CreateIncidentsContextMenu(DataGrid dataGrid)
        {
            var contextMenu = new ContextMenu();

            var menuItemDelete = new MenuItem
            {
                Header = "Удалить происшествие",
                Background = System.Windows.Media.Brushes.Red,
                Foreground = System.Windows.Media.Brushes.White
            };
            menuItemDelete.Click += (s, e) => DeleteSelectedIncident(dataGrid);

            var menuItemDetails = new MenuItem { Header = "Просмотреть детали" };
            menuItemDetails.Click += (s, e) => ShowIncidentDetails(dataGrid);

            var menuItemAssign = new MenuItem { Header = "Назначить на вызов" };
            menuItemAssign.Click += (s, e) => SwitchToAssignmentTab(dataGrid);

            contextMenu.Items.Add(menuItemDetails);
            contextMenu.Items.Add(menuItemAssign);
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(menuItemDelete);

            return contextMenu;
        }

        private void DeleteSelectedIncident(DataGrid dataGrid)
        {
            try
            {
                if (dataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Выберите происшествие для удаления!");
                    return;
                }

                var incident = dataGrid.SelectedItem as Incident;
                if (incident == null) return;

                if (HasAssignments(incident.ID))
                {
                    MessageBox.Show("Нельзя удалить происшествие с активными назначениями!\nСначала удалите назначения.");
                    return;
                }

                var result = MessageBox.Show(
                    $"Удалить происшествие?\n\n" +
                    $"Место: {incident.Location}\n" +
                    $"Тип: {incident.Type}\n" +
                    $"Статус: {incident.Status}",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (DeleteIncidentFromDatabase(incident.ID))
                    {
                        MessageBox.Show("Происшествие удалено!");
                        RefreshIncidents(dataGrid);

                        RefreshAssignmentFormData();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при удалении происшествия!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}");
            }
        }

        private bool HasAssignments(int incidentId)
        {
            try
            {
                string query = @"
            SELECT 
                COUNT(*) as AssignmentsCount,
                STRING_AGG(CAST(н.ID as NVARCHAR(MAX)), ', ') as AssignmentIds
            FROM НазначенияНаВызовы н
            WHERE н.ID_Происшествия = @IncidentId";

                string connectionString = "Server=localhost;Database=RSCHS;User Id=sa;Password=123;TrustServerCertificate=true;";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IncidentId", incidentId);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int count = (int)reader["AssignmentsCount"];
                                if (count > 0)
                                {
                                    string assignmentIds = reader["AssignmentIds"].ToString();
                                    MessageBox.Show(
                                        $"Нельзя удалить происшествие!\n\n" +
                                        $"Найдено связанных назначений: {count}\n" +
                                        $"ID назначений: {assignmentIds}\n\n" +
                                        "Сначала удалите эти назначения во вкладке 'Назначения'.",
                                        "Ошибка удаления",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка проверки назначений: {ex.Message}");
                return true;
            }
        }

        private bool DeleteIncidentFromDatabase(int incidentId)
        {
            try
            {
                string connectionString = "Server=localhost;Database=RSCHS;User Id=sa;Password=123;TrustServerCertificate=true;";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlTransaction transaction = connection.BeginTransaction();

                    try
                    {
                        string deleteReportsQuery = @"
                    DELETE FROM Отчёты 
                    WHERE ID_НазначенияНаВызовы IN (
                        SELECT ID FROM НазначенияНаВызовы 
                        WHERE ID_Происшествия = @IncidentId
                    )";

                        using (var cmd1 = new SqlCommand(deleteReportsQuery, connection, transaction))
                        {
                            cmd1.Parameters.AddWithValue("@IncidentId", incidentId);
                            cmd1.ExecuteNonQuery();
                        }

                        string deleteAssignmentsQuery = "DELETE FROM НазначенияНаВызовы WHERE ID_Происшествия = @IncidentId";
                        using (var cmd2 = new SqlCommand(deleteAssignmentsQuery, connection, transaction))
                        {
                            cmd2.Parameters.AddWithValue("@IncidentId", incidentId);
                            cmd2.ExecuteNonQuery();
                        }

                        string deleteIncidentQuery = "DELETE FROM Происшествия WHERE ID = @IncidentId";
                        using (var cmd3 = new SqlCommand(deleteIncidentQuery, connection, transaction))
                        {
                            cmd3.Parameters.AddWithValue("@IncidentId", incidentId);
                            int result = cmd3.ExecuteNonQuery();

                            transaction.Commit();

                            Console.WriteLine($"Удалено происшествие ID: {incidentId}");
                            return result > 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Ошибка удаления (транзакция откатана): {ex.Message}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления происшествия: {ex.Message}");
                return false;
            }
        }


        private void LoadAssignmentData(ComboBox incidentCombo, ComboBox employeeCombo, ComboBox transportCombo)
        {
            try
            {
                Console.WriteLine("=== Загрузка данных для формы назначения ===");

                incidentCombo.ItemsSource = null;
                incidentCombo.Items.Clear();
                incidentCombo.IsEnabled = true;

                employeeCombo.ItemsSource = null;
                employeeCombo.Items.Clear();
                employeeCombo.IsEnabled = true;

                transportCombo.ItemsSource = null;
                transportCombo.Items.Clear();
                transportCombo.IsEnabled = true;

                Console.WriteLine("Загрузка происшествий...");
                string incidentQuery = @"
            SELECT 
                ID, 
                '#' + CAST(ID as NVARCHAR(10)) + ' - ' + 
                МестоПроисшествия + ' (' + Тип + ')' as Display
            FROM Происшествия 
            WHERE Статус = 'Новый' 
            AND ID IN (SELECT ID FROM Происшествия) -- Двойная проверка
            ORDER BY ID DESC";

                var incidents = ExecuteQuery(incidentQuery);
                Console.WriteLine($"Найдено активных происшествий: {incidents.Rows.Count}");

                if (incidents.Rows.Count > 0)
                {
                    incidentCombo.ItemsSource = incidents.DefaultView;
                    incidentCombo.DisplayMemberPath = "Display";
                    incidentCombo.SelectedValuePath = "ID";
                }
                else
                {
                    incidentCombo.Items.Add("Нет доступных происшествий");
                    incidentCombo.IsEnabled = false;
                    incidentCombo.SelectedIndex = 0;
                }

                Console.WriteLine("Загрузка сотрудников...");
                string employeeQuery = @"
            SELECT 
                ID,
                ФИО + ' (' + Должность + ')' as Display
            FROM Сотрудники 
            WHERE Статус = 'Активен' 
            AND ID NOT IN (
                SELECT н.ID_Сотрудника 
                FROM НазначенияНаВызовы н
                INNER JOIN Происшествия п ON н.ID_Происшествия = п.ID
                WHERE п.Статус = 'В работе'
            )
            ORDER BY ФИО";

                var employees = ExecuteQuery(employeeQuery);
                Console.WriteLine($"Найдено доступных сотрудников: {employees.Rows.Count}");

                if (employees.Rows.Count > 0)
                {
                    employeeCombo.ItemsSource = employees.DefaultView;
                    employeeCombo.DisplayMemberPath = "Display";
                    employeeCombo.SelectedValuePath = "ID";
                }
                else
                {
                    employeeCombo.Items.Add("Нет доступных сотрудников");
                    employeeCombo.IsEnabled = false;
                    employeeCombo.SelectedIndex = 0;
                }

                Console.WriteLine("Загрузка транспорта...");
                string TransportQuery = @"
            SELECT 
                ID,
                Марка + ' ' + Модель + ' (' + ГОСномер + ')' as Display
            FROM Транспорт 
            WHERE ID NOT IN (
                SELECT н.ID_Транспорта 
                FROM НазначенияНаВызовы н
                INNER JOIN Происшествия п ON н.ID_Происшествия = п.ID
                WHERE п.Статус = 'В работе'
            )
            ORDER BY Марка, Модель";

                var Transports = ExecuteQuery(TransportQuery);
                Console.WriteLine($"Найдено доступного транспорта: {Transports.Rows.Count}");

                if (Transports.Rows.Count > 0)
                {
                    transportCombo.ItemsSource = Transports.DefaultView;
                    transportCombo.DisplayMemberPath = "Display";
                    transportCombo.SelectedValuePath = "ID";
                }
                else
                {
                    transportCombo.Items.Add("Нет доступного транспорта");
                    transportCombo.IsEnabled = false;
                    transportCombo.SelectedIndex = 0;
                }

                if (incidents.Rows.Count > 0)
                {
                    Console.WriteLine("Список доступных происшествий:");
                    foreach (DataRow row in incidents.Rows)
                    {
                        Console.WriteLine($"  ID: {row["ID"]}, Display: {row["Display"]}");
                    }
                }

                incidentCombo.SelectedIndex = -1;
                employeeCombo.SelectedIndex = -1;
                transportCombo.SelectedIndex = -1;

                Console.WriteLine("=== Загрузка данных завершена ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА в LoadAssignmentData: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                incidentCombo.Items.Clear();
                incidentCombo.Items.Add($"Ошибка: {ex.Message}");
                incidentCombo.SelectedIndex = 0;
                incidentCombo.IsEnabled = false;

                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}\n\nПроверьте подключение к БД.",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateAssignment(ComboBox incidentCombo, ComboBox employeeCombo, ComboBox TransportCombo)
        {
            try
            {
                if (incidentCombo.SelectedValue == null || employeeCombo.SelectedValue == null || TransportCombo.SelectedValue == null)
                {
                    MessageBox.Show("Выберите все поля!");
                    return;
                }

                int incidentId = (int)incidentCombo.SelectedValue;
                int employeeId = (int)employeeCombo.SelectedValue;
                int TransportId = (int)TransportCombo.SelectedValue;

                string query = @"INSERT INTO НазначенияНаВызовы (ID_Происшествия, ID_Сотрудника, ID_Транспорта, Дата) 
                        VALUES (@IncidentId, @EmployeeId, @TransportId, GETDATE())";

                string connectionString = "Server=localhost;Database=RSCHS;User Id=sa;Password=123;TrustServerCertificate=true;";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IncidentId", incidentId);
                        command.Parameters.AddWithValue("@EmployeeId", employeeId);
                        command.Parameters.AddWithValue("@TransportId", TransportId);

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("Назначение успешно создано!");

                            UpdateIncidentStatus(incidentId, "В работе");

                            RefreshAllDataImmediately();


                            ClearAssignmentForm(incidentCombo, employeeCombo, TransportCombo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания назначения: {ex.Message}");
            }
        }
        private void RefreshAllDataImmediately()
        {
            try
            {
                LoadIncidents();

                RefreshAssignmentsTabImmediately();

                RefreshAssignmentFormData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления данных: {ex.Message}");
            }
        }

        private void RefreshAssignmentsTabImmediately()
        {
            try
            {
                foreach (TabItem tab in MainTabControl.Items)
                {
                    if (tab.Header.ToString() == "Назначения")
                    {
                        var stackPanel = tab.Content as StackPanel;
                        if (stackPanel != null)
                        {
                            foreach (var child in stackPanel.Children)
                            {
                                if (child is DataGrid dataGrid)
                                {
                                    var assignments = LoadAssignments();
                                    dataGrid.ItemsSource = null;
                                    dataGrid.ItemsSource = assignments.DefaultView;
                                    Console.WriteLine($"DataGrid обновлен. Записей: {assignments.Rows.Count}");
                                    return;
                                }
                            }
                        }

                        tab.Content = CreateAssignmentsTab();
                        Console.WriteLine("Вкладка 'Назначения' пересоздана");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления вкладки назначений: {ex.Message}");
            }
        }

        private void RefreshAssignmentFormData()
        {
            try
            {
                foreach (TabItem tab in MainTabControl.Items)
                {
                    if (tab.Header.ToString() == "Назначить вызов")
                    {
                        var stackPanel = tab.Content as StackPanel;
                        if (stackPanel != null && stackPanel.Children.Count >= 3)
                        {
                            var incidentCombo = stackPanel.Children[0] as ComboBox;
                            var employeeCombo = stackPanel.Children[1] as ComboBox;
                            var TransportCombo = stackPanel.Children[2] as ComboBox;

                            if (incidentCombo != null && employeeCombo != null && TransportCombo != null)
                            {
                                LoadAssignmentData(incidentCombo, employeeCombo, TransportCombo);
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления формы назначения: {ex.Message}");
            }
        }

        private void ClearAssignmentForm(ComboBox incidentCombo, ComboBox employeeCombo, ComboBox TransportCombo)
        {
            incidentCombo.SelectedIndex = -1;
            employeeCombo.SelectedIndex = -1;
            TransportCombo.SelectedIndex = -1;
        }

        private void UpdateIncidentStatus(int incidentId, string status)
        {
            try
            {
                string query = "UPDATE Происшествия SET Статус = @Status WHERE ID = @IncidentId";
                string connectionString = "Server=localhost;Database=RSCHS;User Id=sa;Password=123;TrustServerCertificate=true;";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Status", status);
                        command.Parameters.AddWithValue("@IncidentId", incidentId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления статуса: {ex.Message}");
            }
        }

        private void RefreshAllData()
        {
            LoadIncidents();

            foreach (TabItem tab in MainTabControl.Items)
            {
                if (tab.Header.ToString() == "Назначения")
                {
                    var content = tab.Content as StackPanel;
                    if (content != null)
                    {
                        foreach (var child in content.Children)
                        {
                            if (child is DataGrid dataGrid)
                            {
                                var assignments = LoadAssignments();
                                dataGrid.ItemsSource = assignments.DefaultView;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private StackPanel CreateAssignmentForm()
        {
            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            stackPanel.Children.Add(new TextBlock { Text = "Выберите происшествие:", FontWeight = FontWeights.Bold });
            var incidentComboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 10), Height = 25 };

            stackPanel.Children.Add(new TextBlock { Text = "Выберите сотрудника:", FontWeight = FontWeights.Bold });
            var employeeComboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 10), Height = 25 };

            stackPanel.Children.Add(new TextBlock { Text = "Выберите транспорт:", FontWeight = FontWeights.Bold });
            var TransportComboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 10), Height = 25 };

            var assignButton = new Button
            {
                Content = "Назначить на вызов",
                Background = System.Windows.Media.Brushes.Orange,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 10, 0, 0)
            };

            LoadAssignmentData(incidentComboBox, employeeComboBox, TransportComboBox);

            assignButton.Click += (s, e) =>
            {
                CreateAssignment(incidentComboBox, employeeComboBox, TransportComboBox);
            };

            stackPanel.Children.Add(incidentComboBox);
            stackPanel.Children.Add(employeeComboBox);
            stackPanel.Children.Add(TransportComboBox);
            stackPanel.Children.Add(assignButton);

            return stackPanel;
        }

        private StackPanel CreateAssignmentsTab()
        {
            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            try
            {
                var dataGrid = new DataGrid
                {
                    AutoGenerateColumns = true,
                    Margin = new Thickness(0, 10, 0, 10),
                    Height = 300,
                    SelectionMode = DataGridSelectionMode.Single,
                    SelectionUnit = DataGridSelectionUnit.FullRow
                };

                var assignments = LoadAssignments();
                dataGrid.ItemsSource = assignments.DefaultView;

                stackPanel.Children.Add(new TextBlock
                {
                    Text = "Текущие назначения на вызовы:",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 10)
                });

                stackPanel.Children.Add(dataGrid);

                var deleteButton = new Button
                {
                    Content = "Удалить выбранное назначение",
                    Margin = new Thickness(0, 10, 0, 5),
                    Width = 200,
                    Background = System.Windows.Media.Brushes.Red,
                    Foreground = System.Windows.Media.Brushes.White
                };

                deleteButton.Click += (s, e) =>
                {
                    try
                    {
                        if (dataGrid.SelectedItem == null)
                        {
                            MessageBox.Show("Выберите назначение для удаления!");
                            return;
                        }

                        var selectedRow = (dataGrid.SelectedItem as DataRowView)?.Row;
                        if (selectedRow != null)
                        {
                            int assignmentId = (int)selectedRow["ID"];
                            int incidentId = GetIncidentIdFromAssignment(assignmentId);

                            var result = MessageBox.Show(
                                "Удалить это назначение?\n\n" +
                                $"Место: {selectedRow["МестоПроисшествия"]}\n" +
                                $"Сотрудник: {selectedRow["Сотрудник"]}",
                                "Подтверждение удаления",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning);

                            if (result == MessageBoxResult.Yes)
                            {
                                if (DeleteAssignment(assignmentId))
                                {
                                    UpdateIncidentStatus(incidentId, "Новый");

                                    MessageBox.Show("Назначение удалено!");
                                    RefreshAllDataImmediately();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}");
                    }
                };

                stackPanel.Children.Add(deleteButton);

                var refreshButton = new Button
                {
                    Content = "Обновить назначения",
                    Margin = new Thickness(0, 5, 0, 0),
                    Width = 150
                };
                refreshButton.Click += (s, e) => RefreshAssignments(dataGrid);
                stackPanel.Children.Add(refreshButton);
            }
            catch (Exception ex)
            {
                stackPanel.Children.Add(new TextBlock
                {
                    Text = $"Ошибка загрузки назначений: {ex.Message}",
                    Foreground = System.Windows.Media.Brushes.Red
                });
            }

            return stackPanel;
        }

        private bool DeleteAssignment(int assignmentId)
        {
            try
            {
                string query = "DELETE FROM НазначенияНаВызовы WHERE ID = @AssignmentId";
                string connectionString = "Server=localhost;Database=RSCHS;User Id=sa;Password=123;TrustServerCertificate=true;";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AssignmentId", assignmentId);
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления назначения: {ex.Message}");
                return false;
            }
        }

        private int GetIncidentIdFromAssignment(int assignmentId)
        {
            try
            {
                string query = "SELECT ID_Происшествия FROM НазначенияНаВызовы WHERE ID = @AssignmentId";
                string connectionString = "Server=localhost;Database=RSCHS;User Id=sa;Password=123;TrustServerCertificate=true;";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AssignmentId", assignmentId);
                        return (int)command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения ID происшествия: {ex.Message}");
                return -1;
            }
        }


        private DataTable LoadAssignments()
        {
            try
            {
                string query = @"
            SELECT 
                н.ID,
                п.МестоПроисшествия,
                с.ФИО as Сотрудник,
                т.Марка + ' ' + т.Модель + ' (' + т.ГОСномер + ')' as Транспорт,
                н.Дата,
                п.Статус as СтатусПроисшествия
            FROM НазначенияНаВызовы н
            JOIN Происшествия п ON н.ID_Происшествия = п.ID
            JOIN Сотрудники с ON н.ID_Сотрудника = с.ID
            JOIN Транспорт т ON н.ID_Транспорта = т.ID
            ORDER BY н.Дата DESC";

                var result = ExecuteQuery(query);
                Console.WriteLine($"Загружено назначений из БД: {result.Rows.Count}");

                if (result.Rows.Count > 0)
                {
                    Console.WriteLine("=== ТЕКУЩИЕ НАЗНАЧЕНИЯ ===");
                    foreach (DataRow row in result.Rows)
                    {
                        Console.WriteLine($"ID: {row["ID"]}, Место: {row["МестоПроисшествия"]}, Сотрудник: {row["Сотрудник"]}");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки назначений: {ex.Message}");
                return new DataTable();
            }
        }

        private StackPanel CreateReportsTab()
        {
            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Система отчетности",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            });

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Для просмотра и создания отчетов выберите завершенное назначение из списка выше.",
                TextWrapping = TextWrapping.Wrap
            });

            var createReportButton = new Button
            {
                Content = "Создать отчет по завершенному вызову",
                Margin = new Thickness(0, 20, 0, 0),
                Width = 250,
                Background = System.Windows.Media.Brushes.Green,
                Foreground = System.Windows.Media.Brushes.White
            };
            stackPanel.Children.Add(createReportButton);

            return stackPanel;
        }

        private StackPanel CreateNewIncidentForm()
        {
            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            var locationTextBox = new TextBox { Margin = new Thickness(0, 5, 0, 5), Height = 25 };
            var typeComboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 5), Height = 25 };
            typeComboBox.Items.Add("Пожар");
            typeComboBox.Items.Add("ДТП");
            typeComboBox.Items.Add("Авария");
            typeComboBox.Items.Add("Спасение");

            var priorityComboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 5), Height = 25 };
            priorityComboBox.Items.Add("Низкий");
            priorityComboBox.Items.Add("Средний");
            priorityComboBox.Items.Add("Высокий");
            priorityComboBox.SelectedIndex = 1;

            var descriptionTextBox = new TextBox
            {
                Margin = new Thickness(0, 5, 0, 5),
                Height = 60,
                AcceptsReturn = true
            };

            var phoneTextBox = new TextBox { Margin = new Thickness(0, 5, 0, 5), Height = 25 };
            var createButton = new Button
            {
                Content = "Создать происшествие",
                Margin = new Thickness(0, 10, 0, 0),
                Background = System.Windows.Media.Brushes.Red,
                Foreground = System.Windows.Media.Brushes.White
            };

            createButton.Click += (s, e) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(locationTextBox.Text) ||
                        typeComboBox.SelectedItem == null ||
                        priorityComboBox.SelectedItem == null)
                    {
                        MessageBox.Show("Заполните все обязательные поля!");
                        return;
                    }

                    bool success = DatabaseHelper.CreateIncident(
                        locationTextBox.Text,
                        typeComboBox.SelectedItem.ToString(),
                        descriptionTextBox.Text,
                        priorityComboBox.SelectedItem.ToString(),
                        phoneTextBox.Text
                    );

                    if (success)
                    {
                        MessageBox.Show("Происшествие успешно создано!");
                        locationTextBox.Text = "";
                        typeComboBox.SelectedIndex = -1;
                        priorityComboBox.SelectedIndex = 1;
                        descriptionTextBox.Text = "";
                        phoneTextBox.Text = "";

                        LoadIncidents();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка создания происшествия: {ex.Message}");
                }
            };

            stackPanel.Children.Add(new TextBlock { Text = "Место происшествия:" });
            stackPanel.Children.Add(locationTextBox);

            stackPanel.Children.Add(new TextBlock { Text = "Тип происшествия:" });
            stackPanel.Children.Add(typeComboBox);

            stackPanel.Children.Add(new TextBlock { Text = "Приоритет:" });
            stackPanel.Children.Add(priorityComboBox);

            stackPanel.Children.Add(new TextBlock { Text = "Описание:" });
            stackPanel.Children.Add(descriptionTextBox);

            stackPanel.Children.Add(new TextBlock { Text = "Телефон вызова:" });
            stackPanel.Children.Add(phoneTextBox);

            stackPanel.Children.Add(createButton);

            return stackPanel;
        }

        private DataTable ExecuteQuery(string query)
        {
            string connectionString = "Server=localhost;Database=RSCHS;User Id=sa;Password=123;TrustServerCertificate=true;";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    var dataTable = new DataTable();
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                    return dataTable;
                }
            }
        }

        private void RefreshAssignments(DataGrid dataGrid)
        {
            try
            {
                var assignments = LoadAssignments();
                dataGrid.ItemsSource = assignments.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}");
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}