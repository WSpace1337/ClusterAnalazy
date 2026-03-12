using ClasterAnalazy;
using System;
using System.Windows;

namespace ClusterVisualizer.Views
{
    public partial class LoginWindow : Window
    {
        private AuthenticationService authService;

        public LoginWindow()
        {
            InitializeComponent();
            authService = new AuthenticationService();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string password = PasswordBox.Password;

            bool success = authService.Login(username, password);
            if (success)
            {
                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
            }
            else
            {
                ErrorText.Text = "Invalid username or password";
            }
        }
    }
}