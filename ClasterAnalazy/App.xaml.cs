using ClusterVisualizer.Services;
using System.Windows;

namespace ClusterVisualizer
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppSettings.Load();
            base.OnStartup(e);
        }
    }
}