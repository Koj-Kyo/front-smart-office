using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace MySmartApp.Ui.Services;

/// <summary>
/// Service pour récupérer les données météo depuis OpenWeatherMap API
/// </summary>
public interface IWeatherService
{
    Task<WeatherData?> GetCurrentWeatherAsync(string city = "Lomé");
    Task<WeatherData?> GetWeatherByCoordinatesAsync(double latitude, double longitude);
}

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherService> _logger;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.openweathermap.org/data/2.5";

    public WeatherService(HttpClient httpClient, ILogger<WeatherService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        // Récupérer la clé API depuis la configuration
        _apiKey = configuration["OpenWeather:ApiKey"] ?? "d69ffb1ac5f5fd458ab976f3fe2bd17a";
    }

    public async Task<WeatherData?> GetCurrentWeatherAsync(string city = "Lomé")
    {
        try
        {
            var url = $"{BaseUrl}/weather?q={city}&appid={_apiKey}&units=metric&lang=fr";
            var response = await _httpClient.GetFromJsonAsync<OpenWeatherResponse>(url);
            
            if (response == null) return null;

            return new WeatherData
            {
                Temperature = Math.Round(response.Main.Temp, 1),
                FeelsLike = Math.Round(response.Main.FeelsLike, 1),
                Humidity = response.Main.Humidity,
                Description = response.Weather.FirstOrDefault()?.Description ?? "N/A",
                Icon = response.Weather.FirstOrDefault()?.Icon ?? "01d",
                WindSpeed = Math.Round(response.Wind.Speed * 3.6, 1), // Conversion m/s vers km/h
                Pressure = response.Main.Pressure,
                City = response.Name
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la météo pour {City}", city);
            return GetDefaultWeather();
        }
    }

    public async Task<WeatherData?> GetWeatherByCoordinatesAsync(double latitude, double longitude)
    {
        try
        {
            var url = $"{BaseUrl}/weather?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric&lang=fr";
            var response = await _httpClient.GetFromJsonAsync<OpenWeatherResponse>(url);
            
            if (response == null) return null;

            return new WeatherData
            {
                Temperature = Math.Round(response.Main.Temp, 1),
                FeelsLike = Math.Round(response.Main.FeelsLike, 1),
                Humidity = response.Main.Humidity,
                Description = response.Weather.FirstOrDefault()?.Description ?? "N/A",
                Icon = response.Weather.FirstOrDefault()?.Icon ?? "01d",
                WindSpeed = Math.Round(response.Wind.Speed * 3.6, 1),
                Pressure = response.Main.Pressure,
                City = response.Name
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la météo aux coordonnées {Lat},{Lon}", latitude, longitude);
            return GetDefaultWeather();
        }
    }

    private WeatherData GetDefaultWeather()
    {
        return new WeatherData
        {
            Temperature = 20.0,
            FeelsLike = 20.0,
            Humidity = 60,
            Description = "Données météo non disponibles",
            Icon = "01d",
            WindSpeed = 10.0,
            Pressure = 1013,
            City = "Unknown"
        };
    }
}

// Modèles pour la réponse OpenWeather API
public class OpenWeatherResponse
{
    [JsonPropertyName("main")]
    public MainData Main { get; set; } = new();

    [JsonPropertyName("weather")]
    public List<WeatherDescription> Weather { get; set; } = new();

    [JsonPropertyName("wind")]
    public WindData Wind { get; set; } = new();

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class MainData
{
    [JsonPropertyName("temp")]
    public double Temp { get; set; }

    [JsonPropertyName("feels_like")]
    public double FeelsLike { get; set; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }

    [JsonPropertyName("pressure")]
    public int Pressure { get; set; }
}

public class WeatherDescription
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;
}

public class WindData
{
    [JsonPropertyName("speed")]
    public double Speed { get; set; }
}

// Modèle de données météo simplifié pour l'UI
public class WeatherData
{
    public double Temperature { get; set; }
    public double FeelsLike { get; set; }
    public int Humidity { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public double WindSpeed { get; set; }
    public int Pressure { get; set; }
    public string City { get; set; } = string.Empty;

    public string GetIconUrl() => $"https://openweathermap.org/img/wn/{Icon}@2x.png";
    
    public string GetWeatherIcon()
    {
        // Mapping des icônes OpenWeather vers les icônes MudBlazor
        return Icon switch
        {
            "01d" or "01n" => Icons.Material.Filled.WbSunny,
            "02d" or "02n" => Icons.Material.Filled.Cloud,
            "03d" or "03n" or "04d" or "04n" => Icons.Material.Filled.CloudQueue,
            "09d" or "09n" or "10d" or "10n" => Icons.Material.Filled.Umbrella,
            "11d" or "11n" => Icons.Material.Filled.FlashOn,
            "13d" or "13n" => Icons.Material.Filled.AcUnit,
            "50d" or "50n" => Icons.Material.Filled.Waves,
            _ => Icons.Material.Filled.Cloud
        };
    }
}
