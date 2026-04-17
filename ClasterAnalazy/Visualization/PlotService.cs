using ClusterVisualizer.Core.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;



namespace ClusterVisualizer.Visualization {
    public class PlotService
    {
        //Строит кластери на графике
        public PlotModel BuildPlot(ClusterResult result)
        {
            var model = new PlotModel {Title = "Clusters" };

            var color = new[]
            {
                OxyColors.Red,
                OxyColors.Blue,
                OxyColors.Green,
                OxyColors.Orange,
                OxyColors.Purple,
                OxyColors.Brown
            };

            for (int i = 0; i < result.ClusterCount; i++)
            {
                var series = new ScatterSeries
                {
                    MarkerType = MarkerType.Circle,
                    MarkerFill = color[i % color.Length],
                    MarkerSize = 4,
                };
                /*
                foreach (var p in result.Points)
                {
                    Console.WriteLine(p.ClusterId);
                }
                */
                var noisePoints = result.Points.Where(p => p.ClusterId == -2).ToList();
                if (noisePoints.Any())
                {
                    var noiseSeries = new ScatterSeries
                    {
                        MarkerType = MarkerType.Circle,
                        MarkerFill = OxyColors.Gray,
                        MarkerSize = 3,
                        Title = "Noise"
                    };
                    foreach (var p in noisePoints)
                    {
                        noiseSeries.Points.Add(new ScatterPoint(p.X, p.Y));
                    }
                    model.Series.Add(noiseSeries);
                }

                foreach (var p in result.Points.Where(p => p.ClusterId ==i))
                {
                    series.Points.Add(new ScatterPoint(p.X,p.Y));
                }
                model.Series.Add(series);
            }

            AddCentroids(model,result.Centroids);

            return model;
        }

        //ESP дистанция
        public PlotModel BuildKDistancePlot(List<double> distances, double bestEps)
        {
            var model = new PlotModel { Title = "DBSCAN k-distance graph" };

            var lineSeries = new LineSeries
            {
                Title = "k-distance",
                MarkerType = MarkerType.Circle
            };

            for (int i = 0; i < distances.Count; i++)
            {
                lineSeries.Points.Add(new DataPoint(i, distances[i]));
            }

            model.Series.Add(lineSeries);

            var epsLine = new LineSeries
            {
                Title = $"Suggested eps = {bestEps:F3}",
                StrokeThickness = 2
            };

            epsLine.Points.Add(new DataPoint(0, bestEps));
            epsLine.Points.Add(new DataPoint(distances.Count - 1, bestEps));

            model.Series.Add(epsLine);

            return model;
        }


        //Построение графика Elbow
        public PlotModel BuildElbowPlot(Dictionary<int,double> values)
        {
            var model = new PlotModel { Title = "Elbow Methid" };

            var series = new LineSeries
            {
                MarkerType = MarkerType.Circle
            };

            foreach(var v in values)
            {
                series.Points.Add(new DataPoint(v.Key, v.Value));
            }

            model.Series.Add(series);

            return model;
        }

        // создает центроиды кластеров
        private void AddCentroids(PlotModel model, List<PointData> centroids)
        {
            var centroidSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Diamond,
                MarkerFill=OxyColors.Black,
                MarkerSize = 8,
            };

            foreach(var c in centroids)
            {
                centroidSeries.Points.Add(new ScatterPoint(c.X,c.Y));
            }
            model.Series.Add(centroidSeries);
        }

        public PlotModel BuildDendrogram(DendrogramNode root)
        {
            var model = new PlotModel { Title = "Dendrogram" };

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Objects",
                MinimumPadding = 0.02,
                MaximumPadding = 0.02
            });

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Distance",
                Minimum = 0
            });

            var lineSeries = new LineSeries
            {
                Color = OxyColors.Black,
                StrokeThickness = 1,
                LineStyle = LineStyle.Solid
            };

            // 1. назначаем X-позиции листьям
            var leafPositions = new Dictionary<DendrogramNode, double>();
            int leafIndex = 0;
            AssignLeafPositions(root, leafPositions, ref leafIndex);

            // 2. рисуем дерево рекурсивно
            DrawDendrogramNode(root, leafPositions, lineSeries);

            model.Series.Add(lineSeries);

            return model;
        }
        private void AssignLeafPositions(DendrogramNode node, Dictionary<DendrogramNode, double> leafPositions, ref int leafIndex)
        {
            if (node == null)
                return;

            // Лист
            if (node.Left == null && node.Right == null)
            {
                leafPositions[node] = leafIndex;
                leafIndex++;
                return;
            }

            AssignLeafPositions(node.Left, leafPositions, ref leafIndex);
            AssignLeafPositions(node.Right, leafPositions, ref leafIndex);
        }

        private double GetNodeX(
                    DendrogramNode node,
                    Dictionary<DendrogramNode, double> leafPositions)
        {
            if (node.Left == null && node.Right == null)
                return leafPositions[node];

            double leftX = GetNodeX(node.Left, leafPositions);
            double rightX = GetNodeX(node.Right, leafPositions);

            return (leftX + rightX) / 2.0;
        }

        private void DrawDendrogramNode(
                        DendrogramNode node,
                        Dictionary<DendrogramNode, double> leafPositions,
                        LineSeries series)
        {
            if (node == null)
                return;

            // Если лист — рисовать нечего
            if (node.Left == null || node.Right == null)
                return;

            double leftX = GetNodeX(node.Left, leafPositions);
            double rightX = GetNodeX(node.Right, leafPositions);

            double leftY = node.Left.Distance;
            double rightY = node.Right.Distance;
            double currentY = node.Distance;

            // Левая вертикаль
            AddSegment(series, leftX, leftY, leftX, currentY);

            // Правая вертикаль
            AddSegment(series, rightX, rightY, rightX, currentY);

            // Горизонталь соединения
            AddSegment(series, leftX, currentY, rightX, currentY);

            // Рекурсивно рисуем потомков
            DrawDendrogramNode(node.Left, leafPositions, series);
            DrawDendrogramNode(node.Right, leafPositions, series);
        }

        private void AddSegment(LineSeries series, double x1, double y1, double x2, double y2)
        {
            series.Points.Add(new DataPoint(x1, y1));
            series.Points.Add(new DataPoint(x2, y2));
            series.Points.Add(DataPoint.Undefined);
        }
    }
}