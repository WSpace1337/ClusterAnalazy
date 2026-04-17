 namespace ClusterVisualizer.Core.Models { 
    public class PointData
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double OriginalX { get; set; }
        public double OriginalY { get; set; }

        public int ClusterId { get; set; }
    }
}