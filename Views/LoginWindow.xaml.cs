using RSCHS.ViewModels;
using System.Windows;

namespace RSCHS.Views
{
    public partial class LoginWindow : Window
    {
        private LoginViewModel _viewModel;

        public LoginWindow()
        {
            InitializeComponent();

            _viewModel = new LoginViewModel();
            this.DataContext = _viewModel;
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.Password = txtPassword.Password;
            }
        }
    }
}