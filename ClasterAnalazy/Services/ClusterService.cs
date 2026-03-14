using ClusterVisualizer.Interfaces;
using System.Collections.Generic;
using ClusterVisualizer.Core.Models;

namespace ClusterVisualizer.Services
{
    public class ClusterService
    {
        private readonly IClusteringAlgorithm algorithm;

        public ClusterService(IClusteringAlgorithm algorithm)
        {
            this.algorithm = algorithm;
        }

        public ClusterResult Run(List<PointData> points, int k)
        {
            return algorithm.Calculate(points,k);
        }
    }
}