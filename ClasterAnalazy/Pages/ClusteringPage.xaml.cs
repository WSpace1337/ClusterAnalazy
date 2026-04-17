using ClusterVisualizer.Interfaces;
using ClusterVisualizer.Services;
using ClusterVisualizer.ViewModels;
using ClusterVisualizer.Visualization;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;


namespace ClusterVisualizer.Pages
{
    /// <summary>
    /// Interaction logic for ClusteringPage.xaml
    /// </summary>
    public partial class ClusteringPage : Page
    {
        public ClusteringPage()
        {
           
            InitializeComponent();

            algorithms = ClusteringFactory.GetAlgorithms();

            AlgorithmBox.ItemsSource = algorithms;
            AlgorithmBox.DisplayMemberPath = "Name";

            AlgorithmBox.SelectedIndex = 0;

            viewModel = new MainViewModel();
            plotService = new PlotService();

            this.DataContext = viewModel;
            
            //проверка на подгруженость даных, если загружены,
            //то работа идет с этим файлом
            if (DataService.Instance.Points != null )
            {
                viewModel.Points = DataService.Instance.Points;
                StatusText.Text = $"Dataset loaded: {viewModel.Points.Count} points";
            }

        }

        private List<IClusteringAlgorithm> algorithms;
        private MainViewModel viewModel;
        private PlotService plotService;

        private void LoadData_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "CSV Files (*.csv)|*.csv";

            if (dialog.ShowDialog() == true)
            {
                viewModel.LoadData(dialog.FileName);

                DataService.Instance.Points = viewModel.Points;

                StatusText.Text = dialog.FileName;
            }
        }
        private void FindBestK_Click(object sender, RoutedEventArgs e)
        {
            var points = DataService.Instance.Points;

            if (points == null)
            {
                StatusText.Text = "Load data first";
                return;
            }

            var algorithm = AlgorithmBox.SelectedItem as IClusteringAlgorithm;

            var silhouetteService = new SilhouetteService();

            int bestK = silhouetteService.FindBestK(points, algorithm, 10);

            ClusterCountBox.Text = bestK.ToString();

            StatusText.Text = $"Best K (Silhouette): {bestK}";
        }

        private void RunClustering_Click(object sender, RoutedEventArgs e)
        {
            var points = DataService.Instance.Points;

            if (points == null)
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
                var algorithm = AlgorithmBox.SelectedItem as IClusteringAlgorithm;
                var result = algorithm.Calculate(viewModel.Points,k);

                //Сохранение даных для дальлнейшей обработке 
                DataService.Instance.SetClusterResult(result);

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
