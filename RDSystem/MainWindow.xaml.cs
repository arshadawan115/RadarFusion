using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Newtonsoft.Json;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Text;
using System.Net.Http.Headers;

namespace RDSystem
{
    public partial class MainWindow : Window
    {
        string username = "arshadawan115"; // Replace with your OpenSky username
        string password = "786Allah786@"; // Replace with your OpenSky password
        private const string OpenSkyApiUrl = "https://opensky-network.org/api/states/all";

        //all //"https://opensky-network.org/api/states/all";

        //Sweden//https://opensky-network.org/api/states/all?lamin=55.337&lamax=69.060&lomin=10.593&lomax=24.150
        //UAE//string url = "https://opensky-network.org/api/states/all?lamin=22.633&lamax=26.084&lomin=51.583&lomax=56.381";

        // Geographical Filtering
        //You can also use a bounding box around Sweden to get data for aircraft flying within its airspace.

        //Bounding Box for Sweden:
        //North Latitude: 69.060
        //South Latitude: 55.337
        //West Longitude: 10.593
        //East Longitude: 24.150
        //You can include these coordinates in your OpenSky API call using the lamin, lamax, lomin, and lomax parameters.
        private readonly HttpClient _httpClient;
        private readonly DispatcherTimer _timer;
        private bool _isFetchingData;
        public MainWindow()
        {
            InitializeComponent();

            // Initialize GMap Control
            MapControl.MapProvider = GMapProviders.OpenStreetMap;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            double latitude = 25.276987;//25.276987;
            double longitude = 55.296249;//55.296249;

            MapControl.Position = new PointLatLng(latitude, longitude); // Default center position
            MapControl.MinZoom = 2;
            MapControl.MaxZoom = 18;
            MapControl.Zoom = 5;

            MapProviderComboBox.SelectedIndex = 2;

            _httpClient = new HttpClient();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(10); // Set interval to 10 seconds
            _timer.Tick += async (sender, e) => await FetchAircraftDataAsync();

            // Initially set the fetching state to false
            _isFetchingData = false;
            StopButton.IsEnabled = false; // Disable Stop button initially

        }

        private async Task FetchAircraftDataAsync()
        {
            if (!_isFetchingData)
                return;

            try
            {
                var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // Fetch real-time data from OpenSky API (replace with the actual endpoint)
                var response = await _httpClient.GetStringAsync("https://opensky-network.org/api/states/all");
                var aircraftData = ParseAircraftData(response); // Parse the response to get aircraft data

                // Update markers on the map
                UpdateAircraftMarkers(aircraftData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching data: {ex.Message}");
            }
        }

        public List<Aircraft> ParseAircraftData(string response)
        {
            // Deserialize the response into a dynamic object
            var data = JsonConvert.DeserializeObject<dynamic>(response);

            // List to hold the parsed aircraft data
            List<Aircraft> aircraftList = new List<Aircraft>();

            // Check if "states" is present in the response
            if (data != null && data.states != null)
            {
                // Loop through the states array and create Aircraft objects
                foreach (var state in data.states)
                {
                    Aircraft aircraft = new Aircraft
                    {
                        Callsign = state[1] != null ? state[1].ToString() : null,
                        Latitude = state[6] != null ? (double?)state[6] : null,
                        Longitude = state[5] != null ? (double?)state[5] : null,
                        Altitude = state[7] != null ? (double?)state[7] : null,
                        Velocity = state[9] != null ? (double?)state[9] : null,
                        ICAO24 = state[0] != null ? state[0].ToString() : null
                    };

                    // Add the aircraft to the list
                    aircraftList.Add(aircraft);
                }
            }

            return aircraftList;
        }

        private async void FetchDataButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Fetch aircraft data
                var aircraftData = await FetchAircraftData();

                // Bind to DataGrid
                AircraftDataGrid.ItemsSource = aircraftData;

                // Clear previous markers
                MapControl.Markers.Clear();

                foreach (var aircraft in aircraftData)
                {
                    if (aircraft.Latitude != null && aircraft.Longitude != null)
                    {
                        // Creating a custom aircraft-like marker
                        var marker = new GMapMarker(new PointLatLng((double)aircraft.Latitude, (double)aircraft.Longitude))
                        {
                            Shape = new Polygon
                            {
                                Points = new PointCollection
                                {
                                   new Point(25, 15),  // Top point
                                   new Point(15, 30),  // Bottom left point
                                   new Point(36, 30),  // Bottom right point
                                },
                                Stroke = Brushes.Red,    // Aircraft border color (red)
                                Fill = Brushes.SkyBlue,   // Aircraft fill color (sky blue)
                                StrokeThickness = 1       // Border thickness
                            }
                        };

                        MapControl.Markers.Add(marker);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<List<Aircraft>> FetchAircraftData()
        {
            using (HttpClient client = new HttpClient())
            {
                var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                HttpResponseMessage response = await client.GetAsync(OpenSkyApiUrl);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<OpenSkyResponse>(responseBody);

                var aircraftList = new List<Aircraft>();
                foreach (var state in result.States)
                {
                    aircraftList.Add(new Aircraft
                    {
                        ICAO24 = state[0]?.ToString(),
                        Callsign = state[1]?.ToString(),
                        Latitude = state[6] != null ? Convert.ToDouble(state[6]) : (double?)null,
                        Longitude = state[5] != null ? Convert.ToDouble(state[5]) : (double?)null,
                        Altitude = state[7] != null ? Convert.ToDouble(state[7]) : (double?)null,
                        Velocity = state[9] != null ? Convert.ToDouble(state[9]) : (double?)null,
                        Heading = state[10] != null ? Convert.ToDouble(state[10]) : (double?)null
                    });
                }

                return aircraftList;
            }
        }

        //
        private void UpdateAircraftMarkers(List<Aircraft> aircraftData)
        {
            AircraftDataGrid.ItemsSource = aircraftData;

            // Clear existing markers before adding new ones to avoid excessive memory usage
            MapControl.Markers.Clear();

            foreach (var aircraft in aircraftData)
            {
                if (aircraft.Latitude != null && aircraft.Longitude != null)
                {
                    var marker = new GMapMarker(new PointLatLng((double)aircraft.Latitude, (double)aircraft.Longitude))
                    {
                        Shape = new Polygon
                        {
                            Points = new PointCollection
                                {
                                   new Point(25, 15),  // Top point
                                   new Point(15, 30),  // Bottom left point
                                   new Point(36, 30),  // Bottom right point
                                },
                            Stroke = Brushes.Red,    // Aircraft border color (red)
                            Fill = Brushes.SkyBlue,   // Aircraft fill color (sky blue)
                            StrokeThickness = 1       // Border thickness
                        }
                    };

                    MapControl.Markers.Add(marker);
                }
            }
        }
            //

        private void MapProviderComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)MapProviderComboBox.SelectedItem;
            string mapProvider = selectedItem.Content.ToString();

            switch (mapProvider)
            {
                case "OpenStreetMap":
                    MapControl.MapProvider = GMapProviders.OpenStreetMap;
                    break;
                case "Google Maps":
                    MapControl.MapProvider = GMapProviders.GoogleMap;
                    break;
                case "Satellite":
                    MapControl.MapProvider = GMapProviders.GoogleSatelliteMap;
                    break;
                case "Bing Maps":
                    MapControl.MapProvider = GMapProviders.BingMap;
                    break;
                default:
                    MapControl.MapProvider = GMapProviders.OpenStreetMap;
                    break;
            }
        }

        // Zoom In button click
        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            MapControl.Zoom += 1;
        }

        // Zoom Out button click
        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            MapControl.Zoom -= 1;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Start fetching data
            _isFetchingData = true;
            StartButton.IsEnabled = false;  // Disable the Start button
            StopButton.IsEnabled = true;    // Enable the Stop button
            _timer.Start(); // Start the timer to fetch data periodically
        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            // Stop fetching data
            _isFetchingData = false;
            StartButton.IsEnabled = true;   // Enable the Start button
            StopButton.IsEnabled = false;  // Disable the Stop button
            _timer.Stop(); // Stop the timer from fetching data
        }

        private void ButtonClearData_Click(object sender, RoutedEventArgs e)
        {
            MapControl.Markers.Clear();
            AircraftDataGrid.ItemsSource = null;
            AircraftDataGrid.Items.Refresh();

        }
    }

    // Models to represent OpenSky API data
    public class OpenSkyResponse
    {
        [JsonProperty("states")]
        public List<List<object>> States { get; set; }
    }

    public class Aircraft
    {
        public string ICAO24 { get; set; }
        public string Callsign { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Altitude { get; set; }
        public double? Velocity { get; set; }
        public double? Heading { get; set; }
    }
}

