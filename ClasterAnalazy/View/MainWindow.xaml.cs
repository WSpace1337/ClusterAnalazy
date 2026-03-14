
using System.Windows;
using ClusterVisualizer.ViewModels;
using ClusterVisualizer.Visualization;
using ClusterVisualizer.Services;
using ClusterVisualizer.Core.Models;
using ClusterVisualizer.Pages;



namespace ClusterVisualizer.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel viewModel;
        private PlotService plotService;

        public MainWindow()
        {
            InitializeComponent();
            Window_Loaded();

            viewModel = new MainViewModel();
            plotService = new PlotService();

            MainFrame.Navigate(new ClusteringPage());

        }

        private void Clustering_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ClusteringPage());
        }

        private void Elbow_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ElbowPage());
        }

        private void Users_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new UsersPage());
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new SettingPage());
        }

        private void Window_Loaded()
        {
            var user = SessionManager.CurrentUser;

            if(user == null)
            {
                UserInfo.Text = "no user session";
                UsersMenu.Visibility=Visibility.Collapsed;
                return;
            }

            UserInfo.Text = $"User: {user.Username} | Role: {user.Role}";

            if (user.Role != User.UserRole.Admin)
            {
                UsersMenu.Visibility = Visibility.Collapsed;
            }
            else
            {
                UsersMenu.Visibility = Visibility.Visible;
            }
        }
    }
}
