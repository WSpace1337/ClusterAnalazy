namespace ClusterVisualizer.Core.Models
{
    public class RfmCustomer
    {
        public string CustomerId { get; set; }

        public double Recency { get; set; }
        public double Frequency { get; set; }
        public double Monetary { get; set; }

        public int RScore { get; set; }
        public int FScore { get; set; }
        public int MScore { get; set; }

        public int TotalScore => RScore + FScore + MScore;

        public string RfmScore => $"{RScore}{FScore}{MScore}";

        public string Segment { get; set; }
    }
}