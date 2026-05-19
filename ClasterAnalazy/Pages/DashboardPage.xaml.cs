using System.Linq;
using System.Windows.Controls;
using ClusterVisualizer.Services;

namespace ClusterVisualizer.Pages
{
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            LoadDashboard();
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
    }
}