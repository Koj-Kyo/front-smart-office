using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

/// <summary>
/// Service pour gérer le statut des devices IoT (métriques, RFID, alarmes)
/// Compatible avec firmware ESP32 v2.0
/// </summary>
public interface IDeviceStatusService
{
    // Statut complet
    Task<ApiResponse<DeviceFullStatusDto>> GetFullDeviceStatusAsync(string iotDeviceId);
    
    // Métriques système
    Task<ApiResponse<DeviceMetricsDto>> RecordMetricsAsync(string iotDeviceId, RecordMetricsRequest request);
    Task<ApiResponse<List<DeviceMetricsDto>>> GetMetricsAsync(string iotDeviceId, DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<DeviceMetricsDto>> GetLatestMetricsAsync(string iotDeviceId);
    
    // RFID Events
    Task<ApiResponse<RfidEventDto>> RecordRfidEventAsync(string iotDeviceId, RecordRfidEventRequest request);
    Task<ApiResponse<List<RfidEventDto>>> GetRfidEventsAsync(string iotDeviceId, DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<List<RfidEventDto>>> GetRfidEventsByCardAsync(string cardUid, DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<List<RfidEventDto>>> GetDeniedAccessAsync(DateTime? startDate = null, DateTime? endDate = null);
    
    // Smoke Alarms
    Task<ApiResponse<SmokeAlarmEventDto>> RecordSmokeAlarmAsync(string iotDeviceId, RecordSmokeAlarmRequest request);
    Task<ApiResponse> DeactivateSmokeAlarmAsync(string iotDeviceId, DeactivateSmokeAlarmRequest request);
    Task<ApiResponse<List<SmokeAlarmEventDto>>> GetSmokeAlarmsAsync(string iotDeviceId, DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<SmokeAlarmEventDto>> GetActiveSmokeAlarmAsync(string iotDeviceId);
    Task<ApiResponse<List<SmokeAlarmEventDto>>> GetAllActiveAlarmsAsync();
    
    // Maintenance
    Task<ApiResponse<int>> CleanupOldDataAsync(int retentionDays = 90);
}

public class DeviceStatusService : IDeviceStatusService
{
    private readonly IApiService _apiService;
    private readonly ILogger<DeviceStatusService> _logger;
    private readonly IAppStateService _appState;

    public DeviceStatusService(
        IApiService apiService,
        ILogger<DeviceStatusService> logger,
        IAppStateService appState)
    {
        _apiService = apiService;
        _logger = logger;
        _appState = appState;
    }

    // ==========================================
    // STATUT COMPLET
    // ==========================================

    public async Task<ApiResponse<DeviceFullStatusDto>> GetFullDeviceStatusAsync(string iotDeviceId)
    {
        try
        {
            _logger.LogInformation("?? Récupération statut complet device {DeviceId}", iotDeviceId);
            
            var response = await _apiService.GetAsync<DeviceFullStatusDto>($"/api/DeviceStatus/{iotDeviceId}/full-status");
            
            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? Statut complet récupéré pour {DeviceId}", iotDeviceId);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération statut complet {DeviceId}: {Message}", iotDeviceId, ex.Message);
            return ApiResponse<DeviceFullStatusDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // MÉTRIQUES SYSTÈME
    // ==========================================

    public async Task<ApiResponse<DeviceMetricsDto>> RecordMetricsAsync(string iotDeviceId, RecordMetricsRequest request)
    {
        try
        {
            _logger.LogInformation("?? Enregistrement métriques device {DeviceId}", iotDeviceId);
            
            var response = await _apiService.PostAsync<DeviceMetricsDto>($"/api/DeviceStatus/{iotDeviceId}/metrics", request);
            
            if (response.IsSuccess)
            {
                _logger.LogInformation("? Métriques enregistrées: RSSI={Rssi}, Heap={Heap}, Uptime={Uptime}s", 
                    request.Rssi, request.HeapFree, request.UptimeSeconds);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur enregistrement métriques {DeviceId}: {Message}", iotDeviceId, ex.Message);
            return ApiResponse<DeviceMetricsDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<DeviceMetricsDto>>> GetMetricsAsync(string iotDeviceId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            _logger.LogInformation("?? Récupération métriques device {DeviceId}", iotDeviceId);
            
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}");
            
            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var response = await _apiService.GetAsync<List<DeviceMetricsDto>>($"/api/DeviceStatus/{iotDeviceId}/metrics{queryString}");
            
            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? {Count} métriques récupérées", response.Data.Count);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération métriques {DeviceId}: {Message}", iotDeviceId, ex.Message);
            return ApiResponse<List<DeviceMetricsDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<DeviceMetricsDto>> GetLatestMetricsAsync(string iotDeviceId)
    {
        try
        {
            _logger.LogInformation("?? Récupération dernières métriques device {DeviceId}", iotDeviceId);
            
            var response = await _apiService.GetAsync<DeviceMetricsDto>($"/api/DeviceStatus/{iotDeviceId}/metrics/latest");
            
            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? Dernières métriques récupérées: RSSI={Rssi}, Heap={Heap}", 
                    response.Data.Rssi, response.Data.HeapFree);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération dernières métriques {DeviceId}: {Message}", iotDeviceId, ex.Message);
            return ApiResponse<DeviceMetricsDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // RFID EVENTS
    // ==========================================

    public async Task<ApiResponse<RfidEventDto>> RecordRfidEventAsync(string iotDeviceId, RecordRfidEventRequest request)
    {
        try
        {
            _logger.LogInformation("?? Enregistrement événement RFID device {DeviceId}: Card={CardUid}, Access={Access}", 
                iotDeviceId, request.CardUid, request.AccessGranted);
            
            var response = await _apiService.PostAsync<RfidEventDto>($"/api/DeviceStatus/{iotDeviceId}/rfid-events", request);
            
            if (response.IsSuccess)
            {
                if (!request.AccessGranted)
                {
                    await _appState.ShowNotificationAsync($"?? Accès refusé: {request.CardUid} - {request.DenialReason}", NotificationType.Warning);
                    _logger.LogWarning("?? Accès RFID refusé: {CardUid} - {Reason}", request.CardUid, request.DenialReason);
                }
                else
                {
                    _logger.LogInformation("? Accès RFID accordé: {CardUid}", request.CardUid);
                }
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur enregistrement événement RFID {DeviceId}: {Message}", iotDeviceId, ex.Message);
            return ApiResponse<RfidEventDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<RfidEventDto>>> GetRfidEventsAsync(string iotDeviceId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            _logger.LogInformation("?? Récupération événements RFID device {DeviceId}", iotDeviceId);
            
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}");
            
            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var response = await _apiService.GetAsync<List<RfidEventDto>>($"/api/DeviceStatus/{iotDeviceId}/rfid-events{queryString}");
            
            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? {Count} événements RFID récupérés", response.Data.Count);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération événements RFID {DeviceId}: {Message}", iotDeviceId, ex.Message);
            return ApiResponse<List<RfidEventDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<RfidEventDto>>> GetRfidEventsByCardAsync(string cardUid, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            _logger.LogInformation("?? Récupération événements RFID carte {CardUid}", cardUid);
            
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}");
            
            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var response = await _apiService.GetAsync<List<RfidEventDto>>($"/api/DeviceStatus/rfid-events/by-card/{cardUid}{queryString}");
            
            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? {Count} événements récupérés pour carte {CardUid}", response.Data.Count, cardUid);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération événements carte {CardUid}: {Message}", cardUid, ex.Message);
            return ApiResponse<List<RfidEventDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<RfidEventDto>>> GetDeniedAccessAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            _logger.LogInformation("?? Récupération accès RFID refusés");
            
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}");
            
            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var response = await _apiService.GetAsync<List<RfidEventDto>>($"/api/DeviceStatus/rfid-events/denied-access{queryString}");
            
            if (response.IsSuccess && response.Data != null)
            {
                var deniedCount = response.Data.Count;
                _logger.LogInformation("? {Count} accès refusés récupérés", deniedCount);
                
                if (deniedCount > 0)
                {
                    await _appState.ShowNotificationAsync($"?? {deniedCount} accès refusé(s) récent(s) détecté(s)", NotificationType.Warning);
                }
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération accès refusés: {Message}", ex.Message);
            return ApiResponse<List<RfidEventDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // SMOKE ALARMS
    // ==========================================

    public async Task<ApiResponse<SmokeAlarmEventDto>> RecordSmokeAlarmAsync(string iotDeviceId, RecordSmokeAlarmRequest request)
    {
        try
        {
            _logger.LogWarning("?? ALARME FUMÉE activée sur device {DeviceId}", iotDeviceId);
            
            var response = await _apiService.PostAsync<SmokeAlarmEventDto>($"/api/DeviceStatus/{iotDeviceId}/smoke-alarms", request);
            
            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync($"?? ALARME FUMÉE activée sur {iotDeviceId} - Zone {request.ZoneId}", NotificationType.Error);
                _logger.LogCritical("?? ALARME FUMÉE: Device={DeviceId}, Zone={ZoneId}", iotDeviceId, request.ZoneId);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur enregistrement alarme fumée {DeviceId}: {Message}", iotDeviceId, ex.Message);
            return ApiResponse<SmokeAlarmEventDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeactivateSmokeAlarmAsync(string iotDeviceId, DeactivateSmokeAlarmRequest request)
    {
        try
        {
            _logger.LogInformation("?? Désactivation alarme fumée device {DeviceId} par {User}", iotDeviceId, request.DeactivatedBy);
            
            var response = await _apiService.PostAsync<object>($"/api/DeviceStatus/{iotDeviceId}/smoke-alarms/deactivate", request);
            
            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync($"? Alarme fumée désactivée sur {iotDeviceId}", NotificationType.Success);
                _logger.LogInformation("? Alarme désactivée: Device={DeviceId}, Par={User}, Raison={Reason}", 
                    iotDeviceId, request.DeactivatedBy, request.Reason);
            }
            
            return ApiResponse.Success(response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur désactivation alarme {DeviceId}: {Message}", iotDeviceId, ex.Message);
            return ApiResponse.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<SmokeAlarmEventDto>>> GetSmokeAlarmsAsync(string iotDeviceId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            _logger.LogInformation("?? Récupération historique alarmes fumée device {DeviceId}", iotDeviceId);
            
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}");
            
            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var response = await _apiService.GetAsync<List<SmokeAlarmEventDto>>($"/api/DeviceStatus/{iotDeviceId}/smoke-alarms{queryString}");
            
            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? {Count} alarmes récupérées", response.Data.Count);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération alarmes {DeviceId}: {Message}", iotDeviceId, ex.Message);
            return ApiResponse<List<SmokeAlarmEventDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<SmokeAlarmEventDto>> GetActiveSmokeAlarmAsync(string iotDeviceId)
    {
        try
        {
            _logger.LogInformation("?? Vérification alarme active device {DeviceId}", iotDeviceId);
            
            var response = await _apiService.GetAsync<SmokeAlarmEventDto>($"/api/DeviceStatus/{iotDeviceId}/smoke-alarms/active");
            
            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogWarning("?? Alarme active détectée sur device {DeviceId}", iotDeviceId);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur vérification alarme active {DeviceId}: {Message}", iotDeviceId, ex.Message);
            return ApiResponse<SmokeAlarmEventDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<SmokeAlarmEventDto>>> GetAllActiveAlarmsAsync()
    {
        try
        {
            _logger.LogInformation("?? Récupération toutes les alarmes actives");
            
            var response = await _apiService.GetAsync<List<SmokeAlarmEventDto>>($"/api/DeviceStatus/smoke-alarms/all-active");
            
            if (response.IsSuccess && response.Data != null)
            {
                var activeCount = response.Data.Count;
                
                if (activeCount > 0)
                {
                    await _appState.ShowNotificationAsync($"?? {activeCount} alarme(s) fumée active(s)", NotificationType.Error);
                    _logger.LogCritical("?? {Count} ALARMES FUMÉE ACTIVES", activeCount);
                }
                else
                {
                    _logger.LogInformation("? Aucune alarme active");
                }
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération alarmes actives: {Message}", ex.Message);
            return ApiResponse<List<SmokeAlarmEventDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // MAINTENANCE
    // ==========================================

    public async Task<ApiResponse<int>> CleanupOldDataAsync(int retentionDays = 90)
    {
        try
        {
            _logger.LogInformation("?? Nettoyage données anciennes (rétention: {Days} jours)", retentionDays);
            
            var response = await _apiService.PostAsync<CleanupResponse>($"/api/DeviceStatus/cleanup?retentionDays={retentionDays}", null);
            
            if (response.IsSuccess && response.Data != null)
            {
                var deletedCount = response.Data.DeletedCount;
                _logger.LogInformation("? Cleanup terminé: {Count} enregistrements supprimés", deletedCount);
                await _appState.ShowNotificationAsync($"? Nettoyage terminé: {deletedCount} enregistrements supprimés", NotificationType.Success);
                
                return ApiResponse<int>.Success(deletedCount, response.StatusCode);
            }
            
            return ApiResponse<int>.Error("Cleanup échoué");
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur cleanup données: {Message}", ex.Message);
            return ApiResponse<int>.Error($"Erreur technique: {ex.Message}");
        }
    }
}

// ==========================================
// DTOs REQUEST
// ==========================================

/// <summary>
/// DTO pour enregistrer des métriques système
/// </summary>
public class RecordMetricsRequest
{
    public DateTime? Timestamp { get; set; }
    public int? Rssi { get; set; }
    public int? HeapFree { get; set; }
    public int? NvsQueueSize { get; set; }
    public long? UptimeSeconds { get; set; }
    public string? FirmwareVersion { get; set; }
    public string? FirmwareCommit { get; set; }
    public string? AdditionalData { get; set; }
}

/// <summary>
/// DTO pour enregistrer un événement RFID
/// </summary>
public class RecordRfidEventRequest
{
    public DateTime? Timestamp { get; set; }
    public required string CardUid { get; set; }
    public bool AccessGranted { get; set; }
    public string? DenialReason { get; set; }
    public string? UserId { get; set; }
    public string? Action { get; set; }
}

/// <summary>
/// DTO pour enregistrer une alarme fumée
/// </summary>
public class RecordSmokeAlarmRequest
{
    public DateTime? StartTime { get; set; }
    public int? ZoneId { get; set; }
}

/// <summary>
/// DTO pour désactiver une alarme fumée
/// </summary>
public class DeactivateSmokeAlarmRequest
{
    public string? DeactivatedBy { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// DTO pour réponse cleanup
/// </summary>
public class CleanupResponse
{
    public int DeletedCount { get; set; }
}
