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