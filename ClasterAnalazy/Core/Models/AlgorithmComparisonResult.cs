namespace ClusterVisualizer.Core.Models
{
    public class AlgorithmComparisonResult
    {
        public string Algorithm { get; set; }

        public int Clusters { get; set; }

        public int Noise { get; set; }

        public double Silhouette { get; set; }

        public double DaviesBouldin { get; set; }

        public double CalinskiHarabasz { get; set; }
    }
}