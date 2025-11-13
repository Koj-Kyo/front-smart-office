using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

/// <summary>
/// Service pour l'administration compl�te des utilisateurs via UserManagementController
/// </summary>
public interface IAdminUserManagementService
{
    // Gestion des utilisateurs
    Task<ApiResponse<List<UserDetailsDto>>> GetAllUsersAsync();
    Task<ApiResponse<UserDetailsDto>> GetUserByIdAsync(string userId);
    Task<ApiResponse<List<UserDetailsDto>>> SearchUsersAsync(string searchTerm);
    Task<ApiResponse<UserDetailsDto>> CreateUserAsync(CreateUserDto createUserDto);
    Task<ApiResponse> UpdateUserAsync(string userId, UpdateUserDto updateUserDto);
    Task<ApiResponse> ToggleUserStatusAsync(string userId);
    Task<ApiResponse> DeleteUserAsync(string userId);
    
    // Gestion des r�les
    Task<ApiResponse<List<RoleDto>>> GetAllRolesAsync();
    Task<ApiResponse<RoleDto>> CreateRoleAsync(CreateRoleDto createRoleDto);
    Task<ApiResponse> DeleteRoleAsync(string roleId);
    
    // Association utilisateurs-r�les
    Task<ApiResponse<List<string>>> GetUserRolesAsync(string userId);
    Task<ApiResponse> SetUserRolesAsync(string userId, IList<string> roleNames);
    Task<ApiResponse> AddRoleToUserAsync(string userId, string roleName);
    Task<ApiResponse> RemoveRoleFromUserAsync(string userId, string roleName);
    Task<ApiResponse<List<UserDetailsDto>>> GetUsersByRoleAsync(string roleName);
    
    // Statistiques
    Task<ApiResponse<UserStatisticsDto>> GetUserStatisticsAsync();
    
    // Utilitaires
    void InvalidateCache();
}

public class AdminUserManagementService : IAdminUserManagementService
{
    private readonly IApiService _apiService;
    private readonly ILogger<AdminUserManagementService> _logger;
    private readonly IAppStateService _appState;
    
    // Cache local
    private List<UserDetailsDto>? _cachedUsers;
    private List<RoleDto>? _cachedRoles;
    private UserStatisticsDto? _cachedStatistics;
    private DateTime? _cacheExpiry;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(3); // Cache plus court pour admin

    public AdminUserManagementService(
        IApiService apiService,
        ILogger<AdminUserManagementService> logger,
        IAppStateService appState)
    {
        _apiService = apiService;
        _logger = logger;
        _appState = appState;
    }

    // ==========================================
    // GESTION DES UTILISATEURS
    // ==========================================

    public async Task<ApiResponse<List<UserDetailsDto>>> GetAllUsersAsync()
    {
        try
        {
            // V�rifier le cache
            if (IsCacheValid() && _cachedUsers != null)
            {
                _logger.LogDebug("?? Utilisateurs r�cup�r�s depuis le cache");
                return ApiResponse<List<UserDetailsDto>>.Success(_cachedUsers);
            }

            _logger.LogInformation("?? R�cup�ration de tous les utilisateurs");
            
            var response = await _apiService.GetAsync<List<UserDetailsDto>>("/api/UserManagement/users");
            
            if (response.IsSuccess && response.Data != null)
            {
                _cachedUsers = response.Data;
                _cacheExpiry = DateTime.Now.Add(_cacheTimeout);
                
                _logger.LogInformation("? {Count} utilisateurs r�cup�r�s", response.Data.Count);
                return response;
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des utilisateurs: {Message}", ex.Message);
            return ApiResponse<List<UserDetailsDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<UserDetailsDto>> GetUserByIdAsync(string userId)
    {
        try
        {
            _logger.LogInformation("?? R�cup�ration de l'utilisateur {UserId}", userId);
            
            var response = await _apiService.GetAsync<UserDetailsDto>($"/api/UserManagement/users/{userId}");
            
            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? Utilisateur {UserId} r�cup�r�: {UserName}", userId, response.Data.UserName);
                return response;
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration de l'utilisateur {UserId}: {Message}", userId, ex.Message);
            return ApiResponse<UserDetailsDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<UserDetailsDto>>> SearchUsersAsync(string searchTerm)
    {
        try
        {
            _logger.LogInformation("?? Recherche d'utilisateurs: {SearchTerm}", searchTerm);
            
            var response = await _apiService.GetAsync<List<UserDetailsDto>>($"/api/UserManagement/users/search?searchTerm={Uri.EscapeDataString(searchTerm)}");
            
            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? {Count} utilisateurs trouv�s pour '{SearchTerm}'", response.Data.Count, searchTerm);
                return response;
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la recherche d'utilisateurs: {Message}", ex.Message);
            return ApiResponse<List<UserDetailsDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<UserDetailsDto>> CreateUserAsync(CreateUserDto createUserDto)
    {
        try
        {
            _logger.LogInformation("? Cr�ation de l'utilisateur: {Email}", createUserDto.Email);
            
            var response = await _apiService.PostAsync<UserDetailsDto>("/api/UserManagement/users", createUserDto);
            
            if (response.IsSuccess)
            {
                InvalidateCache();
                await _appState.ShowNotificationAsync($"Utilisateur '{createUserDto.UserName}' cr�� avec succ�s", NotificationType.Success);
                _logger.LogInformation("? Utilisateur cr��: {Email}", createUserDto.Email);
                
                return response;
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la cr�ation: {response.ErrorMessage}", NotificationType.Error);
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la cr�ation de l'utilisateur {Email}: {Message}", createUserDto.Email, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<UserDetailsDto>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> UpdateUserAsync(string userId, UpdateUserDto updateUserDto)
    {
        try
        {
            _logger.LogInformation("?? Mise � jour de l'utilisateur {UserId}: {UserName}", userId, updateUserDto.UserName);
            
            var response = await _apiService.PutAsync<object>($"/api/UserManagement/users/{userId}", updateUserDto);
            
            if (response.IsSuccess)
            {
                InvalidateCache();
                await _appState.ShowNotificationAsync($"Utilisateur '{updateUserDto.UserName}' mis � jour avec succ�s", NotificationType.Success);
                _logger.LogInformation("? Utilisateur mis � jour: {UserId}", userId);
                
                return ApiResponse.Success();
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la mise � jour: {response.ErrorMessage}", NotificationType.Error);
                return ApiResponse.Error(response.ErrorMessage ?? "Erreur lors de la mise � jour");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la mise � jour de l'utilisateur {UserId}: {Message}", userId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> ToggleUserStatusAsync(string userId)
    {
        try
        {
            _logger.LogInformation("?? Changement de statut pour l'utilisateur {UserId}", userId);
            
            var response = await _apiService.PostAsync<object>($"/api/UserManagement/users/{userId}/toggle-status", new { });
            
            if (response.IsSuccess)
            {
                InvalidateCache();
                await _appState.ShowNotificationAsync("Statut utilisateur modifi� avec succ�s", NotificationType.Success);
                _logger.LogInformation("? Statut modifi� pour l'utilisateur {UserId}", userId);
                
                return ApiResponse.Success();
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors du changement de statut: {response.ErrorMessage}", NotificationType.Error);
                return ApiResponse.Error(response.ErrorMessage ?? "Erreur lors du changement de statut");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors du changement de statut {UserId}: {Message}", userId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> DeleteUserAsync(string userId)
    {
        try
        {
            _logger.LogInformation("??? Suppression de l'utilisateur {UserId}", userId);
            
            var response = await _apiService.DeleteAsync($"/api/UserManagement/users/{userId}");
            
            if (response.IsSuccess)
            {
                InvalidateCache();
                await _appState.ShowNotificationAsync("Utilisateur supprim� avec succ�s", NotificationType.Success);
                _logger.LogInformation("? Utilisateur supprim�: {UserId}", userId);
                
                return ApiResponse.Success();
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la suppression: {response.ErrorMessage}", NotificationType.Error);
                return ApiResponse.Error(response.ErrorMessage ?? "Erreur lors de la suppression");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la suppression de l'utilisateur {UserId}: {Message}", userId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    // ==========================================
    // GESTION DES R�LES
    // ==========================================

    public async Task<ApiResponse<List<RoleDto>>> GetAllRolesAsync()
    {
        try
        {
            // V�rifier le cache
            if (IsCacheValid() && _cachedRoles != null)
            {
                _logger.LogDebug("?? R�les r�cup�r�s depuis le cache");
                return ApiResponse<List<RoleDto>>.Success(_cachedRoles);
            }

            _logger.LogInformation("?? R�cup�ration de tous les r�les");
            
            var response = await _apiService.GetAsync<List<RoleDto>>("/api/UserManagement/roles");
            
            if (response.IsSuccess && response.Data != null)
            {
                _cachedRoles = response.Data;
                _cacheExpiry = DateTime.Now.Add(_cacheTimeout);
                
                _logger.LogInformation("? {Count} r�les r�cup�r�s", response.Data.Count);
                return response;
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des r�les: {Message}", ex.Message);
            return ApiResponse<List<RoleDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<RoleDto>> CreateRoleAsync(CreateRoleDto createRoleDto)
    {
        try
        {
            _logger.LogInformation("? Cr�ation du r�le: {RoleName}", createRoleDto.Name);
            
            var response = await _apiService.PostAsync<RoleDto>("/api/UserManagement/roles", createRoleDto);
            
            if (response.IsSuccess)
            {
                InvalidateCache();
                await _appState.ShowNotificationAsync($"R�le '{createRoleDto.Name}' cr�� avec succ�s", NotificationType.Success);
                _logger.LogInformation("? R�le cr��: {RoleName}", createRoleDto.Name);
                
                return response;
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la cr�ation du r�le: {response.ErrorMessage}", NotificationType.Error);
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la cr�ation du r�le {RoleName}: {Message}", createRoleDto.Name, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<RoleDto>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> DeleteRoleAsync(string roleId)
    {
        try
        {
            _logger.LogInformation("??? Suppression du r�le {RoleId}", roleId);
            
            var response = await _apiService.DeleteAsync($"/api/UserManagement/roles/{roleId}");
            
            if (response.IsSuccess)
            {
                InvalidateCache();
                await _appState.ShowNotificationAsync("R�le supprim� avec succ�s", NotificationType.Success);
                _logger.LogInformation("? R�le supprim�: {RoleId}", roleId);
                
                return ApiResponse.Success();
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la suppression du r�le: {response.ErrorMessage}", NotificationType.Error);
                return ApiResponse.Error(response.ErrorMessage ?? "Erreur lors de la suppression du r�le");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la suppression du r�le {RoleId}: {Message}", roleId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    // ==========================================
    // ASSOCIATION UTILISATEURS-R�LES
    // ==========================================

    public async Task<ApiResponse<List<string>>> GetUserRolesAsync(string userId)
    {
        try
        {
            _logger.LogInformation("?? R�cup�ration des r�les de l'utilisateur {UserId}", userId);
            
            var response = await _apiService.GetAsync<List<string>>($"/api/UserManagement/users/{userId}/roles");
            
            if (response.IsSuccess)
            {
                _logger.LogInformation("? {Count} r�les r�cup�r�s pour l'utilisateur {UserId}", response.Data?.Count ?? 0, userId);
                return response;
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des r�les pour {UserId}: {Message}", userId, ex.Message);
            return ApiResponse<List<string>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse> SetUserRolesAsync(string userId, IList<string> roleNames)
    {
        try
        {
            _logger.LogInformation("?? Attribution des r�les � l'utilisateur {UserId}: {Roles}", userId, string.Join(", ", roleNames));
            
            var assignRolesDto = new AssignRolesDto(userId, roleNames);
            var response = await _apiService.PutAsync<object>($"/api/UserManagement/users/{userId}/roles", assignRolesDto);
            
            if (response.IsSuccess)
            {
                InvalidateCache();
                await _appState.ShowNotificationAsync("R�les mis � jour avec succ�s", NotificationType.Success);
                _logger.LogInformation("? R�les mis � jour pour l'utilisateur {UserId}", userId);
                
                return ApiResponse.Success();
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la mise � jour des r�les: {response.ErrorMessage}", NotificationType.Error);
                return ApiResponse.Error(response.ErrorMessage ?? "Erreur lors de la mise � jour des r�les");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la mise � jour des r�les pour {UserId}: {Message}", userId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> AddRoleToUserAsync(string userId, string roleName)
    {
        try
        {
            _logger.LogInformation("? Ajout du r�le {RoleName} � l'utilisateur {UserId}", roleName, userId);
            
            var response = await _apiService.PostAsync<object>($"/api/UserManagement/users/{userId}/roles/{roleName}", new { });
            
            if (response.IsSuccess)
            {
                InvalidateCache();
                await _appState.ShowNotificationAsync($"R�le '{roleName}' ajout� avec succ�s", NotificationType.Success);
                _logger.LogInformation("? R�le {RoleName} ajout� � l'utilisateur {UserId}", roleName, userId);
                
                return ApiResponse.Success();
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de l'ajout du r�le: {response.ErrorMessage}", NotificationType.Error);
                return ApiResponse.Error(response.ErrorMessage ?? "Erreur lors de l'ajout du r�le");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de l'ajout du r�le {RoleName} � {UserId}: {Message}", roleName, userId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> RemoveRoleFromUserAsync(string userId, string roleName)
    {
        try
        {
            _logger.LogInformation("? Retrait du r�le {RoleName} de l'utilisateur {UserId}", roleName, userId);
            
            var response = await _apiService.DeleteAsync($"/api/UserManagement/users/{userId}/roles/{roleName}");
            
            if (response.IsSuccess)
            {
                InvalidateCache();
                await _appState.ShowNotificationAsync($"R�le '{roleName}' retir� avec succ�s", NotificationType.Success);
                _logger.LogInformation("? R�le {RoleName} retir� de l'utilisateur {UserId}", roleName, userId);
                
                return ApiResponse.Success();
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors du retrait du r�le: {response.ErrorMessage}", NotificationType.Error);
                return ApiResponse.Error(response.ErrorMessage ?? "Erreur lors du retrait du r�le");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors du retrait du r�le {RoleName} de {UserId}: {Message}", roleName, userId, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    public async Task<ApiResponse<List<UserDetailsDto>>> GetUsersByRoleAsync(string roleName)
    {
        try
        {
            _logger.LogInformation("?? R�cup�ration des utilisateurs avec le r�le {RoleName}", roleName);
            
            var response = await _apiService.GetAsync<List<UserDetailsDto>>($"/api/UserManagement/roles/{roleName}/users");
            
            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? {Count} utilisateurs trouv�s avec le r�le {RoleName}", response.Data.Count, roleName);
                return response;
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des utilisateurs pour le r�le {RoleName}: {Message}", roleName, ex.Message);
            return ApiResponse<List<UserDetailsDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // STATISTIQUES
    // ==========================================

    public async Task<ApiResponse<UserStatisticsDto>> GetUserStatisticsAsync()
    {
        try
        {
            // V�rifier le cache
            if (IsCacheValid() && _cachedStatistics != null)
            {
                _logger.LogDebug("?? Statistiques r�cup�r�es depuis le cache");
                return ApiResponse<UserStatisticsDto>.Success(_cachedStatistics);
            }

            _logger.LogInformation("?? R�cup�ration des statistiques utilisateurs");
            
            var response = await _apiService.GetAsync<UserStatisticsDto>("/api/UserManagement/statistics");
            
            if (response.IsSuccess && response.Data != null)
            {
                _cachedStatistics = response.Data;
                _cacheExpiry = DateTime.Now.Add(_cacheTimeout);
                
                _logger.LogInformation("? Statistiques r�cup�r�es: {TotalUsers} utilisateurs", response.Data.TotalUsers);
                return response;
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des statistiques: {Message}", ex.Message);
            return ApiResponse<UserStatisticsDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // UTILITAIRES
    // ==========================================

    public void InvalidateCache()
    {
        _cachedUsers = null;
        _cachedRoles = null;
        _cachedStatistics = null;
        _cacheExpiry = null;
        _logger.LogDebug("?? Cache de gestion des utilisateurs invalid�");
    }

    private bool IsCacheValid()
    {
        return _cacheExpiry.HasValue && DateTime.Now < _cacheExpiry.Value;
    }
}