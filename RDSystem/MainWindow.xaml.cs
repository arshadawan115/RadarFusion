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
using System.Data.SqlClient;
using System.Diagnostics;

namespace RDSystem
{
    public partial class MainWindow : Window
    {
        string _username = "arshadawan115"; // Replace with your OpenSky username
        string _password = "786Allah786@"; // Replace with your OpenSky password
        static int _postionCount=0;
        List<Position> _flyObjects;
       //string OpenSkyApiUrl = "https://opensky-network.org/api/states/all?bbox=35.0,71.0,-25.0,45.0"; //EU

        //string OpenSkyApiUrl = "https://opensky-network.org/api/states/all"; //world

        //string baseEuUrl = "https://opensky-network.org/api/states/all";
        //string bbox = "35.0,71.0,-25.0,45.0";
        //string url = baseEuUrl + "bbox=" + bbox;

        //private const string OpenSkyApiUrl = "https://opensky-network.org/api/states/all?lamin=55.337&lamax=69.060&lomin=10.593&lomax=24.150";
        //    //"https://opensky-network.org/api/states/all";//"https://opensky-network.org/api/states/all?lamin=22.633&lamax=26.084&lomin=51.583&lomax=56.381";//

        //string apiKey = "YOUR_API_KEY"; // Replace with your actual FlightRadar24 API key
        //string apiUrl = "https://api.flightradar24.com/common/v1/aircraft.json"; // Example endpoint for aircraft data

        //https://adsbexchange.com/api/aircrafts/";

        //all //"https://opensky-network.org/api/states/all";

        //Sweden//
        string OpenSkyApiUrl = "https://opensky-network.org/api/states/all?lamin=55.337&lamax=69.060&lomin=10.593&lomax=24.150";

        //UAE//
        //string OpenSkyApiUrl = "https://opensky-network.org/api/states/all?lamin=22.633&lamax=26.084&lomin=51.583&lomax=56.381";

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
            MapControl.MapProvider = GMapProviders.ArcGIS_World_Topo_Map;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            //double latitude = 25.276987;//UAE
            //double longitude = 55.296249;//UAE

            double latitude = 60.1282;//Sweden
            double longitude = 18.6435;

            MapControl.Position = new PointLatLng(latitude, longitude); // Default center position
            MapControl.MinZoom = 2;
            MapControl.MaxZoom = 18;
            MapControl.Zoom = 5;

            MapProviderComboBox.SelectedIndex = 0;

            _httpClient = new HttpClient();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1); // Set interval to 5 seconds
            _timer.Tick += async (sender, e) => await FetchAircraftDataAsync();

            // Initially set the fetching state to false
            _isFetchingData = false;
            StopButton.IsEnabled = false; // Disable Stop button initially

            string connectionString = @"Server=DESKTOP-77PED1A\SQLEXPRESS;Database=RDS_DB;Trusted_Connection=True;";
            // Fetch FlyObjects data
            _flyObjects = ReadPositionsData(connectionString, "StockholmSelect");

        }

        private async Task FetchAircraftDataAsync()
        {
            if (!_isFetchingData)
                return;

            List<Aircraft> aircraftData = new List<Aircraft>();

            try
            {
                var byteArray = Encoding.ASCII.GetBytes($"{_username}:{_password}");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // Fetch real-time data from OpenSky API (replace with the actual endpoint)
                var response = await _httpClient.GetStringAsync(OpenSkyApiUrl);
                aircraftData = ParseAircraftData(response); // Parse the response to get aircraft data
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching API data: {ex.Message}");
            }

            try
            {
                if (_postionCount < _flyObjects.Count)
                {
                    bool isUnkown = true;

                    Aircraft unkown = new Aircraft(_flyObjects[_postionCount].ICAO24,
                        _flyObjects[_postionCount].ICAO24,
                        _flyObjects[_postionCount].Latitude,
                        _flyObjects[_postionCount].Longitude,
                        _flyObjects[_postionCount].Altitude,
                        _flyObjects[_postionCount].Velocity,
                        _flyObjects[_postionCount].Heading,
                        isUnkown);

                    aircraftData.Add(unkown);

                    //UpdateAircraftMarker(_flyObjects[_postionCount]);
                    _postionCount++;
                }
                else
                {
                    _postionCount = 0;

                    bool isUnkown = true;

                    Aircraft unkown = new Aircraft(_flyObjects[_postionCount].ICAO24,
                        _flyObjects[_postionCount].ICAO24,
                        _flyObjects[_postionCount].Latitude,
                        _flyObjects[_postionCount].Longitude,
                        _flyObjects[_postionCount].Altitude,
                        _flyObjects[_postionCount].Velocity,
                        _flyObjects[_postionCount].Heading,
                        isUnkown);

                    aircraftData.Add(unkown);
                }

                // Update markers on the map
                UpdateAircraftMarkers(aircraftData);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error fetching IoT data: {ex.Message}");
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
                        ICAO24 = state[0] != null ? state[0].ToString() : null,
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
                                   new Point(20, 10),  // Top point
                                   new Point(10, 25),  // Bottom left point
                                   new Point(31, 25),  // Bottom right point
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
                var byteArray = Encoding.ASCII.GetBytes($"{_username}:{_password}");
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
                    SolidColorBrush strokeColor = Brushes.Red;    // Aircraft border color (red)
                    SolidColorBrush fillColor = Brushes.SkyBlue;   // Aircraft fill color (sky blue)
                    double strokeThickness = 1;       // Border thickness
                                                      // 
                    if (aircraft.IsUnknown)
                    {
                        strokeColor = Brushes.YellowGreen;    // Aircraft border color (red)
                        fillColor = Brushes.Black;   // Aircraft fill color (sky blue)
                        strokeThickness = 2;
                    }

                    var marker = new GMapMarker(new PointLatLng((double)aircraft.Latitude, (double)aircraft.Longitude))
                    {
                        Shape = new Polygon
                        {
                            Points = new PointCollection
                                {
                                   new Point(20, 10),  // Top point
                                   new Point(10, 25),  // Bottom left point
                                   new Point(31, 25),  // Bottom right point
                                },
                            Stroke = strokeColor,    // Aircraft border color (red)
                            Fill = fillColor,   // Aircraft fill color (sky blue)
                            StrokeThickness = strokeThickness       // Border thickness
                        }
                    };

                    MapControl.Markers.Add(marker);
                }
            }
        }

        private void UpdateAircraftMarker(Position flyObject)
        {
            var marker = new GMapMarker(new PointLatLng((double)flyObject.Latitude, (double)flyObject.Longitude))
            {
                Shape = new Polygon
                {
                    Points = new PointCollection
                                {
                                   new Point(20, 10),  // Top point
                                   new Point(10, 25),  // Bottom left point
                                   new Point(31, 25),  // Bottom right point
                                },
                    Stroke = Brushes.GreenYellow,    // Aircraft border color (red)
                    Fill = Brushes.Black,   // Aircraft fill color (sky blue)
                    StrokeThickness = 2       // Border thickness
                }
            };

            MapControl.Markers.Add(marker);
        }

        //

        private void MapProviderComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)MapProviderComboBox.SelectedItem;
            string mapProvider = selectedItem.Content.ToString();

            switch (mapProvider)
            {
                case "ArcGIS World Topo Map":
                    MapControl.MapProvider = GMapProviders.ArcGIS_World_Topo_Map;
                    break;
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
            _postionCount = 0;
        }

        // Method to read Aircraft data
        public static void ReadAircraftData(string connectionString)
        {
            string query = "SELECT ICAO24, Callsign, Latitude, Longitude, Altitude, Velocity, Heading FROM [dbo].[Aircraft]";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string icao24 = reader["ICAO24"].ToString();
                            string callsign = reader["Callsign"].ToString();
                            double latitude = reader.IsDBNull(reader.GetOrdinal("Latitude")) ? 0 : Convert.ToDouble(reader["Latitude"]);
                            double longitude = reader.IsDBNull(reader.GetOrdinal("Longitude")) ? 0 : Convert.ToDouble(reader["Longitude"]);
                            double altitude = reader.IsDBNull(reader.GetOrdinal("Altitude")) ? 0 : Convert.ToDouble(reader["Altitude"]);
                            double velocity = reader.IsDBNull(reader.GetOrdinal("Velocity")) ? 0 : Convert.ToDouble(reader["Velocity"]);
                            double heading = reader.IsDBNull(reader.GetOrdinal("Heading")) ? 0 : Convert.ToDouble(reader["Heading"]);

                            Console.WriteLine($"ICAO24: {icao24}, Callsign: {callsign}, Latitude: {latitude}, Longitude: {longitude}, Altitude: {altitude}, Velocity: {velocity}, Heading: {heading}");
                        }
                    }
                }
            }
        }

        // Method to read Positions data for a specific Aircraft (given ICAO24) and return as a list
        public static List<Position> ReadPositionsData(string connectionString, string aircraftICAO24)
        {
            List<Position> positions = new List<Position>();
            string query = "SELECT PositionID, ICAO24, Latitude, Longitude, Altitude, Velocity, Heading, Timestamp FROM [dbo].[Positions]";// WHERE ICAO24 = @ICAO24";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                   //command.Parameters.AddWithValue("@ICAO24", aircraftICAO24);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Position position = new Position
                            {
                                PositionID = reader.GetInt32(reader.GetOrdinal("PositionID")),
                                ICAO24 = reader["ICAO24"].ToString(),
                                Latitude = reader.IsDBNull(reader.GetOrdinal("Latitude")) ? 0 : Convert.ToDouble(reader["Latitude"]),
                                Longitude = reader.IsDBNull(reader.GetOrdinal("Longitude")) ? 0 : Convert.ToDouble(reader["Longitude"]),
                                Altitude = reader.IsDBNull(reader.GetOrdinal("Altitude")) ? (double?)null : Convert.ToDouble(reader["Altitude"]),
                                Velocity = reader.IsDBNull(reader.GetOrdinal("Velocity")) ? (double?)null : Convert.ToDouble(reader["Velocity"]),
                                Heading = reader.IsDBNull(reader.GetOrdinal("Heading")) ? (double?)null : Convert.ToDouble(reader["Heading"]),
                                Timestamp = reader.IsDBNull(reader.GetOrdinal("Timestamp")) ? DateTime.MinValue : Convert.ToDateTime(reader["Timestamp"])
                            };

                            positions.Add(position);
                        }
                    }
                }
            }

            return positions;
        }

        // Method to fetch FlyObjects data
        public static List<FlyObject> GetFlyObjects(string connectionString)
        {
            var flyObjects = new List<FlyObject>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT FlyObjectName, Position.STY AS Latitude, Position.STX AS Longitude, ICAO24 FROM FlyObjects";

                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        flyObjects.Add(new FlyObject
                        {
                            Name = reader["FlyObjectName"].ToString(),
                            Latitude = Convert.ToDouble(reader["Latitude"]),
                            Longitude = Convert.ToDouble(reader["Longitude"]),
                            ICAO24 = reader["ICAO24"].ToString()
                        });
                    }
                }
            }

            return flyObjects;
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
        public bool IsUnknown { get; set; }

        public Aircraft(string iCAO24, string callsign, double? latitude, double? longitude, double? altitude, double? velocity, double? heading, bool isUnknown=false)
        {
            ICAO24 = iCAO24;
            Callsign = callsign;
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
            Velocity = velocity;
            Heading = heading;
            IsUnknown = isUnknown;
        }

        public Aircraft()
        {

        }
    }

    // FlyObject model
    public class FlyObject
    {
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ICAO24 { get; set; }
    }

    public class Position
    {
        public int PositionID { get; set; }
        public string ICAO24 { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Altitude { get; set; }
        public double? Velocity { get; set; }
        public double? Heading { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

