using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;

namespace RSCHS
{
    public static class DatabaseHelper
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["MCHSConnection"].ConnectionString;

        public static Employee AuthenticateUser(string login, string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID, Логин, ФИО, Должность, Телефон, Статус FROM Сотрудники WHERE Логин = @Login AND Пароль = @Password AND Статус = 'Активен'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Login", login);
                    command.Parameters.AddWithValue("@Password", password); // В реальном приложении нужно хэшировать пароли!

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
                            Priority = (int)reader["Приоритет"],
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