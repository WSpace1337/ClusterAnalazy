using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ClusterVisualizer.Services
{
    public class DataLoader
    {
        public List<PointData> Load(string path)
        {
            var points = new List<PointData>();
            
            var lines = File.ReadAllLines(path);

            for (int i = 1; i < lines.Length; i++) 
            {
                var parts = lines[i].Split(',');

                double age = double.Parse(parts[1], CultureInfo.InvariantCulture);
                double income = double.Parse(parts[4], CultureInfo.InvariantCulture);

                points.Add(new PointData
                {
                    X = age/100.0,
                    Y = income/100.0,
                });

            }
            return points;
        }
    }
}