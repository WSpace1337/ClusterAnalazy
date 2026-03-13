using OxyPlot;
using OxyPlot.Series;
using System.Collections.Generic;


namespace ClusterVisualizer.Visualization {
    public class PlotService
    {
        public PlotModel BuildPlot(List<PointData> points)
        {
            var model = new PlotModel();

            var series = new ScatterSeries();

            foreach (var p in points)
            {
                series.Points.Add(new ScatterPoint(p.X, p.Y));
            }

            model.Series.Add(series);

            return model;
        }
    }
}