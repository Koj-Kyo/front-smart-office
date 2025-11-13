using Microsoft.Extensions.Logging;
using MySmartApp.Ui.Models;
using System.Net.Http.Json;

namespace MySmartApp.Ui.Services;

/// <summary>
/// Service pour récupérer les statistiques du Dashboard depuis le backend
/// </summary>
public interface IDashboardStatsService
{
    Task<DashboardStats?> GetDashboardStatsAsync();
    Task<List<EnergyDataPoint>> GetEnergyConsumptionDataAsync(string period = "month");
    Task<List<DeviceActivityData>> GetDeviceActivityDataAsync();
}

public class DashboardStatsService : IDashboardStatsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DashboardStatsService> _logger;
    private readonly ITokenProvider _tokenProvider;
    private readonly IAppStateService _appState;

    public DashboardStatsService(
  HttpClient httpClient,
        ILogger<DashboardStatsService> logger,
        ITokenProvider tokenProvider,
 IAppStateService appState)
    {
        _httpClient = httpClient;
      _logger = logger;
        _tokenProvider = tokenProvider;
        _appState = appState;
    }

    public async Task<DashboardStats?> GetDashboardStatsAsync()
    {
 try
        {
       _logger.LogInformation("?? Récupération des statistiques du Dashboard depuis le backend");
       
            await EnsureAuthenticatedAsync();
      
    // ? Appeler le nouvel endpoint backend
            var response = await _httpClient.GetFromJsonAsync<DashboardApiResponse<DashboardStats>>("/api/Dashboard/stats");
        
            if (response?.Data != null)
      {
                _logger.LogDebug("? Statistiques récupérées: {ActiveDevices} devices actifs, {ActiveUsers} users actifs", 
              response.Data.ActiveDevices, response.Data.ActiveUsers);
    return response.Data;
       }
     
            _logger.LogWarning("?? Aucune donnée de statistiques retournée, utilisation des valeurs par défaut");
            return GetDefaultStats();
        }
        catch (HttpRequestException ex)
        {
        _logger.LogError(ex, "? Erreur réseau lors de la récupération des statistiques");
            await _appState.ShowNotificationAsync("Erreur de connexion lors du chargement des statistiques", NotificationType.Error);
          return GetDefaultStats();
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "? Erreur lors de la récupération des statistiques du Dashboard");
     return GetDefaultStats();
        }
    }

    public async Task<List<EnergyDataPoint>> GetEnergyConsumptionDataAsync(string period = "month")
    {
        try
        {
  _logger.LogInformation("? Récupération des données énergétiques pour la période: {Period}", period);
         
  await EnsureAuthenticatedAsync();
         
            // ? Appeler le nouvel endpoint backend avec paramètre de période
         var response = await _httpClient.GetFromJsonAsync<DashboardApiResponse<List<EnergyDataPoint>>>(
                $"/api/Dashboard/energy?period={period}");
         
            if (response?.Data != null && response.Data.Any())
         {
         _logger.LogDebug("? {Count} points de données énergétiques récupérés", response.Data.Count);
     return response.Data;
         }
      
         _logger.LogWarning("?? Aucune donnée énergétique retournée, génération de données simulées");
       return GetSimulatedEnergyData(period);
   }
        catch (HttpRequestException ex)
        {
         _logger.LogError(ex, "? Erreur réseau lors de la récupération des données énergétiques");
   await _appState.ShowNotificationAsync("Erreur de connexion lors du chargement des données énergétiques", NotificationType.Error);
            return GetSimulatedEnergyData(period);
        }
        catch (Exception ex)
        {
   _logger.LogError(ex, "? Erreur lors de la récupération des données d'énergie");
  return GetSimulatedEnergyData(period);
        }
}

    public async Task<List<DeviceActivityData>> GetDeviceActivityDataAsync()
    {
        try
        {
    _logger.LogInformation("?? Récupération de l'activité des appareils");
        
      await EnsureAuthenticatedAsync();
     
          // ? Appeler le nouvel endpoint backend
     var response = await _httpClient.GetFromJsonAsync<DashboardApiResponse<List<DeviceActivityData>>>(
     "/api/Dashboard/device-activity");
       
            if (response?.Data != null && response.Data.Any())
  {
                _logger.LogDebug("? {Count} types d'appareils récupérés", response.Data.Count);
    return response.Data;
   }
 
      _logger.LogWarning("?? Aucune donnée d'activité retournée, utilisation de données simulées");
  return GetSimulatedDeviceActivity();
        }
        catch (HttpRequestException ex)
        {
         _logger.LogError(ex, "? Erreur réseau lors de la récupération de l'activité des appareils");
        await _appState.ShowNotificationAsync("Erreur de connexion lors du chargement de l'activité des appareils", NotificationType.Error);
    return GetSimulatedDeviceActivity();
   }
        catch (Exception ex)
        {
      _logger.LogError(ex, "? Erreur lors de la récupération de l'activité des appareils");
            return GetSimulatedDeviceActivity();
}
    }

    // ==========================================
    // MÉTHODES PRIVÉES
    // ==========================================

    private async Task EnsureAuthenticatedAsync()
    {
        var token = await _tokenProvider.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
      new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _logger.LogWarning("?? Aucun token disponible pour l'authentification");
        }
    }

    private DashboardStats GetDefaultStats()
    {
   return new DashboardStats
{
     TotalDevices = 12,
         ActiveDevices = 8,
         DevicesChangePercent = 8.5,
        TotalUsers = 15,
     ActiveUsers = 12,
       UsersChangePercent = 12.3,
   TotalIncidents = 5,
         OpenIncidents = 2,
   IncidentsChangePercent = -15.2,
   EnergyConsumed = 245.5,
      EnergyChangePercent = 5.8
        };
    }

private List<EnergyDataPoint> GetSimulatedEnergyData(string period)
    {
        var data = new List<EnergyDataPoint>();

        if (period == "month")
    {
     var months = new[] { "Jan", "Fév", "Mar", "Avr", "Mai", "Jun", "Jul", "Aoû", "Sep", "Oct", "Nov", "Déc" };
            foreach (var month in months)
            {
      data.Add(new EnergyDataPoint
       {
            Label = month,
                 Value = Random.Shared.Next(150, 300)
    });
    }
     }
        else if (period == "week")
    {
            var days = new[] { "Lun", "Mar", "Mer", "Jeu", "Ven", "Sam", "Dim" };
       foreach (var day in days)
         {
   data.Add(new EnergyDataPoint
      {
 Label = day,
   Value = Random.Shared.Next(30, 50)
});
   }
        }
    else // day
        {
     for (int i = 0; i < 24; i++)
        {
 data.Add(new EnergyDataPoint
      {
       Label = $"{i}h",
             Value = Random.Shared.Next(5, 20)
  });
         }
        }

        return data;
    }

    private List<DeviceActivityData> GetSimulatedDeviceActivity()
    {
        return new List<DeviceActivityData>
        {
     new() { DeviceType = "Climatisation", Count = 3 },
  new() { DeviceType = "Éclairage", Count = 5 },
      new() { DeviceType = "Capteurs", Count = 4 },
            new() { DeviceType = "Autres", Count = 2 }
        };
    }
}
