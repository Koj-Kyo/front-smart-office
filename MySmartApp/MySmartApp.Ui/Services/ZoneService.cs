using Microsoft.Extensions.Caching.Memory;
using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

/// <summary>
/// Service pour la gestion des zones avec cache et optimisations
/// </summary>
public interface IZoneService
{
    Task<IEnumerable<ZoneDto>> GetAllZonesAsync();
    Task<ZoneDto?> GetZoneByIdAsync(int id);
    Task<IEnumerable<DeviceDto>> GetZoneDevicesAsync(int zoneId);
    Task<ApiResponse<ZoneDto>> CreateZoneAsync(ZoneCreationRequest request);
    Task<ApiResponse<ZoneDto>> UpdateZoneAsync(int id, ZoneUpdateRequest request);
    Task<ApiResponse> DeleteZoneAsync(int id);
    
    event Action<IEnumerable<ZoneDto>>? ZonesUpdated;
}

public class ZoneService : IZoneService, IDisposable
{
    private readonly IApiService _apiService;
    private readonly IAppStateService _appState;
    private readonly ILogger<ZoneService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = 100 });
    
    private const string ZONES_CACHE_KEY = "all_zones";
    private const int CACHE_DURATION_MINUTES = 5;

    public event Action<IEnumerable<ZoneDto>>? ZonesUpdated;

    public ZoneService(
        IApiService apiService, 
        IAppStateService appState,
        ILogger<ZoneService> logger)
    {
        _apiService = apiService;
        _appState = appState;
        _logger = logger;
    }

    public async Task<IEnumerable<ZoneDto>> GetAllZonesAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            // Vérifier le cache d'abord
            if (_cache.TryGetValue(ZONES_CACHE_KEY, out List<ZoneDto>? cachedZones))
            {
                _logger.LogDebug("?? Zones récupérées depuis le cache ({Count})", cachedZones?.Count ?? 0);
                return cachedZones ?? new List<ZoneDto>();
            }

            _logger.LogDebug("?? Récupération des zones depuis l'API...");
            var response = await _apiService.GetAsync<List<ZoneDto>>("/Zones");

            if (response.IsSuccess && response.Data != null)
            {
                // Mettre en cache
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                    Priority = CacheItemPriority.High,
                    Size = 1
                };
                _cache.Set(ZONES_CACHE_KEY, response.Data, cacheOptions);

                // Mettre à jour l'état global
                await _appState.UpdateZonesAsync(response.Data);
                
                // Notifier les abonnés
                ZonesUpdated?.Invoke(response.Data);

                _logger.LogInformation("? {Count} zones récupérées avec succès", response.Data.Count);
                return response.Data;
            }

            var errorMessage = response.ErrorMessage ?? "Erreur inconnue lors de la récupération des zones";
            await _appState.SetErrorAsync(errorMessage);
            
            return new List<ZoneDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la récupération des zones");
            await _appState.SetErrorAsync($"Erreur: {ex.Message}");
            return new List<ZoneDto>();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<ZoneDto?> GetZoneByIdAsync(int id)
    {
        try
        {
            _logger.LogDebug("?? Récupération de la zone {ZoneId}...", id);
            
            var response = await _apiService.GetAsync<ZoneDto>($"/Zones/{id}");
            
            if (response.IsSuccess)
            {
                _logger.LogDebug("? Zone {ZoneId} récupérée: {ZoneName}", id, response.Data?.Name);
                return response.Data;
            }

            if (response.StatusCode == 404)
            {
                _logger.LogWarning("?? Zone {ZoneId} introuvable", id);
                return null;
            }

            await _appState.SetErrorAsync(response.ErrorMessage ?? $"Erreur lors de la récupération de la zone {id}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la récupération de la zone {ZoneId}", id);
            await _appState.SetErrorAsync($"Erreur: {ex.Message}");
            return null;
        }
    }

    public async Task<IEnumerable<DeviceDto>> GetZoneDevicesAsync(int zoneId)
    {
        try
        {
            _logger.LogDebug("?? Récupération des appareils de la zone {ZoneId}...", zoneId);
            
            var response = await _apiService.GetAsync<List<DeviceDto>>($"/zones/{zoneId}/devices");
            
            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? {Count} appareils récupérés pour la zone {ZoneId}", 
                    response.Data.Count, zoneId);
                return response.Data;
            }

            var errorMessage = response.ErrorMessage ?? $"Erreur lors de la récupération des appareils de la zone {zoneId}";
            await _appState.SetErrorAsync(errorMessage);
            
            return new List<DeviceDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la récupération des appareils de la zone {ZoneId}", zoneId);
            await _appState.SetErrorAsync($"Erreur: {ex.Message}");
            return new List<DeviceDto>();
        }
    }

    public async Task<ApiResponse<ZoneDto>> CreateZoneAsync(ZoneCreationRequest request)
    {
        try
        {
            _logger.LogInformation("? Création de la zone '{ZoneName}'...", request.Name);
            
            var response = await _apiService.PostAsync<ZoneDto>("/Zones", request);
            
            if (response.IsSuccess)
            {
                // Invalider le cache
                _cache.Remove(ZONES_CACHE_KEY);
                
                // Recharger les zones
                _ = Task.Run(async () => await GetAllZonesAsync());
                
                await _appState.ShowNotificationAsync($"Zone '{request.Name}' créée avec succès", NotificationType.Success);
                _logger.LogInformation("? Zone '{ZoneName}' créée avec l'ID {ZoneId}", request.Name, response.Data?.Id);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la création de la zone: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la création de la zone '{ZoneName}'", request.Name);
            var errorMessage = $"Erreur lors de la création: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<ZoneDto>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse<ZoneDto>> UpdateZoneAsync(int id, ZoneUpdateRequest request)
    {
        try
        {
            _logger.LogInformation("?? Mise à jour de la zone {ZoneId}...", id);
            
            var response = await _apiService.PutAsync<ZoneDto>($"/Zones/{id}", request);
            
            if (response.IsSuccess)
            {
                // Invalider le cache
                _cache.Remove(ZONES_CACHE_KEY);
                
                // Recharger les zones
                _ = Task.Run(async () => await GetAllZonesAsync());
                
                await _appState.ShowNotificationAsync("Zone mise à jour avec succès", NotificationType.Success);
                _logger.LogInformation("? Zone {ZoneId} mise à jour", id);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la mise à jour: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la mise à jour de la zone {ZoneId}", id);
            var errorMessage = $"Erreur lors de la mise à jour: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<ZoneDto>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> DeleteZoneAsync(int id)
    {
        try
        {
            _logger.LogInformation("??? Suppression de la zone {ZoneId}...", id);
            
            var response = await _apiService.DeleteAsync($"/Zones/{id}");
            
            if (response.IsSuccess)
            {
                // Invalider le cache
                _cache.Remove(ZONES_CACHE_KEY);
                
                // Recharger les zones
                _ = Task.Run(async () => await GetAllZonesAsync());
                
                await _appState.ShowNotificationAsync("Zone supprimée avec succès", NotificationType.Success);
                _logger.LogInformation("? Zone {ZoneId} supprimée", id);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la suppression: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la suppression de la zone {ZoneId}", id);
            var errorMessage = $"Erreur lors de la suppression: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    public void Dispose()
    {
        _cache?.Dispose();
        _semaphore?.Dispose();
        GC.SuppressFinalize(this);
    }
}

// DTOs pour les requêtes
public record ZoneCreationRequest(string Name, string? Description = null);
public record ZoneUpdateRequest(string Name, string? Description = null);