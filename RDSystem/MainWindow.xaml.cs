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

namespace RDSystem
{
    public partial class MainWindow : Window
    {
        private const string OpenSkyApiUrl = "https://opensky-network.org/api/states/all?lamin=55.337&lamax=69.060&lomin=10.593&lomax=24.150";

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


        public MainWindow()
        {
            InitializeComponent();

            // Initialize GMap Control
            MapControl.MapProvider = GMapProviders.OpenStreetMap;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            double latitude = 59.3293;//25.276987;
            double longitude = 18.0686;//55.296249;

            MapControl.Position = new PointLatLng(latitude, longitude); // Default center position
            MapControl.MinZoom = 2;
            MapControl.MaxZoom = 18;
            MapControl.Zoom = 5;

            MapProviderComboBox.SelectedIndex = 2;
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
                                    new Point(0, 0),       // Nose of the aircraft
                                    new Point(15, 5),      // Right wing
                                    new Point(10, 10),     // Tail (back)
                                    new Point(0, 5),       // Left wing
                                },
                                Stroke = Brushes.Blue,    // Aircraft border color (blue)
                                Fill = Brushes.SkyBlue,   // Aircraft fill color (sky blue)
                                StrokeThickness = 2       // Border thickness
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

        Polygon triangle = new Polygon
        {
            Points = new PointCollection
            {
                new Point(100, 100),
                new Point(200, 100),
                new Point(150, 200)
            },
            Fill = Brushes.Blue,
            Stroke = Brushes.Black,
            StrokeThickness = 2
        };
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

