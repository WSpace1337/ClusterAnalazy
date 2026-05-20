using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClusterVisualizer.Services;
using System.Collections.Generic;
using ClusterVisualizer.Core.Algorithms;
using ClusterVisualizer.Core.Models;
using ClusterVisualizer.Interfaces;


using System.Data;
using System.IO;


namespace ClusterVisualizer.Pages
{
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            LoadDashboard();

            LoadDataPreview();

            LoadAlgorithmComparison();

            LoadRfmAnalytics();
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

        private void LoadAlgorithmComparison()
        {
            ComparisonGrid.ItemsSource = DataService.Instance.AlgorithmComparisonResults;
            StatusText.Text = "Comparison loaded from clustering results.";
        }

        private void LoadRfmAnalytics()
        {
            var customers = DataService.Instance.RfmCustomers;

            if (customers == null || customers.Count == 0)
            {
                RfmStatusText.Text = "RFM data not loaded.";
                RfmDashboardGrid.ItemsSource = null;
                return;
            }

            int total = customers.Count;

            var summary = customers
                .GroupBy(c => c.Segment)
                .Select(g => new
                {
                    Segment = g.Key,
                    Count = g.Count(),
                    Percent = $"{(double)g.Count() / total * 100:F1}%",
                    AvgR = g.Average(c => c.RScore).ToString("F2"),
                    AvgF = g.Average(c => c.FScore).ToString("F2"),
                    AvgM = g.Average(c => c.MScore).ToString("F2"),
                    AvgTotal = g.Average(c => c.TotalScore).ToString("F2")
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            RfmDashboardGrid.ItemsSource = summary;

            RfmStatusText.Text = $"RFM customers loaded: {total}";
        }

        private void LoadDataPreview()
        {
            string path = DataService.Instance.LoadedFilePath;

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;

            var lines = File.ReadLines(path).ToList();

            StatusText.Text = $"Loaded rows: {lines.Count - 1}";

            if (lines.Count == 0)
                return;

            var table = new DataTable();

            var headers = lines[0].Split(',');

            foreach (var header in headers)
            {
                table.Columns.Add(header);
            }

            foreach (var line in lines.Skip(1))
            {
                var values = line.Split(',');

                var row = table.NewRow();

                for (int i = 0; i < headers.Length; i++)
                {
                    row[i] = i < values.Length ? values[i] : "";
                }

                table.Rows.Add(row);
            }

            DataPreviewGrid.ItemsSource = table.DefaultView;
        }
    }
}