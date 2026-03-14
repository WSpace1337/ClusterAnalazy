
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
        public ClusterResult Calculate(List<PointData> points, int k)
        {
            Random rand = new Random();
            var centroids = points
                .OrderBy (x => rand.Next())
                .Take (k)
                .Select (p  => new PointData { X = p.X, Y= p.Y})
                .ToList();

            bool changed = true;
            int maxInterations = 100;
            int iteration = 0;

            while (changed && iteration < maxInterations)
            {
                changed = false;
                iteration++;

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