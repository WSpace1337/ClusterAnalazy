using System;
using System.Collections.Generic;
using ClusterVisualizer.Core.Models;
using Microsoft.ML;




namespace ClusterVisualizer.Services
{
    public sealed class DataService
    {
        private static readonly DataService instance = new DataService();
        public static DataService Instance => instance;

        public string LoadedFilePath { get; set; }

        public void ClearAlgorithmComparison()
        {
            AlgorithmComparisonResults.Clear();
        }


        private DataService() { }

        public List<PointData> Points { get; set; }

        public ClusterResult ClusterResult { get; private set; }

        public Dictionary<int, double> ElbowValues { get; private set; }

        public int? OptimalK { get; private set; }

        public List<AlgorithmComparisonResult> AlgorithmComparisonResults { get; private set; } = new List<AlgorithmComparisonResult>();


        public void AddAlgorithmComparisonResult(AlgorithmComparisonResult result)
        {
            if (AlgorithmComparisonResults == null)
                AlgorithmComparisonResults = new List<AlgorithmComparisonResult>();

            AlgorithmComparisonResults.RemoveAll(x => x.Algorithm == result.Algorithm);
            AlgorithmComparisonResults.Add(result);
        }

        public void SetAlgorithmComparisonResults(List<AlgorithmComparisonResult> results)
        {
            AlgorithmComparisonResults = results ?? new List<AlgorithmComparisonResult>();
        }

        public void SetPoints(List<PointData> points)
        {
            Points = points;
            ClusterResult = null;
            ElbowValues = null;
            OptimalK = null;
        }

        public void SetClusterResult(ClusterResult result)
        {
            ClusterResult = result;
        }

        public void SetElbowResult(Dictionary<int, double> values, int optimalK)
        {
            ElbowValues = values;
            OptimalK = optimalK;
        }

        public bool HasData => Points != null && Points.Count > 0;

        public ITransformer MlModel { get; private set; }

        public void SetMlModel(ITransformer model)
        {
            MlModel = model;
        }

        public double MinX { get; private set; }
        public double MaxX { get; private set; }
        public double MinY { get; private set; }
        public double MaxY { get; private set; }

        public void SetNormalizationParams(double minX, double maxX, double minY, double maxY)
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
        }

        public double NormalizeX(double value)
        {
            double result = MaxX == MinX ? 0 : (value - MinX) / (MaxX - MinX);

            if (result < 0)
                return 0;

            if (result > 1)
                return 1;

            return result;
        }

        public double NormalizeY(double value)
        {
            double result = MaxY == MinY ? 0 : (value - MinY) / (MaxY - MinY);

            if (result < 0)
                return 0;

            if (result > 1)
                return 1;

            return result;
        }

        public List<string> Logs { get; } = new List<string>();

        public void AddLog(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            Logs.Add($"[{time}] {message}");
        }



    }
}
