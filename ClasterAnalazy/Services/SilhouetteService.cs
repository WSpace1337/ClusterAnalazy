using System;
using System.Collections.Generic;
using System.Linq;
using ClusterVisualizer.Core.Models;
using ClusterVisualizer.Interfaces;

namespace ClusterVisualizer.Services
{
    public class SilhouetteService
    {
        public double CalculateScore(List<PointData> points)
        {
            var clusters = points.GroupBy(p => p.ClusterId).ToList();

            double totalScore = 0;

            foreach (var point in points)
            {
                var ownCluster = clusters.First(c => c.Key == point.ClusterId).ToList();

                double a = ownCluster.Count > 1
                    ? ownCluster.Where(p => p != point)
                        .Average(p => Distance(point, p))
                    : 0;

                double b = double.MaxValue;

                foreach (var cluster in clusters)
                {
                    if (cluster.Key == point.ClusterId)
                        continue;

                    double dist = cluster.Average(p => Distance(point, p));

                    if (dist < b)
                        b = dist;
                }

                double s = (b - a) / Math.Max(a, b);

                totalScore += s;
            }

            return totalScore / points.Count;
        }

        public int FindBestK(List<PointData> points,
                     IClusteringAlgorithm algorithm,
                     int maxK = 10)
        {
            var bestK = 2;
            var bestScore = double.MinValue;

            for (int k = 2; k <= maxK; k++)
            {
                // копия данных (ВАЖНО)
                var cloned = points.Select(p => new PointData
                {
                    X = p.X,
                    Y = p.Y
                }).ToList();

                var result = algorithm.Calculate(cloned, k);

                double score = CalculateScore(result.Points);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestK = k;
                }
            }

            return bestK;
        }

        private double Distance(PointData a, PointData b)
        {
            return Math.Sqrt(
                Math.Pow(a.X - b.X, 2) +
                Math.Pow(a.Y - b.Y, 2)
            );
        }
    }
}