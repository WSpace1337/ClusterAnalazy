
using System.Windows;
using ClusterVisualizer.ViewModels;

namespace ClusterVisualizer.Views
{
    public partial class LoginWindow : Window
    {
        private LoginViewModel viewModel;

        public LoginWindow()
        {
            InitializeComponent();

            viewModel = new LoginViewModel();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Username = UsernameBox.Text;
            viewModel.Password = PasswordBox.Password;

            if(viewModel.Login())
            {
                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
            }
            else
            {
                ErrorText.Text = "Invalid login or password";
            }
        }
    }
}