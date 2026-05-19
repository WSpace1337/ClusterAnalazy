using Microsoft.ML.Data;

namespace ClusterVisualizer.Core.Models
{
    public class MlCustomerPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedLabel { get; set; }

        public float[] Score { get; set; }
    }
}