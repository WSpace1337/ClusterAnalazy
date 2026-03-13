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

            this.DataContext = viewModel;
        }

        private void LoadData_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "CSV Files (*.csv)|*.csv";

            if (dialog.ShowDialog() == true)
            {
                viewModel.LoadData(dialog.FileName);
                StatusText.Text = dialog.FileName;
            }
        }

        private void RunClustering_Click(object sender, RoutedEventArgs e)
        {
            if(viewModel.Points == null)
            {
                StatusText.Text = "Load data first";
                return;
            }

            if (!int.TryParse(ClusterCountBox.Text, out int k) || k <= 0)
            {
                StatusText.Text = "Enter a valid cluster count (number >0)";
                return;
            }

            try
            {
                var result = viewModel.RunClustering(k);
                PlotView.Model = plotService.BuildPlot(result);
                StatusText.Text = $"Clustering finished: {k} clusters found.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error: " + ex.Message;
            }
        }
    }
}
