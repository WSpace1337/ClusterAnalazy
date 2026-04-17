using ClusterVisualizer.Core.Algorithms;
using ClusterVisualizer.Core.Models;
using ClusterVisualizer.Interfaces;
using ClusterVisualizer.Services;
using ClusterVisualizer.ViewModels;
using ClusterVisualizer.Visualization;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private async void FindBestK_Click(object sender, RoutedEventArgs e)
        {
            var points = DataService.Instance.Points;

            if (points == null)
            {
                StatusText.Text = "Load data first";
                return;
            }

            var algorithm = AlgorithmBox.SelectedItem as IClusteringAlgorithm;

            if (algorithm == null)
            {
                StatusText.Text = "Select an algorithm";
                return;
            }

            try
            {
                StatusText.Text = "Finding best K...";
                ControlsPanel.IsEnabled = false;

                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Minimum = 0;
                ProgressBar.Maximum = 100;
                ProgressBar.Value = 0;

                var silhouetteService = new SilhouetteService();

                var clonedPoints = points.Select(p => new PointData
                {
                    X = p.X,
                    Y = p.Y
                }).ToList();

                var progress = new Progress<int>(value =>
                {
                    ProgressBar.Value = value;
                    StatusText.Text = $"Finding best K... {value}%";
                });

                int bestK = await Task.Run(() =>
                {
                    return silhouetteService.FindBestK(clonedPoints, algorithm, 10, progress);
                });

                ClusterCountBox.Text = bestK.ToString();
                StatusText.Text = $"Best K (Silhouette): {bestK}";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error: " + ex.Message;
            }
            finally
            {
                ControlsPanel.IsEnabled = true;
                ProgressBar.Visibility = Visibility.Collapsed;
                ProgressBar.IsIndeterminate = true;
                ProgressBar.Value = 0;
            }
        }

        private void ShowDendrogram_Click(object sender, RoutedEventArgs e)
        {
            var points = DataService.Instance.Points;

            if (points == null)
            {
                StatusText.Text = "Load data first";
                return;
            }

            var hierarchical = new HierarchicalClustering();

            var root = hierarchical.BuildTree(points.Take(100).ToList());

            PlotView.Model = plotService.BuildDendrogram(root);

            StatusText.Text = "Dendrogram built";
        }

        private async void RunClustering_Click(object sender, RoutedEventArgs e)
        {
            var points = DataService.Instance.Points;

            if (points == null)
            {
                StatusText.Text = "Load data first";
                return;
            }

            if (!int.TryParse(ClusterCountBox.Text, out int k) || k <= 0)
            {
                StatusText.Text = "Enter valid K";
                return;
            }

            try
            {
                StatusText.Text = "Clustering...";

                ControlsPanel.IsEnabled = false;
                ProgressBar.Visibility = Visibility.Visible;

                var algorithm = AlgorithmBox.SelectedItem as IClusteringAlgorithm;

                var clonedPoints = points.Select(p => new PointData
                {
                    X = p.X,
                    Y = p.Y
                }).ToList();

                var result = await Task.Run(() =>
                {
                    return algorithm.Calculate(clonedPoints, k);
                });

                DataService.Instance.SetClusterResult(result);

                PlotView.Model = plotService.BuildPlot(result);

                StatusText.Text = $"Done ({k} clusters)";
            }
            catch (Exception ex)
            {
                StatusText.Text = ex.Message;
            }
            finally
            { 
                ControlsPanel.IsEnabled = true;
                ProgressBar.Visibility = Visibility.Collapsed;
            }
        }

    }
}
