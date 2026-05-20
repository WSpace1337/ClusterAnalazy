using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML;
using ClusterVisualizer.Core.Models;
using System.IO;

namespace ClusterVisualizer.Services
{
    public enum MlAlgorithmType
    {
        LogisticRegression,
        GradientBoosting,
        RandomForest
    }

    public class MlTrainingService
    {
        private readonly MLContext mlContext = new MLContext(seed: 1);

        public ITransformer Train(List<PointData> points, MlAlgorithmType algorithmType, Action<string> log = null)
        {
            var data = points
                .Where(p => p.ClusterId >= 0)
                .Select(p => new MlCustomerInput
                {
                    X = (float)p.X,
                    Y = (float)p.Y,
                    Label = p.ClusterId.ToString()
                })
                .ToList();

            log?.Invoke("ML training started.");
            log?.Invoke($"Selected algorithm: {algorithmType}");
            log?.Invoke($"Training rows: {data.Count}");

            if (data.Count == 0)
                throw new Exception("No clustered data for ML training.");

            var classGroups = data
    .GroupBy(d => d.Label)
    .OrderBy(g => g.Key)
    .ToList();

            log?.Invoke($"Detected classes: {classGroups.Count}");

            foreach (var group in classGroups)
            {
                log?.Invoke($"Class {group.Key}: {group.Count()} rows");
            }

            if (classGroups.Count < 2)
            {
                log?.Invoke("ML training stopped: less than 2 classes.");
                throw new Exception("ML training requires at least 2 clusters.");
            }

            int classCount = data
    .Select(d => d.Label)
    .Distinct()
    .Count();

            if (classCount < 2)
            {
                throw new Exception(
                    "ML training requires at least 2 clusters. DBSCAN found only 1 cluster. Try another eps/minPts or use K-Means/Hierarchical."
                );
            }

            var dataView = mlContext.Data.LoadFromEnumerable(data);

            IEstimator<ITransformer> pipeline =
                mlContext.Transforms.Conversion.MapValueToKey(
                    outputColumnName: "Label",
                    inputColumnName: "Label")
                .Append(mlContext.Transforms.Concatenate(
                    outputColumnName: "Features",
                    "X", "Y"));

            switch (algorithmType)
            {
                case MlAlgorithmType.LogisticRegression:
                    pipeline = pipeline.Append(
                        mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                            labelColumnName: "Label",
                            featureColumnName: "Features"));
                    break;

                case MlAlgorithmType.GradientBoosting:
                    pipeline = pipeline.Append(
                        mlContext.MulticlassClassification.Trainers.LightGbm(
                            labelColumnName: "Label",
                            featureColumnName: "Features"));
                    break;

                case MlAlgorithmType.RandomForest:
                    pipeline = pipeline.Append(
                        mlContext.MulticlassClassification.Trainers.OneVersusAll(
                            mlContext.BinaryClassification.Trainers.FastForest(
                                labelColumnName: "Label",
                                featureColumnName: "Features")));
                    break;

                default:
                    throw new Exception("Unknown ML algorithm.");
            }

            pipeline = pipeline.Append(
                mlContext.Transforms.Conversion.MapKeyToValue(
                    outputColumnName: "PredictedLabel",
                    inputColumnName: "PredictedLabel"));

            log?.Invoke("Building ML pipeline...");
            log?.Invoke("Feature columns: X, Y");
            log?.Invoke("Label column: ClusterId");

            log?.Invoke("Training model...");

            var model = pipeline.Fit(dataView);

            log?.Invoke("ML model trained successfully.");

            return model;
        }

        public MlCustomerPrediction PredictFull(ITransformer model, double x, double y)
        {
            if (model == null)
                throw new Exception("ML model is not trained.");

            var predictionEngine =
                mlContext.Model.CreatePredictionEngine<MlCustomerInput, MlCustomerPrediction>(model);

            var input = new MlCustomerInput
            {
                X = (float)x,
                Y = (float)y
            };

            return predictionEngine.Predict(input);
        }

        private IEstimator<ITransformer> BuildPipeline(MlAlgorithmType algorithmType)
        {
            IEstimator<ITransformer> pipeline =
                mlContext.Transforms.Conversion.MapValueToKey(
                    outputColumnName: "Label",
                    inputColumnName: "Label")
                .Append(mlContext.Transforms.Concatenate(
                    outputColumnName: "Features",
                    "X", "Y"));

            switch (algorithmType)
            {
                case MlAlgorithmType.LogisticRegression:
                    pipeline = pipeline.Append(
                        mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                            labelColumnName: "Label",
                            featureColumnName: "Features"));
                    break;

                case MlAlgorithmType.GradientBoosting:
                    pipeline = pipeline.Append(
                        mlContext.MulticlassClassification.Trainers.LightGbm(
                            labelColumnName: "Label",
                            featureColumnName: "Features"));
                    break;

                case MlAlgorithmType.RandomForest:
                    pipeline = pipeline.Append(
                        mlContext.MulticlassClassification.Trainers.OneVersusAll(
                            mlContext.BinaryClassification.Trainers.FastForest(
                                labelColumnName: "Label",
                                featureColumnName: "Features")));
                    break;
            }

            pipeline = pipeline.Append(
                mlContext.Transforms.Conversion.MapKeyToValue(
                    outputColumnName: "PredictedLabel",
                    inputColumnName: "PredictedLabel"));

            return pipeline;
        }

        public string Predict(ITransformer model, double x, double y)
        {
            if (model == null)
                throw new Exception("ML model is not trained.");

            var predictionEngine =
                mlContext.Model.CreatePredictionEngine<MlCustomerInput, MlCustomerPrediction>(model);

            var input = new MlCustomerInput
            {
                X = (float)x,
                Y = (float)y
            };

            return predictionEngine.Predict(input).PredictedLabel;
        }

        public MlTrainResult TrainWithDataView(List<PointData> points,MlAlgorithmType algorithmType,Action<string> log = null)
        {
            var data = points
                .Where(p => p.ClusterId >= 0)
                .Select(p => new MlCustomerInput
                {
                    X = (float)p.X,
                    Y = (float)p.Y,
                    Label = p.ClusterId.ToString()
                })
                .ToList();

            var dataView = mlContext.Data.LoadFromEnumerable(data);

            var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            var pipeline = BuildPipeline(algorithmType);

            var model = pipeline.Fit(split.TrainSet);

            return new MlTrainResult
            {
                Model = model,
                Schema = dataView.Schema,
                TestData = split.TestSet
            };
        }

        public class MlTrainResult
        {

            public IDataView TestData { get; set; }
            public ITransformer Model { get; set; }
            public DataViewSchema Schema { get; set; }

        }

        public ITransformer LoadModel(string path, out DataViewSchema schema)
        {
            if (!File.Exists(path))
                throw new Exception("Model file not found.");

            return mlContext.Model.Load(path, out schema);
        }

        public MlMetricsResult EvaluateModel(ITransformer model, IDataView testData)
        {
            var predictions = model.Transform(testData);

            var metrics = mlContext.MulticlassClassification.Evaluate(
                predictions,
                labelColumnName: "Label",
                predictedLabelColumnName: "PredictedLabel");

            return new MlMetricsResult
            {
                Accuracy = metrics.MacroAccuracy,
                MacroAccuracy = metrics.MacroAccuracy,
                MicroAccuracy = metrics.MicroAccuracy,
                LogLoss = metrics.LogLoss
            };
        }
    }
}