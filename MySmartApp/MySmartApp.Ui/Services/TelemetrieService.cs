using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public interface ITelemetrieService
{
    // Récupération des données de télémétrie avec support fresh
    Task<ApiResponse<List<TelemetrieDto>>> GetTelemetriesByPeripheriqueAsync(int peripheriqueId, bool fresh = false);
    Task<ApiResponse<List<TelemetrieDto>>> GetHistoricalDataAsync(DateTime startDate, DateTime endDate, int? peripheriqueId = null, bool fresh = false);
    Task<ApiResponse<List<TelemetrieDto>>> GetRecentTelemetriesAsync(int? peripheriqueId = null, int hours = 24, bool fresh = false);
    
    // Cr�ation de donn�es de t�l�m�trie
    Task<ApiResponse<TelemetrieDto>> AddTelemetryAsync(TelemetrieCreateDto telemetrie);
    Task<ApiResponse> AddBulkTelemetryAsync(List<TelemetrieCreateDto> telemetries);
    
    // Gestion des suggestions IA
    Task<ApiResponse> StoreAISuggestionAsync(SuggestionCreateDto suggestion);
    Task<ApiResponse<List<SuggestionDto>>> GetUserSuggestionsAsync(int userId);
    
    // Statistiques et analyses
    Task<ApiResponse<TelemetrieStatsDto>> GetTelemetrieStatsAsync(int peripheriqueId, DateTime startDate, DateTime endDate);
    Task<ApiResponse<List<TelemetrieTrendDto>>> GetTelemetrieTrendsAsync(List<int> peripheriqueIds, int daysBack = 30);
    Task<ApiResponse<List<TelemetrieAlertDto>>> GetTelemetrieAlertsAsync(int? peripheriqueId = null);
    
    // M�thodes utilitaires
    Task<ApiResponse<Dictionary<string, int>>> GetEventTypesCountAsync(int peripheriqueId, DateTime startDate, DateTime endDate);
    Task<ApiResponse<List<TelemetrieHourlyDto>>> GetHourlyDataAsync(int peripheriqueId, DateTime date);
    Task<double> GetAverageValueAsync(int peripheriqueId, string eventType, int daysBack = 7);
}

public class TelemetrieService : ITelemetrieService
{
    private readonly IApiService _apiService;
    private readonly ILogger<TelemetrieService> _logger;
    private readonly IAppStateService _appState;
    
    // Cache pour les donn�es fr�quemment utilis�es
    private readonly Dictionary<string, (DateTime expiry, object data)> _cache = new();
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);

    public TelemetrieService(
        IApiService apiService,
        ILogger<TelemetrieService> logger,
        IAppStateService appState)
    {
        _apiService = apiService;
        _logger = logger;
        _appState = appState;
    }

    // ==========================================
    // R�CUP�RATION DES DONN�ES DE T�L�M�TRIE
    // ==========================================

    public async Task<ApiResponse<List<TelemetrieDto>>> GetTelemetriesByPeripheriqueAsync(int peripheriqueId, bool fresh = false)
    {
        try
        {
            _logger.LogInformation("📊 Récupération de la télémétrie pour le périphérique {PeripheriqueId} (fresh={Fresh})", peripheriqueId, fresh);
            
            // ✅ Support du paramètre ?fresh=true
            var endpoint = fresh
                ? $"/api/Telemetries/by-peripherique/{peripheriqueId}?fresh=true"
                : $"/api/Telemetries/by-peripherique/{peripheriqueId}";
            
            var response = await _apiService.GetAsync<List<TelemetrieDto>>(endpoint);
            
            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("✅ {Count} enregistrements de télémétrie récupérés pour {PeripheriqueId}", 
                    response.Data.Count, peripheriqueId);
                
                return response;
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la récupération de la télémétrie pour {PeripheriqueId}: {Message}", peripheriqueId, ex.Message);
            return ApiResponse<List<TelemetrieDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<TelemetrieDto>>> GetHistoricalDataAsync(DateTime startDate, DateTime endDate, int? peripheriqueId = null, bool fresh = false)
    {
        try
        {
            _logger.LogInformation("📊 Récupération des données historiques du {StartDate} au {EndDate} pour périphérique {PeripheriqueId} (fresh={Fresh})", 
                startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), peripheriqueId, fresh);
            
            var queryParams = new List<string>
            {
                $"startDate={startDate:yyyy-MM-ddTHH:mm:ss}",
                $"endDate={endDate:yyyy-MM-ddTHH:mm:ss}"
            };
            
            if (peripheriqueId.HasValue)
            {
                queryParams.Add($"peripheriqueId={peripheriqueId.Value}");
            }
            
            // ✅ Ajout du paramètre fresh
            if (fresh)
            {
                queryParams.Add("fresh=true");
            }
            
            var queryString = string.Join("&", queryParams);
            var response = await _apiService.GetAsync<List<TelemetrieDto>>($"/api/Telemetries/history?{queryString}");
            
            if (response.IsSuccess && response.Data != null)
            {
                _logger.LogInformation("✅ {Count} enregistrements historiques récupérés", response.Data.Count);
                return response;
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la récupération des données historiques: {Message}", ex.Message);
            return ApiResponse<List<TelemetrieDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<TelemetrieDto>>> GetRecentTelemetriesAsync(int? peripheriqueId = null, int hours = 24, bool fresh = false)
    {
        try
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddHours(-hours);
            
            return await GetHistoricalDataAsync(startDate, endDate, peripheriqueId, fresh);
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Erreur lors de la récupération des données récentes: {Message}", ex.Message);
            return ApiResponse<List<TelemetrieDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // CR�ATION DE DONN�ES DE T�L�M�TRIE
    // ==========================================

    public async Task<ApiResponse<TelemetrieDto>> AddTelemetryAsync(TelemetrieCreateDto telemetrie)
    {
        try
        {
            _logger.LogInformation("? Ajout d'une t�l�m�trie: {TypeEvenement} = {Valeur} pour p�riph�rique {PeripheriqueId}", 
                telemetrie.TypeEvenement, telemetrie.Valeur, telemetrie.PeripheriqueLogiqueId);
            
            // Cr�er l'objet complet pour l'API backend
            var telemetrieRequest = new
            {
                PeripheriqueLogiqueId = telemetrie.PeripheriqueLogiqueId,
                Horodatage = DateTime.UtcNow,
                TypeEvenement = telemetrie.TypeEvenement,
                Valeur = telemetrie.Valeur,
                DonneesJson = telemetrie.DonneesJson
            };
            
            var response = await _apiService.PostAsync<TelemetrieDto>("/api/Telemetries/add", telemetrieRequest);
            
            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync($"T�l�m�trie ajout�e: {telemetrie.TypeEvenement}", NotificationType.Success);
                _logger.LogInformation("? T�l�m�trie ajout�e avec succ�s");
                
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
            _logger.LogError("? Erreur lors de l'ajout de t�l�m�trie: {Message}", ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse<TelemetrieDto>.Error(errorMessage);
        }
    }

    public async Task<ApiResponse> AddBulkTelemetryAsync(List<TelemetrieCreateDto> telemetries)
    {
        try
        {
            _logger.LogInformation("?? Ajout en lot de {Count} t�l�m�tries", telemetries.Count);
            
            var tasks = telemetries.Select(t => AddTelemetryAsync(t));
            var results = await Task.WhenAll(tasks);
            
            var successCount = results.Count(r => r.IsSuccess);
            
            if (successCount == telemetries.Count)
            {
                await _appState.ShowNotificationAsync($"{successCount} t�l�m�tries ajout�es avec succ�s", NotificationType.Success);
                return ApiResponse.Success();
            }
            else
            {
                await _appState.ShowNotificationAsync($"Seulement {successCount}/{telemetries.Count} t�l�m�tries ajout�es", NotificationType.Warning);
                return ApiResponse.Error($"�chec partiel: {successCount}/{telemetries.Count}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de l'ajout en lot: {Message}", ex.Message);
            return ApiResponse.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // GESTION DES SUGGESTIONS IA
    // ==========================================

    public async Task<ApiResponse> StoreAISuggestionAsync(SuggestionCreateDto suggestion)
    {
        try
        {
            _logger.LogInformation("?? Stockage d'une suggestion IA pour l'utilisateur {UserId}", suggestion.UtilisateurId);
            
            var suggestionRequest = new
            {
                UtilisateurId = suggestion.UtilisateurId,
                ContenuSuggestion = suggestion.ContenuSuggestion,
                DateCreation = DateTime.UtcNow
            };
            
            var response = await _apiService.PostAsync<object>("/api/Telemetries/suggestions", suggestionRequest);
            
            if (response.IsSuccess)
            {
                await _appState.ShowNotificationAsync("Suggestion IA stock�e avec succ�s", NotificationType.Success);
                _logger.LogInformation("? Suggestion IA stock�e");
                return ApiResponse.Success(response.StatusCode);
            }
            else
            {
                await _appState.ShowNotificationAsync($"Erreur lors du stockage: {response.ErrorMessage}", NotificationType.Error);
                return ApiResponse.Error(response.ErrorMessage ?? "Erreur lors du stockage", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors du stockage de la suggestion IA: {Message}", ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return ApiResponse.Error(errorMessage);
        }
    }

    public async Task<ApiResponse<List<SuggestionDto>>> GetUserSuggestionsAsync(int userId)
    {
        try
        {
            _logger.LogInformation("?? R�cup�ration des suggestions pour l'utilisateur {UserId}", userId);
            
            // Note: L'API backend n'a pas d'endpoint pour r�cup�rer les suggestions par utilisateur
            // On peut soit demander l'ajout de cet endpoint, soit simuler avec une liste vide
            var suggestions = new List<SuggestionDto>();
            
            return ApiResponse<List<SuggestionDto>>.Success(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des suggestions: {Message}", ex.Message);
            return ApiResponse<List<SuggestionDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // STATISTIQUES ET ANALYSES
    // ==========================================

    public async Task<ApiResponse<TelemetrieStatsDto>> GetTelemetrieStatsAsync(int peripheriqueId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var cacheKey = $"stats_{peripheriqueId}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
            
            if (TryGetFromCache<TelemetrieStatsDto>(cacheKey, out var cachedStats))
            {
                return ApiResponse<TelemetrieStatsDto>.Success(cachedStats!);
            }

            _logger.LogInformation("?? Calcul des statistiques pour {PeripheriqueId} du {StartDate} au {EndDate}", 
                peripheriqueId, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
            
            var dataResponse = await GetHistoricalDataAsync(startDate, endDate, peripheriqueId);
            
            if (!dataResponse.IsSuccess || dataResponse.Data == null)
            {
                return ApiResponse<TelemetrieStatsDto>.Error("Impossible de r�cup�rer les donn�es");
            }

            var telemetries = dataResponse.Data;
            
            if (!telemetries.Any())
            {
                var emptyStats = new TelemetrieStatsDto(
                    peripheriqueId, null, startDate, endDate, 0, 0, 0, 0, 0,
                    new Dictionary<string, int>(), new List<TelemetrieHourlyDto>()
                );
                return ApiResponse<TelemetrieStatsDto>.Success(emptyStats);
            }

            var stats = new TelemetrieStatsDto(
                PeripheriqueId: peripheriqueId,
                PeripheriqueName: telemetries.FirstOrDefault()?.PeripheriqueName,
                StartDate: startDate,
                EndDate: endDate,
                TotalEvents: telemetries.Count,
                TotalValue: telemetries.Sum(t => t.Valeur),
                AverageValue: telemetries.Average(t => t.Valeur),
                MaxValue: telemetries.Max(t => t.Valeur),
                MinValue: telemetries.Min(t => t.Valeur),
                EventsByType: telemetries.GroupBy(t => t.TypeEvenement)
                    .ToDictionary(g => g.Key, g => g.Count()),
                HourlyData: telemetries.GroupBy(t => new { t.Horodatage.Date, t.Horodatage.Hour, t.TypeEvenement })
                    .Select(g => new TelemetrieHourlyDto(
                        new DateTime(g.Key.Date.Year, g.Key.Date.Month, g.Key.Date.Day, g.Key.Hour, 0, 0),
                        g.Key.TypeEvenement,
                        g.Sum(t => t.Valeur),
                        g.Count(),
                        g.Average(t => t.Valeur)
                    )).OrderBy(h => h.Hour).ToList()
            );
            
            // Mettre en cache
            AddToCache(cacheKey, stats);
            
            return ApiResponse<TelemetrieStatsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors du calcul des statistiques: {Message}", ex.Message);
            return ApiResponse<TelemetrieStatsDto>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<TelemetrieTrendDto>>> GetTelemetrieTrendsAsync(List<int> peripheriqueIds, int daysBack = 30)
    {
        try
        {
            _logger.LogInformation("?? Calcul des tendances pour {Count} p�riph�riques sur {Days} jours", 
                peripheriqueIds.Count, daysBack);
            
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-daysBack);
            
            var trends = new List<TelemetrieTrendDto>();
            
            foreach (var peripheriqueId in peripheriqueIds)
            {
                var dataResponse = await GetHistoricalDataAsync(startDate, endDate, peripheriqueId);
                
                if (dataResponse.IsSuccess && dataResponse.Data?.Any() == true)
                {
                    var telemetries = dataResponse.Data;
                    var eventTypes = telemetries.Select(t => t.TypeEvenement).Distinct();
                    
                    foreach (var eventType in eventTypes)
                    {
                        var eventData = telemetries.Where(t => t.TypeEvenement == eventType).ToList();
                        var dailyData = eventData.GroupBy(t => t.Horodatage.Date)
                            .Select(g => new TelemetrieDailyDto(
                                g.Key,
                                g.Sum(t => t.Valeur),
                                g.Count(),
                                new Dictionary<string, double> { { eventType, g.Sum(t => t.Valeur) } }
                            )).OrderBy(d => d.Date).ToList();
                        
                        if (dailyData.Count >= 2)
                        {
                            var dailyAverage = dailyData.Average(d => d.TotalValue);
                            var firstHalf = dailyData.Take(dailyData.Count / 2).Average(d => d.TotalValue);
                            var secondHalf = dailyData.Skip(dailyData.Count / 2).Average(d => d.TotalValue);
                            var trendPercentage = firstHalf > 0 ? ((secondHalf - firstHalf) / firstHalf) * 100 : 0;
                            
                            var trend = new TelemetrieTrendDto(
                                peripheriqueId,
                                telemetries.FirstOrDefault()?.PeripheriqueName,
                                eventType,
                                daysBack,
                                dailyAverage,
                                trendPercentage,
                                trendPercentage > 5 ? "Hausse" : trendPercentage < -5 ? "Baisse" : "Stable",
                                dailyData
                            );
                            
                            trends.Add(trend);
                        }
                    }
                }
            }
            
            return ApiResponse<List<TelemetrieTrendDto>>.Success(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors du calcul des tendances: {Message}", ex.Message);
            return ApiResponse<List<TelemetrieTrendDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<TelemetrieAlertDto>>> GetTelemetrieAlertsAsync(int? peripheriqueId = null)
    {
        try
        {
            _logger.LogInformation("?? G�n�ration des alertes de t�l�m�trie");
            
            var alerts = new List<TelemetrieAlertDto>();
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddHours(-24); // Derni�res 24h
            
            var dataResponse = await GetHistoricalDataAsync(startDate, endDate, peripheriqueId);
            
            if (dataResponse.IsSuccess && dataResponse.Data?.Any() == true)
            {
                var telemetries = dataResponse.Data;
                
                // D�tecter les valeurs anormalement �lev�es
                var grouped = telemetries.GroupBy(t => new { t.PeripheriqueLogiqueId, t.TypeEvenement });
                
                foreach (var group in grouped)
                {
                    var values = group.Select(t => t.Valeur).ToList();
                    if (values.Count >= 5)
                    {
                        var average = values.Average();
                        var stdDev = Math.Sqrt(values.Average(v => Math.Pow(v - average, 2)));
                        var threshold = average + (2 * stdDev); // Seuil � 2 �carts-types
                        
                        var anomalies = group.Where(t => t.Valeur > threshold).ToList();
                        
                        foreach (var anomaly in anomalies)
                        {
                            var alert = new TelemetrieAlertDto(
                                anomaly.PeripheriqueLogiqueId,
                                anomaly.PeripheriqueName,
                                anomaly.TypeEvenement,
                                anomaly.Horodatage,
                                "Valeur anormale",
                                $"Valeur de {anomaly.Valeur:F2} d�passe le seuil normal de {threshold:F2}",
                                threshold,
                                anomaly.Valeur,
                                anomaly.Valeur > (threshold * 1.5) ? "Critique" : "Avertissement"
                            );
                            
                            alerts.Add(alert);
                        }
                    }
                }
            }
            
            return ApiResponse<List<TelemetrieAlertDto>>.Success(alerts.OrderByDescending(a => a.DetectedAt).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la g�n�ration des alertes: {Message}", ex.Message);
            return ApiResponse<List<TelemetrieAlertDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    // ==========================================
    // M�THODES UTILITAIRES
    // ==========================================

    public async Task<ApiResponse<Dictionary<string, int>>> GetEventTypesCountAsync(int peripheriqueId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var dataResponse = await GetHistoricalDataAsync(startDate, endDate, peripheriqueId);
            
            if (!dataResponse.IsSuccess || dataResponse.Data == null)
            {
                return ApiResponse<Dictionary<string, int>>.Error("Impossible de r�cup�rer les donn�es");
            }

            var eventCounts = dataResponse.Data
                .GroupBy(t => t.TypeEvenement)
                .ToDictionary(g => g.Key, g => g.Count());
            
            return ApiResponse<Dictionary<string, int>>.Success(eventCounts);
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors du comptage des types d'�v�nements: {Message}", ex.Message);
            return ApiResponse<Dictionary<string, int>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<TelemetrieHourlyDto>>> GetHourlyDataAsync(int peripheriqueId, DateTime date)
    {
        try
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);
            
            var dataResponse = await GetHistoricalDataAsync(startDate, endDate, peripheriqueId);
            
            if (!dataResponse.IsSuccess || dataResponse.Data == null)
            {
                return ApiResponse<List<TelemetrieHourlyDto>>.Error("Impossible de r�cup�rer les donn�es");
            }

            var hourlyData = dataResponse.Data
                .GroupBy(t => new { t.Horodatage.Hour, t.TypeEvenement })
                .Select(g => new TelemetrieHourlyDto(
                    new DateTime(date.Year, date.Month, date.Day, g.Key.Hour, 0, 0),
                    g.Key.TypeEvenement,
                    g.Sum(t => t.Valeur),
                    g.Count(),
                    g.Average(t => t.Valeur)
                ))
                .OrderBy(h => h.Hour)
                .ToList();
            
            return ApiResponse<List<TelemetrieHourlyDto>>.Success(hourlyData);
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors de la r�cup�ration des donn�es horaires: {Message}", ex.Message);
            return ApiResponse<List<TelemetrieHourlyDto>>.Error($"Erreur technique: {ex.Message}");
        }
    }

    public async Task<double> GetAverageValueAsync(int peripheriqueId, string eventType, int daysBack = 7)
    {
        try
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-daysBack);
            
            var dataResponse = await GetHistoricalDataAsync(startDate, endDate, peripheriqueId);
            
            if (dataResponse.IsSuccess && dataResponse.Data?.Any() == true)
            {
                var filteredData = dataResponse.Data
                    .Where(t => t.TypeEvenement == eventType)
                    .ToList();
                
                return filteredData.Any() ? filteredData.Average(t => t.Valeur) : 0;
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur lors du calcul de la moyenne: {Message}", ex.Message);
            return 0;
        }
    }

    // ==========================================
    // GESTION DU CACHE
    // ==========================================

    private bool TryGetFromCache<T>(string key, out T? value)
    {
        value = default;
        
        if (_cache.TryGetValue(key, out var cached) && DateTime.Now < cached.expiry)
        {
            value = (T)cached.data;
            return true;
        }
        
        return false;
    }

    private void AddToCache<T>(string key, T value)
    {
        _cache[key] = (DateTime.Now.Add(_cacheTimeout), value!);
    }

    private void InvalidateCache()
    {
        _cache.Clear();
        _logger.LogDebug("?? Cache de t�l�m�trie invalid�");
    }
}