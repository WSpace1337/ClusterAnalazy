using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClusterVisualizer.Services;

namespace ClusterVisualizer.Pages
{
    public partial class PredictionPage : Page
    {
        private MlTrainingService mlService;

        public PredictionPage()
        {
            InitializeComponent();

            mlService = new MlTrainingService();

            MlAlgorithmBox.ItemsSource = Enum.GetValues(typeof(MlAlgorithmType));
            MlAlgorithmBox.SelectedIndex = 0;
            modelStorageService = new ModelStorageService();

            TryAutoLoadModel();

            RefreshLogs();
        }

        private ModelStorageService modelStorageService;


        private void TryAutoLoadModel()
        {
            try
            {
                if (!modelStorageService.ModelExists())
                {
                    AddLog("No saved model found near application.");
                    return;
                }

                var model = modelStorageService.LoadModel(out var schema);

                DataService.Instance.SetMlModel(model);

                AddLog("Saved ML model loaded automatically.");
                AddLog($"Model path: {ModelStorageService.ModelPath}");

                StatusText.Text = "Saved model loaded.";
            }
            catch (Exception ex)
            {
                AddLog("Auto load model error: " + ex.Message);
                StatusText.Text = "Model auto-load failed.";
            }
        }

        private async void TrainModel_Click(object sender, RoutedEventArgs e)
        {
            var clusterResult = DataService.Instance.ClusterResult;

            if (clusterResult == null)
            {
                StatusText.Text = "First run clustering.";
                AddLog("ML training cancelled: first run clustering.");
                return;
            }

            try
            {
                ShowLoading();

                var algorithm = (MlAlgorithmType)MlAlgorithmBox.SelectedItem;

                AddLog("ML training started.");
                AddLog($"Selected algorithm: {algorithm}");

                var groups = clusterResult.Points
                    .Where(p => p.ClusterId >= 0)
                    .GroupBy(p => p.ClusterId)
                    .ToList();

                foreach (var group in groups)
                {
                    AddLog($"Training cluster {group.Key}: {group.Count()} points");
                }

                Action<string> logger = message =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        AddLog(message);
                    });
                };

                var trainResult = await Task.Run(() =>
                {
                    return mlService.TrainWithDataView(
                        clusterResult.Points,
                        algorithm,
                        logger
                    );
                });

                DataService.Instance.SetMlModel(trainResult.Model);

                await Task.Run(() =>
                {
                    modelStorageService.SaveModel(trainResult.Model, trainResult.Schema);
                });

                AddLog("ML model trained and saved.");
                AddLog($"Model saved to: {ModelStorageService.ModelPath}");

                StatusText.Text = $"Model trained: {algorithm}";
            }
            catch (Exception ex)
            {
                AddLog("ML training error: " + ex.Message);
                StatusText.Text = "Error: " + ex.Message;
            }
            finally
            {
                HideLoading();
            }
        }

        private void AddLog(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            LogBox.AppendText($"[{time}] {message}\n");
            LogBox.ScrollToEnd();
        }

        private void LoadSavedModel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var model = modelStorageService.LoadModel(out var schema);

                DataService.Instance.SetMlModel(model);

                AddLog("Saved ML model loaded.");
                AddLog($"Model path: {ModelStorageService.ModelPath}");

                StatusText.Text = "Saved model loaded.";
            }
            catch (Exception ex)
            {
                AddLog("Load model error: " + ex.Message);
                StatusText.Text = "Error: " + ex.Message;
            }
        }

        private void Predict_Click(object sender, RoutedEventArgs e)
        {
            if (DataService.Instance.MlModel == null)
            {
                StatusText.Text = "Train model first.";
                return;
            }

            if (!double.TryParse(AgeBox.Text, out double age))
            {
                StatusText.Text = "Invalid age.";
                return;
            }

            if (!double.TryParse(IncomeBox.Text, out double income))
            {
                StatusText.Text = "Invalid income.";
                return;
            }

            try
            {

                AddLog($"Normalization ranges: Age=[{DataService.Instance.MinX}; {DataService.Instance.MaxX}], Income=[{DataService.Instance.MinY}; {DataService.Instance.MaxY}]");
                AddLog($"Input real: Age={age}, Income={income}");


                double x = DataService.Instance.NormalizeX(age);
                double y = DataService.Instance.NormalizeY(income);

                AddLog($"Input normalized: X={x:F4}, Y={y:F4}");

                var prediction = mlService.PredictFull(DataService.Instance.MlModel, x, y);

                ResultText.Text = $"Predicted segment: {prediction.PredictedLabel}";
                AddLog($"Predicted segment: {prediction.PredictedLabel}");

                if (prediction.Score != null)
                {
                    for (int i = 0; i < prediction.Score.Length; i++)
                    {
                        AddLog($"Score index {i}: {prediction.Score[i]:F4}");
                    }
                }

                StatusText.Text = "Prediction completed.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error: " + ex.Message;
            }
        }
        private void RefreshLogs()
        {
            LogBox.Text = string.Join(Environment.NewLine, DataService.Instance.Logs);
            LogBox.ScrollToEnd();
        }

        private void ShowLoading()
        {
            LoadingOverlay.Visibility = Visibility.Visible;
        }

        private void HideLoading()
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }
}