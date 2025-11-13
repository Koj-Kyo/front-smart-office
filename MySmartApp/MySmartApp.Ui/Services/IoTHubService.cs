using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public interface IIoTHubService
{
    // M�thodes directes sur devices
    Task<ApiResponse<IoTHubOperationResultDto>> InvokeDeviceMethodAsync(string deviceId, string methodName, string? payload = null);
    Task<ApiResponse<IoTHubOperationResultDto>> ExecuteSmartCommandAsync(string deviceId, string command, SmartCommandPayloadDto? payload = null);
    
    // Gestion des device twins
    Task<ApiResponse<DeviceTwinInfoDto>> GetDeviceTwinAsync(string deviceId);
    Task<ApiResponse<IoTHubOperationResultDto>> UpdateDesiredPropertiesAsync(string deviceId, string desiredProperties);
    
    // Messages Cloud-to-Device
    Task<ApiResponse<IoTHubOperationResultDto>> SendCloudToDeviceMessageAsync(string deviceId, string message, Dictionary<string, string>? properties = null);
    
    // Gestion des devices (Admin uniquement)
    Task<ApiResponse<IoTHubOperationResultDto>> CreateOrUpdateDeviceAsync(string deviceId, bool enabled = true);
    Task<ApiResponse<IoTHubOperationResultDto>> DeleteDeviceAsync(string deviceId);
    
    // Statut de connexion
    Task<ApiResponse<List<DeviceConnectionInfoDto>>> GetDevicesConnectionStatusAsync(List<string> deviceIds);
    
    // Liste des devices IoT Hub
    Task<ApiResponse<List<IoTHubDeviceInfoDto>>> GetAllIoTHubDevicesAsync(int max = 100);
    
    // Synchronisation
    Task<ApiResponse<IoTHubSyncResultDto>> SyncDevicesFromIoTHubAsync();
    
    // M�thodes utilitaires
    Task<ApiResponse<List<DeviceConnectionInfoDto>>> GetAllDevicesStatusAsync();
    Task<bool> IsDeviceConnectedAsync(string deviceId);
    Task<Dictionary<string, bool>> GetDevicesHealthStatusAsync(List<string> deviceIds);
}

public class IoTHubService : IIoTHubService
{
    private readonly IApiService _apiService;
    private readonly ILogger<IoTHubService> _logger;
    private readonly IAppStateService _appState;
    
    // Cache pour les statuts de connexion
    private Dictionary<string, DeviceConnectionInfoDto>? _connectionStatusCache;
    private DateTime? _cacheExpiry;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(1); // Cache court pour les statuts IoT

    public IoTHubService(
        IApiService apiService,
        ILogger<IoTHubService> logger,
        IAppStateService appState)
    {
        _apiService = apiService;
        _logger = logger;
        _appState = appState;
    }

    // ==========================================
    // M�THODES DIRECTES SUR DEVICES
    // ==========================================

    public async Task<ApiResponse<IoTHubOperationResultDto>> InvokeDeviceMethodAsync(string deviceId, string methodName, string? payload = null)
    {
        try
        {
            _logger.LogInformation("?? Invocation m�thode {MethodName} sur device {DeviceId}", methodName, deviceId);
            
            var requestPayload = new DeviceMethodPayloadDto(payload ?? "{}");
            var response = await _apiService.PostAsync<IoTHubOperationResultDto>(
                $"/api/IoTHub/device/{deviceId}/method/{methodName}", 
                requestPayload);
            
            if (response.IsSuccess && response.Data != null)
            {
                await _appState.ShowNotificationAsync(
                    $"M�thode {methodName} ex�cut�e sur {deviceId}", 
                    NotificationType.Success);
                
                _logger.LogInformation("? M�thode {MethodName} ex�cut�e avec succ�s sur {DeviceId}", methodName, deviceId);
                
                // Invalider le cache des statuts
                InvalidateConnectionStatusCache();
            }
            else
            {
                await _appState.ShowNotificationAsync(
                    $"Erreur lors de l'ex�cution de {methodName}: {response.ErrorMessage}", 
                    NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de l'invocation de la m�thode {MethodName} sur {DeviceId}: {Message}", methodName, deviceId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<IoTHubOperationResultDto>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse<IoTHubOperationResultDto>> ExecuteSmartCommandAsync(string deviceId, string command, SmartCommandPayloadDto? payload = null)
    {
        try
        {
            _logger.LogInformation("?? Ex�cution commande intelligente {Command} sur device {DeviceId}", command, deviceId);
            
            var response = await _apiService.PostAsync<IoTHubOperationResultDto>(
                $"/api/IoTHub/device/{deviceId}/smart-commands/{command}", 
                payload);
            
            if (response.IsSuccess && response.Data != null)
            {
                await _appState.ShowNotificationAsync(
                    $"Commande {command} ex�cut�e sur {deviceId}", 
                    NotificationType.Success);
                
                _logger.LogInformation("? Commande {Command} ex�cut�e avec succ�s sur {DeviceId}", command, deviceId);
                
                // Invalider le cache des statuts
                InvalidateConnectionStatusCache();
            }
            else
            {
                await _appState.ShowNotificationAsync(
                    $"Erreur lors de l'ex�cution de {command}: {response.ErrorMessage}", 
                    NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de l'ex�cution de la commande {Command} sur {DeviceId}: {Message}", command, deviceId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<IoTHubOperationResultDto>.Error(errorMessage);
        }
    }

    // ==========================================
    // GESTION DES DEVICE TWINS
    // ==========================================

    public async Task<ApiResponse<DeviceTwinInfoDto>> GetDeviceTwinAsync(string deviceId)
    {
        try
        {
            _logger.LogInformation("?? R�cup�ration du twin pour device {DeviceId}", deviceId);
            
            var response = await _apiService.GetAsync<DeviceTwinInfoDto>($"/api/IoTHub/device/{deviceId}/twin");
            
            if (response.IsSuccess)
            {
                _logger.LogDebug("? Twin r�cup�r� pour device {DeviceId}", deviceId);
            }
            else
            {
                _logger.LogWarning("?? Impossible de r�cup�rer le twin pour {DeviceId}: {Error}", deviceId, response.ErrorMessage);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration du twin pour {DeviceId}: {Message}", deviceId, ex.Message);
            return ApiResponse<DeviceTwinInfoDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IoTHubOperationResultDto>> UpdateDesiredPropertiesAsync(string deviceId, string desiredProperties)
    {
        try
        {
            _logger.LogInformation("?? Mise � jour des desired properties pour device {DeviceId}", deviceId);
            
            var payload = new DesiredPropertiesPayloadDto(desiredProperties);
            var response = await _apiService.PutAsync<IoTHubOperationResultDto>(
                $"/api/IoTHub/device/{deviceId}/twin/desired", 
                payload);
            
            if (response.IsSuccess && response.Data != null)
            {
                await _appState.ShowNotificationAsync(
                    $"Propri�t�s mises � jour pour {deviceId}", 
                    NotificationType.Success);
                
                _logger.LogInformation("? Desired properties mises � jour pour {DeviceId}", deviceId);
            }
            else
            {
                await _appState.ShowNotificationAsync(
                    $"Erreur lors de la mise � jour: {response.ErrorMessage}", 
                    NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la mise � jour des properties pour {DeviceId}: {Message}", deviceId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<IoTHubOperationResultDto>.Error(errorMessage);
        }
    }

    // ==========================================
    // MESSAGES CLOUD-TO-DEVICE
    // ==========================================

    public async Task<ApiResponse<IoTHubOperationResultDto>> SendCloudToDeviceMessageAsync(string deviceId, string message, Dictionary<string, string>? properties = null)
    {
        try
        {
            _logger.LogInformation("?? Envoi message cloud-to-device vers {DeviceId}", deviceId);
            
            var payload = new CloudToDeviceMessagePayloadDto(message, properties);
            var response = await _apiService.PostAsync<IoTHubOperationResultDto>(
                $"/api/IoTHub/device/{deviceId}/message", 
                payload);
            
            if (response.IsSuccess && response.Data != null)
            {
                await _appState.ShowNotificationAsync(
                    $"Message envoy� � {deviceId}", 
                    NotificationType.Success);
                
                _logger.LogInformation("? Message envoy� avec succ�s � {DeviceId}", deviceId);
            }
            else
            {
                await _appState.ShowNotificationAsync(
                    $"Erreur lors de l'envoi du message: {response.ErrorMessage}", 
                    NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de l'envoi du message � {DeviceId}: {Message}", deviceId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<IoTHubOperationResultDto>.Error(errorMessage);
        }
    }

    // ==========================================
    // GESTION DES DEVICES (ADMIN UNIQUEMENT)
    // ==========================================

    public async Task<ApiResponse<IoTHubOperationResultDto>> CreateOrUpdateDeviceAsync(string deviceId, bool enabled = true)
    {
        try
        {
            _logger.LogInformation("? Cr�ation/mise � jour du device {DeviceId}", deviceId);
            
            var payload = new CreateDevicePayloadDto(enabled);
            var response = await _apiService.PostAsync<IoTHubOperationResultDto>(
                $"/api/IoTHub/device/{deviceId}", 
                payload);
            
            if (response.IsSuccess && response.Data != null)
            {
                await _appState.ShowNotificationAsync(
                    $"Device {deviceId} cr��/mis � jour avec succ�s", 
                    NotificationType.Success);
                
                _logger.LogInformation("? Device {DeviceId} cr��/mis � jour avec succ�s", deviceId);
                
                // Invalider le cache
                InvalidateConnectionStatusCache();
            }
            else
            {
                await _appState.ShowNotificationAsync(
                    $"Erreur lors de la cr�ation/mise � jour: {response.ErrorMessage}", 
                    NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la cr�ation/mise � jour du device {DeviceId}: {Message}", deviceId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<IoTHubOperationResultDto>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse<IoTHubOperationResultDto>> DeleteDeviceAsync(string deviceId)
    {
        try
        {
            _logger.LogInformation("??? Suppression du device {DeviceId}", deviceId);
            
            var response = await _apiService.DeleteAsync($"/api/IoTHub/device/{deviceId}");
            
            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync(
                    $"Device {deviceId} supprim� avec succ�s", 
                    NotificationType.Success);
                
                _logger.LogInformation("? Device {DeviceId} supprim� avec succ�s", deviceId);
                
                // Invalider le cache
                InvalidateConnectionStatusCache();
                
                return ApiResponse<IoTHubOperationResultDto>.Success(
                    new IoTHubOperationResultDto(true, $"Device {deviceId} supprim� avec succ�s", deviceId, DateTime.UtcNow), 
                    response.StatusCode);
            }
            else
            {
                await _appState.ShowNotificationAsync(
                    $"Erreur lors de la suppression: {response.ErrorMessage}", 
                    NotificationType.Error);
                    
                return ApiResponse<IoTHubOperationResultDto>.Error(
                    response.ErrorMessage ?? "Erreur lors de la suppression", 
                    response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la suppression du device {DeviceId}: {Message}", deviceId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<IoTHubOperationResultDto>.Error(errorMessage);
        }
    }

    // ==========================================
    // LISTE DES DEVICES IOT HUB
    // ==========================================

    public async Task<ApiResponse<List<IoTHubDeviceInfoDto>>> GetAllIoTHubDevicesAsync(int max = 100)
    {
        try
        {
            _logger.LogInformation("📋 Récupération de tous les devices IoT Hub (max: {Max})", max);
            
            var response = await _apiService.GetAsync<List<IoTHubDeviceInfoDto>>(
                $"/api/IoTHub/devices?max={max}");
            
            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("✅ {Count} devices IoT Hub récupérés", response.Data.Count);
            }
            else
            {
                _logger.LogWarning("⚠️ Erreur lors de la récupération des devices IoT Hub: {Error}", response.ErrorMessage);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erreur lors de la récupération des devices IoT Hub");
            return ApiResponse<List<IoTHubDeviceInfoDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // SYNCHRONISATION
    // ==========================================

    public async Task<ApiResponse<IoTHubSyncResultDto>> SyncDevicesFromIoTHubAsync()
    {
        try
        {
            _logger.LogInformation("? D�marrage de la synchronisation des devices depuis IoT Hub...");
            
            await _appState.ShowNotificationAsync(
                "Synchronisation des devices en cours...", 
                NotificationType.Info);
            
            var response = await _apiService.PostAsync<IoTHubSyncResultDto>(
                "/api/IoTHub/devices/sync", 
                null);
            
            if (response.IsSuccess && response.Data != null)
            {
                var result = response.Data;
                var message = $"Synchronisation termin�e: {result.CreatedCount} cr��(s), {result.UpdatedCount} mis � jour, {result.FailedCount} �chec(s)";
                
                await _appState.ShowNotificationAsync(
                    message, 
                    result.FailedCount > 0 ? NotificationType.Warning : NotificationType.Success);
                
                _logger.LogInformation("? {Message}", message);
                
                // Invalider le cache apr�s synchronisation
                InvalidateConnectionStatusCache();
            }
            else
            {
                await _appState.ShowNotificationAsync(
                    $"Erreur lors de la synchronisation: {response.ErrorMessage}", 
                    NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la synchronisation des devices depuis IoT Hub");
            var errorMessage = $"Erreur technique lors de la synchronisation: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<IoTHubSyncResultDto>.Error(errorMessage);
        }
    }

    // ==========================================
    // STATUT DE CONNEXION
    // ==========================================

    public async Task<ApiResponse<List<DeviceConnectionInfoDto>>> GetDevicesConnectionStatusAsync(List<string> deviceIds)
    {
        try
        {
            // V�rifier le cache d'abord
            if (IsCacheValid() && _connectionStatusCache != null)
            {
                var cachedResults = deviceIds
                    .Where(id => _connectionStatusCache.ContainsKey(id))
                    .Select(id => _connectionStatusCache[id])
                    .ToList();
                
                if (cachedResults.Count == deviceIds.Count)
                {
                    _logger.LogDebug("?? Statuts de connexion r�cup�r�s depuis le cache");
                    return ApiResponse<List<DeviceConnectionInfoDto>>.Success(cachedResults);
                }
            }

            _logger.LogInformation("?? R�cup�ration du statut de connexion pour {Count} devices", deviceIds.Count);
            
            var request = new DeviceStatusRequestDto(deviceIds);
            var response = await _apiService.PostAsync<List<DeviceConnectionInfoDto>>(
                "/api/IoTHub/devices/connection-status", 
                request);
            
            if (response.IsSuccess && response.Data != null)
            {
                // Mettre � jour le cache
                UpdateConnectionStatusCache(response.Data);
                
                _logger.LogInformation("? Statuts de connexion r�cup�r�s pour {Count} devices", response.Data.Count);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des statuts de connexion: {Message}", ex.Message);
            return ApiResponse<List<DeviceConnectionInfoDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // M�THODES UTILITAIRES
    // ==========================================

    public async Task<ApiResponse<List<DeviceConnectionInfoDto>>> GetAllDevicesStatusAsync()
    {
        try
        {
            // Cette m�thode pourrait �tre �tendue pour r�cup�rer tous les devices connus
            // Pour l'instant, on retourne une liste vide
            _logger.LogInformation("?? R�cup�ration du statut de tous les devices");
            
            // Simuler une liste de devices connus (� remplacer par une vraie logique)
            var knownDeviceIds = new List<string>
            {
                "smart-light-001",
                "smart-thermostat-001", 
                "smart-door-001",
                "smart-camera-001"
            };
            
            return await GetDevicesConnectionStatusAsync(knownDeviceIds);
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration de tous les statuts: {Message}", ex.Message);
            return ApiResponse<List<DeviceConnectionInfoDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<bool> IsDeviceConnectedAsync(string deviceId)
    {
        try
        {
            var response = await GetDevicesConnectionStatusAsync(new List<string> { deviceId });
            return response.IsSuccess && 
                   response.Data?.FirstOrDefault()?.IsConnected == true;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la v�rification de connexion pour {DeviceId}: {Message}", deviceId, ex.Message);
            return false;
        }
    }

    public async Task<Dictionary<string, bool>> GetDevicesHealthStatusAsync(List<string> deviceIds)
    {
        try
        {
            var response = await GetDevicesConnectionStatusAsync(deviceIds);
            
            if (response.IsSuccess && response.Data != null)
            {
                return response.Data.ToDictionary(
                    d => d.DeviceId, 
                    d => d.IsConnected && d.ConnectionState == "Connected"
                );
            }
            
            return deviceIds.ToDictionary(id => id, _ => false);
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des statuts de sant�: {Message}", ex.Message);
            return deviceIds.ToDictionary(id => id, _ => false);
        }
    }

    // ==========================================
    // GESTION DU CACHE
    // ==========================================

    private bool IsCacheValid()
    {
        return _cacheExpiry.HasValue && DateTime.Now < _cacheExpiry.Value;
    }

    private void UpdateConnectionStatusCache(List<DeviceConnectionInfoDto> connectionInfos)
    {
        _connectionStatusCache ??= new Dictionary<string, DeviceConnectionInfoDto>();
        
        foreach (var info in connectionInfos)
        {
            _connectionStatusCache[info.DeviceId] = info;
        }
        
        _cacheExpiry = DateTime.Now.Add(_cacheTimeout);
        _logger.LogDebug("?? Cache des statuts de connexion mis � jour");
    }

    private void InvalidateConnectionStatusCache()
    {
        _connectionStatusCache = null;
        _cacheExpiry = null;
        _logger.LogDebug("?? Cache des statuts de connexion invalid�");
    }
}