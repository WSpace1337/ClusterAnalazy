using System.Collections.Generic;
using ClusterVisualizer.Core.Models;


namespace ClusterVisualizer.Services
{
    public sealed class DataService
    {
        private static readonly DataService instance = new DataService();
        public static DataService Instance => instance;

        private DataService() { }

        public List<PointData> Points { get; set; }

        public ClusterResult ClusterResult { get; private set; }

        public List<double> ElbowValues { get; private set; }
        public int? OptimalK {  get; private set; }

        public void SetPoints(List<PointData> points)
        {
            Points = points;
            ClusterResult = null;
            ElbowValues = null;
            OptimalK = null;
        }

        public void SetClusterResult(ClusterResult result)
        {
            ClusterResult = result;
        }

        public void SetElbowResult(List<double> values, int optimalK)
        {
            ElbowValues = values;
            OptimalK = optimalK;
        }

        public bool HasData => Points != null && Points.Count > 0;

    }
}
