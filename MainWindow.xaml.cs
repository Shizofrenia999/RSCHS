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
                        tab.Header.ToString() == "Назначения")
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
            string query = @"
                SELECT 
                    н.ID,
                    п.МестоПроисшествия,
                    с.ФИО as Сотрудник,
                    т.Марка + ' ' + т.Модель + ' (' + т.ГОСномер + ')' as Транспорт,
                    н.Дата
                FROM НазначенияНаВызовы н
                JOIN Происшествия п ON н.ID_Происшествия = п.ID
                JOIN Сотрудники с ON н.ID_Сотрудника = с.ID
                JOIN Транспорт т ON н.ID_Транспорта = т.ID
                ORDER BY н.Дата DESC";

            return ExecuteQuery(query);
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