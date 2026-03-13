
using System;
using System.Collections.Generic;
using ClusterVisualizer.Interfaces;

namespace ClusterVisualizer.Algorithms
{
    public class KMeansAlgorithm : IClusterAlgorithm
    {
        public ClusterResult Calculate(List<PointData> points, int k)
        {
            Random rand = new Random();
            foreach (var p in points)
            {
                p.ClusterId = rand.Next(k);
            }
            return new ClusterResult
            {
                Points = points,
                ClusterCount = k
            };
        }    
    }
}