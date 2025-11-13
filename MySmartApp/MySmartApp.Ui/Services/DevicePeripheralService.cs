using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

/// <summary>
/// Service pour gérer les périphériques physiques (twins) des devices IoT
/// </summary>
public interface IDevicePeripheralService
{
    Task<List<PeripheralDto>> GetPeripheralsAsync(string iotDeviceId);
    Task<PeripheralDto?> GetPeripheralAsync(string iotDeviceId, string peripheralName);
    Task<MergedPeripheralDto?> GetMergedPeripheralAsync(string iotDeviceId, string peripheralName);
    Task<ApiResponse> AddPeripheralAsync(string iotDeviceId, PeripheralDto peripheral);
    Task<ApiResponse> ReplacePeripheralsAsync(string iotDeviceId, List<PeripheralDto> peripherals);
    Task<ApiResponse> DeletePeripheralAsync(string iotDeviceId, string peripheralName);
    Task<ApiResponse> ExecuteCommandAsync(string iotDeviceId, string peripheralName, string command, Dictionary<string, object>? parameters = null);
    Task<DeviceTwinFullDto?> GetFullTwinAsync(string iotDeviceId);
}

public class DevicePeripheralService : IDevicePeripheralService
{
    private readonly IApiService _apiService;
    private readonly IAppStateService _appState;
    private readonly ILogger<DevicePeripheralService> _logger;

    public DevicePeripheralService(
        IApiService apiService,
        IAppStateService appState,
        ILogger<DevicePeripheralService> logger)
    {
        _apiService = apiService;
        _appState = appState;
        _logger = logger;
    }

    public async Task<List<PeripheralDto>> GetPeripheralsAsync(string iotDeviceId)
    {
        try
        {
            _logger.LogDebug("?? Récupération des périphériques pour {IotDeviceId}...", iotDeviceId);

            var response = await _apiService.GetAsync<List<PeripheralDto>>(
                $"/api/Devices/{Uri.EscapeDataString(iotDeviceId)}/peripherals");

            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? {Count} périphériques récupérés pour {IotDeviceId}", 
                    response.Data.Count, iotDeviceId);
                return response.Data;
            }

            if (response.StatusCode == 404)
            {
                _logger.LogWarning("Device {IotDeviceId} introuvable", iotDeviceId);
                return new List<PeripheralDto>();
            }

            await _appState.SetErrorAsync(response.ErrorMessage ?? $"Erreur lors de la récupération des périphériques de {iotDeviceId}");
            return new List<PeripheralDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la récupération des périphériques de {IotDeviceId}", iotDeviceId);
            await _appState.SetErrorAsync($"Erreur: {ex.Message}");
            return new List<PeripheralDto>();
        }
    }

    public async Task<PeripheralDto?> GetPeripheralAsync(string iotDeviceId, string peripheralName)
    {
        try
        {
            _logger.LogDebug("?? Récupération du périphérique {Name} sur {IotDeviceId}...", 
                peripheralName, iotDeviceId);

            var response = await _apiService.GetAsync<PeripheralDto>(
                $"/api/Devices/{Uri.EscapeDataString(iotDeviceId)}/peripherals/{Uri.EscapeDataString(peripheralName)}");

            if (response.IsSuccess)
            {
                _logger.LogDebug("? Périphérique {Name} récupéré", peripheralName);
                return response.Data;
            }

            if (response.StatusCode != 404)
            {
                await _appState.SetErrorAsync(response.ErrorMessage ?? $"Erreur lors de la récupération du périphérique {peripheralName}");
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la récupération du périphérique {Name} sur {IotDeviceId}", 
                peripheralName, iotDeviceId);
            await _appState.SetErrorAsync($"Erreur: {ex.Message}");
            return null;
        }
    }

    public async Task<MergedPeripheralDto?> GetMergedPeripheralAsync(string iotDeviceId, string peripheralName)
    {
        try
        {
            _logger.LogDebug("?? Récupération de la vue fusionnée du périphérique {Name} sur {IotDeviceId}...", 
                peripheralName, iotDeviceId);

            var response = await _apiService.GetAsync<MergedPeripheralDto>(
                $"/api/Devices/{Uri.EscapeDataString(iotDeviceId)}/peripherals/{Uri.EscapeDataString(peripheralName)}/merged");

            if (response.IsSuccess)
            {
                _logger.LogDebug("? Vue fusionnée récupérée pour {Name}", peripheralName);
                return response.Data;
            }

            if (response.StatusCode != 404)
            {
                await _appState.SetErrorAsync(response.ErrorMessage ?? $"Erreur lors de la récupération de la vue fusionnée de {peripheralName}");
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la récupération de la vue fusionnée du périphérique {Name} sur {IotDeviceId}", 
                peripheralName, iotDeviceId);
            await _appState.SetErrorAsync($"Erreur: {ex.Message}");
            return null;
        }
    }

    public async Task<ApiResponse> AddPeripheralAsync(string iotDeviceId, PeripheralDto peripheral)
    {
        try
        {
            _logger.LogInformation("??? Ajout du périphérique '{Name}' sur {IotDeviceId}...", 
                peripheral.Name, iotDeviceId);

            var response = await _apiService.PostAsync<object>(
                $"/api/Devices/{Uri.EscapeDataString(iotDeviceId)}/peripherals",
                peripheral);

            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync($"Périphérique '{peripheral.Name}' ajouté avec succès", NotificationType.Success);
                _logger.LogInformation("? Périphérique '{Name}' ajouté sur {IotDeviceId}", 
                    peripheral.Name, iotDeviceId);
                return ApiResponse.Success(response.StatusCode);
            }

            await _appState.ShowNotificationAsync($"Erreur lors de l'ajout: {response.ErrorMessage}", NotificationType.Error);
            return ApiResponse.Error(response.ErrorMessage ?? "Erreur lors de l'ajout", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de l'ajout du périphérique '{Name}' sur {IotDeviceId}", 
                peripheral.Name, iotDeviceId);
            var errorMessage = $"Erreur: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> ReplacePeripheralsAsync(string iotDeviceId, List<PeripheralDto> peripherals)
    {
        try
        {
            _logger.LogInformation("??? Remplacement des périphériques sur {IotDeviceId} ({Count} périphériques)...", 
                iotDeviceId, peripherals.Count);

            var payload = new PeripheralsPayloadDto(peripherals);
            var response = await _apiService.PutAsync<object>(
                $"/api/Devices/{Uri.EscapeDataString(iotDeviceId)}/peripherals",
                payload);

            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync("Périphériques remplacés avec succès", NotificationType.Success);
                _logger.LogInformation("? Périphériques remplacés sur {IotDeviceId}", iotDeviceId);
                return ApiResponse.Success(response.StatusCode);
            }

            await _appState.ShowNotificationAsync($"Erreur lors du remplacement: {response.ErrorMessage}", NotificationType.Error);
            return ApiResponse.Error(response.ErrorMessage ?? "Erreur lors du remplacement", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors du remplacement des périphériques sur {IotDeviceId}", iotDeviceId);
            var errorMessage = $"Erreur: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> DeletePeripheralAsync(string iotDeviceId, string peripheralName)
    {
        try
        {
            _logger.LogInformation("??? Suppression du périphérique '{Name}' sur {IotDeviceId}...", 
                peripheralName, iotDeviceId);

            var response = await _apiService.DeleteAsync(
                $"/api/Devices/{Uri.EscapeDataString(iotDeviceId)}/peripherals/{Uri.EscapeDataString(peripheralName)}");

            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync($"Périphérique '{peripheralName}' supprimé avec succès", NotificationType.Success);
                _logger.LogInformation("? Périphérique '{Name}' supprimé de {IotDeviceId}", 
                    peripheralName, iotDeviceId);
                return ApiResponse.Success(response.StatusCode);
            }

            await _appState.ShowNotificationAsync($"Erreur lors de la suppression: {response.ErrorMessage}", NotificationType.Error);
            return ApiResponse.Error(response.ErrorMessage ?? "Erreur lors de la suppression", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la suppression du périphérique '{Name}' sur {IotDeviceId}", 
                peripheralName, iotDeviceId);
            var errorMessage = $"Erreur: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> ExecuteCommandAsync(string iotDeviceId, string peripheralName, string command, 
        Dictionary<string, object>? parameters = null)
    {
        try
        {
            _logger.LogInformation("?? Exécution de la commande '{Command}' sur {Name} @ {IotDeviceId}...", 
                command, peripheralName, iotDeviceId);

            var payload = new PeripheralCommandPayloadDto(parameters);
            var response = await _apiService.PostAsync<object>(
                $"/api/Devices/{Uri.EscapeDataString(iotDeviceId)}/peripherals/{Uri.EscapeDataString(peripheralName)}/commands/{Uri.EscapeDataString(command)}",
                payload);

            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync($"Commande '{command}' exécutée avec succès sur '{peripheralName}'", NotificationType.Success);
                _logger.LogInformation("? Commande '{Command}' exécutée sur {Name} @ {IotDeviceId}", 
                    command, peripheralName, iotDeviceId);
                return ApiResponse.Success(response.StatusCode);
            }

            await _appState.ShowNotificationAsync($"Erreur lors de l'exécution: {response.ErrorMessage}", NotificationType.Warning);
            return ApiResponse.Error(response.ErrorMessage ?? "Erreur lors de l'exécution de la commande", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de l'exécution de la commande '{Command}' sur {Name} @ {IotDeviceId}", 
                command, peripheralName, iotDeviceId);
            var errorMessage = $"Erreur: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    public async Task<DeviceTwinFullDto?> GetFullTwinAsync(string iotDeviceId)
    {
        try
        {
            _logger.LogDebug("?? Récupération du twin complet pour {IotDeviceId}...", iotDeviceId);

            var response = await _apiService.GetAsync<DeviceTwinFullDto>(
                $"/api/Devices/{Uri.EscapeDataString(iotDeviceId)}/twin/full");

            if (response.IsSuccess)
            {
                _logger.LogDebug("? Twin complet récupéré pour {IotDeviceId}", iotDeviceId);
                return response.Data;
            }

            if (response.StatusCode != 404)
            {
                await _appState.SetErrorAsync(response.ErrorMessage ?? $"Erreur lors de la récupération du twin de {iotDeviceId}");
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la récupération du twin complet de {IotDeviceId}", iotDeviceId);
            await _appState.SetErrorAsync($"Erreur: {ex.Message}");
            return null;
        }
    }
}
