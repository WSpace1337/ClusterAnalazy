using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClusterVisualizer.Services;
using System.Collections.Generic;
using ClusterVisualizer.Core.Algorithms;
using ClusterVisualizer.Core.Models;
using ClusterVisualizer.Interfaces;



namespace ClusterVisualizer.Pages
{
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            LoadDashboard();

            Loaded += DashboardPage_Loaded;
        }

        private void LoadDashboard()
        {
            var points = DataService.Instance.Points;
            var result = DataService.Instance.ClusterResult;

            CustomersText.Text = points == null ? "0" : points.Count.ToString();

            ClustersText.Text = result == null
                ? "-"
                : result.ClusterCount.ToString();

            OptimalKText.Text = DataService.Instance.OptimalK.HasValue
                ? DataService.Instance.OptimalK.Value.ToString()
                : "-";

            NoiseText.Text = result == null
                ? "-"
                : result.Points.Count(p => p.ClusterId == -2).ToString();

            AvgAgeText.Text = points == null
                ? "-"
                : points.Average(p => p.OriginalX).ToString("F1");

            AvgIncomeText.Text = points == null
                ? "-"
                : points.Average(p => p.OriginalY).ToString("F1");
        }

        private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            await RunComparisonAsync();
        }

        private async Task RunComparisonAsync()
        {
            var points = DataService.Instance.Points;

            if (points == null)
            {
                StatusText.Text = "Load data first.";
                return;
            }

            StatusText.Text = "Running comparison...";

            var results = await Task.Run(() =>
            {
                var list = new List<AlgorithmComparisonResult>();
                var quality = new ClusterQualityService();

                var algorithms = new List<(string Name, IClusteringAlgorithm Algorithm, int K)>
        {
            ("K-Means", new KMeansAlgorithm(), 3),
            ("Hierarchical", new HierarchicalClustering(), 3),
            ("DBSCAN", new DBSCANAlgorithm(AppSettings.DefaultEps, AppSettings.DefaultMinPts), 0)
        };

                foreach (var item in algorithms)
                {
                    var cloned = points.Select(p => new PointData
                    {
                        X = p.X,
                        Y = p.Y
                    }).ToList();

                    var result = item.Algorithm.Calculate(cloned, item.K);

                    list.Add(new AlgorithmComparisonResult
                    {
                        Algorithm = item.Name,
                        Clusters = result.ClusterCount,
                        Noise = result.Points.Count(p => p.ClusterId == -2),
                        Silhouette = quality.SilhouetteScore(result.Points),
                        DaviesBouldin = quality.DaviesBouldinIndex(result.Points),
                        CalinskiHarabasz = quality.CalinskiHarabaszIndex(result.Points)
                    });
                }

                return list;
            });

            ComparisonGrid.ItemsSource = results;
            StatusText.Text = "Comparison completed.";
        }
    }
}