using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace RSCHS
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
                    Margin = new Thickness(10)
                };

                foreach (TabItem tab in MainTabControl.Items)
                {
                    if (tab.Header.ToString() == "Происшествия")
                    {
                        tab.Content = new ScrollViewer
                        {
                            Content = dataGrid,
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                        };
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

        private void LoadAssignmentData(ComboBox incidentCombo, ComboBox employeeCombo, ComboBox vehicleCombo)
        {
            try
            {
                string incidentQuery = "SELECT ID, МестоПроисшествия + ' (' + Тип + ')' as Display FROM Происшествия WHERE Статус = 'Новый'";
                var incidents = ExecuteQuery(incidentQuery);
                incidentCombo.ItemsSource = incidents.DefaultView;
                incidentCombo.DisplayMemberPath = "Display";
                incidentCombo.SelectedValuePath = "ID";

                string employeeQuery = "SELECT ID, ФИО + ' (' + Должность + ')' as Display FROM Сотрудники WHERE Статус = 'Активен'";
                var employees = ExecuteQuery(employeeQuery);
                employeeCombo.ItemsSource = employees.DefaultView;
                employeeCombo.DisplayMemberPath = "Display";
                employeeCombo.SelectedValuePath = "ID";

                string vehicleQuery = "SELECT ID, Марка + ' ' + Модель + ' (' + ГОСномер + ')' as Display FROM Транспорт";
                var vehicles = ExecuteQuery(vehicleQuery);
                vehicleCombo.ItemsSource = vehicles.DefaultView;
                vehicleCombo.DisplayMemberPath = "Display";
                vehicleCombo.SelectedValuePath = "ID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void CreateAssignment(ComboBox incidentCombo, ComboBox employeeCombo, ComboBox vehicleCombo)
        {
            try
            {
                if (incidentCombo.SelectedValue == null || employeeCombo.SelectedValue == null || vehicleCombo.SelectedValue == null)
                {
                    MessageBox.Show("Выберите все поля!");
                    return;
                }

                int incidentId = (int)incidentCombo.SelectedValue;
                int employeeId = (int)employeeCombo.SelectedValue;
                int vehicleId = (int)vehicleCombo.SelectedValue;

                string query = @"INSERT INTO НазначенияНаВызовы (ID_Происшествия, ID_Сотрудника, ID_Транспорта, Дата) 
                        VALUES (@IncidentId, @EmployeeId, @VehicleId, GETDATE())";

                string connectionString = "Server=localhost;Database=RSCHS;User Id=sa;Password=123;TrustServerCertificate=true;";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IncidentId", incidentId);
                        command.Parameters.AddWithValue("@EmployeeId", employeeId);
                        command.Parameters.AddWithValue("@VehicleId", vehicleId);

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("Назначение успешно создано!");

                            UpdateIncidentStatus(incidentId, "В работе");

                            RefreshAllDataImmediately();


                            ClearAssignmentForm(incidentCombo, employeeCombo, vehicleCombo);
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
                            var vehicleCombo = stackPanel.Children[2] as ComboBox;

                            if (incidentCombo != null && employeeCombo != null && vehicleCombo != null)
                            {
                                LoadAssignmentData(incidentCombo, employeeCombo, vehicleCombo);
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

        private void ClearAssignmentForm(ComboBox incidentCombo, ComboBox employeeCombo, ComboBox vehicleCombo)
        {
            incidentCombo.SelectedIndex = -1;
            employeeCombo.SelectedIndex = -1;
            vehicleCombo.SelectedIndex = -1;
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
            var vehicleComboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 10), Height = 25 };

            var assignButton = new Button
            {
                Content = "Назначить на вызов",
                Background = System.Windows.Media.Brushes.Orange,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 10, 0, 0)
            };

            LoadAssignmentData(incidentComboBox, employeeComboBox, vehicleComboBox);

            assignButton.Click += (s, e) =>
            {
                CreateAssignment(incidentComboBox, employeeComboBox, vehicleComboBox);
            };

            stackPanel.Children.Add(incidentComboBox);
            stackPanel.Children.Add(employeeComboBox);
            stackPanel.Children.Add(vehicleComboBox);
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
                    Height = 300
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

                var refreshButton = new Button
                {
                    Content = "Обновить назначения",
                    Margin = new Thickness(0, 10, 0, 0),
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

                // Выводим для отладки
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