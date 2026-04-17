using ClusterVisualizer.Core.Models;
using System.Collections.Generic;
using System;
using ClusterVisualizer.Interfaces;
using System.Linq;


namespace ClusterVisualizer.Core.Algorithms
{
    public class DBSCANAlgorithm : IClusteringAlgorithm
    {
        public string Name => "DBSCAN";

        private readonly double eps;
        private readonly int minPts;

        public DBSCANAlgorithm(double eps = 0.039, int minPts = 4)
        {
            this.eps = eps;
            this.minPts = minPts;
        }

        public ClusterResult Calculate(List<PointData> points, int k)
        {
            int clusterId = 0;

            foreach (var point in points)
                point.ClusterId = -1;

            foreach (var point in points)
            {
                if (point.ClusterId != -1)
                    continue;

                var neighbors = GetNeighbors(point, points);

                if (neighbors.Count < minPts)
                {
                    point.ClusterId = -2;
                    continue;
                }

                ExpandCluster(point, neighbors, clusterId, points);
                clusterId++;
            }

            return new ClusterResult
            {
                Points = points,
                ClusterCount = clusterId,
                Centroids = new List<PointData>()
            };
        }

        private void ExpandCluster(PointData point, List<PointData> neighbors, int clusterId, List<PointData> points)
        {
            point.ClusterId = clusterId;

            for (int i = 0; i < neighbors.Count; i++)
            {
                var neighbor = neighbors[i];

                if (neighbor.ClusterId == -2)
                    neighbor.ClusterId = clusterId;

                if (neighbor.ClusterId != -1)
                    continue;

                neighbor.ClusterId = clusterId;

                var newNeighbors = GetNeighbors(neighbor, points);

                if (newNeighbors.Count >= minPts)
                {
                    foreach (var p in newNeighbors)
                    {
                        if (!neighbors.Contains(p))
                            neighbors.Add(p);
                    }
                }
            }
        }

        private List<PointData> GetNeighbors(PointData point, List<PointData> points)
        {
            return points.Where(p => Distance(p, point) <= eps).ToList();
        }

        private double Distance(PointData a, PointData b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}