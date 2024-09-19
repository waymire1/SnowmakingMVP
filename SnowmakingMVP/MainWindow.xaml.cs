using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using GMap.NET.MapProviders;
using GMap.NET;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using GMap.NET.WindowsPresentation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SnowmakingMVP
{
    // The MainWindow class represents the main window of the application.
    public partial class MainWindow : Window
    {
        // OpenWeatherMap API key (replace with a secure method in production).
        private const string ApiKey = "YOUR_API_KEY"; // Remember to replace with your actual API key.
        // Default city name for fetching weather data.
        private const string cityName = "Big Bear Lake,CA,US";

        // List to store snowgun data.
        private List<Snowgun> snowguns = new List<Snowgun>();

        // Constructor for the MainWindow class.
        public MainWindow()
        {
            InitializeComponent();      // Initializes UI components.
            FetchWeatherData();         // Fetches initial weather data.
            InitializeMap();            // Sets up the map control.
            LoadSkiAreaData();          // Loads data for ski areas.
            PopulateSkiAreaComboBox();  // Populates the combo box with ski areas.
        }

        // Class representing a ski area.
        public class SkiArea
        {
            public string Name { get; set; }          // Name of the ski area.
            public PointLatLng Location { get; set; } // Geographic location (latitude and longitude).
            public int ZoomLevel { get; set; }        // Zoom level for the map when this ski area is selected.
        }

        // List to store ski areas.
        private List<SkiArea> skiAreas = new List<SkiArea>();

        // Loads data for available ski areas.
        private void LoadSkiAreaData()
        {
            // Adding Big Bear Lake ski area.
            skiAreas.Add(new SkiArea
            {
                Name = "Big Bear Lake, CA",
                Location = new PointLatLng(34.2439, -116.9114), // Coordinates for Big Bear Lake.
                ZoomLevel = 13
            });

            // Adding Aspen Snowmass ski area.
            skiAreas.Add(new SkiArea
            {
                Name = "Aspen Snowmass, CO",
                Location = new PointLatLng(39.2097, -106.9498), // Coordinates for Aspen Snowmass.
                ZoomLevel = 13
            });

            // Additional ski areas can be added here.
        }

        // Populates the ski area combo box in the UI.
        private void PopulateSkiAreaComboBox()
        {
            SkiAreaComboBox.ItemsSource = skiAreas;    // Sets the items to display in the combo box.
            SkiAreaComboBox.DisplayMemberPath = "Name"; // Specifies that the 'Name' property should be displayed.
            SkiAreaComboBox.SelectedIndex = 0;          // Selects the first ski area by default.
        }

        // Event handler for when the selected ski area changes.
        private void SkiAreaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SkiAreaComboBox.SelectedItem is SkiArea selectedSkiArea)
            {
                // Updates the map position and zoom level based on the selected ski area.
                MapControl.Position = selectedSkiArea.Location;
                MapControl.Zoom = selectedSkiArea.ZoomLevel;

                // Updates the window title to include the selected ski area's name.
                this.Title = $"Snowmaking MVP Dashboard - {selectedSkiArea.Name}";

                // Clears existing markers and routes from the map.
                MapControl.Markers.Clear();

                // Loads and displays data specific to the selected ski area.
                LoadSnowgunData(selectedSkiArea);
                AddSnowgunMarkers();
                LoadRunData(selectedSkiArea);
                AddRunPolylines();
            }
        }

        // Class representing a ski run (trail).
        public class Run
        {
            public string Name { get; set; }             // Name of the run.
            public List<PointLatLng> Points { get; set; } // List of geographic points defining the run.
        }

        // List to store ski runs.
        private List<Run> runs = new List<Run>();

        // Loads run data for the selected ski area.
        private void LoadRunData(SkiArea skiArea)
        {
            runs.Clear(); // Clears any existing run data.

            if (skiArea.Name == "Big Bear Lake, CA")
            {
                // Adding a run for Big Bear Lake.
                runs.Add(new Run
                {
                    Name = "Beginner's Trail",
                    Points = new List<PointLatLng>
                    {
                        new PointLatLng(34.2440, -116.9125),
                        new PointLatLng(34.2445, -116.9120),
                        new PointLatLng(34.2450, -116.9115)
                    }
                });
                // Additional runs can be added here.
            }
            else if (skiArea.Name == "Aspen Snowmass, CO")
            {
                // Adding a run for Aspen Snowmass.
                runs.Add(new Run
                {
                    Name = "Expert Run",
                    Points = new List<PointLatLng>
                    {
                        new PointLatLng(39.2090, -106.9500),
                        new PointLatLng(39.2095, -106.9495),
                        new PointLatLng(39.2100, -106.9490)
                    }
                });
                // Additional runs can be added here.
            }
            // Additional ski areas and their runs can be added similarly.
        }

        // Adds the runs to the map as polylines (routes).
        private void AddRunPolylines()
        {
            foreach (var run in runs)
            {
                // Creates a new route based on the run's points.
                GMapRoute route = new GMapRoute(run.Points)
                {
                    Tag = run // Attaches the run data to the route.
                };

                // Adds the route to the map control.
                MapControl.Markers.Add(route);

                // Forces the shape (visual representation) of the route to generate.
                route.RegenerateShape(MapControl);

                // Customizes the appearance of the route.
                if (route.Shape is Path path)
                {
                    path.Stroke = Brushes.Blue;       // Sets the color of the line to blue.
                    path.StrokeThickness = 3;         // Sets the thickness of the line.
                    path.Tag = run;                   // Attaches the run data to the path.

                    // Adds an event handler for when the run is clicked.
                    path.MouseLeftButtonUp += Run_MouseLeftButtonUp;
                }
            }
        }

        // Event handler for when a run is clicked.
        private void Run_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Path path && path.Tag is Run run)
            {
                // Displays a message showing which run was selected.
                MessageBox.Show($"You selected the run: {run.Name}", "Run Selected", MessageBoxButton.OK, MessageBoxImage.Information);

                // Highlights the selected run and resets others.
                foreach (var marker in MapControl.Markers)
                {
                    if (marker is GMapRoute route && route.Tag is Run routeRun)
                    {
                        if (route.Shape is Path routePath)
                        {
                            if (routeRun == run)
                            {
                                routePath.Stroke = Brushes.Red; // Highlights the selected run in red.
                            }
                            else
                            {
                                routePath.Stroke = Brushes.Blue; // Resets other runs to blue.
                            }
                        }
                    }
                }
            }
        }

        // Initializes the map control settings.
        private void InitializeMap()
        {
            // Sets the map provider to OpenStreetMap.
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            MapControl.MapProvider = GMapProviders.OpenStreetMap;

            // Sets the initial position and zoom level of the map.
            MapControl.Position = new PointLatLng(34.2439, -116.9114); // Big Bear Lake coordinates.
            MapControl.MinZoom = 13;
            MapControl.MaxZoom = 13;
            MapControl.Zoom = 13;

            // Disables zooming via mouse wheel.
            MapControl.MouseWheelZoomEnabled = false;

            // Prevents zooming on double-click.
            MapControl.MouseDoubleClick += (sender, args) =>
            {
                args.Handled = true; // Stops the default zoom behavior.
            };

            // Allows the user to drag the map.
            MapControl.CanDragMap = true;

            // Ensures markers remain clickable even when scrolling.
            MapControl.IgnoreMarkerOnMouseWheel = true;
        }

        // Fetches weather data from the OpenWeatherMap API.
        private async void FetchWeatherData()
        {
            try
            {
                // Builds the API URL with the city name and API key.
                string apiUrl = $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(cityName)}&units=imperial&appid={ApiKey}";

                using (HttpClient client = new HttpClient())
                {
                    // Sends a GET request to the API.
                    string response = await client.GetStringAsync(apiUrl);

                    // Parses the JSON response.
                    JObject data = JObject.Parse(response);

                    // Extracts weather data from the JSON.
                    double temperature = data["main"]["temp"].ToObject<double>();
                    double humidity = data["main"]["humidity"].ToObject<double>();
                    double windSpeed = data["wind"]["speed"].ToObject<double>();
                    double windDirection = data["wind"]["deg"].ToObject<double>();

                    // Calculates the wet-bulb temperature.
                    double wetBulbTemp = CalculateWetBulbTemperature(temperature, humidity);

                    // Updates the UI with the fetched data.
                    TemperatureTextBlock.Text = $"{temperature} °F";
                    HumidityTextBlock.Text = $"{humidity} %";
                    WindSpeedTextBlock.Text = $"{windSpeed} mph";
                    WindDirectionTextBlock.Text = $"{windDirection}°";
                    WetBulbTempTextBlock.Text = $"{wetBulbTemp:F2} °F";
                }
            }
            catch (Exception ex)
            {
                // Displays an error message if fetching data fails.
                MessageBox.Show($"Error fetching weather data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Calculates the wet-bulb temperature using temperature and humidity.
        private double CalculateWetBulbTemperature(double temperature, double humidity)
        {
            // Converts temperature from Fahrenheit to Celsius.
            double tempC = (temperature - 32) * (5.0 / 9.0);

            // Calculates the vapor pressure.
            double e = (humidity / 100) * 6.105 * Math.Exp((17.27 * tempC) / (237.7 + tempC));

            // Simplified formula for wet-bulb temperature in Celsius.
            double wetBulbC = tempC * Math.Atan(0.151977 * Math.Sqrt(humidity + 8.313659)) +
                              Math.Atan(tempC + humidity) -
                              Math.Atan(humidity - 1.676331) +
                              0.00391838 * Math.Pow(humidity, 1.5) * Math.Atan(0.023101 * humidity) -
                              4.686035;

            // Converts wet-bulb temperature back to Fahrenheit.
            double wetBulbF = (wetBulbC * 9.0 / 5.0) + 32;

            return wetBulbF; // Returns the calculated wet-bulb temperature.
        }

        // Loads snowgun data for the selected ski area.
        private void LoadSnowgunData(SkiArea skiArea)
        {
            snowguns.Clear(); // Clears any existing snowgun data.

            if (skiArea.Name == "Big Bear Lake, CA")
            {
                // Adding a snowgun for Big Bear Lake.
                snowguns.Add(new Snowgun
                {
                    Id = "SG-001",
                    Location = new PointLatLng(34.2439, -116.9114),
                    Status = "Active" // Status can be "Active", "Inactive", etc.
                });
                // Additional snowguns can be added here.
            }
            else if (skiArea.Name == "Aspen Snowmass, CO")
            {
                // Adding a snowgun for Aspen Snowmass.
                snowguns.Add(new Snowgun
                {
                    Id = "SG-101",
                    Location = new PointLatLng(39.2097, -106.9498),
                    Status = "Active"
                });
                // Additional snowguns can be added here.
            }
            // Additional ski areas and their snowguns can be added similarly.
        }

        // Adds snowgun markers to the map.
        private void AddSnowgunMarkers()
        {
            foreach (var snowgun in snowguns)
            {
                // Creates an ellipse shape to represent the snowgun.
                Ellipse ellipse = new Ellipse
                {
                    Width = 20,
                    Height = 20,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.5,
                    Fill = snowgun.Status == "Active" ? Brushes.Green : Brushes.Red, // Green if active, red if inactive.
                    ToolTip = $"Snowgun {snowgun.Id}\nStatus: {snowgun.Status}", // Tooltip to display snowgun info.
                    Tag = snowgun // Attaches the snowgun data to the ellipse.
                };

                // Adds an event handler for when the snowgun marker is clicked.
                ellipse.MouseLeftButtonUp += Marker_MouseLeftButtonUp;

                // Creates a GMapMarker with the ellipse as its visual representation.
                GMapMarker marker = new GMapMarker(snowgun.Location)
                {
                    Shape = ellipse,
                    Offset = new Point(-10, -10) // Centers the ellipse over the marker's location.
                };

                // Adds the marker to the map.
                MapControl.Markers.Add(marker);
            }
        }

        // Event handler for when a snowgun marker is clicked.
        private void Marker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse ellipse && ellipse.Tag is Snowgun snowgun)
            {
                // Updates the info panel with the selected snowgun's data.
                SnowgunIdTextBlock.Text = snowgun.Id;
                SnowgunStatusTextBlock.Text = snowgun.Status;

                // Placeholder for future data (e.g., temperature, humidity from the snowgun).
                //SnowgunTempTextBlock.Text = "N/A";
                //SnowgunHumidityTextBlock.Text = "N/A";
            }
        }
    }

    // Class representing a snowgun.
    public class Snowgun
    {
        public string Id { get; set; }             // Unique identifier for the snowgun.
        public PointLatLng Location { get; set; }  // Geographic location of the snowgun.
        public string Status { get; set; }         // Operational status (e.g., "Active", "Inactive").
        // Additional properties (e.g., associated weather data) can be added here.
    }
}
