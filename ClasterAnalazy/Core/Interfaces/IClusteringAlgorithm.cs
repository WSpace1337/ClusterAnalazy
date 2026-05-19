using System;
using System.Collections.Generic;
using ClusterVisualizer.Core.Models;

namespace ClusterVisualizer.Interfaces
{
    public interface IClusteringAlgorithm
    {
        string Name { get; }

        ClusterResult Calculate(List<PointData> points, int k, Action<string> log = null);
    }
}