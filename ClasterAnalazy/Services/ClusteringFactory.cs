using System.Collections.Generic;
using ClusterVisualizer.Interfaces;
using ClusterVisualizer.Algoritms;
using ClusterVisualizer.Core.Algorithms;

namespace ClusterVisualizer.Services
{
    public static class ClusteringFactory
    {
        public static List<IClusteringAlgorithm> GetAlgorithms()
        {
            return new List<IClusteringAlgorithm>()
            {
                new KMeansAlgorithm(),
                new DBSCANAlgorithm(),
                new HierarchicalClustering(),
            };
            }
    }
}