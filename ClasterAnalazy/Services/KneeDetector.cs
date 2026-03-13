using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace ClusterVisualizer.Services
{
    public class KneeDetector {
        public int FindOptimalK(Dictionary<int, double> values)
        {
            var points = values
                .OrderBy(v => v.Value)
                .Select(v => (x: (double)v.Key, y: v.Value))
                .ToList();

            var first = points.First();
            var last = points.Last();

            double maxDistance = 0;
            int bestK = (int)first.x;

            foreach (var p in points)
            {
                double distance = DistanceFromLine(first, last, p);

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    bestK = (int)p.x;
                }
            }

            return bestK;
        }

        private double DistanceFromLine( 
            (double x, double y) a,
            (double x, double y) b,
            (double x, double y) p)
        {
            double numerator = Math.Abs(
                (b.y - a.y) * p.x -
                (b.x - a.x) * p.y +
                b.x * a.y -
                b.y * a.x);

            double denominator = Math.Sqrt(Math.Pow(b.y - a.y, 2)
                + Math.Pow(b.x - a.x, 2));

            return (numerator / denominator);
        }
    }
}
