using PersonalFinanceApp.DataAccess;
using System.Windows;

namespace PersonalFinanceApp
{
    public partial class MainWindow : Window
    {
        private DatabaseContext _dbContext;

        public MainWindow()
        {
            InitializeComponent();
            _dbContext = new DatabaseContext();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var user = _dbContext.LoginUser(UsernameTextBox.Text, PasswordBox.Password);
            if (user != null)
            {
                var dashboard = new DashboardWindow(user.Id);
                dashboard.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Неверные данные для входа.");
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (_dbContext.RegisterUser(UsernameTextBox.Text, PasswordBox.Password))
            {
                MessageBox.Show("Регистрация прошла успешно! Пожалуйста, войдите.");
            }
            else
            {
                MessageBox.Show("Имя пользователя уже занято.");
            }
        }
    }
}