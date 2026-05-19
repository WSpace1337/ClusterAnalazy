using System;
using System.IO;
using System.Text.Json;

namespace ClusterVisualizer.Services
{
    public static class AppSettings
    {

        public static string SettingsFilePath => FilePath;
        private static readonly string FolderPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClusterVisualizer");

        private static readonly string FilePath =
            Path.Combine(FolderPath, "settings.json");

        public static bool NormalizeData { get; set; } = true;
        public static int MaxPreviewRows { get; set; } = 10;
        public static double DefaultEps { get; set; } = 0.039;
        public static int DefaultMinPts { get; set; } = 4;
        public static int DefaultAlgorithmIndex { get; set; } = 0;

        public static void Save()
        {
            var data = new AppSettingsData
            {
                NormalizeData = NormalizeData,
                MaxPreviewRows = MaxPreviewRows,
                DefaultEps = DefaultEps,
                DefaultMinPts = DefaultMinPts,
                DefaultAlgorithmIndex = DefaultAlgorithmIndex
            };

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            Directory.CreateDirectory(FolderPath);
            File.WriteAllText(FilePath, json);
        }

        public static void Load()
        {
            if (!File.Exists(FilePath))
                return;

            string json = File.ReadAllText(FilePath);
            var data = JsonSerializer.Deserialize<AppSettingsData>(json);

            if (data == null)
                return;

            NormalizeData = data.NormalizeData;
            MaxPreviewRows = data.MaxPreviewRows;
            DefaultEps = data.DefaultEps;
            DefaultMinPts = data.DefaultMinPts;
            DefaultAlgorithmIndex = data.DefaultAlgorithmIndex;
        }
    }

    public class AppSettingsData
    {
        public bool NormalizeData { get; set; }
        public int MaxPreviewRows { get; set; }
        public double DefaultEps { get; set; }
        public int DefaultMinPts { get; set; }
        public int DefaultAlgorithmIndex { get; set; }
    }
}