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
            // V�rifier le cache d'abord
            if (_cache.TryGetValue(ZONES_CACHE_KEY, out List<ZoneDto>? cachedZones))
            {
                _logger.LogDebug("?? Zones r�cup�r�es depuis le cache ({Count})", cachedZones?.Count ?? 0);
                return cachedZones ?? new List<ZoneDto>();
            }

            _logger.LogDebug("?? R�cup�ration des zones depuis l'API...");
            var response = await _apiService.GetAsync<List<ZoneDto>>("api/Zones");

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

                // Mettre � jour l'�tat global
                await _appState.UpdateZonesAsync(response.Data);
                
                // Notifier les abonn�s
                ZonesUpdated?.Invoke(response.Data);

                _logger.LogInformation("? {Count} zones r�cup�r�es avec succ�s", response.Data.Count);
                return response.Data;
            }

            var errorMessage = response.ErrorMessage ?? "Erreur inconnue lors de la r�cup�ration des zones";
            await _appState.SetErrorAsync(errorMessage);
            
            return new List<ZoneDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des zones: {Message}", ex.Message);
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
            _logger.LogDebug("?? R�cup�ration de la zone {ZoneId}...", id);
            
            var response = await _apiService.GetAsync<ZoneDto>($"api/Zones/{id}");
            
            if (response.IsSuccess)
            {
                _logger.LogDebug("? Zone {ZoneId} r�cup�r�e: {ZoneName}", id, response.Data?.Name);
                return response.Data;
            }

            if (response.StatusCode == 404)
            {
                _logger.LogWarning("?? Zone {ZoneId} introuvable", id);
                return null;
            }

            await _appState.SetErrorAsync(response.ErrorMessage ?? $"Erreur lors de la r�cup�ration de la zone {id}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration de la zone {ZoneId}: {Message}", id, ex.Message);
            await _appState.SetErrorAsync($"Erreur: {ex.Message}");
            return null;
        }
    }

    public async Task<IEnumerable<DeviceDto>> GetZoneDevicesAsync(int zoneId)
    {
        try
        {
            _logger.LogDebug("?? R�cup�ration des appareils de la zone {ZoneId}...", zoneId);
            
            var response = await _apiService.GetAsync<List<DeviceDto>>($"api/zones/{zoneId}/devices");
            
            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? {Count} appareils r�cup�r�s pour la zone {ZoneId}", 
                    response.Data.Count, zoneId);
                return response.Data;
            }

            var errorMessage = response.ErrorMessage ?? $"Erreur lors de la r�cup�ration des appareils de la zone {zoneId}";
            await _appState.SetErrorAsync(errorMessage);
            
            return new List<DeviceDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des appareils de la zone {ZoneId}: {Message}", zoneId, ex.Message);
            await _appState.SetErrorAsync($"Erreur: {ex.Message}");
            return new List<DeviceDto>();
        }
    }

    public async Task<ApiResponse<ZoneDto>> CreateZoneAsync(ZoneCreationRequest request)
    {
        try
        {
            _logger.LogInformation("? Cr�ation de la zone '{ZoneName}'...", request.Name);
            
            var response = await _apiService.PostAsync<ZoneDto>("api/Zones", request);
            
            if (response.IsSuccess)
            {
                // Invalider le cache
                _cache.Remove(ZONES_CACHE_KEY);
                
                // Recharger les zones
                _ = Task.Run(async () => await GetAllZonesAsync());
                
                await _appState.ShowNotificationAsync($"Zone '{request.Name}' cr��e avec succ�s", NotificationType.Success);
                _logger.LogInformation("? Zone '{ZoneName}' cr��e avec l'ID {ZoneId}", request.Name, response.Data?.Id);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la cr�ation de la zone: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la cr�ation de la zone '{ZoneName}': {Message}", request.Name, ex.Message);
            var errorMessage = $"Erreur lors de la cr�ation: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<ZoneDto>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse<ZoneDto>> UpdateZoneAsync(int id, ZoneUpdateRequest request)
    {
        try
        {
            _logger.LogInformation("?? Mise � jour de la zone {ZoneId}...", id);
            
            var response = await _apiService.PutAsync<ZoneDto>($"api/Zones/{id}", request);
            
            if (response.IsSuccess)
            {
                // Invalider le cache
                _cache.Remove(ZONES_CACHE_KEY);
                
                // Recharger les zones
                _ = Task.Run(async () => await GetAllZonesAsync());
                
                await _appState.ShowNotificationAsync("Zone mise � jour avec succ�s", NotificationType.Success);
                _logger.LogInformation("? Zone {ZoneId} mise � jour", id);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la mise � jour: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la mise � jour de la zone {ZoneId}: {Message}", id, ex.Message);
            var errorMessage = $"Erreur lors de la mise � jour: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<ZoneDto>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> DeleteZoneAsync(int id)
    {
        try
        {
            _logger.LogInformation("??? Suppression de la zone {ZoneId}...", id);
            
            var response = await _apiService.DeleteAsync($"api/Zones/{id}");
            
            if (response.IsSuccess)
            {
                // Invalider le cache
                _cache.Remove(ZONES_CACHE_KEY);
                
                // Recharger les zones
                _ = Task.Run(async () => await GetAllZonesAsync());
                
                await _appState.ShowNotificationAsync("Zone supprim�e avec succ�s", NotificationType.Success);
                _logger.LogInformation("? Zone {ZoneId} supprim�e", id);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la suppression: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la suppression de la zone {ZoneId}: {Message}", id, ex.Message);
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

// DTOs pour les requ�tes
public record ZoneCreationRequest(string Name, string? Description = null);
public record ZoneUpdateRequest(string Name, string? Description = null);