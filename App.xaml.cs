using System.Configuration;
using System.Data;
using System.Windows;

namespace RSCHS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Запускаем с окна авторизации
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}