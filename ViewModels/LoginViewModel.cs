using RSCHS.Helpers;
using RSCHS.Services;
using RSCHS.Views;
using System.Windows;
using System.Windows.Input;

namespace RSCHS.ViewModels
{
    public class LoginViewModel
    {
        public string Login { get; set; }
        public string Password { get; set; }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
            RegisterCommand = new RelayCommand(ExecuteRegister);
        }

        private void ExecuteLogin()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            var user = DatabaseHelper.AuthenticateUser(Login, Password);
            if (user != null)
            {
                // Создаем главное окно с ViewModel
                var mainWindow = new MainWindow();
                var viewModel = new MainViewModel(user);
                mainWindow.DataContext = viewModel;

                mainWindow.Show();

                // Закрываем окно входа
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is LoginWindow)
                    {
                        window.Close();
                        break;
                    }
                }

                Application.Current.MainWindow = mainWindow;
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!");
            }
        }

        private void ExecuteRegister()
        {
            var registerWindow = new RegistrationWindow();
            registerWindow.ShowDialog();
        }
    }
}