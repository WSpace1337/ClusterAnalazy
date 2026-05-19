using System;
using System.Windows;
using System.Windows.Controls;
using ClusterVisualizer.Services;

namespace ClusterVisualizer.Pages
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();


            AppSettings.Load();
            LoadSettings();
        }

        private void LoadSettings()
        {
            NormalizeCheckBox.IsChecked = AppSettings.NormalizeData;
            PreviewRowsBox.Text = AppSettings.MaxPreviewRows.ToString();
            DefaultEpsBox.Text = AppSettings.DefaultEps.ToString(System.Globalization.CultureInfo.InvariantCulture);
            DefaultMinPtsBox.Text = AppSettings.DefaultMinPts.ToString();

            if (AppSettings.DefaultAlgorithmIndex >= 0 &&
                AppSettings.DefaultAlgorithmIndex < DefaultAlgorithmBox.Items.Count)
            {
                DefaultAlgorithmBox.SelectedIndex = AppSettings.DefaultAlgorithmIndex;
            }
            else
            {
                DefaultAlgorithmBox.SelectedIndex = 0;
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.NormalizeData = NormalizeCheckBox.IsChecked == true;

            if (int.TryParse(PreviewRowsBox.Text, out int rows))
                AppSettings.MaxPreviewRows = rows;

            if (double.TryParse(DefaultEpsBox.Text.Replace(',', '.'),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out double eps))
            {
                AppSettings.DefaultEps = eps;
            }

            if (int.TryParse(DefaultMinPtsBox.Text, out int minPts))
                AppSettings.DefaultMinPts = minPts;

            AppSettings.DefaultAlgorithmIndex = DefaultAlgorithmBox.SelectedIndex;

            AppSettings.Save();
            StatusText.Text = "Settings saved.";
            DataService.Instance.AddLog("Settings updated.");
        }

        private void ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            DataService.Instance.Logs.Clear();
            StatusText.Text = "Logs cleared.";
        }
    }
}