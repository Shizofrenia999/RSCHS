using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Windows;
using RSCHS.Models;

namespace RSCHS.Services
{
    public static class DatabaseHelper
    {
        private static string connectionString = "Server=WIN-VH3ASTRV6E6\\SQLEXPRESS;Database=RSCHS;Trusted_Connection=True;"; // дом
        //private static string connectionString = "Server=localhost;Database=RSCHS;User Id=sa;Password=123;TrustServerCertificate=true;";  // колледж

        public static Employee AuthenticateUser(string login, string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID, Логин, ФИО, Должность, Телефон, Статус FROM Сотрудники WHERE Логин = @Login AND Пароль = @Password AND Статус = 'Активен'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Login", login);
                    command.Parameters.AddWithValue("@Password", password);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Employee
                            {
                                ID = (int)reader["ID"],
                                Login = reader["Логин"].ToString(),
                                FullName = reader["ФИО"].ToString(),
                                Position = reader["Должность"].ToString(),
                                Phone = reader["Телефон"].ToString(),
                                Status = reader["Статус"].ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }

        public static List<Employee> GetEmployees()
        {
            List<Employee> employees = new List<Employee>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID, Логин, ФИО, Должность, Телефон, Статус FROM Сотрудники WHERE Статус = 'Активен' ORDER BY ФИО";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        employees.Add(new Employee
                        {
                            ID = (int)reader["ID"],
                            Login = reader["Логин"].ToString(),
                            FullName = reader["ФИО"].ToString(),
                            Position = reader["Должность"].ToString(),
                            Phone = reader["Телефон"].ToString(),
                            Status = reader["Статус"].ToString()
                        });
                    }
                }
            }
            return employees;
        }

        public static List<Incident> GetIncidents()
        {
            List<Incident> incidents = new List<Incident>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Происшествия ORDER BY Дата DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        incidents.Add(new Incident
                        {
                            ID = (int)reader["ID"],
                            Location = reader["МестоПроисшествия"].ToString(),
                            Type = reader["Тип"].ToString(),
                            Description = reader["Описание"].ToString(),
                            Priority = reader["Приоритет"].ToString(),
                            Status = reader["Статус"].ToString(),
                            CallPhone = reader["ТелефонВызова"].ToString(),
                            Date = (DateTime)reader["Дата"]
                        });
                    }
                }
            }
            return incidents;
        }

        public static bool CreateIncident(string location, string type, string description, string priority, string callPhone)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"INSERT INTO Происшествия (МестоПроисшествия, Тип, Описание, Приоритет, Статус, ТелефонВызова, Дата) 
                           VALUES (@Location, @Type, @Description, @Priority, 'Новый', @CallPhone, GETDATE())";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Location", location);
                        command.Parameters.AddWithValue("@Type", type);
                        command.Parameters.AddWithValue("@Description", description);
                        command.Parameters.AddWithValue("@Priority", priority);
                        command.Parameters.AddWithValue("@CallPhone", callPhone);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания происшествия: {ex.Message}");
                return false;
            }
        }

        public static bool DeleteIncident(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM Происшествия WHERE ID = @Id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления происшествия: {ex.Message}");
                return false;
            }
        }

        public static bool UpdateIncidentStatus(int id, string status)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Происшествия SET Статус = @Status WHERE ID = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Status", status);
                    command.Parameters.AddWithValue("@Id", id);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public static List<Transport> GetTransports()
        {
            List<Transport> transports = new List<Transport>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID, Марка as Brand, Модель as Model, ГОСномер as LicensePlate FROM Транспорт ORDER BY Марка, Модель";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        transports.Add(new Transport
                        {
                            ID = (int)reader["ID"],
                            Brand = reader["Brand"].ToString(),
                            Model = reader["Model"].ToString(),
                            LicensePlate = reader["LicensePlate"].ToString()
                        });
                    }
                }
            }
            return transports;
        }

        public static List<Assignment> GetAssignments()
        {
            List<Assignment> assignments = new List<Assignment>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
            SELECT 
                н.ID,
                н.ID_Происшествия as IncidentID,
                н.ID_Сотрудника as EmployeeID,
                н.ID_Транспорта as TransportID,
                н.Дата,
                п.МестоПроисшествия as IncidentLocation,
                с.ФИО as EmployeeName,
                т.Марка + ' ' + т.Модель + ' (' + т.ГОСномер + ')' as TransportInfo
            FROM НазначенияНаВызовы н
            JOIN Происшествия п ON н.ID_Происшествия = п.ID
            JOIN Сотрудники с ON н.ID_Сотрудника = с.ID
            JOIN Транспорт т ON н.ID_Транспорта = т.ID
            ORDER BY н.Дата DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        assignments.Add(new Assignment
                        {
                            ID = (int)reader["ID"],
                            IncidentID = (int)reader["IncidentID"],
                            EmployeeID = (int)reader["EmployeeID"],
                            TransportID = (int)reader["TransportID"],
                            Date = (DateTime)reader["Дата"],
                            IncidentLocation = reader["IncidentLocation"].ToString(),
                            EmployeeName = reader["EmployeeName"].ToString(),
                            TransportInfo = reader["TransportInfo"].ToString()
                        });
                    }
                }
            }
            return assignments;
        }

        public static bool CreateAssignment(int incidentId, int employeeId, int transportId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"INSERT INTO НазначенияНаВызовы (ID_Происшествия, ID_Сотрудника, ID_Транспорта, Дата) 
                           VALUES (@IncidentId, @EmployeeId, @TransportId, GETDATE())";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IncidentId", incidentId);
                        command.Parameters.AddWithValue("@EmployeeId", employeeId);
                        command.Parameters.AddWithValue("@TransportId", transportId);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания назначения: {ex.Message}");
                return false;
            }
        }

        public static bool DeleteAssignment(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM НазначенияНаВызовы WHERE ID = @AssignmentId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AssignmentId", id);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления назначения: {ex.Message}");
                return false;
            }
        }

        public static List<Report> GetReports()
        {
            List<Report> reports = new List<Report>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
            SELECT 
                о.ID,
                о.ID_НазначенияНаВызовы as AssignmentID,
                о.Содержание as Content,
                о.Дата as CreationDate,
                п.МестоПроисшествия as IncidentLocation,
                с.ФИО as EmployeeName,
                т.Марка + ' ' + т.Модель + ' (' + т.ГОСномер + ')' as TransportInfo
            FROM Отчёты о
            JOIN НазначенияНаВызовы н ON о.ID_НазначенияНаВызовы = н.ID
            JOIN Происшествия п ON н.ID_Происшествия = п.ID
            JOIN Сотрудники с ON н.ID_Сотрудника = с.ID
            JOIN Транспорт т ON н.ID_Транспорта = т.ID
            ORDER BY о.Дата DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reports.Add(new Report
                        {
                            ID = (int)reader["ID"],
                            AssignmentID = (int)reader["AssignmentID"],
                            Content = reader["Content"].ToString(),
                            CreationDate = (DateTime)reader["CreationDate"],
                            IncidentLocation = reader["IncidentLocation"].ToString(),
                            EmployeeName = reader["EmployeeName"].ToString(),
                            TransportInfo = reader["TransportInfo"].ToString()
                        });
                    }
                }
            }
            return reports;
        }

        public static bool CreateReport(int assignmentId, string content)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Начинаем транзакцию
                    SqlTransaction transaction = connection.BeginTransaction();

                    try
                    {
                        // 1. Создаём отчёт
                        string reportQuery = @"INSERT INTO Отчёты (ID_НазначенияНаВызовы, Содержание, Дата) 
                                     VALUES (@AssignmentId, @Content, GETDATE())";

                        using (SqlCommand reportCommand = new SqlCommand(reportQuery, connection, transaction))
                        {
                            reportCommand.Parameters.AddWithValue("@AssignmentId", assignmentId);
                            reportCommand.Parameters.AddWithValue("@Content", content);
                            reportCommand.ExecuteNonQuery();
                        }

                        // 2. Находим ID происшествия по ID назначения
                        string incidentQuery = @"SELECT ID_Происшествия FROM НазначенияНаВызовы WHERE ID = @AssignmentId";
                        int incidentId;

                        using (SqlCommand incidentCommand = new SqlCommand(incidentQuery, connection, transaction))
                        {
                            incidentCommand.Parameters.AddWithValue("@AssignmentId", assignmentId);
                            incidentId = (int)incidentCommand.ExecuteScalar();
                        }

                        // 3. Обновляем статус происшествия
                        string updateQuery = "UPDATE Происшествия SET Статус = 'Завершено' WHERE ID = @IncidentId";

                        using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection, transaction))
                        {
                            updateCommand.Parameters.AddWithValue("@IncidentId", incidentId);
                            updateCommand.ExecuteNonQuery();
                        }

                        // 4. Коммитим транзакцию
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания отчёта: {ex.Message}");
                return false;
            }
        }


        public static bool CompleteIncidentWithReport(int incidentId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE Происшествия SET Статус = 'Завершено' WHERE ID = @IncidentId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IncidentId", incidentId);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления статуса: {ex.Message}");
                return false;
            }
        }

    }
}