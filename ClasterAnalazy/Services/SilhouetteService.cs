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
                      int maxK = 10,
                      IProgress<int> progress = null,
                      Action<string> log = null)
        {
            int bestK = 2;
            double bestScore = double.MinValue;

            int totalSteps = maxK - 1;
            int currentStep = 0;

            log?.Invoke($"Silhouette search started. MaxK = {maxK}");
            log?.Invoke($"Algorithm: {algorithm.Name}");
            log?.Invoke($"Dataset size: {points.Count} points");

            for (int k = 2; k <= maxK; k++)
            {
                log?.Invoke($"Calculating silhouette for K = {k}");

                var cloned = points.Select(p => new PointData
                {
                    X = p.X,
                    Y = p.Y
                }).ToList();

                var result = algorithm.Calculate(cloned, k);

                double score = CalculateScore(result.Points);

                log?.Invoke($"K = {k}, Silhouette score = {score:F4}");

                if (score > bestScore)
                {
                    bestScore = score;
                    bestK = k;

                    log?.Invoke($"New best K found: {bestK}, score = {bestScore:F4}");
                }

                currentStep++;
                int percent = currentStep * 100 / totalSteps;
                progress?.Report(percent);
            }

            log?.Invoke($"Silhouette search finished. Best K = {bestK}");
            log?.Invoke($"Best score = {bestScore:F4}");

            return bestK;
        }

        private double Distance(PointData a, PointData b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;

            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}