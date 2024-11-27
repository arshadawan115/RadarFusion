using System;
using System.Windows;
using GMap.NET;
using GMap.NET.MapProviders;
using System.Threading;
using System.Threading.Tasks;

namespace RadarMapping
{
    public partial class MainWindow : Window
    {
        private RadarDataSimulator _simulator;
        private CancellationTokenSource _cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
            InitializeMap();
        }

        private void InitializeMap()
        {
            RadarMap.MapProvider = GMapProviders.GoogleMap;
            RadarMap.Position = new PointLatLng(25.276987, 55.296249); // Example: Dubai coordinates
            RadarMap.ShowCenter = false;
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;

            _cancellationTokenSource = new CancellationTokenSource();
            _simulator = new RadarDataSimulator();

            try
            {
                await Task.Run(() => _simulator.StartSimulation(UpdateMapMarkers, _cancellationTokenSource.Token));
            }
            catch (OperationCanceledException)
            {
                // Simulation stopped
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }

        private void UpdateMapMarkers(PointLatLng position)
        {
            Dispatcher.Invoke(() =>
            {
                RadarMap.Markers.Clear();
                var marker = new GMap.NET.WindowsPresentation.GMapMarker(position)
                {
                    Shape = new System.Windows.Shapes.Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = System.Windows.Media.Brushes.Red
                    }
                };
                RadarMap.Markers.Add(marker);
            });
        }
    }
}
