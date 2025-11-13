using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public interface INotificationService
{
    // Gestion des notifications utilisateur
    Task<ApiResponse<List<UserNotificationDto>>> GetUserNotificationsAsync();
    Task<ApiResponse> MarkAsReadAsync(int notificationId);
    Task<ApiResponse> MarkAllAsReadAsync();
    Task<ApiResponse> DeleteNotificationAsync(int notificationId);
    
    // M�thodes utilitaires
    Task<ApiResponse<NotificationStatsDto>> GetNotificationStatsAsync();
    Task<ApiResponse<List<UserNotificationDto>>> GetUnreadNotificationsAsync();
    Task<int> GetUnreadCountAsync();
    
    // �v�nements pour mise � jour temps r�el
    event Action<List<UserNotificationDto>>? NotificationsUpdated;
    event Action<int>? UnreadCountChanged;
    event Action<UserNotificationDto>? NewNotificationReceived;
    
    // M�thodes utilitaires
    void InvalidateCache();
}

public class NotificationService : INotificationService
{
    private readonly IApiService _apiService;
    private readonly ILogger<NotificationService> _logger;
    private readonly IAppStateService _appState;
    
    // Cache local
    private List<UserNotificationDto>? _cachedNotifications;
    private DateTime? _cacheExpiry;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);

    // �v�nements
    public event Action<List<UserNotificationDto>>? NotificationsUpdated;
    public event Action<int>? UnreadCountChanged;
    public event Action<UserNotificationDto>? NewNotificationReceived;

    public NotificationService(
        IApiService apiService,
        ILogger<NotificationService> logger,
        IAppStateService appState)
    {
        _apiService = apiService;
        _logger = logger;
        _appState = appState;
    }

    // ==========================================
    // GESTION DES NOTIFICATIONS UTILISATEUR
    // ==========================================

    public async Task<ApiResponse<List<UserNotificationDto>>> GetUserNotificationsAsync()
    {
        try
        {
            // V�rifier le cache
            if (IsCacheValid() && _cachedNotifications != null)
            {
                _logger.LogDebug("?? Notifications r�cup�r�es depuis le cache");
                return ApiResponse<List<UserNotificationDto>>.Success(_cachedNotifications);
            }

            _logger.LogInformation("?? R�cup�ration des notifications utilisateur");
            
            var response = await _apiService.GetAsync<List<UserNotificationDto>>("/api/Notification");
            
            if (response.IsSuccess && response.Data != null)
            {
                // Trier par date (plus r�cent en premier)
                _cachedNotifications = response.Data.OrderByDescending(n => n.Horodatage).ToList();
                _cacheExpiry = DateTime.Now.Add(_cacheTimeout);
                
                _logger.LogInformation("? {Count} notifications r�cup�r�es", _cachedNotifications.Count);
                
            // D�clencher l'�v�nement de mise � jour
            NotificationsUpdated?.Invoke(_cachedNotifications);
            UnreadCountChanged?.Invoke(_cachedNotifications.Count(n => !n.EstLue));
                
                return ApiResponse<List<UserNotificationDto>>.Success(_cachedNotifications);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des notifications: {Message}", ex.Message);
            return ApiResponse<List<UserNotificationDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse> MarkAsReadAsync(int notificationId)
    {
        try
        {
            _logger.LogInformation("??? Marquage notification {Id} comme lue", notificationId);
            
            var response = await _apiService.PutAsync<object>($"/api/Notification/{notificationId}", new { });
            
            if (response.IsSuccess)
            {
                // Mettre � jour le cache local
                if (_cachedNotifications != null)
                {
                    var notification = _cachedNotifications.FirstOrDefault(n => n.Id == notificationId);
                    if (notification != null)
                    {
                        // Cr�er une nouvelle instance avec EstLue = true
                        var updatedNotification = notification with { EstLue = true };
                        var index = _cachedNotifications.IndexOf(notification);
                        _cachedNotifications[index] = updatedNotification;
                        
                        // D�clencher les �v�nements
                        NotificationsUpdated?.Invoke(_cachedNotifications);
                        UnreadCountChanged?.Invoke(_cachedNotifications.Count(n => !n.EstLue));
                    }
                }
                
                _logger.LogInformation("? Notification {Id} marqu�e comme lue", notificationId);
                return ApiResponse.Success(response.StatusCode);
            }
            
            return ApiResponse.Error(response.ErrorMessage ?? "Erreur lors du marquage", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors du marquage de la notification {Id}: {Message}", notificationId, ex.Message);
            return ApiResponse.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse> MarkAllAsReadAsync()
    {
        try
        {
            if (_cachedNotifications == null)
            {
                await GetUserNotificationsAsync();
            }

            if (_cachedNotifications == null || !_cachedNotifications.Any(n => !n.EstLue))
            {
                return ApiResponse.Success();
            }

            _logger.LogInformation("??? Marquage de toutes les notifications comme lues");
            
            var unreadNotifications = _cachedNotifications.Where(n => !n.EstLue).ToList();
            var tasks = unreadNotifications.Select(n => MarkAsReadAsync(n.Id));
            
            var results = await Task.WhenAll(tasks);
            var successCount = results.Count(r => r.IsSuccess);
            
            _logger.LogInformation("? {Success}/{Total} notifications marqu�es comme lues", 
                successCount, unreadNotifications.Count);
            
            if (successCount == unreadNotifications.Count)
            {
                await _appState.ShowNotificationAsync("Toutes les notifications ont �t� marqu�es comme lues", NotificationType.Success);
                return ApiResponse.Success();
            }
            else
            {
                await _appState.ShowNotificationAsync($"Seulement {successCount}/{unreadNotifications.Count} notifications marqu�es", NotificationType.Warning);
                return ApiResponse.Error($"�chec partiel: {successCount}/{unreadNotifications.Count}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors du marquage global: {Message}", ex.Message);
            return ApiResponse.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeleteNotificationAsync(int notificationId)
    {
        try
        {
            _logger.LogInformation("??? Suppression de la notification {Id}", notificationId);
            
            // Note: L'API backend ne semble pas avoir d'endpoint DELETE, 
            // mais on peut simuler en marquant comme lue et en supprimant du cache local
            var markResult = await MarkAsReadAsync(notificationId);
            
            if (markResult.IsSuccess && _cachedNotifications != null)
            {
                _cachedNotifications.RemoveAll(n => n.Id == notificationId);
                NotificationsUpdated?.Invoke(_cachedNotifications);
                UnreadCountChanged?.Invoke(_cachedNotifications.Count(n => !n.EstLue));
                
                await _appState.ShowNotificationAsync("Notification supprim�e", NotificationType.Success);
                return ApiResponse.Success();
            }
            
            return markResult;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la suppression de la notification {Id}: {Message}", notificationId, ex.Message);
            return ApiResponse.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // M�THODES UTILITAIRES
    // ==========================================

    public async Task<ApiResponse<NotificationStatsDto>> GetNotificationStatsAsync()
    {
        try
        {
            var notificationsResponse = await GetUserNotificationsAsync();
            
            if (!notificationsResponse.IsSuccess || notificationsResponse.Data == null)
            {
                return ApiResponse<NotificationStatsDto>.Error("Impossible de r�cup�rer les notifications");
            }

            var notifications = notificationsResponse.Data;
            var stats = new NotificationStatsDto(
                TotalNotifications: notifications.Count,
                NotificationsNonLues: notifications.Count(n => !n.EstLue),
                NotificationsLues: notifications.Count(n => n.EstLue),
                DerniereNotification: notifications.FirstOrDefault()?.Horodatage
            );

            return ApiResponse<NotificationStatsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors du calcul des statistiques: {Message}", ex.Message);
            return ApiResponse<NotificationStatsDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<UserNotificationDto>>> GetUnreadNotificationsAsync()
    {
        try
        {
            var allNotificationsResponse = await GetUserNotificationsAsync();
            
            if (!allNotificationsResponse.IsSuccess || allNotificationsResponse.Data == null)
            {
                return allNotificationsResponse;
            }

            var unreadNotifications = allNotificationsResponse.Data
                .Where(n => !n.EstLue)
                .ToList();

            return ApiResponse<List<UserNotificationDto>>.Success(unreadNotifications);
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des notifications non lues: {Message}", ex.Message);
            return ApiResponse<List<UserNotificationDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<int> GetUnreadCountAsync()
    {
        try
        {
            var unreadResponse = await GetUnreadNotificationsAsync();
            return unreadResponse.IsSuccess && unreadResponse.Data != null 
                ? unreadResponse.Data.Count 
                : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors du comptage des notifications non lues: {Message}", ex.Message);
            return 0;
        }
    }

    // ==========================================
    // M�THODES PUBLIQUES POUR SIGNALR
    // ==========================================

    /// <summary>
    /// M�thode appel�e par SignalR lors de la r�ception d'une nouvelle notification
    /// </summary>
    public void OnNewNotificationReceived(UserNotificationDto notification)
    {
        try
        {
            _logger.LogInformation("?? Nouvelle notification re�ue: {Titre}", notification.Titre);
            
            // Ajouter � la cache locale
            if (_cachedNotifications != null)
            {
                _cachedNotifications.Insert(0, notification);
                
                // Limiter le cache � 100 notifications
                if (_cachedNotifications.Count > 100)
                {
                    _cachedNotifications = _cachedNotifications.Take(100).ToList();
                }
            }
            
            // D�clencher les �v�nements
            NewNotificationReceived?.Invoke(notification);
            NotificationsUpdated?.Invoke(_cachedNotifications ?? new List<UserNotificationDto> { notification });
            
            if (!notification.EstLue)
            {
                var unreadCount = _cachedNotifications?.Count(n => !n.EstLue) ?? 1;
                UnreadCountChanged?.Invoke(unreadCount);
            }
            
            // Afficher une notification toast
            _appState.ShowNotificationAsync($"?? {notification.Titre}", NotificationType.Info);
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors du traitement de la nouvelle notification: {Message}", ex.Message);
        }
    }

    // ==========================================
    // M�THODES PRIV�ES
    // ==========================================

    private bool IsCacheValid()
    {
        return _cacheExpiry.HasValue && DateTime.Now < _cacheExpiry.Value;
    }
    
    /// <summary>
    /// Invalide le cache et force un rechargement
    /// </summary>
    public void InvalidateCache()
    {
        _cachedNotifications = null;
        _cacheExpiry = null;
        _logger.LogDebug("?? Cache des notifications invalid�");
    }
}