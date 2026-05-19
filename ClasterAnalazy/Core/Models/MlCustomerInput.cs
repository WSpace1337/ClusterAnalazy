using Microsoft.ML.Data;

namespace ClusterVisualizer.Core.Models
{
    public class MlCustomerInput
    {
        public float X { get; set; }
        public float Y { get; set; }

        public string Label { get; set; }
    }
}