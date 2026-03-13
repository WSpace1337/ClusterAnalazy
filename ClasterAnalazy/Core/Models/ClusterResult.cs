using System.Collections.Generic;


namespace ClusterVisualizer.Core.Models { 
    public class ClusterResult
    {
        public List<PointData> Points { get; set; }

        public int ClusterCount { get; set; }

        public List<PointData> Centroids { get; set; }
    }
}