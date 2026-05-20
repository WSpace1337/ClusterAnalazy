
using System.Windows;
using ClusterVisualizer.ViewModels;
using ClusterVisualizer.Visualization;
using ClusterVisualizer.Services;
using ClusterVisualizer.Core.Models;
using ClusterVisualizer.Pages;
using System.Windows.Controls;



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

            MainFrame.Navigate(new DashboardPage());
            SetActiveMenu(DashboardButton);

        }

        private void Clustering_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ClusteringPage());
            SetActiveMenu(ClusteringButton);
        }

        private void Elbow_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ElbowPage());
            SetActiveMenu(ElbowButton);
        }
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardPage());
            SetActiveMenu(DashboardButton);
        }

        private void Prediction_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PredictionPage());
            SetActiveMenu(PredictionButton);
        }

        private void Rfm_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new RfmPage());
            SetActiveMenu(RfmButton);
        }
        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new SettingsPage());
            SetActiveMenu(SettingsButton);
        }

        private void Users_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new UsersPage());
            SetActiveMenu(UsersMenu);
        }


        private void SetActiveMenu(Button activeButton)
        {
            DashboardButton.Background = System.Windows.Media.Brushes.Transparent;
            ClusteringButton.Background = System.Windows.Media.Brushes.Transparent;
            ElbowButton.Background = System.Windows.Media.Brushes.Transparent;
            PredictionButton.Background = System.Windows.Media.Brushes.Transparent;
            RfmButton.Background = System.Windows.Media.Brushes.Transparent;
            SettingsButton.Background = System.Windows.Media.Brushes.Transparent;
            UsersMenu.Background = System.Windows.Media.Brushes.Transparent;

            activeButton.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(37, 99, 235)
            );
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
