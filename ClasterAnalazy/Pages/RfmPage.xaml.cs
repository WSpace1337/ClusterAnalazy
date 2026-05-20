using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ClusterVisualizer.Services;

namespace ClusterVisualizer.Pages
{
    public partial class RfmPage : Page
    {
        private readonly RfmService rfmService = new RfmService();

        public RfmPage()
        {
            InitializeComponent();

            if (DataService.Instance.RfmCustomers != null &&
                DataService.Instance.RfmCustomers.Count > 0)
            {
                LoadToUI();
            }
        }

        private void LoadRfm_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "CSV Files (*.csv)|*.csv";

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                var customers = rfmService.LoadFromCsv(dialog.FileName);

                DataService.Instance.SetRfmCustomers(customers);

                DataService.Instance.AddLog($"RFM file loaded: {dialog.FileName}");
                DataService.Instance.AddLog($"RFM customers: {customers.Count}");

                LoadToUI();

                StatusText.Text = $"Loaded {customers.Count} RFM customers.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error: " + ex.Message;
            }
        }

        private void LoadToUI()
        {
            var customers = DataService.Instance.RfmCustomers;

            RfmGrid.ItemsSource = customers;

            var summary = customers
                .GroupBy(c => c.Segment)
                .Select(g => $"{g.Key}: {g.Count()} clients")
                .OrderBy(x => x)
                .ToList();

            SegmentSummaryList.ItemsSource = summary;
        }
    }
}