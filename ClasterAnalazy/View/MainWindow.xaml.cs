using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ClusterVisualizer.ViewModels;
using ClusterVisualizer.Visualization;
using Microsoft.Win32;
using ClusterVisualizer.Services;
using OxyPlot.Wpf;
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
    }
}
