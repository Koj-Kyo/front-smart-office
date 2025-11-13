using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public interface IDroitAccesService
{
    // Endpoints administrateur (CRUD complet)
    Task<ApiResponse<List<DroitAccesDto>>> GetDroitsAccesAsync();
    Task<ApiResponse<DroitAccesDto>> GetDroitAccesByIdAsync(int id);
    Task<ApiResponse<DroitAccesDto>> AddDroitAccesAsync(DroitAccesCreationDto droitAccesDto);
    Task<ApiResponse<bool>> UpdateDroitAccesAsync(int id, DroitAccesCreationDto droitAccesDto);
    Task<ApiResponse<bool>> DeleteDroitAccesAsync(int id);
    
    // ✅ NOUVEAUX - Endpoints assignation admin
    Task<ApiResponse<DroitAccesDto>> AssignZoneToUserAsync(string userId, int zoneId, string accessType = "Lecture");
    Task<ApiResponse<DroitAccesDto>> AssignDeviceToUserAsync(string userId, int peripheriqueId, string accessType = "Lecture");
    Task<ApiResponse<List<DroitAccesDto>>> GetUserAccessRightsAsync(string userId);
    Task<ApiResponse<bool>> RevokeZoneAccessAsync(string userId, int zoneId);
    Task<ApiResponse<bool>> RevokeDeviceAccessAsync(string userId, int peripheriqueId);
    Task<ApiResponse<List<DroitAccesDto>>> BulkAssignZonesAsync(string userId, List<int> zoneIds, string accessType = "Lecture");
    Task<ApiResponse<List<DroitAccesDto>>> BulkAssignDevicesAsync(string userId, List<int> peripheriqueIds, string accessType = "Lecture");
    
    // Endpoints utilisateur (consultation des droits personnels)
    Task<ApiResponse<List<UserZoneAccessRightDto>>> GetUserZoneAccessRightsAsync();
    Task<ApiResponse<List<UserPeripheriqueAccessRightDto>>> GetUserPeripheriqueAccessRightsAsync();
    
    // Méthodes utilitaires
    Task<bool> HasZoneAccessAsync(int zoneId);
    Task<bool> HasPeripheriqueAccessAsync(int peripheriqueId);
    Task<List<int>> GetAccessibleZoneIdsAsync();
    Task<List<int>> GetAccessiblePeripheriqueIdsAsync();
}

public class DroitAccesService : IDroitAccesService
{
    private readonly IApiService _apiService;
    private readonly ILogger<DroitAccesService> _logger;
    private readonly IAppStateService _appState;
    
    // Cache local des droits d'acc�s
    private List<UserZoneAccessRightDto>? _cachedZoneRights;
    private List<UserPeripheriqueAccessRightDto>? _cachedPeripheriqueRights;
    private DateTime? _cacheExpiry;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);

    public DroitAccesService(
        IApiService apiService,
        ILogger<DroitAccesService> logger,
        IAppStateService appState)
    {
        _apiService = apiService;
        _logger = logger;
        _appState = appState;
    }

    // ==========================================
    // ENDPOINTS ADMINISTRATEUR
    // ==========================================

    public async Task<ApiResponse<List<DroitAccesDto>>> GetDroitsAccesAsync()
    {
        try
        {
            _logger.LogInformation("📊 Récupération de tous les droits d'accès (admin)");
            
            var response = await _apiService.GetAsync<List<DroitAccesDto>>("/api/admin/access-rights");
            
            if (response.IsSuccess)
            {
                _logger.LogInformation("✅ {Count} droits d'accès récupérés", response.Data?.Count ?? 0);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la récupération des droits: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la récupération des droits d'accès: {Message}", ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<List<DroitAccesDto>>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse<DroitAccesDto>> GetDroitAccesByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("🔍 Récupération du droit d'accès {Id}", id);
            
            var response = await _apiService.GetAsync<DroitAccesDto>($"/api/admin/access-rights/{id}");
            
            if (response.IsSuccess)
            {
                _logger.LogDebug("✅ Droit d'accès {Id} récupéré", id);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la récupération du droit d'accès {Id}: {Message}", id, ex.Message);
            return ApiResponse<DroitAccesDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<DroitAccesDto>> AddDroitAccesAsync(DroitAccesCreationDto droitAccesDto)
    {
        try
        {
            _logger.LogInformation("➕ Ajout d'un nouveau droit d'accès pour {UserId}", droitAccesDto.UserId);
            
            var response = await _apiService.PostAsync<DroitAccesDto>("/api/admin/access-rights", droitAccesDto);
            
            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync("Droit d'accès ajouté avec succès", NotificationType.Success);
                _logger.LogInformation("✅ Droit d'accès créé avec l'ID {Id}", response.Data?.Id);
                
                // Invalider le cache
                InvalidateCache();
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de l'ajout: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de l'ajout du droit d'accès: {Message}", ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<DroitAccesDto>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse<bool>> UpdateDroitAccesAsync(int id, DroitAccesCreationDto droitAccesDto)
    {
        try
        {
            _logger.LogInformation("✏️ Mise à jour du droit d'accès {Id}", id);
            
            var response = await _apiService.PutAsync<object>($"/api/admin/access-rights/{id}", droitAccesDto);
            
            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync("Droit d'accès mis à jour avec succès", NotificationType.Success);
                _logger.LogInformation("✅ Droit d'accès {Id} mis à jour", id);
                
                // Invalider le cache
                InvalidateCache();
                
                return ApiResponse<bool>.Success(true, response.StatusCode);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la mise à jour: {response.ErrorMessage}", NotificationType.Error);
                return ApiResponse<bool>.Error(response.ErrorMessage ?? "Erreur lors de la mise à jour", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la mise à jour du droit d'accès {Id}: {Message}", id, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<bool>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse<bool>> DeleteDroitAccesAsync(int id)
    {
        try
        {
            _logger.LogInformation("🗑️ Suppression du droit d'accès {Id}", id);
            
            var response = await _apiService.DeleteAsync($"/api/admin/access-rights/{id}");
            
            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync("Droit d'accès supprimé avec succès", NotificationType.Success);
                _logger.LogInformation("✅ Droit d'accès {Id} supprimé", id);
                
                // Invalider le cache
                InvalidateCache();
                
                return ApiResponse<bool>.Success(true, response.StatusCode);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la suppression: {response.ErrorMessage}", NotificationType.Error);
                return ApiResponse<bool>.Error(response.ErrorMessage ?? "Erreur lors de la suppression", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la suppression du droit d'accès {Id}: {Message}", id, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<bool>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse<DroitAccesDto>> AssignZoneToUserAsync(string userId, int zoneId, string accessType = "Lecture")
    {
        try
        {
            _logger.LogInformation("🔑 Assignation zone {ZoneId} à utilisateur {UserId} (type: {Type})", 
                zoneId, userId, accessType);

            var request = new DroitAccesCreationDto(userId, zoneId, null, accessType);
            var response = await _apiService.PostAsync<DroitAccesDto>(
                $"/api/admin/access-rights/assign-zone-to-user", request);

            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync($"Zone assignée avec succès à l'utilisateur", NotificationType.Success);
                _logger.LogInformation("✅ Zone {ZoneId} assignée à {UserId}", zoneId, userId);
                InvalidateCache();
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur assignation: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur assignation zone {ZoneId} à {UserId}: {Message}", zoneId, userId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<DroitAccesDto>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse<DroitAccesDto>> AssignDeviceToUserAsync(string userId, int peripheriqueId, string accessType = "Lecture")
    {
        try
        {
            _logger.LogInformation("🔑 Assignation device {DeviceId} à utilisateur {UserId} (type: {Type})", 
                peripheriqueId, userId, accessType);

            var request = new DroitAccesCreationDto(userId, null, peripheriqueId, accessType);
            var response = await _apiService.PostAsync<DroitAccesDto>(
                $"/api/admin/access-rights/assign-device-to-user", request);

            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync($"Device assigné avec succès à l'utilisateur", NotificationType.Success);
                _logger.LogInformation("✅ Device {DeviceId} assigné à {UserId}", peripheriqueId, userId);
                InvalidateCache();
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur assignation: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur assignation device {DeviceId} à {UserId}: {Message}", peripheriqueId, userId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<DroitAccesDto>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse<List<DroitAccesDto>>> GetUserAccessRightsAsync(string userId)
    {
        try
        {
            _logger.LogInformation("📋 Récupération droits d'accès utilisateur {UserId}", userId);

            var response = await _apiService.GetAsync<List<DroitAccesDto>>(
                $"/api/admin/access-rights/user/{userId}");

            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("✅ {Count} droits d'accès récupérés pour {UserId}", 
                    response.Data.Count, userId);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur récupération droits utilisateur {UserId}: {Message}", userId, ex.Message);
            return ApiResponse<List<DroitAccesDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> RevokeZoneAccessAsync(string userId, int zoneId)
    {
        try
        {
            _logger.LogInformation("🚫 Révocation accès zone {ZoneId} pour utilisateur {UserId}", zoneId, userId);

            var response = await _apiService.DeleteAsync(
                $"/api/admin/access-rights/revoke-zone/{userId}/{zoneId}");

            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync("Accès zone révoqué avec succès", NotificationType.Success);
                _logger.LogInformation("✅ Accès zone {ZoneId} révoqué pour {UserId}", zoneId, userId);
                InvalidateCache();

                return ApiResponse<bool>.Success(true, response.StatusCode);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur révocation: {response.ErrorMessage}", NotificationType.Error);
                return ApiResponse<bool>.Error(response.ErrorMessage ?? "Erreur révocation", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur révocation zone {ZoneId} pour {UserId}: {Message}", zoneId, userId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<bool>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse<bool>> RevokeDeviceAccessAsync(string userId, int peripheriqueId)
    {
        try
        {
            _logger.LogInformation("🚫 Révocation accès device {DeviceId} pour utilisateur {UserId}", peripheriqueId, userId);

            var response = await _apiService.DeleteAsync(
                $"/api/admin/access-rights/revoke-device/{userId}/{peripheriqueId}");

            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync("Accès device révoqué avec succès", NotificationType.Success);
                _logger.LogInformation("✅ Accès device {DeviceId} révoqué pour {UserId}", peripheriqueId, userId);
                InvalidateCache();

                return ApiResponse<bool>.Success(true, response.StatusCode);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur révocation: {response.ErrorMessage}", NotificationType.Error);
                return ApiResponse<bool>.Error(response.ErrorMessage ?? "Erreur révocation", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur révocation device {DeviceId} pour {UserId}: {Message}", peripheriqueId, userId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<bool>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse<List<DroitAccesDto>>> BulkAssignZonesAsync(string userId, List<int> zoneIds, string accessType = "Lecture")
    {
        try
        {
            _logger.LogInformation("🔑 Assignation bulk de {Count} zones à utilisateur {UserId}", zoneIds.Count, userId);

            var request = new BulkAssignRequest
            {
                UserId = userId,
                ResourceIds = zoneIds,
                AccessType = accessType
            };

            var response = await _apiService.PostAsync<List<DroitAccesDto>>(
                $"/api/admin/access-rights/bulk-assign-zones", request);

            if (response.IsSuccess && response.Data != null)
            {
                await _appState.ShowNotificationAsync($"{response.Data.Count} zone(s) assignée(s) avec succès", NotificationType.Success);
                _logger.LogInformation("✅ {Count} zones assignées à {UserId}", response.Data.Count, userId);
                InvalidateCache();
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur assignation bulk: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur assignation bulk zones à {UserId}: {Message}", userId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<List<DroitAccesDto>>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse<List<DroitAccesDto>>> BulkAssignDevicesAsync(string userId, List<int> peripheriqueIds, string accessType = "Lecture")
    {
        try
        {
            _logger.LogInformation("🔑 Assignation bulk de {Count} devices à utilisateur {UserId}", peripheriqueIds.Count, userId);

            var request = new BulkAssignRequest
            {
                UserId = userId,
                ResourceIds = peripheriqueIds,
                AccessType = accessType
            };

            var response = await _apiService.PostAsync<List<DroitAccesDto>>(
                $"/api/admin/access-rights/bulk-assign-devices", request);

            if (response.IsSuccess && response.Data != null)
            {
                await _appState.ShowNotificationAsync($"{response.Data.Count} device(s) assigné(s) avec succès", NotificationType.Success);
                _logger.LogInformation("✅ {Count} devices assignés à {UserId}", response.Data.Count, userId);
                InvalidateCache();
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur assignation bulk: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur assignation bulk devices à {UserId}: {Message}", userId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<List<DroitAccesDto>>.Error(errorMessage);
        }
    }

    // ==========================================
    // ENDPOINTS UTILISATEUR
    // ==========================================

    public async Task<ApiResponse<List<UserZoneAccessRightDto>>> GetUserZoneAccessRightsAsync()
    {
        try
        {
            // V�rifier le cache
            if (IsCacheValid() && _cachedZoneRights != null)
            {
                _logger.LogDebug("?? Droits d'acc�s zones r�cup�r�s depuis le cache");
                return ApiResponse<List<UserZoneAccessRightDto>>.Success(_cachedZoneRights);
            }

            _logger.LogInformation("?? R�cup�ration des droits d'acc�s zones de l'utilisateur");
            
            var response = await _apiService.GetAsync<List<UserZoneAccessRightDto>>("/api/user/access-rights/zones");
            
            if (response.IsSuccess && response.Data != null)
            {
                // Mettre en cache
                _cachedZoneRights = response.Data;
                _cacheExpiry = DateTime.Now.Add(_cacheTimeout);
                
                _logger.LogInformation("? Droits d'acc�s � {Count} zones r�cup�r�s", response.Data.Count);
            }
            else if (response.StatusCode == 401)
            {
                _logger.LogWarning("?? Non autoris� - redirection vers la connexion");
                await _appState.SetErrorAsync("Session expir�e, veuillez vous reconnecter");
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des droits d'acc�s zones: {Message}", ex.Message);
            return ApiResponse<List<UserZoneAccessRightDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<UserPeripheriqueAccessRightDto>>> GetUserPeripheriqueAccessRightsAsync()
    {
        try
        {
            // V�rifier le cache
            if (IsCacheValid() && _cachedPeripheriqueRights != null)
            {
                _logger.LogDebug("?? Droits d'acc�s p�riph�riques r�cup�r�s depuis le cache");
                return ApiResponse<List<UserPeripheriqueAccessRightDto>>.Success(_cachedPeripheriqueRights);
            }

            _logger.LogInformation("?? R�cup�ration des droits d'acc�s p�riph�riques de l'utilisateur");
            
            var response = await _apiService.GetAsync<List<UserPeripheriqueAccessRightDto>>("/api/user/access-rights/peripheriques");
            
            if (response.IsSuccess && response.Data != null)
            {
                // Mettre en cache
                _cachedPeripheriqueRights = response.Data;
                _cacheExpiry = DateTime.Now.Add(_cacheTimeout);
                
                _logger.LogInformation("? Droits d'acc�s � {Count} p�riph�riques r�cup�r�s", response.Data.Count);
            }
            else if (response.StatusCode == 401)
            {
                _logger.LogWarning("?? Non autoris� - redirection vers la connexion");
                await _appState.SetErrorAsync("Session expir�e, veuillez vous reconnecter");
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des droits d'acc�s p�riph�riques: {Message}", ex.Message);
            return ApiResponse<List<UserPeripheriqueAccessRightDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // M�THODES UTILITAIRES
    // ==========================================

    public async Task<bool> HasZoneAccessAsync(int zoneId)
    {
        try
        {
            var response = await GetUserZoneAccessRightsAsync();
            return response.IsSuccess && response.Data?.Any(z => z.ZoneId == zoneId) == true;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la v�rification d'acc�s � la zone {ZoneId}: {Message}", zoneId, ex.Message);
            return false;
        }
    }

    public async Task<bool> HasPeripheriqueAccessAsync(int peripheriqueId)
    {
        try
        {
            var response = await GetUserPeripheriqueAccessRightsAsync();
            return response.IsSuccess && response.Data?.Any(p => p.PeripheriqueId == peripheriqueId) == true;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la v�rification d'acc�s au p�riph�rique {PeripheriqueId}: {Message}", peripheriqueId, ex.Message);
            return false;
        }
    }

    public async Task<List<int>> GetAccessibleZoneIdsAsync()
    {
        try
        {
            var response = await GetUserZoneAccessRightsAsync();
            return response.IsSuccess && response.Data != null 
                ? response.Data.Select(z => z.ZoneId).ToList()
                : new List<int>();
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des IDs de zones accessibles: {Message}", ex.Message);
            return new List<int>();
        }
    }

    public async Task<List<int>> GetAccessiblePeripheriqueIdsAsync()
    {
        try
        {
            var response = await GetUserPeripheriqueAccessRightsAsync();
            return response.IsSuccess && response.Data != null 
                ? response.Data.Select(p => p.PeripheriqueId).ToList()
                : new List<int>();
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des IDs de p�riph�riques accessibles: {Message}", ex.Message);
            return new List<int>();
        }
    }

    // ==========================================
    // GESTION DU CACHE
    // ==========================================

    private bool IsCacheValid()
    {
        return _cacheExpiry.HasValue && DateTime.Now < _cacheExpiry.Value;
    }

    private void InvalidateCache()
    {
        _cachedZoneRights = null;
        _cachedPeripheriqueRights = null;
        _cacheExpiry = null;
        _logger.LogDebug("🔄 Cache des droits d'accès invalidé");
    }
}

// ==========================================
// DTOs REQUEST
// ==========================================

/// <summary>
/// DTO pour assignation bulk de zones ou devices
/// </summary>
public class BulkAssignRequest
{
    public required string UserId { get; set; }
    public required List<int> ResourceIds { get; set; }
    public string AccessType { get; set; } = "Lecture";
}