using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace SnowmakingMVP
{
    public partial class MainWindow : Window
    {
        private const string ApiKey = "3e13618c2afab2ffd3eb99ec87225a02";
        private const string Location = "Big Bear Lake,CA,US";

        public MainWindow()
        {
            InitializeComponent();
            FetchWeatherData();
        }

        private async void FetchWeatherData()
        {
            try
            {
                string apiUrl = $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(Location)}&units=imperial&appid={ApiKey}";
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
    }
}
