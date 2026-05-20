using System;
using System.Collections.Generic;
using System.Linq;
using ClusterVisualizer.Core.Models;

namespace ClusterVisualizer.Services
{
    public class ClusterQualityService
    {
        public double SilhouetteScore(List<PointData> points)
        {
            var validPoints = points.Where(p => p.ClusterId >= 0).ToList();
            var clusters = validPoints.GroupBy(p => p.ClusterId).ToList();

            if (clusters.Count < 2)
                return 0;

            double total = 0;

            foreach (var point in validPoints)
            {
                var ownCluster = validPoints.Where(p => p.ClusterId == point.ClusterId).ToList();

                double a = ownCluster.Count > 1
                    ? ownCluster.Where(p => p != point).Average(p => Distance(point, p))
                    : 0;

                double b = clusters
                    .Where(c => c.Key != point.ClusterId)
                    .Min(c => c.Average(p => Distance(point, p)));

                double s = (b - a) / Math.Max(a, b);
                total += s;
            }

            return total / validPoints.Count;
        }

        public double DaviesBouldinIndex(List<PointData> points)
        {
            var validPoints = points.Where(p => p.ClusterId >= 0).ToList();
            var clusters = validPoints.GroupBy(p => p.ClusterId).ToList();

            if (clusters.Count < 2)
                return 0;

            var centroids = clusters.ToDictionary(
                c => c.Key,
                c => new PointData
                {
                    X = c.Average(p => p.X),
                    Y = c.Average(p => p.Y)
                });

            var scatter = clusters.ToDictionary(
                c => c.Key,
                c => c.Average(p => Distance(p, centroids[c.Key])));

            double sum = 0;

            foreach (var ci in clusters)
            {
                double maxR = double.MinValue;

                foreach (var cj in clusters)
                {
                    if (ci.Key == cj.Key)
                        continue;

                    double centroidDistance = Distance(centroids[ci.Key], centroids[cj.Key]);

                    if (centroidDistance == 0)
                        continue;

                    double r = (scatter[ci.Key] + scatter[cj.Key]) / centroidDistance;

                    if (r > maxR)
                        maxR = r;
                }

                sum += maxR;
            }

            return sum / clusters.Count;
        }

        public double CalinskiHarabaszIndex(List<PointData> points)
        {
            var validPoints = points.Where(p => p.ClusterId >= 0).ToList();
            var clusters = validPoints.GroupBy(p => p.ClusterId).ToList();

            int n = validPoints.Count;
            int k = clusters.Count;

            if (k < 2 || n <= k)
                return 0;

            var globalCentroid = new PointData
            {
                X = validPoints.Average(p => p.X),
                Y = validPoints.Average(p => p.Y)
            };

            double between = 0;
            double within = 0;

            foreach (var cluster in clusters)
            {
                var centroid = new PointData
                {
                    X = cluster.Average(p => p.X),
                    Y = cluster.Average(p => p.Y)
                };

                between += cluster.Count() * Math.Pow(Distance(centroid, globalCentroid), 2);

                foreach (var p in cluster)
                {
                    within += Math.Pow(Distance(p, centroid), 2);
                }
            }

            if (within == 0)
                return 0;

            return (between / (k - 1)) / (within / (n - k));
        }

        private double Distance(PointData a, PointData b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}