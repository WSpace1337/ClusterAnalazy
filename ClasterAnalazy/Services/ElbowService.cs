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
        public Dictionary<int, double> Calculate(List<PointData> points, int maxK, Action<string> log = null)
        {
            var result = new Dictionary<int, double>();

            log?.Invoke($"Elbow Method started. MaxK = {maxK}");
            log?.Invoke($"Dataset size: {points.Count} points");

            for (int k = 1; k <= maxK; k++)
            {
                log?.Invoke($"Calculating K-Means for k = {k}");

                var clonedPoints = points.Select(p => new PointData
                {
                    X = p.X,
                    Y = p.Y
                }).ToList();

                var algorithm = new KMeansAlgorithm();

                var clusterResult = algorithm.Calculate(clonedPoints, k);

                double sse = CalculateSSE(clusterResult);

                result.Add(k, sse);

                log?.Invoke($"k = {k}, SSE = {sse:F4}");
            }

            log?.Invoke("Elbow Method finished.");

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
