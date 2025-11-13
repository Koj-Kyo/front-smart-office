using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public interface IBackendZoneService
{
    // Op�rations CRUD de base (Admin seulement)
    Task<ApiResponse<List<BackendZoneDto>>> GetAllZonesAsync();
    Task<ApiResponse<BackendZoneDto>> GetZoneByIdAsync(int id);
    Task<ApiResponse<BackendZoneDto>> CreateZoneAsync(ZoneCreationDto zone);
    Task<ApiResponse> UpdateZoneAsync(int id, ZoneUpdateDto zone);
    Task<ApiResponse> DeleteZoneAsync(int id);
    
    // M�thodes pour compatibilit� avec l'interface existante
    Task<List<ZoneDto>> GetAllZonesForUIAsync();
    Task<ZoneDto?> GetZoneForUIByIdAsync(int id);
    
    // Op�rations �tendues
    Task<ApiResponse<ZoneExtendedDto>> GetZoneWithDetailsAsync(int id);
    Task<ApiResponse<List<ZoneExtendedDto>>> GetAllZonesWithStatsAsync();
    
    // M�thodes utilitaires
    Task<bool> ZoneExistsAsync(int id);
    Task<int> GetZoneCountAsync();
    void InvalidateCache();
}

public class BackendZoneService : IBackendZoneService
{
    private readonly IApiService _apiService;
    private readonly ILogger<BackendZoneService> _logger;
    private readonly IAppStateService _appState;
    
    // Cache local
    private List<BackendZoneDto>? _cachedZones;
    private DateTime? _cacheExpiry;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);

    public BackendZoneService(
        IApiService apiService, 
        ILogger<BackendZoneService> logger,
        IAppStateService appState)
    {
        _apiService = apiService;
        _logger = logger;
        _appState = appState;
    }

    // ==========================================
    // OP�RATIONS CRUD DE BASE (Admin seulement)
    // ==========================================

    public async Task<ApiResponse<List<BackendZoneDto>>> GetAllZonesAsync()
    {
        try
        {
            // V�rifier le cache
            if (IsCacheValid() && _cachedZones != null)
            {
                _logger.LogDebug("?? Zones r�cup�r�es depuis le cache");
                return ApiResponse<List<BackendZoneDto>>.Success(_cachedZones);
            }

            _logger.LogInformation("?? R�cup�ration de toutes les zones");
            
            var response = await _apiService.GetAsync<List<BackendZoneDto>>("/api/Zones");
            
            if (response.IsSuccess && response.Data != null)
            {
                _cachedZones = response.Data;
                _cacheExpiry = DateTime.Now.Add(_cacheTimeout);
                
                _logger.LogInformation("? {Count} zones r�cup�r�es", response.Data.Count);
                return response;
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des zones: {Message}", ex.Message);
            return ApiResponse<List<BackendZoneDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BackendZoneDto>> GetZoneByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("?? R�cup�ration de la zone {ZoneId}", id);
            
            var response = await _apiService.GetAsync<BackendZoneDto>($"/api/Zones/{id}");
            
            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("? Zone {ZoneId} r�cup�r�e: {ZoneName}", id, response.Data.Nom);
                return response;
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration de la zone {ZoneId}: {Message}", id, ex.Message);
            return ApiResponse<BackendZoneDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BackendZoneDto>> CreateZoneAsync(ZoneCreationDto zone)
    {
        try
        {
            _logger.LogInformation("? Cr�ation de la zone: {ZoneName}", zone.Nom);
            
            var response = await _apiService.PostAsync<BackendZoneDto>("/api/Zones", zone);
            
            if (response.IsSuccess)
            {
                // Invalider le cache
                InvalidateCache();
                
                await _appState.ShowNotificationAsync($"Zone '{zone.Nom}' cr��e avec succ�s", NotificationType.Success);
                _logger.LogInformation("? Zone cr��e avec succ�s: {ZoneName}", zone.Nom);
                
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
            _logger.LogError("? Erreur lors de la cr�ation de la zone {ZoneName}: {Message}", zone.Nom, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<BackendZoneDto>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> UpdateZoneAsync(int id, ZoneUpdateDto zone)
    {
        try
        {
            _logger.LogInformation("?? Mise � jour de la zone {ZoneId}: {ZoneName}", id, zone.Nom);
            
            var response = await _apiService.PutAsync<object>($"/api/Zones/{id}", zone);
            
            if (response.IsSuccess)
            {
                // Invalider le cache
                InvalidateCache();
                
                await _appState.ShowNotificationAsync($"Zone '{zone.Nom}' mise � jour avec succ�s", NotificationType.Success);
                _logger.LogInformation("? Zone mise � jour avec succ�s: {ZoneName}", zone.Nom);
                
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
            _logger.LogError("? Erreur lors de la mise � jour de la zone {ZoneId}: {Message}", id, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> DeleteZoneAsync(int id)
    {
        try
        {
            _logger.LogInformation("??? Suppression de la zone {ZoneId}", id);
            
            var response = await _apiService.DeleteAsync($"/api/Zones/{id}");
            
            if (response.IsSuccess)
            {
                // Invalider le cache
                InvalidateCache();
                
                await _appState.ShowNotificationAsync("Zone supprim�e avec succ�s", NotificationType.Success);
                _logger.LogInformation("? Zone {ZoneId} supprim�e avec succ�s", id);
                
                return response;
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la suppression: {response.ErrorMessage}", NotificationType.Error);
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la suppression de la zone {ZoneId}: {Message}", id, ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    // ==========================================
    // COMPATIBILIT� AVEC L'INTERFACE EXISTANTE
    // ==========================================

    public async Task<List<ZoneDto>> GetAllZonesForUIAsync()
    {
        try
        {
            var response = await GetAllZonesAsync();
            
            if (response.IsSuccess && response.Data != null)
            {
                // Convertir BackendZoneDto vers ZoneDto pour l'UI
                return response.Data.Select(z => new ZoneDto(
                    Id: z.Id,
                    Name: z.Nom,
                    Description: null,
                    Devices: null
                )).ToList();
            }
            
            return new List<ZoneDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la conversion des zones pour l'UI: {Message}", ex.Message);
            return new List<ZoneDto>();
        }
    }

    public async Task<ZoneDto?> GetZoneForUIByIdAsync(int id)
    {
        try
        {
            var response = await GetZoneByIdAsync(id);
            
            if (response.IsSuccess && response.Data != null)
            {
                // Convertir BackendZoneDto vers ZoneDto pour l'UI
                return new ZoneDto(
                    Id: response.Data.Id,
                    Name: response.Data.Nom,
                    Description: null,
                    Devices: null
                );
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la conversion de la zone {ZoneId} pour l'UI: {Message}", id, ex.Message);
            return null;
        }
    }

    // ==========================================
    // OP�RATIONS �TENDUES
    // ==========================================

    public async Task<ApiResponse<ZoneExtendedDto>> GetZoneWithDetailsAsync(int id)
    {
        try
        {
            _logger.LogInformation("?? R�cup�ration des d�tails complets de la zone {ZoneId}", id);
            
            // R�cup�rer la zone de base
            var zoneResponse = await GetZoneByIdAsync(id);
            if (!zoneResponse.IsSuccess || zoneResponse.Data == null)
            {
                return ApiResponse<ZoneExtendedDto>.Error("Zone non trouv�e");
            }

            var zone = zoneResponse.Data;
            
            // Pour l'instant, cr�er un DTO �tendu basique
            // Dans une impl�mentation compl�te, on r�cup�rerait les p�riph�riques et droits d'acc�s
            var extendedZone = new ZoneExtendedDto(
                Id: zone.Id,
                Nom: zone.Nom,
                PeripheriquesCount: 0, // � impl�menter avec DeviceService
                UtilisateursCount: 0,  // � impl�menter avec DroitAccesService
                DerniereActivite: null,
                Peripheriques: new List<PeripheriqueLogiqueDto>(),
                DroitsAcces: new List<DroitAccesDto>()
            );
            
            return ApiResponse<ZoneExtendedDto>.Success(extendedZone);
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des d�tails de la zone {ZoneId}: {Message}", id, ex.Message);
            return ApiResponse<ZoneExtendedDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<ZoneExtendedDto>>> GetAllZonesWithStatsAsync()
    {
        try
        {
            _logger.LogInformation("?? R�cup�ration de toutes les zones avec statistiques");
            
            var zonesResponse = await GetAllZonesAsync();
            if (!zonesResponse.IsSuccess || zonesResponse.Data == null)
            {
                return ApiResponse<List<ZoneExtendedDto>>.Error("Impossible de r�cup�rer les zones");
            }

            var extendedZones = new List<ZoneExtendedDto>();
            
            foreach (var zone in zonesResponse.Data)
            {
                var detailsResponse = await GetZoneWithDetailsAsync(zone.Id);
                if (detailsResponse.IsSuccess && detailsResponse.Data != null)
                {
                    extendedZones.Add(detailsResponse.Data);
                }
            }
            
            return ApiResponse<List<ZoneExtendedDto>>.Success(extendedZones);
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des zones avec statistiques: {Message}", ex.Message);
            return ApiResponse<List<ZoneExtendedDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // M�THODES UTILITAIRES
    // ==========================================

    public async Task<bool> ZoneExistsAsync(int id)
    {
        try
        {
            var response = await GetZoneByIdAsync(id);
            return response.IsSuccess && response.Data != null;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la v�rification d'existence de la zone {ZoneId}: {Message}", id, ex.Message);
            return false;
        }
    }

    public async Task<int> GetZoneCountAsync()
    {
        try
        {
            var response = await GetAllZonesAsync();
            return response.IsSuccess && response.Data != null ? response.Data.Count : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors du comptage des zones: {Message}", ex.Message);
            return 0;
        }
    }

    public void InvalidateCache()
    {
        _cachedZones = null;
        _cacheExpiry = null;
        _logger.LogDebug("?? Cache des zones invalid�");
    }

    // ==========================================
    // GESTION DU CACHE
    // ==========================================

    private bool IsCacheValid()
    {
        return _cacheExpiry.HasValue && DateTime.Now < _cacheExpiry.Value;
    }
}