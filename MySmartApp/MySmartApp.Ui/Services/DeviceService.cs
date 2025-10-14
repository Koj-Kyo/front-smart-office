using Microsoft.Extensions.Caching.Memory;
using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

/// <summary>
/// Service pour la gestion des appareils IoT avec cache et optimisations
/// </summary>
public interface IDeviceService
{
    Task<IEnumerable<DeviceDto>> GetAllDevicesAsync();
    Task<DeviceDto?> GetDeviceByIdAsync(int id);
    Task<ApiResponse<DeviceDto>> CreateDeviceAsync(DeviceCreationRequest request);
    Task<ApiResponse<DeviceDto>> UpdateDeviceAsync(int id, DeviceUpdateRequest request);
    Task<ApiResponse> DeleteDeviceAsync(int id);
    
    // Commandes IoT
    Task<IoTResponse> SendSmartCommandAsync(string deviceId, string command, object? parameters = null);
    Task<IoTResponse> InvokeMethodAsync(string deviceId, string methodName, object? parameters = null);
    Task<DeviceTwinDto?> GetDeviceTwinAsync(string deviceId);
    Task<ApiResponse> UpdateDeviceTwinAsync(string deviceId, object desiredProperties);
    Task<ApiResponse> SendMessageToDeviceAsync(string deviceId, string message);
    
    event Action<IEnumerable<DeviceDto>>? DevicesUpdated;
    event Action<DeviceStatusUpdate>? DeviceStatusChanged;
}

public class DeviceService : IDeviceService, IDisposable
{
    private readonly IApiService _apiService;
    private readonly IAppStateService _appState;
    private readonly ILogger<DeviceService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = 200 });
    
    private const string DEVICES_CACHE_KEY = "all_devices";
    private const int CACHE_DURATION_MINUTES = 3;

    public event Action<IEnumerable<DeviceDto>>? DevicesUpdated;
    public event Action<DeviceStatusUpdate>? DeviceStatusChanged;

    public DeviceService(
        IApiService apiService, 
        IAppStateService appState,
        ILogger<DeviceService> logger)
    {
        _apiService = apiService;
        _appState = appState;
        _logger = logger;
    }

    public async Task<IEnumerable<DeviceDto>> GetAllDevicesAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            // Vérifier le cache d'abord
            if (_cache.TryGetValue(DEVICES_CACHE_KEY, out List<DeviceDto>? cachedDevices))
            {
                _logger.LogDebug("?? Appareils récupérés depuis le cache ({Count})", cachedDevices?.Count ?? 0);
                return cachedDevices ?? new List<DeviceDto>();
            }

            _logger.LogDebug("?? Récupération des appareils depuis l'API...");
            var response = await _apiService.GetAsync<List<DeviceDto>>("/Devices");

            if (response.IsSuccess && response.Data != null)
            {
                // Mettre en cache
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                    Priority = CacheItemPriority.High,
                    Size = 1
                };
                _cache.Set(DEVICES_CACHE_KEY, response.Data, cacheOptions);

                // Mettre à jour l'état global
                await _appState.UpdateDevicesAsync(response.Data);
                
                // Notifier les abonnés
                DevicesUpdated?.Invoke(response.Data);

                _logger.LogInformation("? {Count} appareils récupérés avec succès", response.Data.Count);
                return response.Data;
            }

            var errorMessage = response.ErrorMessage ?? "Erreur inconnue lors de la récupération des appareils";
            await _appState.SetErrorAsync(errorMessage);
            
            return new List<DeviceDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la récupération des appareils");
            await _appState.SetErrorAsync($"Erreur: {ex.Message}");
            return new List<DeviceDto>();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<DeviceDto?> GetDeviceByIdAsync(int id)
    {
        try
        {
            _logger.LogDebug("?? Récupération de l'appareil {DeviceId}...", id);
            
            var response = await _apiService.GetAsync<DeviceDto>($"/Devices/{id}");
            
            if (response.IsSuccess)
            {
                _logger.LogDebug("? Appareil {DeviceId} récupéré: {DeviceName}", id, response.Data?.Name);
                return response.Data;
            }

            if (response.StatusCode == 404)
            {
                _logger.LogWarning("?? Appareil {DeviceId} introuvable", id);
                return null;
            }

            await _appState.SetErrorAsync(response.ErrorMessage ?? $"Erreur lors de la récupération de l'appareil {id}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la récupération de l'appareil {DeviceId}", id);
            await _appState.SetErrorAsync($"Erreur: {ex.Message}");
            return null;
        }
    }

    public async Task<ApiResponse<DeviceDto>> CreateDeviceAsync(DeviceCreationRequest request)
    {
        try
        {
            _logger.LogInformation("? Création de l'appareil '{DeviceName}'...", request.Name);
            
            var response = await _apiService.PostAsync<DeviceDto>("/Devices", request);
            
            if (response.IsSuccess)
            {
                // Invalider le cache
                _cache.Remove(DEVICES_CACHE_KEY);
                
                // Recharger les appareils
                _ = Task.Run(async () => await GetAllDevicesAsync());
                
                await _appState.ShowNotificationAsync($"Appareil '{request.Name}' créé avec succès", NotificationType.Success);
                _logger.LogInformation("? Appareil '{DeviceName}' créé avec l'ID {DeviceId}", request.Name, response.Data?.Id);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la création de l'appareil: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la création de l'appareil '{DeviceName}'", request.Name);
            var errorMessage = $"Erreur lors de la création: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<DeviceDto>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse<DeviceDto>> UpdateDeviceAsync(int id, DeviceUpdateRequest request)
    {
        try
        {
            _logger.LogInformation("?? Mise à jour de l'appareil {DeviceId}...", id);
            
            var response = await _apiService.PutAsync<DeviceDto>($"/Devices/{id}", request);
            
            if (response.IsSuccess)
            {
                // Invalider le cache
                _cache.Remove(DEVICES_CACHE_KEY);
                
                // Recharger les appareils
                _ = Task.Run(async () => await GetAllDevicesAsync());
                
                await _appState.ShowNotificationAsync("Appareil mis à jour avec succès", NotificationType.Success);
                _logger.LogInformation("? Appareil {DeviceId} mis à jour", id);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la mise à jour: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la mise à jour de l'appareil {DeviceId}", id);
            var errorMessage = $"Erreur lors de la mise à jour: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<DeviceDto>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> DeleteDeviceAsync(int id)
    {
        try
        {
            _logger.LogInformation("??? Suppression de l'appareil {DeviceId}...", id);
            
            var response = await _apiService.DeleteAsync($"/Devices/{id}");
            
            if (response.IsSuccess)
            {
                // Invalider le cache
                _cache.Remove(DEVICES_CACHE_KEY);
                
                // Recharger les appareils
                _ = Task.Run(async () => await GetAllDevicesAsync());
                
                await _appState.ShowNotificationAsync("Appareil supprimé avec succès", NotificationType.Success);
                _logger.LogInformation("? Appareil {DeviceId} supprimé", id);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la suppression: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la suppression de l'appareil {DeviceId}", id);
            var errorMessage = $"Erreur lors de la suppression: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    // === COMMANDES IOT ===

    public async Task<IoTResponse> SendSmartCommandAsync(string deviceId, string command, object? parameters = null)
    {
        try
        {
            _logger.LogInformation("?? Envoi de la commande '{Command}' à l'appareil {DeviceId}...", command, deviceId);
            
            var endpoint = $"/IoTHub/device/{deviceId}/smart-commands/{command}";
            var response = await _apiService.PostAsync<IoTResponse>(endpoint, parameters ?? new { });
            
            if (response.IsSuccess && response.Data != null)
            {
                if (response.Data.Success)
                {
                    await _appState.ShowNotificationAsync($"Commande '{command}' exécutée avec succès", NotificationType.Success);
                    _logger.LogInformation("? Commande '{Command}' exécutée sur {DeviceId}: {Message}", command, deviceId, response.Data.Message);
                }
                else
                {
                    await _appState.ShowNotificationAsync($"Échec de la commande: {response.Data.Message}", NotificationType.Warning);
                    _logger.LogWarning("?? Échec de la commande '{Command}' sur {DeviceId}: {Message}", command, deviceId, response.Data.Message);
                }
                
                return response.Data;
            }

            var errorMessage = response.ErrorMessage ?? "Erreur lors de l'envoi de la commande";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return new IoTResponse(false, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de l'envoi de la commande '{Command}' à {DeviceId}", command, deviceId);
            var errorMessage = $"Erreur: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return new IoTResponse(false, errorMessage);
        }
    }

    public async Task<IoTResponse> InvokeMethodAsync(string deviceId, string methodName, object? parameters = null)
    {
        try
        {
            _logger.LogInformation("?? Invocation de la méthode '{MethodName}' sur l'appareil {DeviceId}...", methodName, deviceId);
            
            var endpoint = $"/IoTHub/device/{deviceId}/method/{methodName}";
            var response = await _apiService.PostAsync<IoTResponse>(endpoint, parameters ?? new { });
            
            if (response.IsSuccess && response.Data != null)
            {
                if (response.Data.Success)
                {
                    await _appState.ShowNotificationAsync($"Méthode '{methodName}' invoquée avec succès", NotificationType.Success);
                    _logger.LogInformation("? Méthode '{MethodName}' invoquée sur {DeviceId}: {Message}", methodName, deviceId, response.Data.Message);
                }
                else
                {
                    await _appState.ShowNotificationAsync($"Échec de la méthode: {response.Data.Message}", NotificationType.Warning);
                    _logger.LogWarning("?? Échec de la méthode '{MethodName}' sur {DeviceId}: {Message}", methodName, deviceId, response.Data.Message);
                }
                
                return response.Data;
            }

            var errorMessage = response.ErrorMessage ?? "Erreur lors de l'invocation de la méthode";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return new IoTResponse(false, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de l'invocation de la méthode '{MethodName}' sur {DeviceId}", methodName, deviceId);
            var errorMessage = $"Erreur: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return new IoTResponse(false, errorMessage);
        }
    }

    public async Task<DeviceTwinDto?> GetDeviceTwinAsync(string deviceId)
    {
        try
        {
            _logger.LogDebug("?? Récupération du twin de l'appareil {DeviceId}...", deviceId);
            
            var response = await _apiService.GetAsync<DeviceTwinDto>($"/IoTHub/device/{deviceId}/twin");
            
            if (response.IsSuccess)
            {
                _logger.LogDebug("? Twin récupéré pour {DeviceId}", deviceId);
                return response.Data;
            }

            if (response.StatusCode != 404)
            {
                await _appState.SetErrorAsync(response.ErrorMessage ?? $"Erreur lors de la récupération du twin de {deviceId}");
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la récupération du twin de {DeviceId}", deviceId);
            await _appState.SetErrorAsync($"Erreur: {ex.Message}");
            return null;
        }
    }

    public async Task<ApiResponse> UpdateDeviceTwinAsync(string deviceId, object desiredProperties)
    {
        try
        {
            _logger.LogInformation("?? Mise à jour du twin de l'appareil {DeviceId}...", deviceId);
            
            var response = await _apiService.PutAsync<object>($"/IoTHub/device/{deviceId}/twin/desired", desiredProperties);
            
            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync("Propriétés du twin mises à jour avec succès", NotificationType.Success);
                _logger.LogInformation("? Twin mis à jour pour {DeviceId}", deviceId);
                return ApiResponse.Success(response.StatusCode);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la mise à jour du twin: {response.ErrorMessage}", NotificationType.Error);
                return ApiResponse.Error(response.ErrorMessage ?? "Erreur lors de la mise à jour", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la mise à jour du twin de {DeviceId}", deviceId);
            var errorMessage = $"Erreur: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> SendMessageToDeviceAsync(string deviceId, string message)
    {
        try
        {
            _logger.LogInformation("?? Envoi de message à l'appareil {DeviceId}...", deviceId);
            
            var response = await _apiService.PostAsync<object>($"/IoTHub/device/{deviceId}/message", new { message });
            
            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync("Message envoyé avec succès", NotificationType.Success);
                _logger.LogInformation("? Message envoyé à {DeviceId}", deviceId);
                return ApiResponse.Success(response.StatusCode);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de l'envoi du message: {response.ErrorMessage}", NotificationType.Error);
                return ApiResponse.Error(response.ErrorMessage ?? "Erreur lors de l'envoi", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de l'envoi de message à {DeviceId}", deviceId);
            var errorMessage = $"Erreur: {ex.Message}";
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
public record DeviceCreationRequest(
    string Name, 
    string? Description = null, 
    string? DeviceType = null, 
    int? ZoneId = null);

public record DeviceUpdateRequest(
    string Name, 
    string? Description = null, 
    string? DeviceType = null, 
    int? ZoneId = null);