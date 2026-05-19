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

        public ClusterResult Calculate(List<PointData> points, int k, Action<string> log = null)
        {

            log?.Invoke($"Hierarchical clustering started. Points: {points.Count}, Target K = {k}");

            var clusters = new List<List<PointData>>();

            foreach (var p in points)
            {
                clusters.Add(new List<PointData> { p });
            }

            int mergeStep = 0;

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

                mergeStep++;

                if (mergeStep <= 10 || mergeStep % 10 == 0 || clusters.Count == k + 1)
                {
                    log?.Invoke(
                        $"Step {mergeStep}: merging clusters {clusterA} and {clusterB}, " +
                        $"distance={minDistance:F4}, remaining={clusters.Count - 1}");
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

            for (int i = 0; i < clusters.Count; i++)
            {
                log?.Invoke($"Cluster {i}: {clusters[i].Count} points");
            }

            log?.Invoke($"Hierarchical clustering finished. Clusters = {clusters.Count}");

            return new ClusterResult
            {
                Points = points,
                ClusterCount = clusters.Count,
                Centroids = centroids
            };
        }

        public DendrogramNode BuildTree(List<PointData> points)
        {
            var nodes = points.Select((p, i) => new DendrogramNode
            {
                Id = i
            }).ToList();

            while (nodes.Count > 1)
            {
                double minDist = double.MaxValue;
                int a = 0, b = 1;

                for (int i = 0; i < nodes.Count; i++)
                {
                    for (int j = i + 1; j < nodes.Count; j++)
                    {
                        double dist = NodeDistance(nodes[i], nodes[j], points);

                        if (dist < minDist)
                        {
                            minDist = dist;
                            a = i;
                            b = j;
                        }
                    }
                }

                var newNode = new DendrogramNode
                {
                    Left = nodes[a],
                    Right = nodes[b],
                    Distance = minDist
                };

                nodes[a] = newNode;
                nodes.RemoveAt(b);
            }

            return nodes[0];
        }

        private double NodeDistance(DendrogramNode a, DendrogramNode b, List<PointData> points)
        {
            var pointsA = GetPoints(a, points);
            var pointsB = GetPoints(b, points);

            double minDist = double.MaxValue;

            foreach (var p1 in pointsA)
            {
                foreach (var p2 in pointsB)
                {
                    double dist = Distance(p1, p2);

                    if (dist < minDist)
                        minDist = dist;
                }
            }

            return minDist;
        }

        private List<PointData> GetPoints(DendrogramNode node, List<PointData> points)
        {
            if (node.Left == null && node.Right == null)
                return new List<PointData> { points[node.Id] };

            var result = new List<PointData>();

            if (node.Left != null)
                result.AddRange(GetPoints(node.Left, points));

            if (node.Right != null)
                result.AddRange(GetPoints(node.Right, points));

            return result;
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