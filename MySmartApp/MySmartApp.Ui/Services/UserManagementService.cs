using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public interface IUserManagementService
{
    // Gestion du profil utilisateur
    Task<ApiResponse<BackendUserProfileDto>> GetUserProfileAsync();
    Task<ApiResponse> UpdateUserProfileAsync(UserProfileUpdateDto profile);
    
    // Administration des r�les (Admin seulement)
    Task<ApiResponse<RoleOperationResultDto>> AssignRoleAsync(string userId, string roleName);
    Task<ApiResponse<RoleOperationResultDto>> RemoveRoleAsync(string userId, string roleName);
    
    // M�thodes utilitaires
    Task<ApiResponse<List<string>>> GetAvailableRolesAsync();
    Task<ApiResponse<List<string>>> GetUserRolesAsync(string userId);
    Task<bool> HasRoleAsync(string roleName);
}

public class UserManagementService : IUserManagementService
{
    private readonly IApiService _apiService;
    private readonly ILogger<UserManagementService> _logger;
    private readonly IAuthService _authService;
    private readonly IAppStateService _appState;
    
    // Cache pour les donn�es utilisateur
    private BackendUserProfileDto? _cachedProfile;
    private DateTime? _cacheExpiry;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(10);

    public UserManagementService(
        IApiService apiService,
        ILogger<UserManagementService> logger,
        IAuthService authService,
        IAppStateService appState)
    {
        _apiService = apiService;
        _logger = logger;
        _authService = authService;
        _appState = appState;
    }

    // ==========================================
    // GESTION DU PROFIL UTILISATEUR
    // ==========================================

    public async Task<ApiResponse<BackendUserProfileDto>> GetUserProfileAsync()
    {
        try
        {
            // V�rifier le cache
            if (IsCacheValid() && _cachedProfile != null)
            {
                _logger.LogDebug("?? Profil utilisateur r�cup�r� depuis le cache");
                return ApiResponse<BackendUserProfileDto>.Success(_cachedProfile);
            }

            _logger.LogInformation("?? R�cup�ration du profil utilisateur");
            
            var response = await _apiService.GetAsync<BackendUserProfileDto>("/api/User/profile");
            
            if (response.IsSuccess && response.Data != null)
            {
                _cachedProfile = response.Data;
                _cacheExpiry = DateTime.Now.Add(_cacheTimeout);
                
                _logger.LogInformation("? Profil utilisateur r�cup�r�: {Email}", _cachedProfile.Email);
                return response;
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration du profil utilisateur: {Message}", ex.Message);
            return ApiResponse<BackendUserProfileDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse> UpdateUserProfileAsync(UserProfileUpdateDto profile)
    {
        try
        {
            _logger.LogInformation("?? Mise � jour du profil utilisateur: {Email}", profile.Email);
            
            var response = await _apiService.PutAsync<object>("/api/User/profile", profile);
            
            if (response.IsSuccess)
            {
                // Invalider le cache
                InvalidateCache();
                
                // Mettre � jour le cache avec les nouvelles donn�es
                _cachedProfile = new BackendUserProfileDto(profile.Email, profile.FullName);
                _cacheExpiry = DateTime.Now.Add(_cacheTimeout);
                
                await _appState.ShowNotificationAsync("Profil mis � jour avec succ�s", NotificationType.Success);
                _logger.LogInformation("? Profil utilisateur mis � jour");
                
                return ApiResponse.Success(response.StatusCode);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la mise � jour: {response.ErrorMessage}", NotificationType.Error);
                return ApiResponse.Error(response.ErrorMessage ?? "Erreur lors de la mise � jour", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la mise � jour du profil: {Message}", ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    // ==========================================
    // ADMINISTRATION DES R�LES (ADMIN SEULEMENT)
    // ==========================================

    public async Task<ApiResponse<RoleOperationResultDto>> AssignRoleAsync(string userId, string roleName)
    {
        try
        {
            _logger.LogInformation("?? Attribution du r�le {RoleName} � l'utilisateur {UserId}", roleName, userId);
            
            var roleUpdateDto = new RoleUpdateDto(roleName);
            var response = await _apiService.PostAsync<object>($"/api/User/admin/users/{userId}/roles", roleUpdateDto);
            
            if (response.IsSuccess)
            {
                var result = new RoleOperationResultDto(
                    Success: true,
                    Message: $"R�le '{roleName}' attribu� avec succ�s",
                    UserId: userId,
                    RoleName: roleName
                );
                
                await _appState.ShowNotificationAsync($"R�le '{roleName}' attribu� avec succ�s", NotificationType.Success);
                _logger.LogInformation("? R�le {RoleName} attribu� � {UserId}", roleName, userId);
                
                return ApiResponse<RoleOperationResultDto>.Success(result);
            }
            else
            {
                var result = new RoleOperationResultDto(
                    Success: false,
                    Message: response.ErrorMessage ?? "Erreur lors de l'attribution du r�le",
                    UserId: userId,
                    RoleName: roleName
                );
                
                await _appState.ShowNotificationAsync($"Erreur lors de l'attribution: {response.ErrorMessage}", NotificationType.Error);
                return ApiResponse<RoleOperationResultDto>.Error(response.ErrorMessage ?? "Erreur lors de l'attribution", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de l'attribution du r�le {RoleName} � {UserId}: {Message}", roleName, userId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            
            var result = new RoleOperationResultDto(
                Success: false,
                Message: errorMessage,
                UserId: userId,
                RoleName: roleName
            );
            
            return ApiResponse<RoleOperationResultDto>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse<RoleOperationResultDto>> RemoveRoleAsync(string userId, string roleName)
    {
        try
        {
            _logger.LogInformation("?? Retrait du r�le {RoleName} de l'utilisateur {UserId}", roleName, userId);
            
            var roleUpdateDto = new RoleUpdateDto(roleName);
            var response = await _apiService.DeleteAsync<object>($"/api/User/admin/users/{userId}/roles", roleUpdateDto);
            
            if (response.IsSuccess)
            {
                var result = new RoleOperationResultDto(
                    Success: true,
                    Message: $"R�le '{roleName}' retir� avec succ�s",
                    UserId: userId,
                    RoleName: roleName
                );
                
                await _appState.ShowNotificationAsync($"R�le '{roleName}' retir� avec succ�s", NotificationType.Success);
                _logger.LogInformation("? R�le {RoleName} retir� de {UserId}", roleName, userId);
                
                return ApiResponse<RoleOperationResultDto>.Success(result);
            }
            else
            {
                var result = new RoleOperationResultDto(
                    Success: false,
                    Message: response.ErrorMessage ?? "Erreur lors du retrait du r�le",
                    UserId: userId,
                    RoleName: roleName
                );
                
                await _appState.ShowNotificationAsync($"Erreur lors du retrait: {response.ErrorMessage}", NotificationType.Error);
                return ApiResponse<RoleOperationResultDto>.Error(response.ErrorMessage ?? "Erreur lors du retrait", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors du retrait du r�le {RoleName} de {UserId}: {Message}", roleName, userId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            
            var result = new RoleOperationResultDto(
                Success: false,
                Message: errorMessage,
                UserId: userId,
                RoleName: roleName
            );
            
            return ApiResponse<RoleOperationResultDto>.Error(errorMessage);
        }
    }

    // ==========================================
    // M�THODES UTILITAIRES
    // ==========================================

    public async Task<ApiResponse<List<string>>> GetAvailableRolesAsync()
    {
        try
        {
            // Cette m�thode devrait id�alement appeler un endpoint API qui retourne les r�les disponibles
            // Pour l'instant, on retourne une liste statique bas�e sur l'architecture de l'app
            var availableRoles = new List<string>
            {
                "Admin",
                "Manager", 
                "Employee",
                "Viewer"
            };
            
            _logger.LogDebug("?? R�les disponibles r�cup�r�s: {Count}", availableRoles.Count);
            return ApiResponse<List<string>>.Success(availableRoles);
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des r�les disponibles: {Message}", ex.Message);
            return ApiResponse<List<string>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<string>>> GetUserRolesAsync(string userId)
    {
        try
        {
            // Cette m�thode devrait appeler un endpoint API pour r�cup�rer les r�les d'un utilisateur
            // Pour l'instant, on utilise les informations du token JWT
            var currentUser = await _authService.GetCurrentUserAsync();
            
            if (currentUser != null && currentUser.Id == userId)
            {
                return ApiResponse<List<string>>.Success(currentUser.Roles);
            }
            
            // Si c'est un autre utilisateur et qu'on est admin, on pourrait faire un appel API
            // Pour l'instant, on retourne une liste vide
            _logger.LogWarning("?? Impossible de r�cup�rer les r�les pour l'utilisateur {UserId}", userId);
            return ApiResponse<List<string>>.Success(new List<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des r�les utilisateur: {Message}", ex.Message);
            return ApiResponse<List<string>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<bool> HasRoleAsync(string roleName)
    {
        try
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            return currentUser?.Roles.Contains(roleName) ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la v�rification du r�le {RoleName}: {Message}", roleName, ex.Message);
            return false;
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
        _cachedProfile = null;
        _cacheExpiry = null;
        _logger.LogDebug("?? Cache du profil utilisateur invalid�");
    }
}