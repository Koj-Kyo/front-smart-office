using System.Net.Http;
using System.Text.Json;
using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public interface IConsommationStatsService
{
    // Statistiques de base
    Task<ApiResponse<ConsommationStatsDto>> GetConsommationStatsAsync(int peripheriqueId, DateTime startDate, DateTime endDate);
    Task<ApiResponse<ConsommationStatsDto>> GetConsommationStatsByZoneAsync(int zoneId, DateTime startDate, DateTime endDate);
    
    // Donn�es temporelles
    Task<ApiResponse<List<ConsommationHoraireDto>>> GetConsommationHoraireAsync(int peripheriqueId, DateTime date);
    Task<ApiResponse<ConsommationTrendDto>> GetConsommationTrendAsync(int peripheriqueId, int nombreJours = 30);
    
    // Analyse avanc�e
    Task<ApiResponse<List<AnomalieConsommationDto>>> DetectAnomaliesAsync(int peripheriqueId, DateTime startDate, DateTime endDate);
    Task<ApiResponse<RapportConsommationDto>> GenererRapportConsommationAsync(int? zoneId, DateTime startDate, DateTime endDate);
    
    // Calculs �conomiques
    Task<ApiResponse<CoutEnergetiqueDto>> CalculerCoutEnergetiqueAsync(int peripheriqueId, DateTime startDate, DateTime endDate, decimal tarifKwh);
    Task<ApiResponse<List<TopConsommateursDto>>> GetTopConsommateursAsync(int? zoneId, DateTime startDate, DateTime endDate, int top = 10);
    
    // Comparaisons
    Task<ApiResponse<ComparaisonConsommationDto>> ComparerConsommationAsync(int peripheriqueId, DateTime startDate1, DateTime endDate1, DateTime startDate2, DateTime endDate2);
}

public class ConsommationStatsService : IConsommationStatsService
{
    private readonly IApiService _apiService;
    private readonly ILogger<ConsommationStatsService> _logger;
    private readonly IAppStateService _appState;

    public ConsommationStatsService(
        IApiService apiService,
        ILogger<ConsommationStatsService> logger,
        IAppStateService appState)
    {
        _apiService = apiService;
        _logger = logger;
        _appState = appState;
    }

    public async Task<ApiResponse<ConsommationStatsDto>> GetConsommationStatsAsync(int peripheriqueId, DateTime startDate, DateTime endDate)
    {
        try
        {
            _logger.LogInformation("📊 Récupération stats consommation périphérique {Id} du {Start} au {End}", 
                peripheriqueId, startDate.ToShortDateString(), endDate.ToShortDateString());

            var queryParams = $"?startDate={startDate:yyyy-MM-ddTHH:mm:ss}&endDate={endDate:yyyy-MM-ddTHH:mm:ss}";
            // ✅ FIX: Ajouter /api/ dans l'URL
            var response = await _apiService.GetAsync<ConsommationStatsDto>($"/api/ConsommationStats/peripherique/{peripheriqueId}{queryParams}");

            if (response.IsSuccess)
            {
                _logger.LogInformation("✅ Stats consommation récupérées: {Consommation} mWh", response.Data?.ConsommationTotaleMwh);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors de la récupération des statistiques: {response.ErrorMessage}", NotificationType.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la récupération des stats de consommation: {Message}", ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<ConsommationStatsDto>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse<ConsommationStatsDto>> GetConsommationStatsByZoneAsync(int zoneId, DateTime startDate, DateTime endDate)
    {
        try
        {
            _logger.LogInformation("📊 Récupération stats consommation zone {ZoneId} du {Start} au {End}", 
                zoneId, startDate.ToShortDateString(), endDate.ToShortDateString());

            var queryParams = $"?startDate={startDate:yyyy-MM-ddTHH:mm:ss}&endDate={endDate:yyyy-MM-ddTHH:mm:ss}";
            // ✅ FIX: Ajouter /api/ dans l'URL
            var response = await _apiService.GetAsync<ConsommationStatsDto>($"/api/ConsommationStats/zone/{zoneId}{queryParams}");

            if (response.IsSuccess)
            {
                _logger.LogInformation("✅ Stats zone récupérées: {Consommation} mWh", response.Data?.ConsommationTotaleMwh);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la récupération des stats de zone: {Message}", ex.Message);
            return ApiResponse<ConsommationStatsDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<ConsommationHoraireDto>>> GetConsommationHoraireAsync(int peripheriqueId, DateTime date)
    {
        try
        {
            _logger.LogInformation("📊 Récupération données horaires périphérique {Id} pour {Date}", 
                peripheriqueId, date.ToShortDateString());

            var queryParams = $"?date={date:yyyy-MM-ddTHH:mm:ss}";
            // ✅ FIX: Ajouter /api/ dans l'URL
            var response = await _apiService.GetAsync<List<ConsommationHoraireDto>>($"/api/ConsommationStats/peripherique/{peripheriqueId}/horaire{queryParams}");

            if (response.IsSuccess)
            {
                _logger.LogInformation("✅ {Count} données horaires récupérées", response.Data?.Count ?? 0);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la récupération des données horaires: {Message}", ex.Message);
            return ApiResponse<List<ConsommationHoraireDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ConsommationTrendDto>> GetConsommationTrendAsync(int peripheriqueId, int nombreJours = 30)
    {
        try
        {
            _logger.LogInformation("📊 Récupération tendances périphérique {Id} sur {Jours} jours", 
                peripheriqueId, nombreJours);

            var queryParams = $"?nombreJours={nombreJours}";
            // ✅ FIX: Ajouter /api/ dans l'URL
            var response = await _apiService.GetAsync<ConsommationTrendDto>($"/api/ConsommationStats/peripherique/{peripheriqueId}/tendances{queryParams}");

            if (response.IsSuccess)
            {
                _logger.LogInformation("✅ Tendances récupérées: {Tendance}% sur {Jours} jours", 
                    response.Data?.TendancePourcentage, nombreJours);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la récupération des tendances: {Message}", ex.Message);
            return ApiResponse<ConsommationTrendDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<AnomalieConsommationDto>>> DetectAnomaliesAsync(int peripheriqueId, DateTime startDate, DateTime endDate)
    {
        try
        {
            _logger.LogInformation("🔍 Détection anomalies périphérique {Id} du {Start} au {End}", 
                peripheriqueId, startDate.ToShortDateString(), endDate.ToShortDateString());

            var queryParams = $"?startDate={startDate:yyyy-MM-ddTHH:mm:ss}&endDate={endDate:yyyy-MM-ddTHH:mm:ss}";
            // ✅ FIX: Ajouter /api/ dans l'URL
            var response = await _apiService.GetAsync<List<AnomalieConsommationDto>>($"/api/ConsommationStats/peripherique/{peripheriqueId}/anomalies{queryParams}");

            if (response.IsSuccess)
            {
                var anomaliesCount = response.Data?.Count ?? 0;
                _logger.LogInformation("✅ {Count} anomalies détectées", anomaliesCount);
                
                if (anomaliesCount > 0)
                {
                    await _appState.ShowNotificationAsync($"{anomaliesCount} anomalie(s) détectée(s)", NotificationType.Warning);
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la détection d'anomalies: {Message}", ex.Message);
            return ApiResponse<List<AnomalieConsommationDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<RapportConsommationDto>> GenererRapportConsommationAsync(int? zoneId, DateTime startDate, DateTime endDate)
    {
        try
        {
            _logger.LogInformation("📄 Génération rapport consommation zone {ZoneId} du {Start} au {End}", 
                zoneId?.ToString() ?? "toutes", startDate.ToShortDateString(), endDate.ToShortDateString());

            var queryParams = $"?startDate={startDate:yyyy-MM-ddTHH:mm:ss}&endDate={endDate:yyyy-MM-ddTHH:mm:ss}";
            if (zoneId.HasValue)
                queryParams += $"&zoneId={zoneId.Value}";

            // ✅ FIX: Ajouter /api/ dans l'URL
            var response = await _apiService.GetAsync<RapportConsommationDto>($"/api/ConsommationStats/rapport{queryParams}");

            if (response.IsSuccess)
            {
                _logger.LogInformation("✅ Rapport généré: {Consommation} mWh, {Cout}€", 
                    response.Data?.ConsommationTotaleMwh, response.Data?.CoutTotalEstime);
                await _appState.ShowNotificationAsync("Rapport généré avec succès", NotificationType.Success);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la génération du rapport: {Message}", ex.Message);
            return ApiResponse<RapportConsommationDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CoutEnergetiqueDto>> CalculerCoutEnergetiqueAsync(int peripheriqueId, DateTime startDate, DateTime endDate, decimal tarifKwh)
    {
        try
        {
            _logger.LogInformation("💰 Calcul coût énergétique périphérique {Id} tarif {Tarif}€/kWh", 
                peripheriqueId, tarifKwh);

            var queryParams = $"?startDate={startDate:yyyy-MM-ddTHH:mm:ss}&endDate={endDate:yyyy-MM-ddTHH:mm:ss}&tarifKwh={tarifKwh}";
            // ✅ FIX: Ajouter /api/ dans l'URL
            var response = await _apiService.GetAsync<CoutEnergetiqueDto>($"/api/ConsommationStats/peripherique/{peripheriqueId}/cout{queryParams}");

            if (response.IsSuccess)
            {
                _logger.LogInformation("✅ Coût calculé: {Cout}€ pour {Consommation} mWh", 
                    response.Data?.CoutTotal, response.Data?.ConsommationMwh);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors du calcul du coût énergétique: {Message}", ex.Message);
            return ApiResponse<CoutEnergetiqueDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<TopConsommateursDto>>> GetTopConsommateursAsync(int? zoneId, DateTime startDate, DateTime endDate, int top = 10)
    {
        try
        {
            _logger.LogInformation("🏆 Récupération top {Top} consommateurs zone {ZoneId}", 
                top, zoneId?.ToString() ?? "toutes");

            var queryParams = $"?startDate={startDate:yyyy-MM-ddTHH:mm:ss}&endDate={endDate:yyyy-MM-ddTHH:mm:ss}&top={top}";
            if (zoneId.HasValue)
                queryParams += $"&zoneId={zoneId.Value}";

            // ✅ FIX: Ajouter /api/ dans l'URL
            var response = await _apiService.GetAsync<List<TopConsommateursDto>>($"/api/ConsommationStats/top-consommateurs{queryParams}");

            if (response.IsSuccess)
            {
                _logger.LogInformation("✅ Top {Count} consommateurs récupéré", response.Data?.Count ?? 0);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la récupération du top consommateurs: {Message}", ex.Message);
            return ApiResponse<List<TopConsommateursDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ComparaisonConsommationDto>> ComparerConsommationAsync(int peripheriqueId, DateTime startDate1, DateTime endDate1, DateTime startDate2, DateTime endDate2)
    {
        try
        {
            _logger.LogInformation("📊 Comparaison consommation périphérique {Id} entre 2 périodes", peripheriqueId);

            var queryParams = $"?startDate1={startDate1:yyyy-MM-ddTHH:mm:ss}&endDate1={endDate1:yyyy-MM-ddTHH:mm:ss}" +
                             $"&startDate2={startDate2:yyyy-MM-ddTHH:mm:ss}&endDate2={endDate2:yyyy-MM-ddTHH:mm:ss}";

            // ✅ FIX: Ajouter /api/ dans l'URL
            var response = await _apiService.GetAsync<ComparaisonConsommationDto>($"/api/ConsommationStats/peripherique/{peripheriqueId}/comparaison{queryParams}");

            if (response.IsSuccess)
            {
                _logger.LogInformation("✅ Comparaison effectuée: {Diff}% de variation", 
                    response.Data?.DifferencePourcentage);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la comparaison de consommation: {Message}", ex.Message);
            return ApiResponse<ComparaisonConsommationDto>.Error($"Erreur technique: {ex.Message}");
        }
    }
}