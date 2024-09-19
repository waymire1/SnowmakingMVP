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
    public partial class MainWindow : Window
    {
        private const string ApiKey = "YOUR_API_KEY"; // Replace with secure method
        private const string cityName = "Big Bear Lake,CA,US";

        // Sample data for snowguns
        private List<Snowgun> snowguns = new List<Snowgun>();

        public MainWindow()
        {
            InitializeComponent();
            FetchWeatherData();
            InitializeMap();
            LoadSnowgunData();
            AddSnowgunMarkers();
        }

        private void InitializeMap()
        {
            // Set the map provider
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            MapControl.MapProvider = GMapProviders.OpenStreetMap;

            // Set the position to the ski area (replace with actual coordinates)
            MapControl.Position = new PointLatLng(34.2439, -116.9114); // Big Bear Lake coordinates

            // Set zoom levels
            MapControl.MinZoom = 2;
            MapControl.MaxZoom = 18;
            MapControl.Zoom = 13;

            // Enable mouse wheel zooming
            MapControl.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
            MapControl.CanDragMap = true;
            MapControl.DragButton = MouseButton.Left;
        }

        private async void FetchWeatherData()
        {
            try
            {
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

                    // Update UI
                    TemperatureTextBlock.Text = $"{temperature} °F";
                    HumidityTextBlock.Text = $"{humidity} %";
                    WindSpeedTextBlock.Text = $"{windSpeed} mph";
                    WindDirectionTextBlock.Text = $"{windDirection}°";
                    WetBulbTempTextBlock.Text = $"{wetBulbTemp:F2} °F";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching weather data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private double CalculateWetBulbTemperature(double temperature, double humidity)
        {
            // Simplified wet-bulb temperature calculation
            double tempC = (temperature - 32) * (5.0 / 9.0);
            double e = (humidity / 100) * 6.105 * Math.Exp((17.27 * tempC) / (237.7 + tempC));
            double wetBulbC = tempC * Math.Atan(0.151977 * Math.Sqrt(humidity + 8.313659)) +
                              Math.Atan(tempC + humidity) -
                              Math.Atan(humidity - 1.676331) +
                              0.00391838 * Math.Pow(humidity, 1.5) * Math.Atan(0.023101 * humidity) -
                              4.686035;
            double wetBulbF = (wetBulbC * 9.0 / 5.0) + 32;
            return wetBulbF;
        }

        private void LoadSnowgunData()
        {
            // Sample data
            snowguns.Add(new Snowgun
            {
                Id = "SG-001",
                Location = new PointLatLng(34.2439, -116.9114),
                Status = "Active"
            });
            snowguns.Add(new Snowgun
            {
                Id = "SG-002",
                Location = new PointLatLng(34.2445, -116.9120),
                Status = "Inactive"
            });
            // Add more snowguns as needed
        }

        private void AddSnowgunMarkers()
        {
            foreach (var snowgun in snowguns)
            {
                // Create an Ellipse shape
                Ellipse ellipse = new Ellipse
                {
                    Width = 20,
                    Height = 20,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.5,
                    Fill = snowgun.Status == "Active" ? Brushes.Green : Brushes.Red,
                    ToolTip = $"Snowgun {snowgun.Id}\nStatus: {snowgun.Status}",
                    Tag = snowgun // Attach the snowgun data directly to the ellipse
                };

                // Add event handler for clicks
                ellipse.MouseLeftButtonUp += Marker_MouseLeftButtonUp;

                // Create the GMapMarker
                GMapMarker marker = new GMapMarker(snowgun.Location)
                {
                    Shape = ellipse,
                    Offset = new Point(-10, -10) // Center the marker
                };

                // Add the marker to the map
                MapControl.Markers.Add(marker);
            }
        }



        private void Marker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse ellipse && ellipse.Tag is Snowgun snowgun)
            {
                // Update the info panel with snowgun data
                SnowgunIdTextBlock.Text = snowgun.Id;
                SnowgunStatusTextBlock.Text = snowgun.Status;

                // Placeholder for future weather station data
                SnowgunTempTextBlock.Text = "N/A";
                SnowgunHumidityTextBlock.Text = "N/A";
            }
        }

    }

    // Define the Snowgun class
    public class Snowgun
    {
        public string Id { get; set; }
        public PointLatLng Location { get; set; }
        public string Status { get; set; }
        // Future weather station data can be added here
    }
}
