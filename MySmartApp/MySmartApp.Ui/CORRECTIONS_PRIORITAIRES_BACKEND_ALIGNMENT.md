# ? CORRECTIONS PRIORITAIRES À APPLIQUER

**Date:** 2025-01-30  
**Objectif:** Aligner le frontend Blazor WASM avec les contrôleurs backend

---

## ?? PHASE 1 - CORRECTIONS CRITIQUES (À FAIRE IMMÉDIATEMENT)

### 1.1 ? Ajouter support `?fresh=true` dans CommandeService

**Fichier:** `Services/CommandeService.cs`

```csharp
// Ajouter ces nouvelles méthodes

public async Task<ApiResponse<IEnumerable<CommandeResponseDto>>> GetMyCommandsAsync(
    bool fresh = false, 
    int limit = 50, 
    int offset = 0)
{
    try
    {
        var queryParams = new List<string>
        {
            $"limit={limit}",
            $"offset={offset}"
        };
        
        if (fresh)
        {
            queryParams.Add("fresh=true");
        }
        
        var queryString = string.Join("&", queryParams);
        var response = await _apiService.GetAsync<IEnumerable<CommandeResponseDto>>($"/api/Commandes/my-commands?{queryString}");
        
        return response;
    }
    catch (Exception ex)
    {
        _logger.LogError("? Erreur récupération commandes: {Message}", ex.Message);
        return ApiResponse<IEnumerable<CommandeResponseDto>>.Error($"Erreur: {ex.Message}");
    }
}

public async Task<ApiResponse<IEnumerable<CommandeResponseDto>>> GetCommandsByPeripheriqueAsync(
    int peripheriqueId,
    bool fresh = false,
    int limit = 50)
{
    try
    {
        var queryParams = new List<string>
        {
            $"limit={limit}"
        };
        
        if (fresh)
        {
            queryParams.Add("fresh=true");
        }
        
        var queryString = string.Join("&", queryParams);
        var response = await _apiService.GetAsync<IEnumerable<CommandeResponseDto>>($"/api/Commandes/by-peripherique/{peripheriqueId}?{queryString}");
        
        return response;
    }
    catch (Exception ex)
    {
        _logger.LogError("? Erreur récupération commandes périphérique: {Message}", ex.Message);
        return ApiResponse<IEnumerable<CommandeResponseDto>>.Error($"Erreur: {ex.Message}");
    }
}
```

---

### 1.2 ? Créer DeviceStatusService COMPLET

**Nouveau fichier:** `Services/DeviceStatusService.cs`

```csharp
using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

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

    // STATUT COMPLET
    public async Task<ApiResponse<DeviceFullStatusDto>> GetFullDeviceStatusAsync(string iotDeviceId)
    {
        try
        {
            _logger.LogInformation("?? Récupération statut complet device {DeviceId}", iotDeviceId);
            var response = await _apiService.GetAsync<DeviceFullStatusDto>($"/api/DeviceStatus/{iotDeviceId}/full-status");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération statut: {Message}", ex.Message);
            return ApiResponse<DeviceFullStatusDto>.Error($"Erreur: {ex.Message}");
        }
    }

    // MÉTRIQUES SYSTÈME
    public async Task<ApiResponse<DeviceMetricsDto>> RecordMetricsAsync(string iotDeviceId, RecordMetricsRequest request)
    {
        try
        {
            var response = await _apiService.PostAsync<DeviceMetricsDto>($"/api/DeviceStatus/{iotDeviceId}/metrics", request);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur enregistrement métriques: {Message}", ex.Message);
            return ApiResponse<DeviceMetricsDto>.Error($"Erreur: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<DeviceMetricsDto>>> GetMetricsAsync(string iotDeviceId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}");
            
            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var response = await _apiService.GetAsync<List<DeviceMetricsDto>>($"/api/DeviceStatus/{iotDeviceId}/metrics{queryString}");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération métriques: {Message}", ex.Message);
            return ApiResponse<List<DeviceMetricsDto>>.Error($"Erreur: {ex.Message}");
        }
    }

    public async Task<ApiResponse<DeviceMetricsDto>> GetLatestMetricsAsync(string iotDeviceId)
    {
        try
        {
            var response = await _apiService.GetAsync<DeviceMetricsDto>($"/api/DeviceStatus/{iotDeviceId}/metrics/latest");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération dernières métriques: {Message}", ex.Message);
            return ApiResponse<DeviceMetricsDto>.Error($"Erreur: {ex.Message}");
        }
    }

    // RFID EVENTS
    public async Task<ApiResponse<RfidEventDto>> RecordRfidEventAsync(string iotDeviceId, RecordRfidEventRequest request)
    {
        try
        {
            var response = await _apiService.PostAsync<RfidEventDto>($"/api/DeviceStatus/{iotDeviceId}/rfid-events", request);
            
            if (response.IsSuccess && !request.AccessGranted)
            {
                await _appState.ShowNotificationAsync($"?? Accès refusé: {request.CardUid}", NotificationType.Warning);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur enregistrement événement RFID: {Message}", ex.Message);
            return ApiResponse<RfidEventDto>.Error($"Erreur: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<RfidEventDto>>> GetRfidEventsAsync(string iotDeviceId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}");
            
            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var response = await _apiService.GetAsync<List<RfidEventDto>>($"/api/DeviceStatus/{iotDeviceId}/rfid-events{queryString}");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération événements RFID: {Message}", ex.Message);
            return ApiResponse<List<RfidEventDto>>.Error($"Erreur: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<RfidEventDto>>> GetRfidEventsByCardAsync(string cardUid, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}");
            
            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var response = await _apiService.GetAsync<List<RfidEventDto>>($"/api/DeviceStatus/rfid-events/by-card/{cardUid}{queryString}");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération événements carte: {Message}", ex.Message);
            return ApiResponse<List<RfidEventDto>>.Error($"Erreur: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<RfidEventDto>>> GetDeniedAccessAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}");
            
            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var response = await _apiService.GetAsync<List<RfidEventDto>>($"/api/DeviceStatus/rfid-events/denied-access{queryString}");
            
            if (response.IsSuccess && response.Data != null && response.Data.Count > 0)
            {
                await _appState.ShowNotificationAsync($"?? {response.Data.Count} accès refusé(s) récent(s)", NotificationType.Warning);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération accès refusés: {Message}", ex.Message);
            return ApiResponse<List<RfidEventDto>>.Error($"Erreur: {ex.Message}");
        }
    }

    // SMOKE ALARMS
    public async Task<ApiResponse<SmokeAlarmEventDto>> RecordSmokeAlarmAsync(string iotDeviceId, RecordSmokeAlarmRequest request)
    {
        try
        {
            var response = await _apiService.PostAsync<SmokeAlarmEventDto>($"/api/DeviceStatus/{iotDeviceId}/smoke-alarms", request);
            
            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync($"?? ALARME FUMÉE activée sur {iotDeviceId}", NotificationType.Error);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur enregistrement alarme: {Message}", ex.Message);
            return ApiResponse<SmokeAlarmEventDto>.Error($"Erreur: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeactivateSmokeAlarmAsync(string iotDeviceId, DeactivateSmokeAlarmRequest request)
    {
        try
        {
            var response = await _apiService.PostAsync<object>($"/api/DeviceStatus/{iotDeviceId}/smoke-alarms/deactivate", request);
            
            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync($"? Alarme désactivée sur {iotDeviceId}", NotificationType.Success);
            }
            
            return ApiResponse.Success(response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur désactivation alarme: {Message}", ex.Message);
            return ApiResponse.Error($"Erreur: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<SmokeAlarmEventDto>>> GetSmokeAlarmsAsync(string iotDeviceId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}");
            
            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var response = await _apiService.GetAsync<List<SmokeAlarmEventDto>>($"/api/DeviceStatus/{iotDeviceId}/smoke-alarms{queryString}");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération alarmes: {Message}", ex.Message);
            return ApiResponse<List<SmokeAlarmEventDto>>.Error($"Erreur: {ex.Message}");
        }
    }

    public async Task<ApiResponse<SmokeAlarmEventDto>> GetActiveSmokeAlarmAsync(string iotDeviceId)
    {
        try
        {
            var response = await _apiService.GetAsync<SmokeAlarmEventDto>($"/api/DeviceStatus/{iotDeviceId}/smoke-alarms/active");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération alarme active: {Message}", ex.Message);
            return ApiResponse<SmokeAlarmEventDto>.Error($"Erreur: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<SmokeAlarmEventDto>>> GetAllActiveAlarmsAsync()
    {
        try
        {
            var response = await _apiService.GetAsync<List<SmokeAlarmEventDto>>($"/api/DeviceStatus/smoke-alarms/all-active");
            
            if (response.IsSuccess && response.Data != null && response.Data.Count > 0)
            {
                await _appState.ShowNotificationAsync($"?? {response.Data.Count} alarme(s) active(s)", NotificationType.Error);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération toutes les alarmes: {Message}", ex.Message);
            return ApiResponse<List<SmokeAlarmEventDto>>.Error($"Erreur: {ex.Message}");
        }
    }

    // MAINTENANCE
    public async Task<ApiResponse<int>> CleanupOldDataAsync(int retentionDays = 90)
    {
        try
        {
            var response = await _apiService.PostAsync<CleanupResponse>($"/api/DeviceStatus/cleanup?retentionDays={retentionDays}", null);
            
            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? Cleanup: {Count} enregistrements supprimés", response.Data.DeletedCount);
                return ApiResponse<int>.Success(response.Data.DeletedCount, response.StatusCode);
            }
            
            return ApiResponse<int>.Error("Cleanup échoué");
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur cleanup: {Message}", ex.Message);
            return ApiResponse<int>.Error($"Erreur: {ex.Message}");
        }
    }
}

// DTOs Request nécessaires
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

public class RecordRfidEventRequest
{
    public DateTime? Timestamp { get; set; }
    public required string CardUid { get; set; }
    public bool AccessGranted { get; set; }
    public string? DenialReason { get; set; }
    public string? UserId { get; set; }
    public string? Action { get; set; }
}

public class RecordSmokeAlarmRequest
{
    public DateTime? StartTime { get; set; }
    public int? ZoneId { get; set; }
}

public class DeactivateSmokeAlarmRequest
{
    public string? DeactivatedBy { get; set; }
    public string? Reason { get; set; }
}

public class CleanupResponse
{
    public int DeletedCount { get; set; }
}
```

---

### 1.3 ? Ajouter DTOs Manquants dans ApiModels.cs

**Fichier:** `Models/ApiModels.cs`

Ajouter à la fin du fichier :

```csharp
// ==========================================
// DTOs DeviceStatus (Nouveau)
// ==========================================

public record DeviceMetricsDto(
    int Id,
    string IotDeviceId,
    DateTime Timestamp,
    int? Rssi,
    int? HeapFree,
    int? NvsQueueSize,
    long? UptimeSeconds,
    string? FirmwareVersion,
    string? FirmwareCommit,
    string? AdditionalData
);

public record RfidEventDto(
    int Id,
    string IotDeviceId,
    DateTime Timestamp,
    string CardUid,
    bool AccessGranted,
    string? DenialReason,
    string? UserId,
    string? Action
);

public record SmokeAlarmEventDto(
    int Id,
    string IotDeviceId,
    DateTime StartTime,
    DateTime? EndTime,
    bool IsActive,
    int? ZoneId,
    string? ZoneName,
    string? DeactivatedBy,
    string? DeactivationReason
);

public record DeviceFullStatusDto(
    string IotDeviceId,
    DeviceMetricsDto? LatestMetrics,
    List<RfidEventDto>? RecentRfidEvents,
    SmokeAlarmEventDto? ActiveAlarm,
    List<PeripheralDto>? Peripherals,
    DateTime RetrievedAt
);

// ==========================================
// DTOs DeviceLogs (Nouveau)
// ==========================================

public record DeviceLogDto(
    int Id,
    string DeviceId,
    DateTime Timestamp,
    string Level,
    string Source,
    string Message,
    string? Peripheral,
    string? AdditionalData
);

// ==========================================
// DTOs UserActivity (Nouveau)
// ==========================================

public record UserActivityDto(
    int Id,
    string UserId,
    string ActivityType,
    string Description,
    DateTime Timestamp,
    string? IpAddress,
    string? UserAgent,
    string? ResourceType,
    int? ResourceId,
    string? OldValue,
    string? NewValue
);

public record UserActivityStatsDto(
    int TotalActivities,
    int CommandesCount,
    int AuditsCount,
    DateTime? LastActivity,
    Dictionary<string, int> ActivitiesByType,
    List<UserActivityDto> RecentActivities
);

// ==========================================
// DTOs Pagination (Nouveau)
// ==========================================

public record PaginatedResponse<T>(
    List<T> Data,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages,
    bool HasPrevious,
    bool HasNext
);

// ==========================================
// Support ApiResponse Étendu
// ==========================================

public record ApiResponse
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public int StatusCode { get; init; }

    public static ApiResponse Success(int statusCode = 200) =>
        new() { IsSuccess = true, StatusCode = statusCode };

    public static ApiResponse Error(string errorMessage, int statusCode = 400) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage, StatusCode = statusCode };
}
```

---

## ?? PHASE 2 - CORRECTIONS IMPORTANTES (Cette semaine)

### 2.1 ? Compléter DroitAccesService

**Fichier:** `Services/DroitAccesService.cs`

Ajouter ces méthodes :

```csharp
// ADMIN ENDPOINTS
public async Task<ApiResponse<DroitAccesDto>> AssignZoneToUserAsync(string userId, int zoneId, DroitAccesType accessType)
{
    try
    {
        var payload = new
        {
            UserId = userId,
            ZoneId = zoneId,
            AccessType = (int)accessType
        };
        
        var response = await _apiService.PostAsync<DroitAccesDto>("/api/admin/assign-zone-to-user", payload);
        
        if (response.IsSuccess)
        {
            await _appState.ShowNotificationAsync("? Zone affectée à l'utilisateur", NotificationType.Success);
        }
        
        return response;
    }
    catch (Exception ex)
    {
        _logger.LogError("? Erreur assignation zone: {Message}", ex.Message);
        return ApiResponse<DroitAccesDto>.Error($"Erreur: {ex.Message}");
    }
}

public async Task<ApiResponse<DroitAccesDto>> AssignDeviceToUserAsync(string userId, int peripheriqueLogiqueId, DroitAccesType accessType)
{
    try
    {
        var payload = new
        {
            UserId = userId,
            PeripheriqueLogiqueId = peripheriqueLogiqueId,
            AccessType = (int)accessType
        };
        
        var response = await _apiService.PostAsync<DroitAccesDto>("/api/admin/assign-device-to-user", payload);
        
        if (response.IsSuccess)
        {
            await _appState.ShowNotificationAsync("? Périphérique affecté à l'utilisateur", NotificationType.Success);
        }
        
        return response;
    }
    catch (Exception ex)
    {
        _logger.LogError("? Erreur assignation périphérique: {Message}", ex.Message);
        return ApiResponse<DroitAccesDto>.Error($"Erreur: {ex.Message}");
    }
}

public async Task<ApiResponse<List<DroitAccesDto>>> GetAllAccessRightsAsync()
{
    try
    {
        var response = await _apiService.GetAsync<List<DroitAccesDto>>("/api/admin/access-rights");
        return response;
    }
    catch (Exception ex)
    {
        _logger.LogError("? Erreur récupération droits: {Message}", ex.Message);
        return ApiResponse<List<DroitAccesDto>>.Error($"Erreur: {ex.Message}");
    }
}

public async Task<ApiResponse> DeleteAccessRightAsync(int accessRightId)
{
    try
    {
        var response = await _apiService.DeleteAsync($"/api/admin/access-rights/{accessRightId}");
        
        if (response.IsSuccess)
        {
            await _appState.ShowNotificationAsync("? Droit d'accès supprimé", NotificationType.Success);
        }
        
        return response;
    }
    catch (Exception ex)
    {
        _logger.LogError("? Erreur suppression droit: {Message}", ex.Message);
        return ApiResponse.Error($"Erreur: {ex.Message}");
    }
}
```

---

### 2.2 ? Créer UserActivityService

**Nouveau fichier:** `Services/UserActivityService.cs`

```csharp
using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public interface IUserActivityService
{
    Task<ApiResponse<PaginatedResponse<UserActivityDto>>> GetUserActivitiesAsync(
        string userId,
        int pageNumber = 1,
        int pageSize = 20,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? activityType = null);
    
    Task<ApiResponse<UserActivityStatsDto>> GetUserActivityStatsAsync(
        string userId,
        DateTime? startDate = null,
        DateTime? endDate = null);
    
    Task<ApiResponse<List<UserActivityDto>>> GetRecentActivityAsync(string userId);
}

public class UserActivityService : IUserActivityService
{
    private readonly IApiService _apiService;
    private readonly ILogger<UserActivityService> _logger;

    public UserActivityService(IApiService apiService, ILogger<UserActivityService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<ApiResponse<PaginatedResponse<UserActivityDto>>> GetUserActivitiesAsync(
        string userId,
        int pageNumber = 1,
        int pageSize = 20,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? activityType = null)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };
            
            if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}");
            if (!string.IsNullOrEmpty(activityType)) queryParams.Add($"activityType={activityType}");
            
            var queryString = string.Join("&", queryParams);
            
            // Note: Le backend retourne un objet avec { activities, pagination }
            // On doit adapter la réponse
            var response = await _apiService.GetAsync<object>($"/api/user/activity/history?{queryString}");
            
            if (response.IsSuccess && response.Data != null)
            {
                // TODO: Parser la réponse complexe en PaginatedResponse<UserActivityDto>
                // Pour l'instant, on simule
                var paginatedResponse = new PaginatedResponse<UserActivityDto>(
                    new List<UserActivityDto>(),
                    0, pageNumber, pageSize, 0, false, false
                );
                
                return ApiResponse<PaginatedResponse<UserActivityDto>>.Success(paginatedResponse);
            }
            
            return ApiResponse<PaginatedResponse<UserActivityDto>>.Error(response.ErrorMessage ?? "Erreur inconnue");
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération activités: {Message}", ex.Message);
            return ApiResponse<PaginatedResponse<UserActivityDto>>.Error($"Erreur: {ex.Message}");
        }
    }

    public async Task<ApiResponse<UserActivityStatsDto>> GetUserActivityStatsAsync(
        string userId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}");
            
            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var response = await _apiService.GetAsync<UserActivityStatsDto>($"/api/user/activity/stats{queryString}");
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération stats activités: {Message}", ex.Message);
            return ApiResponse<UserActivityStatsDto>.Error($"Erreur: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<UserActivityDto>>> GetRecentActivityAsync(string userId)
    {
        try
        {
            var response = await _apiService.GetAsync<List<UserActivityDto>>("/api/user/activity/recent");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération activités récentes: {Message}", ex.Message);
            return ApiResponse<List<UserActivityDto>>.Error($"Erreur: {ex.Message}");
        }
    }
}
```

---

## ?? PHASE 3 - AMÉLIORATIONS (Sprint suivant)

### 3.1 Compléter UserManagementService
- Ajouter méthode `SearchUsersAsync(string searchTerm)`
- Ajouter méthode `ToggleUserActiveStatusAsync(string userId)`
- Ajouter méthode `GetUserStatisticsAsync()`

### 3.2 Créer DeviceLogService
- Tous les endpoints du contrôleur DeviceLogsController

### 3.3 Étendre IoTHubService
- Ajouter méthodes ESP32 firmware
- Smart commands
- Peripherals sync

---

## ?? ENREGISTREMENT DES SERVICES DANS Program.cs

**Fichier:** `Program.cs`

Ajouter dans la section `// Services` :

```csharp
// ? NOUVEAUX SERVICES CRITIQUES
builder.Services.AddScoped<IDeviceStatusService, DeviceStatusService>();
builder.Services.AddScoped<IUserActivityService, UserActivityService>();
// builder.Services.AddScoped<IDeviceLogService, DeviceLogService>(); // À créer Phase 3
```

---

## ?? TESTS À EFFECTUER APRÈS CHAQUE CORRECTION

1. ? Appel API avec Postman
2. ? Test `?fresh=true` vs cache
3. ? Test gestion erreurs 401/403/404/500
4. ? Test notifications UI
5. ? Test intégration pages

---

## ?? PRIORISATION

| Service | Criticité | Temps estimé | Dépendances |
|---------|-----------|--------------|-------------|
| DeviceStatusService | ?? CRITIQUE | 2h | DTOs nouveaux |
| Support fresh=true | ?? CRITIQUE | 30min | Aucune |
| DroitAccesService | ?? IMPORTANT | 1h | Aucune |
| UserActivityService | ?? IMPORTANT | 1h30 | DTOs nouveaux |
| DeviceLogService | ?? MOYEN | 1h | DTOs nouveaux |

---

**Total estimé Phase 1:** 4h  
**Total estimé Phase 2:** 3h  
**Total estimé Phase 3:** 5h

---

? **PROCHAINE ÉTAPE:** Commencer par appliquer les corrections de la Phase 1.
