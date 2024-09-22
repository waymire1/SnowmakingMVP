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
                Name = "Bear Mountain, CA",
                Location = new PointLatLng(34.2439, -116.9114), // Coordinates for Big Bear Lake.
                ZoomLevel = 14
            });

            // Adding Snow Valley ski area.
            skiAreas.Add(new SkiArea
            {
                Name = "Snow Valley, CA",
                Location = new PointLatLng(34.21979912472986, -117.03464925387631), // Coordinates for Snow Valley.
                ZoomLevel = 14
            });

            // Adding Snow Summit ski area.
            skiAreas.Add(new SkiArea
            {
                Name = "Snow Summit, CA",
                Location = new PointLatLng(34.230654, -116.891155), // Coordinates for Big Bear Snow Summit.
                ZoomLevel = 14
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

                // Update the snowgun ComboBox
                SnowgunComboBox.ItemsSource = snowguns;
                SnowgunComboBox.DisplayMemberPath = "Id";
                if (snowguns.Count > 0)
                {
                    SnowgunComboBox.SelectedIndex = 0;
                }

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
        // Loads run data for the selected ski area.
        private void LoadRunData(SkiArea skiArea)
        {
            runs.Clear(); // Clears any existing run data.

            if (skiArea.Name == "Bear Mountain, CA")
            {
                // Adding runs for Bear Mountain.
                runs.Add(new Run
                {
                    Name = "Chair 9 Run",
                    Points = new List<PointLatLng>
            {
                new PointLatLng(34.2420, -116.9130),
                new PointLatLng(34.2430, -116.9120),
                new PointLatLng(34.2440, -116.9110)
            }
                });

                // Additional runs can be added here.
            }
            else if (skiArea.Name == "Snow Valley, CA")
            {
                // Adding runs for Snow Valley.
                runs.Add(new Run
                {
                    Name = "Expert Run",
                    Points = new List<PointLatLng>
            {
                new PointLatLng(34.2200, -117.0350),
                new PointLatLng(34.2210, -117.0340),
                new PointLatLng(34.2220, -117.0330)
            }
                });

                // Additional runs can be added here.
            }
            // Include other ski areas as needed.
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

                // Customizes the appearance of the route.
                route.Shape = new Path
                {
                    Stroke = Brushes.Blue,
                    StrokeThickness = 3,
                    //Data = route.RegenerateRouteGeometry(MapControl),
                    Tag = run
                };

                // Adds an event handler for when the run is clicked.
                route.Shape.MouseLeftButtonUp += Run_MouseLeftButtonUp;
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
            MapControl.MaxZoom = 18;
            MapControl.Zoom = 15;

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
            List<WeatherData> weatherDataList = new List<WeatherData>();

            try
            {
                // OpenWeatherMap data
                string apiUrl = $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(cityName)}&units=imperial&appid={ApiKey}";

                using (HttpClient client = new HttpClient())
                {
                    string response = await client.GetStringAsync(apiUrl);
                    JObject data = JObject.Parse(response);

                    double temperature = data["main"]["temp"].ToObject<double>();
                    double humidity = data["main"]["humidity"].ToObject<double>();
                    double windSpeed = data["wind"]["speed"].ToObject<double>();
                    double windDirection = data["wind"]["deg"].ToObject<double>();

                    double wetBulbTemp = CalculateWetBulbTemperature(temperature, humidity);

                    WeatherData openWeatherMapData = new WeatherData
                    {
                        SourceName = "OpenWeatherMap",
                        Temperature = temperature,
                        Humidity = humidity,
                        WindSpeed = windSpeed,
                        WindDirection = windDirection,
                        WetBulbTemperature = wetBulbTemp
                    };

                    weatherDataList.Add(openWeatherMapData);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching weather data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Build: Additional sources can be added here

            // Update the ListView
            WeatherDataListView.ItemsSource = weatherDataList;
        }

        // Calculates the wet-bulb temperature using temperature and humidity.
        private double CalculateWetBulbTemperature(double temperature, double humidity)
        {
            // Converts temperature from Fahrenheit to Celsius.
            double tempC = (temperature - 32) * (5.0 / 9.0);

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
        // Loads snowgun data for the selected ski area.
        private void LoadSnowgunData(SkiArea skiArea)
        {
            snowguns.Clear(); // Clears any existing snowgun data.

            if (skiArea.Name == "Bear Mountain, CA")
            {
                // Adding snowguns for Bear Mountain.
                snowguns.Add(new Snowgun
                {
                    Id = "RAT-001",
                    Location = new PointLatLng(34.2439, -116.9114),
                    Status = "Active",
                    Type = "Compression Gun",
                    Nozzle = "Standard",
                    Run = "Summit Run",
                    HydrantNumber = 12,
                    // Simulated weather data
                    Temperature = 28.5,
                    Humidity = 65.0,
                    WindSpeed = 5.0,
                    WindDirection = 90.0,
                    WetBulbTemperature = CalculateWetBulbTemperature(28.5, 65.0)
                });

                // Additional snowguns can be added here.
            }
            else if (skiArea.Name == "Snow Valley, CA")
            {
                // Adding snowguns for Snow Valley.
                snowguns.Add(new Snowgun
                {
                    Id = "SG-101",
                    Location = new PointLatLng(34.2200, -117.0350),
                    Status = "Active",
                    Type = "Fan Gun",
                    Nozzle = "High-Efficiency",
                    Run = "Expert Run",
                    HydrantNumber = 5,
                    // Simulated weather data
                    Temperature = 26.0,
                    Humidity = 70.0,
                    WindSpeed = 7.0,
                    WindDirection = 80.0,
                    WetBulbTemperature = CalculateWetBulbTemperature(26.0, 70.0)
                });

                // Additional snowguns can be added here.
            }
            // Include other ski areas as needed.
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
                // Set the selected item in the ComboBox
                SnowgunComboBox.SelectedItem = snowgun;
            }
        }

        // Event handler for when the selected snowgun changes.
        private void SnowgunComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SnowgunComboBox.SelectedItem is Snowgun selectedSnowgun)
            {
                // Update the info panel with the selected snowgun's data.
                SnowgunIdTextBox.Text = selectedSnowgun.Id;
                SnowgunStatusTextBox.Text = selectedSnowgun.Status;
                SnowgunTypeTextBox.Text = selectedSnowgun.Type;
                SnowgunNozzleTextBox.Text = selectedSnowgun.Nozzle;
                SnowgunRunTextBox.Text = selectedSnowgun.Run;
                SnowgunHydrantNumberTextBox.Text = selectedSnowgun.HydrantNumber.ToString();
                SnowgunTemperatureTextBlock.Text = $"{selectedSnowgun.Temperature} °F";
                SnowgunHumidityTextBlock.Text = $"{selectedSnowgun.Humidity} %";
                SnowgunWindSpeedTextBlock.Text = $"{selectedSnowgun.WindSpeed} mph";
                SnowgunWindDirectionTextBlock.Text = $"{selectedSnowgun.WindDirection}°";
                SnowgunWetBulbTempTextBlock.Text = $"{selectedSnowgun.WetBulbTemperature:F2} °F";

                // Highlight the selected snowgun on the map
                HighlightSelectedSnowgunOnMap(selectedSnowgun);
            }
        }

        // Highlights the selected snowgun on the map.
        private void HighlightSelectedSnowgunOnMap(Snowgun selectedSnowgun)
        {
            foreach (var marker in MapControl.Markers)
            {
                if (marker.Shape is Ellipse ellipse && ellipse.Tag is Snowgun snowgun)
                {
                    if (snowgun == selectedSnowgun)
                    {
                        // Highlight the selected snowgun
                        ellipse.Stroke = Brushes.Yellow;
                        ellipse.StrokeThickness = 3;
                    }
                    else
                    {
                        // Reset other snowguns
                        ellipse.Stroke = Brushes.Black;
                        ellipse.StrokeThickness = 1.5;
                    }
                }
            }
        }

        // Event handler for when the Run text box loses focus.
        private void SnowgunRunTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SnowgunComboBox.SelectedItem is Snowgun selectedSnowgun)
            {
                selectedSnowgun.Run = SnowgunRunTextBox.Text;
            }
        }

        // Event handler for when the Hydrant Number text box loses focus.
        private void SnowgunHydrantNumberTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SnowgunComboBox.SelectedItem is Snowgun selectedSnowgun)
            {
                if (int.TryParse(SnowgunHydrantNumberTextBox.Text, out int hydrantNumber))
                {
                    selectedSnowgun.HydrantNumber = hydrantNumber;
                }
                else
                {
                    MessageBox.Show("Please enter a valid number for Hydrant Number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    SnowgunHydrantNumberTextBox.Text = selectedSnowgun.HydrantNumber.ToString();
                }
            }
        }

        // Event handler for the Historical Data button click.
        private void HistoricalDataButton_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for opening the historical data window
            MessageBox.Show("Historical data window will be implemented later.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SnowgunIdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }

    // Class representing a snowgun.
    public class Snowgun
    {
        public string Id { get; set; }             // Unique identifier for the snowgun.
        public PointLatLng Location { get; set; }  // Geographic location of the snowgun.
        public string Status { get; set; }         // Operational status (e.g., "Active", "Inactive").
        public string Type { get; set; }           // Type of gun.
        public string Nozzle { get; set; }         // Gun nozzle.
        public string Run { get; set; }            // Run on which the snowgun is located.
        public int HydrantNumber { get; set; }     // Hydrant number.

        // Weather data from edge weather station
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double WindSpeed { get; set; }
        public double WindDirection { get; set; }
        public double WetBulbTemperature { get; set; }
    }

    // Class representing weather data from different sources.
    public class WeatherData
    {
        public string SourceName { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double WindSpeed { get; set; }
        public double WindDirection { get; set; }
        public double WetBulbTemperature { get; set; }
    }
}
