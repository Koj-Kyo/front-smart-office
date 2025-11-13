using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

/// <summary>
/// Service pour gérer l'audit trail et l'activité des utilisateurs
/// Compatible avec UserActivityController backend
/// </summary>
public interface IUserActivityService
{
    // Activités utilisateur
    Task<ApiResponse<PaginatedResponse<UserActivityDto>>> GetUserActivitiesAsync(
        string userId, 
        int pageNumber = 1, 
        int pageSize = 20,
        string? activityType = null,
        DateTime? startDate = null,
        DateTime? endDate = null);
    
    Task<ApiResponse<PaginatedResponse<UserActivityDto>>> GetAllActivitiesAsync(
        int pageNumber = 1,
        int pageSize = 50,
        string? activityType = null,
        string? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null);
    
    Task<ApiResponse<UserActivityDto>> GetActivityByIdAsync(int activityId);
    
    // Statistiques
    Task<ApiResponse<UserActivityStatsDto>> GetUserStatsAsync(string userId, int days = 30);
    Task<ApiResponse<UserActivityStatsDto>> GetGlobalStatsAsync(int days = 30);
    
    // Logs d'audit
    Task<ApiResponse<UserActivityDto>> LogActivityAsync(
        string activityType,
        string description,
        string? resourceType = null,
        int? resourceId = null,
        string? oldValue = null,
        string? newValue = null);
    
    // Recherche et export
    Task<ApiResponse<List<UserActivityDto>>> SearchActivitiesAsync(
        string searchTerm,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int maxResults = 100);
    
    Task<ApiResponse<byte[]>> ExportActivitiesAsync(
        string userId,
        DateTime startDate,
        DateTime endDate,
        string format = "csv");
    
    // Cleanup
    Task<ApiResponse<int>> CleanupOldActivitiesAsync(int retentionDays = 365);
}

public class UserActivityService : IUserActivityService
{
    private readonly IApiService _apiService;
    private readonly ILogger<UserActivityService> _logger;
    private readonly IAppStateService _appState;

    public UserActivityService(
        IApiService apiService,
        ILogger<UserActivityService> logger,
        IAppStateService appState)
    {
        _apiService = apiService;
        _logger = logger;
        _appState = appState;
    }

    // ==========================================
    // ACTIVITÉS UTILISATEUR
    // ==========================================

    public async Task<ApiResponse<PaginatedResponse<UserActivityDto>>> GetUserActivitiesAsync(
        string userId,
        int pageNumber = 1,
        int pageSize = 20,
        string? activityType = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            _logger.LogInformation("?? Récupération activités utilisateur {UserId} - Page {Page}/{Size}",
                userId, pageNumber, pageSize);

            var queryParams = BuildQueryParams(pageNumber, pageSize, activityType, null, startDate, endDate);
            var response = await _apiService.GetAsync<PaginatedResponse<UserActivityDto>>(
                $"/api/UserActivity/user/{userId}{queryParams}");

            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? {Count} activités récupérées (Total: {Total})",
                    response.Data.Data.Count, response.Data.TotalCount);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération activités utilisateur {UserId}: {Message}", userId, ex.Message);
            return ApiResponse<PaginatedResponse<UserActivityDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PaginatedResponse<UserActivityDto>>> GetAllActivitiesAsync(
        int pageNumber = 1,
        int pageSize = 50,
        string? activityType = null,
        string? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            _logger.LogInformation("?? Récupération toutes activités - Page {Page}/{Size}", pageNumber, pageSize);

            var queryParams = BuildQueryParams(pageNumber, pageSize, activityType, userId, startDate, endDate);
            var response = await _apiService.GetAsync<PaginatedResponse<UserActivityDto>>(
                $"/api/UserActivity{queryParams}");

            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? {Count} activités récupérées (Total: {Total})",
                    response.Data.Data.Count, response.Data.TotalCount);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération toutes activités: {Message}", ex.Message);
            return ApiResponse<PaginatedResponse<UserActivityDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<UserActivityDto>> GetActivityByIdAsync(int activityId)
    {
        try
        {
            _logger.LogInformation("?? Récupération activité {ActivityId}", activityId);

            var response = await _apiService.GetAsync<UserActivityDto>($"/api/UserActivity/{activityId}");

            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? Activité récupérée: {Type} - {Description}",
                    response.Data.ActivityType, response.Data.Description);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération activité {ActivityId}: {Message}", activityId, ex.Message);
            return ApiResponse<UserActivityDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // STATISTIQUES
    // ==========================================

    public async Task<ApiResponse<UserActivityStatsDto>> GetUserStatsAsync(string userId, int days = 30)
    {
        try
        {
            _logger.LogInformation("?? Récupération stats utilisateur {UserId} sur {Days} jours", userId, days);

            var response = await _apiService.GetAsync<UserActivityStatsDto>(
                $"/api/UserActivity/user/{userId}/stats?days={days}");

            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? Stats récupérées: {Total} activités, {Commandes} commandes",
                    response.Data.TotalActivities, response.Data.CommandesCount);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération stats utilisateur {UserId}: {Message}", userId, ex.Message);
            return ApiResponse<UserActivityStatsDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<UserActivityStatsDto>> GetGlobalStatsAsync(int days = 30)
    {
        try
        {
            _logger.LogInformation("?? Récupération stats globales sur {Days} jours", days);

            var response = await _apiService.GetAsync<UserActivityStatsDto>(
                $"/api/UserActivity/stats?days={days}");

            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? Stats globales: {Total} activités totales",
                    response.Data.TotalActivities);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur récupération stats globales: {Message}", ex.Message);
            return ApiResponse<UserActivityStatsDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // LOGS D'AUDIT
    // ==========================================

    public async Task<ApiResponse<UserActivityDto>> LogActivityAsync(
        string activityType,
        string description,
        string? resourceType = null,
        int? resourceId = null,
        string? oldValue = null,
        string? newValue = null)
    {
        try
        {
            _logger.LogInformation("?? Enregistrement activité: {Type} - {Description}", activityType, description);

            var request = new LogActivityRequest
            {
                ActivityType = activityType,
                Description = description,
                ResourceType = resourceType,
                ResourceId = resourceId,
                OldValue = oldValue,
                NewValue = newValue
            };

            var response = await _apiService.PostAsync<UserActivityDto>("/api/UserActivity", request);

            if (response.IsSuccess)
            {
                _logger.LogInformation("? Activité enregistrée: ID={Id}", response.Data?.Id);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur enregistrement activité: {Message}", ex.Message);
            return ApiResponse<UserActivityDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // RECHERCHE ET EXPORT
    // ==========================================

    public async Task<ApiResponse<List<UserActivityDto>>> SearchActivitiesAsync(
        string searchTerm,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int maxResults = 100)
    {
        try
        {
            _logger.LogInformation("?? Recherche activités: '{SearchTerm}' (max: {Max})", searchTerm, maxResults);

            var queryParams = new List<string> { $"searchTerm={Uri.EscapeDataString(searchTerm)}", $"maxResults={maxResults}" };
            if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}");

            var queryString = "?" + string.Join("&", queryParams);
            var response = await _apiService.GetAsync<List<UserActivityDto>>(
                $"/api/UserActivity/search{queryString}");

            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? {Count} activités trouvées", response.Data.Count);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur recherche activités: {Message}", ex.Message);
            return ApiResponse<List<UserActivityDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<byte[]>> ExportActivitiesAsync(
        string userId,
        DateTime startDate,
        DateTime endDate,
        string format = "csv")
    {
        try
        {
            _logger.LogInformation("?? Export activités utilisateur {UserId} du {Start} au {End} (format: {Format})",
                userId, startDate.ToShortDateString(), endDate.ToShortDateString(), format);

            var queryParams = $"?startDate={startDate:yyyy-MM-ddTHH:mm:ss}&endDate={endDate:yyyy-MM-ddTHH:mm:ss}&format={format}";
            var response = await _apiService.GetAsync<byte[]>(
                $"/api/UserActivity/user/{userId}/export{queryParams}");

            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? Export généré: {Size} bytes", response.Data.Length);
                await _appState.ShowNotificationAsync($"Export généré avec succès ({response.Data.Length / 1024} KB)", NotificationType.Success);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur export activités: {Message}", ex.Message);
            return ApiResponse<byte[]>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // CLEANUP
    // ==========================================

    public async Task<ApiResponse<int>> CleanupOldActivitiesAsync(int retentionDays = 365)
    {
        try
        {
            _logger.LogInformation("?? Nettoyage activités anciennes (rétention: {Days} jours)", retentionDays);

            var response = await _apiService.PostAsync<CleanupResponse>(
                $"/api/UserActivity/cleanup?retentionDays={retentionDays}", null);

            if (response.IsSuccess && response.Data != null)
            {
                var deletedCount = response.Data.DeletedCount;
                _logger.LogInformation("? Cleanup terminé: {Count} activités supprimées", deletedCount);
                await _appState.ShowNotificationAsync($"? {deletedCount} activités supprimées", NotificationType.Success);

                return ApiResponse<int>.Success(deletedCount, response.StatusCode);
            }

            return ApiResponse<int>.Error("Cleanup échoué");
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur cleanup activités: {Message}", ex.Message);
            return ApiResponse<int>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // HELPERS
    // ==========================================

    private static string BuildQueryParams(
        int pageNumber,
        int pageSize,
        string? activityType,
        string? userId,
        DateTime? startDate,
        DateTime? endDate)
    {
        var queryParams = new List<string>
        {
            $"pageNumber={pageNumber}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrEmpty(activityType))
            queryParams.Add($"activityType={Uri.EscapeDataString(activityType)}");

        if (!string.IsNullOrEmpty(userId))
            queryParams.Add($"userId={Uri.EscapeDataString(userId)}");

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
/// DTO pour enregistrer une activité utilisateur
/// </summary>
public class LogActivityRequest
{
    public required string ActivityType { get; set; }
    public required string Description { get; set; }
    public string? ResourceType { get; set; }
    public int? ResourceId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}
