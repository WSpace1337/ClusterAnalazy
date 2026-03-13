using ClusterVisualizer.Core.Models;
using OxyPlot;
using OxyPlot.Series;
using System;
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

                foreach (var p in result.Points.Where(p => p.ClusterId ==i))
                {
                    series.Points.Add(new ScatterPoint(p.X,p.Y));
                }
                model.Series.Add(series);
            }

            AddCentroids(model,result.Centroids);

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
    }
}