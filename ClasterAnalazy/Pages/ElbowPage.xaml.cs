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

            this.DataContext= viewModel;
        }

        private void CalculateElbow_Click(object sender, RoutedEventArgs e)
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

                var values = elbowService.Calculate(points, maxK);

                var kneeDetector = new KneeDetector();

                int optimalK = kneeDetector.FindOptimalK(values);

                //Сохранеие значения SSE и "локтя" (оптимальное количество кластеров)
                DataService.Instance.SetElbowResult(values.Values.ToList(), optimalK);

                PlotView.Model = plotService.BuildElbowPlot(values);
                StatusText.Text = $"Optimal K = {optimalK}.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error: " + ex.Message;
            }
        }

    }
}
