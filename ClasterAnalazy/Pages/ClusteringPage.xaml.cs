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
using System.Globalization;
using System.IO;






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

            RefreshLogs();

            algorithms = ClusteringFactory.GetAlgorithms();

            AlgorithmBox.ItemsSource = algorithms;
            AlgorithmBox.DisplayMemberPath = "Name";

    

            viewModel = new MainViewModel();
            plotService = new PlotService();

            this.DataContext = viewModel;

            UpdateAlgorithmUI();

            AppSettings.Load();


            AlgorithmBox.SelectedIndex = AppSettings.DefaultAlgorithmIndex;
            EpsBox.Text = AppSettings.DefaultEps.ToString();
            MinPtsBox.Text = AppSettings.DefaultMinPts.ToString();
            

            //проверка на подгруженость даных, если загружены,
            //то работа идет с этим файлом
            if (DataService.Instance.Points != null )
            {
                viewModel.Points = DataService.Instance.Points;


                StatusText.Text = $"Dataset loaded: {viewModel.Points.Count} points";
            }

            if (DataService.Instance.ClusterResult != null)
            {
                PlotView.Model = plotService.BuildPlot(DataService.Instance.ClusterResult);
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

                AddLog($"Dataset loaded: {dialog.FileName}");
                LogTablePreview(dialog.FileName);
            }
        }

        private void LogTablePreview(string path)
        {
            var lines = File.ReadLines(path)
                .Take(AppSettings.MaxPreviewRows + 1)
                .ToList();

            if (lines.Count <= 1)
            {
                AddLog("CSV is empty.");
                return;
            }

            AddLog("Table preview:", false);
            AddLog("------------------------------------------------------------", false);
            AddLog($"{"Id",-8} {"Age",-6} {"Income",-8} {"Debt",-8} {"Address",-10}", false);
            AddLog("------------------------------------------------------------", false);

            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');

                if (parts.Length < 9)
                    continue;

                string id = parts[0];
                string age = parts[1];
                string income = parts[4];
                string debt = parts[7];
                string address = parts[8];

                AddLog($"{id,-8} {age,-6} {income,-8} {debt,-8} {address,-10}", false);
            }

            AddLog("------------------------------------------------------------\n", false);
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

                Action<string> logger = message =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        AddLog(message);
                    });
                };

                int bestK = await Task.Run(() =>
                {
                    return silhouetteService.FindBestK( points, algorithm, 10, progress, logger);
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

        private void AddLog(string message, bool withTime = true)
        {
            if (withTime)
            {
                string time = DateTime.Now.ToString("HH:mm:ss");
                DataService.Instance.Logs.Add($"[{time}] {message}");
            }
            else
            {
                DataService.Instance.Logs.Add(message);
            }

            RefreshLogs();
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
                EpsBox.Text = AppSettings.DefaultEps.ToString();
                string minPtsText = MinPtsBox.Text;
                string clusterCountText = ClusterCountBox.Text;

                var clonedPoints = points.Select(p => new PointData
                {
                    X = p.X,
                    Y = p.Y
                }).ToList();

                Action<string> logger = message =>
                {
                    Dispatcher.Invoke(() => AddLog(message));
                };
                ClusterResult result = await Task.Run(() =>
                {
                    if (selectedAlgorithm is DBSCANAlgorithm)
                    {
                        epsText = epsText.Replace(',', '.');

                        if (!double.TryParse(
                                epsText,
                                NumberStyles.Float,
                                CultureInfo.InvariantCulture,
                                out double eps) || eps <= 0)
                        {
                            throw new Exception("Invalid eps value");
                        }

                        if (!int.TryParse(minPtsText, out int minPts) || minPts <= 0)
                            throw new Exception("Invalid minPts value");

                        var dbscan = new DBSCANAlgorithm(eps, minPts);
                        return dbscan.Calculate(clonedPoints, 0);
                    }
                    else
                    {
                        if (!int.TryParse(clusterCountText, out int k) || k <= 0)
                            throw new Exception("Invalid cluster count");

                        return selectedAlgorithm.Calculate(clonedPoints, k, logger);
                    }
                });

                DataService.Instance.SetClusterResult(result);

                var mlService = new MlTrainingService();


                var modelPlot = plotService.BuildPlot(result);


                /// оцінка якості кластерів

                var qualityService = new ClusterQualityService();

                double silhouette = qualityService.SilhouetteScore(result.Points);
                double dbi = qualityService.DaviesBouldinIndex(result.Points);
                double ch = qualityService.CalinskiHarabaszIndex(result.Points);


                AddLog("------------------------------------------------------------");
                AddLog($"Algorithm clustering: {selectedAlgorithm.Name}");
                AddLog("Cluster quality metrics:");
                AddLog($"Silhouette Score: {silhouette:F4}  (higher is better)");
                AddLog($"Davies-Bouldin Index: {dbi:F4}  (lower is better)");
                AddLog($"Calinski-Harabasz Index: {ch:F4}  (higher is better)");
                AddLog("------------------------------------------------------------");

                ///

                PlotView.Model = modelPlot;

                DataService.Instance.SetClusterResult(result);

                StatusText.Text = $"Done ({result.ClusterCount} clusters)";

                int classCount = result.Points
                                            .Where(p => p.ClusterId >= 0)
                                            .Select(p => p.ClusterId)
                                            .Distinct()
                                            .Count();

                if (classCount < 2)
                {
                    AddLog("ML training skipped.");
                    AddLog("Reason: less than 2 clusters were found.");
                    AddLog("DBSCAN may produce 1 cluster + noise.");
                    return;
                }
                var model = mlService.Train(
                    result.Points,
                    MlAlgorithmType.GradientBoosting
                );

                DataService.Instance.SetMlModel(model);
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
                EpsBox.Text = result.Eps.ToString("F3", CultureInfo.InvariantCulture);
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

        private void RefreshLogs()
        {
            LogBox.Text = string.Join(Environment.NewLine, DataService.Instance.Logs);

            LogBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                LogBox.ScrollToEnd();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}
