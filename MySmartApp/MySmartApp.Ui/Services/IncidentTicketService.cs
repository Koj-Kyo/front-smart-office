using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public interface IIncidentTicketService
{
    // Endpoints principaux CRUD
    Task<ApiResponse<IncidentTicketDto>> CreateTicketAsync(IncidentTicketCreateDto ticketDto);
    Task<ApiResponse<IncidentTicketDto>> GetTicketByIdAsync(int id);
    Task<ApiResponse<List<IncidentTicketDto>>> GetTicketsByStatusAsync(string status);
    Task<ApiResponse<List<IncidentTicketDto>>> GetTicketsByUserIdAsync(string userId);
    Task<ApiResponse> UpdateTicketStatusAsync(int id, string newStatus);
    Task<ApiResponse> AssignTicketAsync(int id, string assignedToId);
    Task<ApiResponse> DeleteTicketAsync(int id);
    
    // Méthodes utilitaires
    Task<ApiResponse<List<IncidentTicketDto>>> GetMyTicketsAsync();
    Task<ApiResponse<List<IncidentTicketDto>>> GetAllTicketsAsync();
    Task<List<string>> GetAvailableStatusesAsync();
    Task<Dictionary<string, int>> GetTicketStatisticsAsync();
    Task<string?> GetCurrentUserIdAsync();
}

public class IncidentTicketService : IIncidentTicketService
{
    private readonly IApiService _apiService;
    private readonly ILogger<IncidentTicketService> _logger;
    private readonly IAppStateService _appState;
    private readonly IAuthService _authService;
    
    // Cache local
    private List<IncidentTicketDto>? _cachedTickets;
    private DateTime? _cacheExpiry;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(2);

    public IncidentTicketService(
        IApiService apiService,
        ILogger<IncidentTicketService> logger,
        IAppStateService appState,
        IAuthService authService)
    {
        _apiService = apiService;
        _logger = logger;
        _appState = appState;
        _authService = authService;
    }

    // ==========================================
    // ENDPOINTS PRINCIPAUX
    // ==========================================

    public async Task<ApiResponse<IncidentTicketDto>> CreateTicketAsync(IncidentTicketCreateDto ticketDto)
    {
        try
        {
            _logger.LogInformation("📝 Création d'un nouveau ticket: {Sujet}", ticketDto.Sujet);
            
            // ✅ ALIGNEMENT BACKEND: POST /api/IncidentTickets
            var response = await _apiService.PostAsync<IncidentTicketDto>("/api/IncidentTickets", ticketDto);
            
            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync("✅ Ticket créé avec succès", NotificationType.Success);
                _logger.LogInformation("✅ Ticket créé avec l'ID {Id}", response.Data?.Id);
                
                // Invalider le cache
                InvalidateCache();
            }
            else
            {
                await _appState.ShowNotificationAsync($"❌ Erreur lors de la création: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la création du ticket: {Message}", ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<IncidentTicketDto>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse<IncidentTicketDto>> GetTicketByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("🔍 Récupération du ticket {Id}", id);
            
            // ✅ ALIGNEMENT BACKEND: GET /api/IncidentTickets/{id}
            var response = await _apiService.GetAsync<IncidentTicketDto>($"/api/IncidentTickets/{id}");
            
            if (response.IsSuccess)
            {
                _logger.LogDebug("✅ Ticket {Id} récupéré", id);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la récupération du ticket {Id}: {Message}", id, ex.Message);
            return ApiResponse<IncidentTicketDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<IncidentTicketDto>>> GetTicketsByStatusAsync(string status)
    {
        try
        {
            _logger.LogInformation("🔍 Récupération des tickets avec statut: {Status}", status);
            
            // ✅ ALIGNEMENT BACKEND: GET /api/IncidentTickets/status/{status}
            var response = await _apiService.GetAsync<List<IncidentTicketDto>>($"/api/IncidentTickets/status/{Uri.EscapeDataString(status)}");
            
            if (response.IsSuccess)
            {
                _logger.LogInformation("✅ {Count} tickets récupérés pour le statut {Status}", 
                    response.Data?.Count ?? 0, status);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la récupération des tickets par statut {Status}: {Message}", status, ex.Message);
            return ApiResponse<List<IncidentTicketDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<IncidentTicketDto>>> GetTicketsByUserIdAsync(string userId)
    {
        try
        {
            _logger.LogInformation("🔍 Récupération des tickets pour l'utilisateur: {UserId}", userId);
            
            // ✅ ALIGNEMENT BACKEND: GET /api/IncidentTickets/user/{userId}
            var response = await _apiService.GetAsync<List<IncidentTicketDto>>($"/api/IncidentTickets/user/{userId}");
            
            if (response.IsSuccess)
            {
                _logger.LogInformation("✅ {Count} tickets récupérés pour l'utilisateur {UserId}", 
                    response.Data?.Count ?? 0, userId);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la récupération des tickets utilisateur {UserId}: {Message}", userId, ex.Message);
            return ApiResponse<List<IncidentTicketDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse> UpdateTicketStatusAsync(int id, string newStatus)
    {
        try
        {
            _logger.LogInformation("🔄 Mise à jour du statut du ticket {Id} vers: {Status}", id, newStatus);
            
            // ✅ ALIGNEMENT BACKEND: PUT /api/IncidentTickets/{id}/status avec string direct
            var response = await _apiService.PutAsync<object>($"/api/IncidentTickets/{id}/status", newStatus);
            
            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync("✅ Statut du ticket mis à jour", NotificationType.Success);
                _logger.LogInformation("✅ Statut du ticket {Id} mis à jour vers {Status}", id, newStatus);
                
                // Invalider le cache
                InvalidateCache();
                
                return ApiResponse.Success(response.StatusCode);
            }
            else
            {
                await _appState.ShowNotificationAsync($"❌ Erreur lors de la mise à jour: {response.ErrorMessage}", NotificationType.Error);
                return ApiResponse.Error(response.ErrorMessage ?? "Erreur lors de la mise à jour", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la mise à jour du statut du ticket {Id}: {Message}", id, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> AssignTicketAsync(int id, string assignedToId)
    {
        try
        {
            _logger.LogInformation("👤 Assignation du ticket {Id} à l'utilisateur: {UserId}", id, assignedToId);
            
            // ✅ ALIGNEMENT BACKEND: PUT /api/IncidentTickets/{id}/assign avec string direct
            var response = await _apiService.PutAsync<object>($"/api/IncidentTickets/{id}/assign", assignedToId);
            
            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync("✅ Ticket assigné avec succès", NotificationType.Success);
                _logger.LogInformation("✅ Ticket {Id} assigné à {UserId}", id, assignedToId);
                
                // Invalider le cache
                InvalidateCache();
                
                return ApiResponse.Success(response.StatusCode);
            }
            else
            {
                await _appState.ShowNotificationAsync($"❌ Erreur lors de l'assignation: {response.ErrorMessage}", NotificationType.Error);
                return ApiResponse.Error(response.ErrorMessage ?? "Erreur lors de l'assignation", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de l'assignation du ticket {Id}: {Message}", id, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> DeleteTicketAsync(int id)
    {
        try
        {
            _logger.LogInformation("🗑️ Suppression du ticket {Id}", id);
            
            // ✅ ALIGNEMENT BACKEND: DELETE /api/IncidentTickets/{id}
            var response = await _apiService.DeleteAsync($"/api/IncidentTickets/{id}");
            
            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync("✅ Ticket supprimé avec succès", NotificationType.Success);
                _logger.LogInformation("✅ Ticket {Id} supprimé", id);
                
                // Invalider le cache
                InvalidateCache();
            }
            else
            {
                await _appState.ShowNotificationAsync($"❌ Erreur lors de la suppression: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la suppression du ticket {Id}: {Message}", id, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    // ==========================================
    // MÉTHODES UTILITAIRES
    // ==========================================

    public async Task<ApiResponse<List<IncidentTicketDto>>> GetMyTicketsAsync()
    {
        try
        {
            _logger.LogInformation("📄 Récupération des tickets de l'utilisateur connecté");
            
            var userId = await GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("⚠️ Impossible de récupérer l'ID utilisateur");
                return ApiResponse<List<IncidentTicketDto>>.Success(new List<IncidentTicketDto>());
            }
            
            // Utiliser l'endpoint /user/{userId}
            return await GetTicketsByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la récupération des tickets personnels: {Message}", ex.Message);
            return ApiResponse<List<IncidentTicketDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<IncidentTicketDto>>> GetAllTicketsAsync()
    {
        try
        {
            // Vérifier le cache
            if (IsCacheValid() && _cachedTickets != null)
            {
                _logger.LogDebug("📦 Tickets récupérés depuis le cache");
                return ApiResponse<List<IncidentTicketDto>>.Success(_cachedTickets);
            }

            _logger.LogInformation("📄 Récupération de tous les tickets");

            // ✅ ALIGNEMENT BACKEND: GET /api/IncidentTickets
            var response = await _apiService.GetAsync<List<IncidentTicketDto>>("/api/IncidentTickets");
            
            if (response.IsSuccess && response.Data != null)
            {
                _cachedTickets = response.Data;
                _cacheExpiry = DateTime.Now.Add(_cacheTimeout);
                _logger.LogInformation("✅ {Count} tickets récupérés", _cachedTickets.Count);
            }
            else
            {
                _logger.LogWarning("⚠️ Erreur lors de la récupération des tickets: {Error}", response.ErrorMessage);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la récupération de tous les tickets: {Message}", ex.Message);
            return ApiResponse<List<IncidentTicketDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<List<string>> GetAvailableStatusesAsync()
    {
        // Statuts standards des tickets d'incidents
        return await Task.FromResult(new List<string>
        {
            "Ouvert",
            "En cours",
            "En attente",
            "Résolu",
            "Fermé",
            "Annulé"
        });
    }

    public async Task<Dictionary<string, int>> GetTicketStatisticsAsync()
    {
        try
        {
            var allTicketsResponse = await GetAllTicketsAsync();
            if (!allTicketsResponse.IsSuccess || allTicketsResponse.Data == null)
            {
                return new Dictionary<string, int>();
            }

            var tickets = allTicketsResponse.Data;
            var stats = new Dictionary<string, int>
            {
                ["Total"] = tickets.Count,
                ["Ouvert"] = tickets.Count(t => t.Statut == "Ouvert"),
                ["En cours"] = tickets.Count(t => t.Statut == "En cours"),
                ["Résolu"] = tickets.Count(t => t.Statut == "Résolu"),
                ["Fermé"] = tickets.Count(t => t.Statut == "Fermé"),
                ["Cette semaine"] = tickets.Count(t => t.CreatedAt >= DateTime.Now.AddDays(-7)),
                ["Ce mois"] = tickets.Count(t => t.CreatedAt >= DateTime.Now.AddDays(-30))
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors du calcul des statistiques: {Message}", ex.Message);
            return new Dictionary<string, int>();
        }
    }

    // ==========================================
    // MÉTHODES PRIVÉES
    // ==========================================

    public async Task<string?> GetCurrentUserIdAsync()
    {
        try
        {
            return await _authService.GetCurrentUserIdAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la récupération de l'ID utilisateur: {Message}", ex.Message);
            return null;
        }
    }

    private bool IsCacheValid()
    {
        return _cacheExpiry.HasValue && DateTime.Now < _cacheExpiry.Value;
    }

    private void InvalidateCache()
    {
        _cachedTickets = null;
        _cacheExpiry = null;
        _logger.LogDebug("📦 Cache des tickets invalidé");
    }
}