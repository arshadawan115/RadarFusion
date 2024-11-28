using System;
using System.Windows;
using GMap.NET;
using GMap.NET.MapProviders;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

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
            RadarMap.MapProvider = GMapProviders.OpenStreetMap;
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

        private void MapProviderComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)MapProviderComboBox.SelectedItem;
            string mapProvider = selectedItem.Content.ToString();

            switch (mapProvider)
            {
                case "OpenStreetMap":
                    RadarMap.MapProvider = GMapProviders.OpenStreetMap;
                    break;
                case "Google Maps":
                    RadarMap.MapProvider = GMapProviders.GoogleMap;
                    break;
                case "Satellite":
                    RadarMap.MapProvider = GMapProviders.GoogleSatelliteMap;
                    break;
                case "Bing Maps":
                    RadarMap.MapProvider = GMapProviders.BingMap;
                    break;
                default:
                    RadarMap.MapProvider = GMapProviders.OpenStreetMap;
                    break;
            }
        }

        // Zoom In button click
        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            RadarMap.Zoom += 1;
        }

        // Zoom Out button click
        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            RadarMap.Zoom -= 1;
        }
    }
}
