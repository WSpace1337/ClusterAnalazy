using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClusterVisualizer.Core.Models;
using ClusterVisualizer.Core.Algorithms;

namespace ClusterVisualizer.Services
{
    public class ElbowService
    {
        public Dictionary<int,double> Calculate(List<PointData>points, int maxK)
        {
            var result = new Dictionary<int,double>();

            for (int k = 1; k < maxK; k++)
            {
                var algoritm = new KMeansAlgorithm();

                var clusterResult = algoritm.Calculate(points, k);

                double sse = CalculateSSE(clusterResult);

                result.Add(k, sse);
            }

            return result;
        }
        private double CalculateSSE(ClusterResult result)
        {
            double sum = 0;

            foreach (var point in result.Points)
            {
                if (point.ClusterId < 0 || point.ClusterId >= result.Centroids.Count)
                    continue;

                var centroid = result.Centroids[point.ClusterId];

                double dx = point.X - centroid.X;
                double dy = point.Y - centroid.Y;

                sum += dx * dx + dy * dy;
            }
            return sum;
        }
    }
}
