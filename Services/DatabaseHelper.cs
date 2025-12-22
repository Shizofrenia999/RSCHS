using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Windows;
using RSCHS.Models;

namespace RSCHS.Services
{
    public static class DatabaseHelper
    {
        private static string connectionString = "Server=localhost;Database=RSCHS;User Id=sa;Password=123;TrustServerCertificate=true;"; public static Employee AuthenticateUser(string login, string password)
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
    }
}