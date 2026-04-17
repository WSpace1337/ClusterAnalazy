using System;
using System.Collections.Generic;
using System.Linq;
using ClusterVisualizer.Core.Models;

namespace ClusterVisualizer.Services
{
    public class DbscanParameterService
    {
        public List<double> CalculateKDistance(List<PointData> points, int minPts)
        {
            var distances = new List<double>();

            foreach (var point in points)
            {
                var orderedDistances = points
                    .Where(p => p != point)
                    .Select(p => Distance(point, p))
                    .OrderBy(d => d)
                    .ToList();

                if (orderedDistances.Count >= minPts)
                {
                    distances.Add(orderedDistances[minPts - 1]);
                }
            }

            distances.Sort();
            return distances;
        }

        public double FindBestEps(List<double> distances)
        {
            if (distances == null || distances.Count < 3)
                return 0;

            double maxDistance = 0;
            int bestIndex = 0;

            var first = (x: 0.0, y: distances.First());
            var last = (x: (double)(distances.Count - 1), y: distances.Last());

            for (int i = 1; i < distances.Count - 1; i++)
            {
                var current = (x: (double)i, y: distances[i]);
                double dist = DistanceFromLine(first, last, current);

                if (dist > maxDistance)
                {
                    maxDistance = dist;
                    bestIndex = i;
                }
            }

            return distances[bestIndex];
        }

        private double Distance(PointData a, PointData b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
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

            double denominator = Math.Sqrt(
                Math.Pow(b.y - a.y, 2) +
                Math.Pow(b.x - a.x, 2));

            return denominator == 0 ? 0 : numerator / denominator;
        }
    }
}