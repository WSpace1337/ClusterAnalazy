using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Animation;
using ClusterVisualizer.Core.Models;
using ClusterVisualizer.Interfaces;


namespace ClusterVisualizer.Algoritms
{
    public class DBSCANAlgorithm : IClusteringAlgorithm
    {
        public string Name => "DBSCAN";

        private double eps = 5.0;
        private int minPts = 4;

        public ClusterResult Calculate(List<PointData> points, int k)
        {
            int clusterId = 0; 
            foreach(var point in points)
            {
                point.ClusterId = -1;
            }

            foreach (var point in points)
            {
                if (point.ClusterId != -1) continue;

                var neighbors = GetNeighbors(point,points);

                if(neighbors.Count <minPts)
                {
                    point.ClusterId = -2; //шумы
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
            for( int i = 0; i < neighbors.Count; i++)
            {
                var neighbor = neighbors[i];

                if (neighbor.ClusterId == -2)
                    neighbor.ClusterId = clusterId;

                if (neighbor.ClusterId != -1) continue;

                neighbor.ClusterId = clusterId;
                var newNeighbors = GetNeighbors(neighbor, points);

                if(newNeighbors.Count >= minPts) neighbors.AddRange(newNeighbors);
            }
        }

        private List<PointData> GetNeighbors(PointData point,
            List<PointData> points)
        {
            return points
                .Where(p => Distance(p, point) <= eps)
                .ToList();
        }

        private double Distance(PointData a, PointData b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }
    }
}
