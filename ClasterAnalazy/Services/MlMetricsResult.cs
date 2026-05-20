namespace ClusterVisualizer.Services
{
    public class MlMetricsResult
    {
        public double Accuracy { get; set; }
        public double MacroAccuracy { get; set; }
        public double MicroAccuracy { get; set; }
        public double LogLoss { get; set; }
    }
}