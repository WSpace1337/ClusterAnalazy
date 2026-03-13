using ClusterVisualizer.Algorithms;
using ClusterVisualizer.Services;
using System.Windows.Documents;
using System.Collections.Generic;

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

        public void LoadData(string path)
        {
            Points = loader.Load(path);
        }

        public ClusterResult RunClustering(int k)
        {
            return clusterService.Run(Points, k);
        }
    }
}