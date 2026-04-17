namespace ClusterVisualizer.Core.Models
{
    public class DendrogramNode
    {
        public DendrogramNode Left { get; set; }
        public DendrogramNode Right { get; set; }

        public double Distance { get; set; }

        public int Id { get; set; } 
    }
}