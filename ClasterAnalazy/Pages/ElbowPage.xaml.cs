using ClusterVisualizer.ViewModels;
using ClusterVisualizer.Visualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using ClusterVisualizer.Services;

namespace ClusterVisualizer.Pages
{
    /// <summary>
    /// Interaction logic for ElbowPage.xaml
    /// </summary>
    public partial class ElbowPage : Page
    {
        private MainViewModel viewModel;
        private PlotService plotService;


        public ElbowPage()
        {

            InitializeComponent();

            viewModel = new MainViewModel();
            plotService = new PlotService();

            if (DataService.Instance.Points != null)
            {
                viewModel.Points = DataService.Instance.Points;
                StatusText.Text = $"Dataset loaded: {viewModel.Points.Count} points";
            }

            if (DataService.Instance.ElbowValues != null)
            {
                PlotView.Model = plotService.BuildElbowPlot(DataService.Instance.ElbowValues);
            }

            RefreshLogs();
            this.DataContext= viewModel;
        }

        private async void CalculateElbow_Click(object sender, RoutedEventArgs e)
        {
            var points = DataService.Instance.Points;

            if(points== null)
            {
                StatusText.Text = "Load data on clustering page first!";
                return;
            }

            if(!int.TryParse(MaxKBox.Text, out int maxK) || maxK <=1)
            {
                StatusText.Text = "Enter valid Max K(>1)!";
                return;
            }

            try
            {
                var elbowService = new ElbowService();

                Action<string> logger = message =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        AddLog(message);
                    });
                };

                var values = await Task.Run(() =>
                {
                    return elbowService.Calculate(points, maxK, logger);
                });

                var kneeDetector = new KneeDetector();

                int optimalK = kneeDetector.FindOptimalK(values);

                AddLog($"Optimal K detected: {optimalK}");

                //Сохранеие значения SSE и "локтя" (оптимальное количество кластеров)
                DataService.Instance.SetElbowResult(values, optimalK);


                PlotView.Model = plotService.BuildElbowPlot(values);
                DataService.Instance.SetElbowResult(values, optimalK);

                StatusText.Text = $"Optimal K = {optimalK}.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error: " + ex.Message;
            }
        }

        private void AddLog(string message)
        {
            DataService.Instance.AddLog(message);
            RefreshLogs();
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
