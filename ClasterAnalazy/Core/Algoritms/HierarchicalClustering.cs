using System;
using System.Collections.Generic;
using System.Linq;
using ClusterVisualizer.Core.Models;
using ClusterVisualizer.Interfaces;

namespace ClusterVisualizer.Core.Algorithms
{
    public class HierarchicalClustering : IClusteringAlgorithm
    {
        public string Name => "Hierarchical (Agglomerative)";

        public ClusterResult Calculate(List<PointData> points, int k)
        {

            var clusters = new List<List<PointData>>();

            foreach (var p in points)
            {
                clusters.Add(new List<PointData> { p });
            }


            while (clusters.Count > k)
            {
                double minDistance = double.MaxValue;
                int clusterA = 0;
                int clusterB = 1;

                for (int i = 0; i < clusters.Count; i++)
                {
                    for (int j = i + 1; j < clusters.Count; j++)
                    {
                        double dist = ClusterDistance(clusters[i], clusters[j]);

                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            clusterA = i;
                            clusterB = j;
                        }
                    }
                }


                clusters[clusterA].AddRange(clusters[clusterB]);
                clusters.RemoveAt(clusterB);
            }

            for (int i = 0; i < clusters.Count; i++)
            {
                foreach (var p in clusters[i])
                {
                    p.ClusterId = i;
                }
            }

            var centroids = new List<PointData>();

            foreach (var cluster in clusters)
            {
                centroids.Add(new PointData
                {
                    X = cluster.Average(p => p.X),
                    Y = cluster.Average(p => p.Y)
                });
            }

            return new ClusterResult
            {
                Points = points,
                ClusterCount = clusters.Count,
                Centroids = centroids
            };
        }

        private double ClusterDistance(List<PointData> c1, List<PointData> c2)
        {
            double maxDist = double.MinValue;

            foreach (var p1 in c1)
            {
                foreach (var p2 in c2)
                {
                    double dist = Distance(p1, p2);

                    if (dist > maxDist)
                        maxDist = dist;
                }
            }

            return maxDist;
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