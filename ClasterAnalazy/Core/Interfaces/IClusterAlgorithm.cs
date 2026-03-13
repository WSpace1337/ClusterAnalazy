using System.Collections.Generic;
using ClusterVisualizer.Core.Models;

namespace ClusterVisualizer.Interfaces
{
    public interface IClusterAlgorithm
    {
        ClusterResult Calculate(List<PointData> points, int clusterCount);
    }
}