using Microsoft.Data.SqlClient;
using RSCHS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace RSCHS.Services
{
    public interface IDatabaseService
    {
        Employee AuthenticateUser(string login, string password);
        List<Incident> GetIncidents();
        List<Employee> GetEmployees();
        List<Transport> GetTransports();
        List<Assignment> GetAssignments();
        bool CreateIncident(Incident incident);
        bool CreateAssignment(Assignment assignment);
        bool DeleteIncident(int id);
        bool DeleteAssignment(int id);
        bool UpdateIncidentStatus(int id, string status);
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Employee AuthenticateUser(string login, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT ID, Логин, ФИО, Должность, Телефон, Статус " +
                              "FROM Сотрудники WHERE Логин = @Login AND Пароль = @Password";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Login", login);
                    command.Parameters.AddWithValue("@Password", password);

                    using (var reader = command.ExecuteReader())
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

        public bool CreateIncident(Incident incident)
        {
            throw new NotImplementedException();
        }

        public bool DeleteAssignment(int id)
        {
            throw new NotImplementedException();
        }

        public bool DeleteIncident(int id)
        {
            throw new NotImplementedException();
        }

        public List<Assignment> GetAssignments()
        {
            throw new NotImplementedException();
        }

        public List<Employee> GetEmployees()
        {
            throw new NotImplementedException();
        }

        public List<Incident> GetIncidents()
        {
            var incidents = new List<Incident>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Происшествия ORDER BY Дата DESC";

                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
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

        public List<Transport> GetTransports()
        {
            throw new NotImplementedException();
        }

        public bool UpdateIncidentStatus(int id, string status)
        {
            throw new NotImplementedException();
        }

        public bool CreateAssignment(Assignment assignment)
        {
            throw new NotImplementedException();
        }
    }
}