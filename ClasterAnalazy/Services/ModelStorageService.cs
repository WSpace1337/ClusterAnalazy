using System;
using System.IO;
using System.Text.Json;
using Microsoft.ML;

namespace ClusterVisualizer.Services
{
    public class ModelStorageService
    {
        private readonly MLContext mlContext = new MLContext(seed: 1);


        public static readonly string ModelPath =
    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ml_model.zip");

        public static readonly string InfoPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ml_model_info.json");

        public void SaveModel(ITransformer model, DataViewSchema schema)
        {   
            mlContext.Model.Save(model, schema, ModelPath);

            var info = new MlModelInfo
            {
                MinX = DataService.Instance.MinX,
                MaxX = DataService.Instance.MaxX,
                MinY = DataService.Instance.MinY,
                MaxY = DataService.Instance.MaxY,
                SavedAt = DateTime.Now
            };

            string json = JsonSerializer.Serialize(info, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(InfoPath, json);
        }

        public ITransformer LoadModel(out DataViewSchema schema)
        {
            if (!File.Exists(ModelPath))
                throw new Exception("Saved model not found.");

            if (File.Exists(InfoPath))
            {
                string json = File.ReadAllText(InfoPath);
                var info = JsonSerializer.Deserialize<MlModelInfo>(json);

                if (info != null)
                {
                    DataService.Instance.SetNormalizationParams(
                        info.MinX,
                        info.MaxX,
                        info.MinY,
                        info.MaxY
                    );
                }
            }

            return mlContext.Model.Load(ModelPath, out schema);
        }

        public bool ModelExists()
        {
            return File.Exists(ModelPath);
        }
    }

    public class MlModelInfo
    {
        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }
        public DateTime SavedAt { get; set; }
    }
}