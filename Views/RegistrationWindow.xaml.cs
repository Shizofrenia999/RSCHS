using Microsoft.Data.SqlClient;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RSCHS.Views
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

                if (!ValidateRegistration())
                    return;

                string position = "";
                if (cmbPosition.SelectedItem is ComboBoxItem selectedItem)
                {
                    position = selectedItem.Content.ToString();
                }

                if (RegisterEmployee(
                    txtLogin.Text.Trim(),
                    txtPassword.Password,
                    txtFullName.Text.Trim(),
                    position,
                    txtPhone.Text.Trim()))
                {
                    MessageBox.Show("Сотрудник успешно зарегистрирован!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private bool RegisterEmployee(string login, string password, string fullName, string position, string phone)
        {
            string connectionString = "Server=WIN-VH3ASTRV6E6\\SQLEXPRESS;Database=RSCHS;Trusted_Connection=True;TrustServerCertificate=true;";
            //string connectionString = "Server=localhost;Database=RSCHS;User Id=sa;Password=123;TrustServerCertificate=true;";

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

        private bool ValidateRegistration()
        {
            // Проверка логина (только латиница и цифры, 3-20 символов)
            if (string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                MessageBox.Show("Введите логин!");
                txtLogin.Focus();
                return false;
            }

            if (!Regex.IsMatch(txtLogin.Text, @"^[a-zA-Z0-9]{3,20}$"))
            {
                MessageBox.Show("Логин может содержать только латинские буквы и цифры (3-20 символов)!");
                txtLogin.Focus();
                txtLogin.SelectAll();
                return false;
            }

            // Проверка пароля (минимум 6 символов)
            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Введите пароль!");
                txtPassword.Focus();
                return false;
            }

            if (txtPassword.Password.Length < 6)
            {
                MessageBox.Show("Пароль должен быть не менее 6 символов!");
                txtPassword.Focus();
                txtPassword.SelectAll();
                return false;
            }

            // Проверка ФИО (только буквы, пробелы и дефисы, минимум 5 символов)
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Введите ФИО!");
                txtFullName.Focus();
                return false;
            }

            if (!Regex.IsMatch(txtFullName.Text.Trim(), @"^[а-яА-ЯёЁa-zA-Z\s\-]{5,100}$"))
            {
                MessageBox.Show("ФИО может содержать только буквы, пробелы и дефис (минимум 5 символов)!");
                txtFullName.Focus();
                txtFullName.SelectAll();
                return false;
            }

            // Проверка телефона (только цифры, +, скобки, пробелы и дефисы)
            if (!string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                // Убираем все символы кроме цифр и +
                string cleanPhone = new string(txtPhone.Text.Where(c => char.IsDigit(c) || c == '+').ToArray());

                // Проверяем, что осталось минимум 10 цифр (без +)
                int digitCount = cleanPhone.Count(char.IsDigit);
                if (digitCount < 10 || digitCount > 15)
                {
                    MessageBox.Show("Введите корректный номер телефона (от 10 до 15 цифр)!");
                    txtPhone.Focus();
                    txtPhone.SelectAll();
                    return false;
                }

                // Проверяем, что + только в начале
                if (cleanPhone.Contains('+') && !cleanPhone.StartsWith("+"))
                {
                    MessageBox.Show("Знак '+' может быть только в начале номера!");
                    txtPhone.Focus();
                    txtPhone.SelectAll();
                    return false;
                }
            }

            return true;
        }


        private void txtPhone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                // Разрешаем: цифры, +, (, ), -, пробел
                if (!Regex.IsMatch(e.Text, @"^[\d\+\-\(\)\s]$"))
                {
                    e.Handled = true;
                }

                // + может быть только в начале
                if (e.Text == "+" && textBox.Text.Length > 0 && !textBox.Text.StartsWith("+"))
                {
                    e.Handled = true;
                }
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}