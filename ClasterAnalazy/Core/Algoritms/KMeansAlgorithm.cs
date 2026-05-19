
using System;
using System.Collections.Generic;
using System.Linq;
using ClusterVisualizer.Core.Models;
using ClusterVisualizer.Interfaces;

namespace ClusterVisualizer.Core.Algorithms
{
    public class KMeansAlgorithm : IClusteringAlgorithm
    {
        public string Name => "K-means";
        public ClusterResult Calculate(List<PointData> points, int k, Action<string> log = null)
        {
            log?.Invoke($"K-Means started. Points: {points.Count}, K: {k}");
            Random rand = new Random();
            var centroids = points
                .OrderBy (x => rand.Next())
                .Take (k)
                .Select (p  => new PointData { X = p.X, Y= p.Y})
                .ToList();

            log?.Invoke("Initial centroids selected.");

            bool changed = true;
            int maxInterations = 100;
            int iteration = 0;

            while (changed && iteration < maxInterations)
            {
                changed = false;
                iteration++;
                log?.Invoke($"Iteration {iteration} started.");

                foreach (var point in points) 
                {
                    double minDistance = double.MaxValue;
                    int cluster = 0;

                    for (int i = 0; i < k; i++)
                    {
                        double dist = Distance(point, centroids[i]);

                        if(dist < minDistance)
                        {
                            minDistance = dist;
                            cluster = i;
                        }
                    }
                        if(point.ClusterId!= cluster)
                        {
                            point.ClusterId = cluster;
                            changed = true;
                        }
                }
                for (int i = 0;i<k; i++)
                {
                    var clusterPoints = points.Where(p=>p.ClusterId == i).ToList();

                    if (clusterPoints.Count == 0)
                        continue;

                    centroids[i].X = clusterPoints.Average(p => p.X);
                    centroids[i].Y = clusterPoints.Average(p => p.Y);
                }
            }

            for (int i = 0; i < k; i++)
            {
                int count = points.Count(p => p.ClusterId == i);

                log?.Invoke(
                    $"Iteration {iteration}: Cluster {i} = {count} points, " +
                    $"Centroid=({centroids[i].X:F4}, {centroids[i].Y:F4})");
            }

            log?.Invoke($"K-Means finished after {iteration} iterations.");

            return new ClusterResult
            {
                Points = points,
                ClusterCount = k,
                Centroids = centroids
            };


        }
        
        private double Distance(PointData a, PointData b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2)+ Math.Pow(a.Y - b.Y, 2));
        }
    }
}