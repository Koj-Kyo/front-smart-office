using Microsoft.Extensions.Caching.Memory;
using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

/// <summary>
/// Service pour la gestion des appareils IoT avec cache et optimisations
/// </summary>
public interface IDeviceService
{
    // API Backend Devices/PeripheriqueLogique
    Task<IEnumerable<PeripheriqueLogiqueDto>> GetAllDevicesAsync();
    Task<PeripheriqueLogiqueDto?> GetDeviceByIdAsync(int id);
    Task<IEnumerable<PeripheriqueLogiqueDto>> GetDevicesByZoneAsync(int zoneId);
    Task<ApiResponse<PeripheriqueLogiqueDto>> CreateDeviceAsync(PeripheriqueLogiqueCreationDto request);
    Task<ApiResponse<PeripheriqueLogiqueDto>> UpdateDeviceAsync(int id, PeripheriqueLogiqueUpdateDto request);
    Task<ApiResponse> DeleteDeviceAsync(int id);
    Task<ApiResponse> SetDesiredPropertiesAsync(string iotDeviceId, object desired);
    
    // Anciennes méthodes pour compatibilité UI
    Task<IEnumerable<DeviceDto>> GetAllDevicesLegacyAsync();
    Task<DeviceDto?> GetDeviceByIdLegacyAsync(int id);
    Task<ApiResponse<DeviceDto>> CreateDeviceLegacyAsync(DeviceCreationRequest request);
    Task<ApiResponse<DeviceDto>> UpdateDeviceLegacyAsync(int id, DeviceUpdateRequest request);
    
    // Commandes IoT Hub (anciennes)
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
    private readonly IIoTHubService _iotHubService;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = 200 });
    
    private const string DEVICES_CACHE_KEY = "all_devices";
    private const string PERIPHERIQUES_CACHE_KEY = "peripheriques_cache";
    private const int CACHE_DURATION_MINUTES = 3;

    public event Action<IEnumerable<DeviceDto>>? DevicesUpdated;
    public event Action<DeviceStatusUpdate>? DeviceStatusChanged;

    public DeviceService(
        IApiService apiService, 
        IAppStateService appState,
        ILogger<DeviceService> logger,
        IIoTHubService iotHubService)
    {
        _apiService = apiService;
        _appState = appState;
        _logger = logger;
        _iotHubService = iotHubService;
    }

    // ==========================================
    // API BACKEND PERIPHERIQUE LOGIQUE
    // ==========================================

    public async Task<IEnumerable<PeripheriqueLogiqueDto>> GetAllDevicesAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_cache.TryGetValue(PERIPHERIQUES_CACHE_KEY, out List<PeripheriqueLogiqueDto>? cachedDevices))
            {
                _logger.LogDebug("✅ Périphériques récupérés depuis le cache ({Count})", cachedDevices?.Count ?? 0);
                return cachedDevices ?? new List<PeripheriqueLogiqueDto>();
            }

            _logger.LogDebug("➡️ Récupération des périphériques depuis l'API backend...");
            var response = await _apiService.GetAsync<List<PeripheriqueLogiqueDto>>("/api/Devices");

            if (response.IsSuccess && response.Data != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                    Priority = CacheItemPriority.High,
                    Size = 1
                };
                _cache.Set(PERIPHERIQUES_CACHE_KEY, response.Data, cacheOptions);
                
                _logger.LogInformation("✅ {Count} périphériques récupérés avec succès", response.Data.Count);
                return response.Data;
            }

            var errorMessage = response.ErrorMessage ?? "Erreur lors de la récupération des périphériques";
            await _appState.SetErrorAsync(errorMessage);
            
            return new List<PeripheriqueLogiqueDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la récupération des périphériques: {Message}", ex.Message);
            await _appState.SetErrorAsync($"Erreur: {ex.Message}");
            return new List<PeripheriqueLogiqueDto>();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<PeripheriqueLogiqueDto?> GetDeviceByIdAsync(int id)
    {
        try
        {
            _logger.LogDebug("➡️ Récupération du périphérique {DeviceId}...", id);
            
            var response = await _apiService.GetAsync<PeripheriqueLogiqueDto>($"/api/Devices/{id}");
            
            if (response.IsSuccess)
            {
                _logger.LogDebug("✅ Périphérique {DeviceId} récupéré: {DeviceName}", id, response.Data?.Name);
                return response.Data;
            }

            if (response.StatusCode == 404)
            {
                _logger.LogWarning("ℹ️ Périphérique {DeviceId} introuvable", id);
                return null;
            }

            await _appState.SetErrorAsync(response.ErrorMessage ?? $"Erreur lors de la récupération du périphérique {id}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la récupération du périphérique {DeviceId}: {Message}", id, ex.Message);
            await _appState.SetErrorAsync($"Erreur: {ex.Message}");
            return null;
        }
    }

    public async Task<IEnumerable<PeripheriqueLogiqueDto>> GetDevicesByZoneAsync(int zoneId)
    {
        try
        {
            _logger.LogDebug("➡️ Récupération des périphériques de la zone {ZoneId}...", zoneId);
            
            var response = await _apiService.GetAsync<List<PeripheriqueLogiqueDto>>($"/api/zones/{zoneId}/devices");
            
            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("✅ {Count} périphériques récupérés pour la zone {ZoneId}", response.Data.Count, zoneId);
                return response.Data;
            }

            var errorMessage = response.ErrorMessage ?? $"Erreur lors de la récupération des périphériques de la zone {zoneId}";
            await _appState.SetErrorAsync(errorMessage);
            return new List<PeripheriqueLogiqueDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la récupération des périphériques de la zone {ZoneId}: {Message}", zoneId, ex.Message);
            await _appState.SetErrorAsync($"Erreur: {ex.Message}");
            return new List<PeripheriqueLogiqueDto>();
        }
    }

    public async Task<ApiResponse<PeripheriqueLogiqueDto>> CreateDeviceAsync(PeripheriqueLogiqueCreationDto request)
    {
        try
        {
            _logger.LogInformation("🛠️ Création du périphérique '{DeviceName}'...", request.Name);
            
            var response = await _apiService.PostAsync<PeripheriqueLogiqueDto>("/api/Devices", request);
            
            if (response.IsSuccess)
            {
                _cache.Remove(PERIPHERIQUES_CACHE_KEY);
                    await _appState.ShowNotificationAsync($"Périphérique '{request.Name}' créé avec succès", NotificationType.Success);
                _logger.LogInformation("✅ Périphérique '{DeviceName}' créé avec l'ID {DeviceId}", request.Name, response.Data?.Id);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la création: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la création du périphérique '{DeviceName}': {Message}", request.Name, ex.Message);
            var errorMessage = $"Erreur lors de la création: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<PeripheriqueLogiqueDto>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse<PeripheriqueLogiqueDto>> UpdateDeviceAsync(int id, PeripheriqueLogiqueUpdateDto request)
    {
        try
        {
            _logger.LogInformation("🛠️ Mise à jour du périphérique {DeviceId}...", id);
            
            var response = await _apiService.PutAsync<PeripheriqueLogiqueDto>($"/api/Devices/{id}", request);
            
            if (response.IsSuccess)
            {
                _cache.Remove(PERIPHERIQUES_CACHE_KEY);
                await _appState.ShowNotificationAsync("Périphérique mis à jour avec succès", NotificationType.Success);
                _logger.LogInformation("✅ Périphérique {DeviceId} mis à jour", id);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la mise à jour: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la mise à jour du périphérique {DeviceId}: {Message}", id, ex.Message);
            var errorMessage = $"Erreur lors de la mise à jour: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<PeripheriqueLogiqueDto>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> DeleteDeviceAsync(int id)
    {
        try
        {
            _logger.LogInformation("🗑️ Suppression du périphérique {DeviceId}...", id);
            
            var response = await _apiService.DeleteAsync($"/api/Devices/{id}");
            
            if (response.IsSuccess)
            {
                _cache.Remove(PERIPHERIQUES_CACHE_KEY);
                await _appState.ShowNotificationAsync("Périphérique supprimé avec succès", NotificationType.Success);
                _logger.LogInformation("✅ Périphérique {DeviceId} supprimé", id);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la suppression: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la suppression du périphérique {DeviceId}: {Message}", id, ex.Message);
            var errorMessage = $"Erreur lors de la suppression: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> SetDesiredPropertiesAsync(string iotDeviceId, object desired)
    {
        try
        {
            _logger.LogInformation("⚙️ Mise à jour des desired properties IoT Device {DeviceId}...", iotDeviceId);
            
            var response = await _apiService.PostAsync<object>($"/api/Devices/{iotDeviceId}/desired", desired);
            
            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync("Propriétés IoT mises à jour avec succès", NotificationType.Success);
                _logger.LogInformation("✅ Desired properties mises à jour pour {DeviceId}", iotDeviceId);
                return ApiResponse.Success(response.StatusCode);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la mise à jour IoT: {response.ErrorMessage}", NotificationType.Error);
                return ApiResponse.Error(response.ErrorMessage ?? "Erreur lors de la mise à jour", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la mise à jour des desired properties {DeviceId}: {Message}", iotDeviceId, ex.Message);
            var errorMessage = $"Erreur: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    // ==========================================
    // MÉTHODES LEGACY POUR COMPATIBILITÉ UI
    // ==========================================

    public async Task<IEnumerable<DeviceDto>> GetAllDevicesLegacyAsync()
    {
        var peripheriques = await GetAllDevicesAsync();
        
        return peripheriques.Select(p => new DeviceDto(
            p.Id,
            p.Name,
            $"Périphérique IoT - {p.Type}",
            p.Type,
            p.ZoneId,
            null,
            true,
            DateTime.Now
        ));
    }

    public async Task<DeviceDto?> GetDeviceByIdLegacyAsync(int id)
    {
        var peripherique = await GetDeviceByIdAsync(id);
        if (peripherique == null) return null;

        return new DeviceDto(
            peripherique.Id,
            peripherique.Name,
            $"Périphérique IoT - {peripherique.Type}",
            peripherique.Type,
            peripherique.ZoneId,
            null,
            true,
            DateTime.Now
        );
    }

    public async Task<ApiResponse<DeviceDto>> CreateDeviceLegacyAsync(DeviceCreationRequest request)
    {
        var creationDto = new PeripheriqueLogiqueCreationDto(
            request.Name,
            request.DeviceType ?? "Generic",
            $"iot-{request.Name.ToLower().Replace(" ", "-")}-{DateTime.Now.Ticks}",
            request.ZoneId ?? 1
        );

        var response = await CreateDeviceAsync(creationDto);
        
        if (response.IsSuccess && response.Data != null)
        {
            var deviceDto = new DeviceDto(
                response.Data.Id,
                response.Data.Name,
                $"Périphérique IoT - {response.Data.Type}",
                response.Data.Type,
                response.Data.ZoneId,
                null,
                true,
                DateTime.Now
            );
            
            return ApiResponse<DeviceDto>.Success(deviceDto, response.StatusCode);
        }

        return ApiResponse<DeviceDto>.Error(response.ErrorMessage ?? "Erreur lors de la création", response.StatusCode);
    }

    public async Task<ApiResponse<DeviceDto>> UpdateDeviceLegacyAsync(int id, DeviceUpdateRequest request)
    {
        var updateDto = new PeripheriqueLogiqueUpdateDto(
            request.Name,
            request.DeviceType ?? "Generic"
        );

        var response = await UpdateDeviceAsync(id, updateDto);
        
        if (response.IsSuccess && response.Data != null)
        {
            var deviceDto = new DeviceDto(
                response.Data.Id,
                response.Data.Name,
                $"Périphérique IoT - {response.Data.Type}",
                response.Data.Type,
                response.Data.ZoneId,
                null,
                true,
                DateTime.Now
            );
            
            return ApiResponse<DeviceDto>.Success(deviceDto, response.StatusCode);
        }

        return ApiResponse<DeviceDto>.Error(response.ErrorMessage ?? "Erreur lors de la mise à jour", response.StatusCode);
    }

    // ==========================================
    // MÉTHODES IOT HUB (LEGACY)
    // ==========================================

    public async Task<IoTResponse> SendSmartCommandAsync(string deviceId, string command, object? parameters = null)
    {
        try
        {
            _logger.LogInformation("➡️ Envoi de la commande '{Command}' à l'appareil {DeviceId}...", command, deviceId);
            
            var endpoint = $"/api/IoTHub/device/{deviceId}/smart-commands/{command}";
            var response = await _apiService.PostAsync<IoTResponse>(endpoint, parameters ?? new { });
            
            if (response.IsSuccess && response.Data != null)
            {
                if (response.Data.Success)
                {
                    await _appState.ShowNotificationAsync($"Commande '{command}' exécutée avec succès", NotificationType.Success);
                    _logger.LogInformation("✅ Commande '{Command}' exécutée sur {DeviceId}: {Message}", command, deviceId, response.Data.Message);
                }
                else
                {
                    await _appState.ShowNotificationAsync($"Échec de la commande: {response.Data.Message}", NotificationType.Warning);
                    _logger.LogWarning("⚠️ Échec de la commande '{Command}' sur {DeviceId}: {Message}", command, deviceId, response.Data.Message);
                }
                
                return response.Data;
            }

            var errorMessage = response.ErrorMessage ?? "Erreur lors de l'envoi de la commande";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return new IoTResponse(false, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de l'envoi de la commande '{Command}' à {DeviceId}: {Message}", command, deviceId, ex.Message);
            var errorMessage = $"Erreur: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return new IoTResponse(false, errorMessage);
        }
    }

    public async Task<IoTResponse> InvokeMethodAsync(string deviceId, string methodName, object? parameters = null)
    {
        try
        {
            _logger.LogInformation("➡️ Invocation de la méthode '{MethodName}' sur l'appareil {DeviceId}...", methodName, deviceId);
            
            var endpoint = $"/api/IoTHub/device/{deviceId}/method/{methodName}";
            var response = await _apiService.PostAsync<IoTResponse>(endpoint, parameters ?? new { });
            
            if (response.IsSuccess && response.Data != null)
            {
                if (response.Data.Success)
                {
                    await _appState.ShowNotificationAsync($"Méthode '{methodName}' invoquée avec succès", NotificationType.Success);
                    _logger.LogInformation("✅ Méthode '{MethodName}' invoquée sur {DeviceId}: {Message}", methodName, deviceId, response.Data.Message);
                }
                else
                {
                    await _appState.ShowNotificationAsync($"Échec de la méthode: {response.Data.Message}", NotificationType.Warning);
                    _logger.LogWarning("⚠️ Échec de la méthode '{MethodName}' sur {DeviceId}: {Message}", methodName, deviceId, response.Data.Message);
                }
                
                return response.Data;
            }

            var errorMessage = response.ErrorMessage ?? "Erreur lors de l'invocation de la méthode";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return new IoTResponse(false, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de l'invocation de la méthode '{MethodName}' sur {DeviceId}: {Message}", methodName, deviceId, ex.Message);
            var errorMessage = $"Erreur: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return new IoTResponse(false, errorMessage);
        }
    }

    public async Task<DeviceTwinDto?> GetDeviceTwinAsync(string deviceId)
    {
        try
        {
            _logger.LogDebug("➡️ Récupération du twin de l'appareil {DeviceId}...", deviceId);
            
            var response = await _apiService.GetAsync<DeviceTwinDto>($"/api/IoTHub/device/{deviceId}/twin");
            
            if (response.IsSuccess)
            {
                _logger.LogDebug("✅ Twin récupéré pour {DeviceId}", deviceId);
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
            _logger.LogError("❌ Erreur lors de la récupération du twin de {DeviceId}: {Message}", deviceId, ex.Message);
            await _appState.SetErrorAsync($"Erreur: {ex.Message}");
            return null;
        }
    }

    public async Task<ApiResponse> UpdateDeviceTwinAsync(string deviceId, object desiredProperties)
    {
        try
        {
            _logger.LogInformation("🛠️ Mise à jour du twin de l'appareil {DeviceId}...", deviceId);
            
            var response = await _apiService.PutAsync<object>($"/api/IoTHub/device/{deviceId}/twin/desired", desiredProperties);
            
            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync("Propriétés du twin mises à jour avec succès", NotificationType.Success);
                _logger.LogInformation("✅ Twin mis à jour pour {DeviceId}", deviceId);
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
            _logger.LogError("❌ Erreur lors de la mise à jour du twin de {DeviceId}: {Message}", deviceId, ex.Message);
            var errorMessage = $"Erreur: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> SendMessageToDeviceAsync(string deviceId, string message)
    {
        try
        {
            _logger.LogInformation("➡️ Envoi de message à l'appareil {DeviceId}...", deviceId);
            
            var response = await _apiService.PostAsync<object>($"/api/IoTHub/device/{deviceId}/message", new { message });
            
            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync("Message envoyé avec succès", NotificationType.Success);
                _logger.LogInformation("✅ Message envoyé à {DeviceId}", deviceId);
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
            _logger.LogError("❌ Erreur lors de l'envoi de message à {DeviceId}: {Message}", deviceId, ex.Message);
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

// DTOs pour les requêtes legacy
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