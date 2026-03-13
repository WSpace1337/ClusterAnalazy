using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ClusterVisualizer.Services
{
    public class DataLoader
    {
        public List<PointData> Load(string path)
        {
            var points = new List<PointData>();

            foreach(var line in File.ReadLines(path).Skip(1))
            {
                var parts = line.Split(',');

                points.Add(new PointData
                {
                    X = double.Parse(parts[0]),
                    Y = double.Parse(parts[1])
                });
            }
            return points;
        }
    }
}