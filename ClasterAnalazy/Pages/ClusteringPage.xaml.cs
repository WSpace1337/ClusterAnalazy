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

            UpdateAlgorithmUI();
            
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

        //денденограмма 
        private async void ShowDendrogram_Click(object sender, RoutedEventArgs e)
        {
            var points = DataService.Instance.Points;

            if (points == null)
            {
                StatusText.Text = "Load data first";
                return;
            }

            try
            {
                ControlsPanel.IsEnabled = false;
                ProgressBar.Visibility = Visibility.Visible;
                StatusText.Text = "Building dendrogram...";

                var clonedPoints = points.Take(50).Select(p => new PointData
                {
                    X = p.X,
                    Y = p.Y
                }).ToList();

                var hierarchical = new HierarchicalClustering();

                var root = await Task.Run(() =>
                {
                    return hierarchical.BuildTree(clonedPoints);
                });

                PlotView.Model = plotService.BuildDendrogram(root);

                StatusText.Text = "Dendrogram built";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error: " + ex.Message;
            }
            finally
            {
                ControlsPanel.IsEnabled = true;
                ProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private async void RunClustering_Click(object sender, RoutedEventArgs e)
        {
            var points = DataService.Instance.Points;

            if (points == null)
            {
                StatusText.Text = "Load data first";
                return;
            }

            var selectedAlgorithm = AlgorithmBox.SelectedItem as IClusteringAlgorithm;

            if (selectedAlgorithm == null)
            {
                StatusText.Text = "Select an algorithm";
                return;
            }

            try
            {
                ControlsPanel.IsEnabled = false;
                ProgressBar.Visibility = Visibility.Visible;
                StatusText.Text = "Clustering...";

                string epsText = EpsBox.Text;
                string minPtsText = MinPtsBox.Text;
                string clusterCountText = ClusterCountBox.Text;

                var clonedPoints = points.Select(p => new PointData
                {
                    X = p.X,
                    Y = p.Y
                }).ToList();

                ClusterResult result = await Task.Run(() =>
                {
                    if (selectedAlgorithm is DBSCANAlgorithm)
                    {
                        if (!double.TryParse(epsText, out double eps) || eps <= 0)
                            throw new Exception("Invalid eps value");

                        if (!int.TryParse(minPtsText, out int minPts) || minPts <= 0)
                            throw new Exception("Invalid minPts value");

                        var dbscan = new DBSCANAlgorithm(eps, minPts);
                        return dbscan.Calculate(clonedPoints, 0);
                    }
                    else
                    {
                        if (!int.TryParse(clusterCountText, out int k) || k <= 0)
                            throw new Exception("Invalid cluster count");

                        return selectedAlgorithm.Calculate(clonedPoints, k);
                    }
                });

                DataService.Instance.SetClusterResult(result);
                PlotView.Model = plotService.BuildPlot(result);
                StatusText.Text = $"Done ({result.ClusterCount} clusters)";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error: " + ex.Message;
            }
            finally
            {
                ControlsPanel.IsEnabled = true;
                ProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private async void FindEps_Click(object sender, RoutedEventArgs e)
        {
            var points = DataService.Instance.Points;

            if (points == null)
            {
                StatusText.Text = "Load data first";
                return;
            }

            if (!int.TryParse(MinPtsBox.Text, out int minPts) || minPts <= 0)
            {
                StatusText.Text = "Invalid minPts";
                return;
            }

            try
            {
                ControlsPanel.IsEnabled = false;
                ProgressBar.Visibility = Visibility.Visible;
                StatusText.Text = "Calculating eps...";

                var clonedPoints = points.Select(p => new PointData
                {
                    X = p.X,
                    Y = p.Y
                }).ToList();

                var service = new DbscanParameterService();

                var result = await Task.Run(() =>
                {
                    var distances = service.CalculateKDistance(clonedPoints, minPts);
                    var eps = service.FindBestEps(distances);

                    return new { Distances = distances, Eps = eps };
                });

                PlotView.Model = plotService.BuildKDistancePlot(result.Distances, result.Eps);
                EpsBox.Text = result.Eps.ToString("F3");
                StatusText.Text = $"Suggested eps = {result.Eps:F3}";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error: " + ex.Message;
            }
            finally
            {
                ControlsPanel.IsEnabled = true;
                ProgressBar.Visibility = Visibility.Collapsed;
            }
        }
        private void AlgorithmBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAlgorithmUI();
        }

        private void UpdateAlgorithmUI()
        {
            var algorithm = AlgorithmBox.SelectedItem as IClusteringAlgorithm;

            if (algorithm == null)
                return;

            KControlsPanel.Visibility = Visibility.Collapsed;
            DbscanControlsPanel.Visibility = Visibility.Collapsed;
            DendrogramButton.Visibility = Visibility.Collapsed;

            if (algorithm is KMeansAlgorithm)
            {
                KControlsPanel.Visibility = Visibility.Visible;
            }
            else if (algorithm is DBSCANAlgorithm)
            {
                DbscanControlsPanel.Visibility = Visibility.Visible;
            }
            else if (algorithm is HierarchicalClustering)
            {
                KControlsPanel.Visibility = Visibility.Visible;
                DendrogramButton.Visibility = Visibility.Visible;
            }
        }

    }
}
