using ClusterVisualizer.Core.Algorithms;
using ClusterVisualizer.Services;
using System.Collections.Generic;
using ClusterVisualizer.Core.Models;
using System.Linq;
using System;

namespace ClusterVisualizer.ViewModels
{
    public class MainViewModel
    {
        private DataLoader loader;
        private ClusterService clusterService;

        public List<PointData> Points { get; set; }

        public MainViewModel()
        {
            loader = new DataLoader();
            clusterService = new ClusterService(new KMeansAlgorithm());
        }

        //нормализация даных
        private void Normalize(List<PointData> points)
        {
            double minX = points.Min(p => p.X);
            double maxX = points.Max(p => p.X);

            double minY = points.Min(p => p.Y);
            double maxY = points.Max(p => p.Y);

            double rangeX = maxX - minX;
            double rangeY = maxY - minY;
            
            
            foreach (var p in points)
            {
                p.OriginalX = p.X;
                p.OriginalY = p.Y;

                p.X = rangeX == 0 ? 0 : (p.X - minX) / rangeX;
                p.Y = rangeY == 0 ? 0 : (p.Y - minY) / rangeY;
            }
        }

        public void LoadData(string path)
        {
            Points = loader.Load(path);

            Normalize(Points);
        }

        public ClusterResult RunClustering(int k)
        {
            return clusterService.Run(Points, k);
        }
    }
}