using Microsoft.Data.SqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace RSCHS
{
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtLogin.Text) ||
                    string.IsNullOrWhiteSpace(txtPassword.Password) ||
                    string.IsNullOrWhiteSpace(txtFullName.Text) ||
                    cmbPosition.SelectedItem == null)
                {
                    MessageBox.Show("Заполните все обязательные поля!");
                    return;
                }

                if (RegisterEmployee(
                    txtLogin.Text,
                    txtPassword.Password,
                    txtFullName.Text,
                    (cmbPosition.SelectedItem as ComboBoxItem).Content.ToString(),
                    txtPhone.Text))
                {
                    MessageBox.Show("Сотрудник успешно зарегистрирован!");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации: {ex.Message}");
            }
        }

        private bool RegisterEmployee(string login, string password, string fullName, string position, string phone)
        {
            string connectionString = "Server=localhost;Database=RSCHS;User Id=sa;Password=123;TrustServerCertificate=true;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string checkQuery = "SELECT COUNT(*) FROM Сотрудники WHERE Логин = @Login";
                using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@Login", login);
                    int existingCount = (int)checkCommand.ExecuteScalar();

                    if (existingCount > 0)
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!");
                        return false;
                    }
                }


                string insertQuery = @"INSERT INTO Сотрудники (Логин, Пароль, ФИО, Должность, Телефон, Статус) 
                                     VALUES (@Login, @Password, @FullName, @Position, @Phone, 'Активен')";

                using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@Login", login);
                    insertCommand.Parameters.AddWithValue("@Password", password); 
                    insertCommand.Parameters.AddWithValue("@FullName", fullName);
                    insertCommand.Parameters.AddWithValue("@Position", position);
                    insertCommand.Parameters.AddWithValue("@Phone", string.IsNullOrWhiteSpace(phone) ? (object)DBNull.Value : phone);

                    return insertCommand.ExecuteNonQuery() > 0;
                }
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}