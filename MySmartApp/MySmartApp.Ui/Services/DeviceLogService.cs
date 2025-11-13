using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

/// <summary>
/// Service pour gérer les logs système des devices IoT
/// Compatible avec DeviceLogsController backend
/// </summary>
public interface IDeviceLogService
{
    // Logs par device
    Task<ApiResponse<PaginatedResponse<DeviceLogDto>>> GetDeviceLogsAsync(
        string deviceId,
        int pageNumber = 1,
        int pageSize = 50,
        string? level = null,
        string? source = null,
        DateTime? startDate = null,
        DateTime? endDate = null);
    
    Task<ApiResponse<DeviceLogDto>> GetLogByIdAsync(int logId);
    
    // Logs par niveau
    Task<ApiResponse<PaginatedResponse<DeviceLogDto>>> GetLogsByLevelAsync(
        string level,
        int pageNumber = 1,
        int pageSize = 50,
        DateTime? startDate = null,
        DateTime? endDate = null);
    
    // Logs par source
    Task<ApiResponse<PaginatedResponse<DeviceLogDto>>> GetLogsBySourceAsync(
        string source,
        int pageNumber = 1,
        int pageSize = 50,
        DateTime? startDate = null,
        DateTime? endDate = null);
    
    // Logs critiques
    Task<ApiResponse<List<DeviceLogDto>>> GetCriticalLogsAsync(
        int hours = 24,
        int maxResults = 100);
    
    Task<ApiResponse<List<DeviceLogDto>>> GetErrorLogsAsync(
        int hours = 24,
        int maxResults = 100);
    
    // Enregistrement
    Task<ApiResponse<DeviceLogDto>> LogMessageAsync(
        string deviceId,
        string level,
        string source,
        string message,
        string? peripheral = null,
        string? additionalData = null);
    
    // Recherche
    Task<ApiResponse<List<DeviceLogDto>>> SearchLogsAsync(
        string searchTerm,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int maxResults = 100);
    
    // Statistiques
    Task<ApiResponse<DeviceLogStatsDto>> GetLogStatsAsync(
        string? deviceId = null,
        int days = 7);
    
    // Export
    Task<ApiResponse<byte[]>> ExportLogsAsync(
        string deviceId,
        DateTime startDate,
        DateTime endDate,
        string format = "csv");
    
    // Cleanup
    Task<ApiResponse<int>> CleanupOldLogsAsync(int retentionDays = 30);
}

public class DeviceLogService : IDeviceLogService
{
    private readonly IApiService _apiService;
    private readonly ILogger<DeviceLogService> _logger;
    private readonly IAppStateService _appState;

    public DeviceLogService(
        IApiService apiService,
        ILogger<DeviceLogService> logger,
        IAppStateService appState)
    {
        _apiService = apiService;
        _logger = logger;
        _appState = appState;
    }

    // ==========================================
    // LOGS PAR DEVICE
    // ==========================================

    public async Task<ApiResponse<PaginatedResponse<DeviceLogDto>>> GetDeviceLogsAsync(
        string deviceId,
        int pageNumber = 1,
        int pageSize = 50,
        string? level = null,
        string? source = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            _logger.LogInformation("?? Récupération logs device {DeviceId} - Page {Page}/{Size}",
                deviceId, pageNumber, pageSize);

            var queryParams = BuildQueryParams(pageNumber, pageSize, level, source, startDate, endDate);
            var response = await _apiService.GetAsync<PaginatedResponse<DeviceLogDto>>(
                $"/api/DeviceLogs/device/{deviceId}{queryParams}");

            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? {Count} logs récupérés (Total: {Total})",
                    response.Data.Data.Count, response.Data.TotalCount);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération logs device {DeviceId}: {Message}", deviceId, ex.Message);
            return ApiResponse<PaginatedResponse<DeviceLogDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<DeviceLogDto>> GetLogByIdAsync(int logId)
    {
        try
        {
            _logger.LogInformation("?? Récupération log {LogId}", logId);

            var response = await _apiService.GetAsync<DeviceLogDto>($"/api/DeviceLogs/{logId}");

            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? Log récupéré: {Level} - {Message}",
                    response.Data.Level, response.Data.Message);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération log {LogId}: {Message}", logId, ex.Message);
            return ApiResponse<DeviceLogDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // LOGS PAR NIVEAU
    // ==========================================

    public async Task<ApiResponse<PaginatedResponse<DeviceLogDto>>> GetLogsByLevelAsync(
        string level,
        int pageNumber = 1,
        int pageSize = 50,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            _logger.LogInformation("?? Récupération logs niveau {Level} - Page {Page}/{Size}",
                level, pageNumber, pageSize);

            var queryParams = BuildQueryParams(pageNumber, pageSize, null, null, startDate, endDate);
            var response = await _apiService.GetAsync<PaginatedResponse<DeviceLogDto>>(
                $"/api/DeviceLogs/level/{level}{queryParams}");

            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? {Count} logs niveau {Level} récupérés",
                    response.Data.Data.Count, level);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération logs niveau {Level}: {Message}", level, ex.Message);
            return ApiResponse<PaginatedResponse<DeviceLogDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // LOGS PAR SOURCE
    // ==========================================

    public async Task<ApiResponse<PaginatedResponse<DeviceLogDto>>> GetLogsBySourceAsync(
        string source,
        int pageNumber = 1,
        int pageSize = 50,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            _logger.LogInformation("?? Récupération logs source {Source} - Page {Page}/{Size}",
                source, pageNumber, pageSize);

            var queryParams = BuildQueryParams(pageNumber, pageSize, null, null, startDate, endDate);
            var response = await _apiService.GetAsync<PaginatedResponse<DeviceLogDto>>(
                $"/api/DeviceLogs/source/{Uri.EscapeDataString(source)}{queryParams}");

            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? {Count} logs source {Source} récupérés",
                    response.Data.Data.Count, source);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération logs source {Source}: {Message}", source, ex.Message);
            return ApiResponse<PaginatedResponse<DeviceLogDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // LOGS CRITIQUES
    // ==========================================

    public async Task<ApiResponse<List<DeviceLogDto>>> GetCriticalLogsAsync(int hours = 24, int maxResults = 100)
    {
        try
        {
            _logger.LogInformation("?? Récupération logs critiques sur {Hours}h (max: {Max})", hours, maxResults);

            var response = await _apiService.GetAsync<List<DeviceLogDto>>(
                $"/api/DeviceLogs/critical?hours={hours}&maxResults={maxResults}");

            if (response.IsSuccess && response.Data != null)
            {
                var criticalCount = response.Data.Count;
                _logger.LogInformation("? {Count} logs critiques récupérés", criticalCount);

                if (criticalCount > 0)
                {
                    await _appState.ShowNotificationAsync($"?? {criticalCount} log(s) critique(s) détecté(s)", NotificationType.Warning);
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération logs critiques: {Message}", ex.Message);
            return ApiResponse<List<DeviceLogDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<DeviceLogDto>>> GetErrorLogsAsync(int hours = 24, int maxResults = 100)
    {
        try
        {
            _logger.LogInformation("?? Récupération logs erreurs sur {Hours}h (max: {Max})", hours, maxResults);

            var response = await _apiService.GetAsync<List<DeviceLogDto>>(
                $"/api/DeviceLogs/errors?hours={hours}&maxResults={maxResults}");

            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? {Count} logs erreurs récupérés", response.Data.Count);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération logs erreurs: {Message}", ex.Message);
            return ApiResponse<List<DeviceLogDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // ENREGISTREMENT
    // ==========================================

    public async Task<ApiResponse<DeviceLogDto>> LogMessageAsync(
        string deviceId,
        string level,
        string source,
        string message,
        string? peripheral = null,
        string? additionalData = null)
    {
        try
        {
            _logger.LogInformation("?? Enregistrement log device {DeviceId}: [{Level}] {Message}",
                deviceId, level, message);

            var request = new LogMessageRequest
            {
                DeviceId = deviceId,
                Level = level,
                Source = source,
                Message = message,
                Peripheral = peripheral,
                AdditionalData = additionalData
            };

            var response = await _apiService.PostAsync<DeviceLogDto>("/api/DeviceLogs", request);

            if (response.IsSuccess)
            {
                _logger.LogInformation("? Log enregistré: ID={Id}", response.Data?.Id);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur enregistrement log: {Message}", ex.Message);
            return ApiResponse<DeviceLogDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // RECHERCHE
    // ==========================================

    public async Task<ApiResponse<List<DeviceLogDto>>> SearchLogsAsync(
        string searchTerm,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int maxResults = 100)
    {
        try
        {
            _logger.LogInformation("?? Recherche logs: '{SearchTerm}' (max: {Max})", searchTerm, maxResults);

            var queryParams = new List<string>
            {
                $"searchTerm={Uri.EscapeDataString(searchTerm)}",
                $"maxResults={maxResults}"
            };

            if (startDate.HasValue)
                queryParams.Add($"startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}");

            if (endDate.HasValue)
                queryParams.Add($"endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}");

            var queryString = "?" + string.Join("&", queryParams);
            var response = await _apiService.GetAsync<List<DeviceLogDto>>(
                $"/api/DeviceLogs/search{queryString}");

            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? {Count} logs trouvés", response.Data.Count);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur recherche logs: {Message}", ex.Message);
            return ApiResponse<List<DeviceLogDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // STATISTIQUES
    // ==========================================

    public async Task<ApiResponse<DeviceLogStatsDto>> GetLogStatsAsync(string? deviceId = null, int days = 7)
    {
        try
        {
            var logContext = deviceId != null ? $"device {deviceId}" : "global";
            _logger.LogInformation("?? Récupération stats logs {Context} sur {Days} jours", logContext, days);

            var queryParams = $"?days={days}";
            if (!string.IsNullOrEmpty(deviceId))
                queryParams += $"&deviceId={Uri.EscapeDataString(deviceId)}";

            var response = await _apiService.GetAsync<DeviceLogStatsDto>(
                $"/api/DeviceLogs/stats{queryParams}");

            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? Stats logs: {Total} total, {Errors} erreurs, {Critical} critiques",
                    response.Data.TotalLogs, response.Data.ErrorCount, response.Data.CriticalCount);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération stats logs: {Message}", ex.Message);
            return ApiResponse<DeviceLogStatsDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // EXPORT
    // ==========================================

    public async Task<ApiResponse<byte[]>> ExportLogsAsync(
        string deviceId,
        DateTime startDate,
        DateTime endDate,
        string format = "csv")
    {
        try
        {
            _logger.LogInformation("?? Export logs device {DeviceId} du {Start} au {End} (format: {Format})",
                deviceId, startDate.ToShortDateString(), endDate.ToShortDateString(), format);

            var queryParams = $"?startDate={startDate:yyyy-MM-ddTHH:mm:ss}&endDate={endDate:yyyy-MM-ddTHH:mm:ss}&format={format}";
            var response = await _apiService.GetAsync<byte[]>(
                $"/api/DeviceLogs/device/{deviceId}/export{queryParams}");

            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? Export généré: {Size} bytes", response.Data.Length);
                await _appState.ShowNotificationAsync($"Export généré avec succès ({response.Data.Length / 1024} KB)", NotificationType.Success);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur export logs: {Message}", ex.Message);
            return ApiResponse<byte[]>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // CLEANUP
    // ==========================================

    public async Task<ApiResponse<int>> CleanupOldLogsAsync(int retentionDays = 30)
    {
        try
        {
            _logger.LogInformation("?? Nettoyage logs anciens (rétention: {Days} jours)", retentionDays);

            var response = await _apiService.PostAsync<CleanupResponse>(
                $"/api/DeviceLogs/cleanup?retentionDays={retentionDays}", null);

            if (response.IsSuccess && response.Data != null)
            {
                var deletedCount = response.Data.DeletedCount;
                _logger.LogInformation("? Cleanup terminé: {Count} logs supprimés", deletedCount);
                await _appState.ShowNotificationAsync($"? {deletedCount} logs supprimés", NotificationType.Success);

                return ApiResponse<int>.Success(deletedCount, response.StatusCode);
            }

            return ApiResponse<int>.Error("Cleanup échoué");
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur cleanup logs: {Message}", ex.Message);
            return ApiResponse<int>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // HELPERS
    // ==========================================

    private static string BuildQueryParams(
        int pageNumber,
        int pageSize,
        string? level,
        string? source,
        DateTime? startDate,
        DateTime? endDate)
    {
        var queryParams = new List<string>
        {
            $"pageNumber={pageNumber}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrEmpty(level))
            queryParams.Add($"level={Uri.EscapeDataString(level)}");

        if (!string.IsNullOrEmpty(source))
            queryParams.Add($"source={Uri.EscapeDataString(source)}");

        if (startDate.HasValue)
            queryParams.Add($"startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}");

        if (endDate.HasValue)
            queryParams.Add($"endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}");

        return "?" + string.Join("&", queryParams);
    }
}

// ==========================================
// DTOs REQUEST
// ==========================================

/// <summary>
/// DTO pour enregistrer un log device
/// </summary>
public class LogMessageRequest
{
    public required string DeviceId { get; set; }
    public required string Level { get; set; }
    public required string Source { get; set; }
    public required string Message { get; set; }
    public string? Peripheral { get; set; }
    public string? AdditionalData { get; set; }
}

/// <summary>
/// DTO pour statistiques de logs
/// </summary>
public record DeviceLogStatsDto(
    int TotalLogs,
    int InfoCount,
    int WarningCount,
    int ErrorCount,
    int CriticalCount,
    Dictionary<string, int> LogsByLevel,
    Dictionary<string, int> LogsBySource,
    DateTime? LastLogTimestamp
);
