using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MySmartApp.Ui.Configuration;
using MySmartApp.Ui.Models;
using MySmartApp.Ui.Converters;
using Polly;
using Polly.Extensions.Http;

namespace MySmartApp.Ui.Services;

/// <summary>
/// Service HTTP robuste avec retry, timeout et gestion d'erreurs selon les bonnes pratiques
/// </summary>
public interface IApiService
{
    Task<ApiResponse<T>> GetAsync<T>(string endpoint);
    Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data);
    Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data);
    Task<ApiResponse> DeleteAsync(string endpoint);
    Task<ApiResponse<T>> DeleteAsync<T>(string endpoint, object data); // Pour DELETE avec body
    Task<ApiResponse<T>> PostAsync<T>(string endpoint); // Pour les appels sans body
}

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public int StatusCode { get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    public static ApiResponse<T> Success(T data, int statusCode = 200)
        => new() { IsSuccess = true, Data = data, StatusCode = statusCode };

    public static ApiResponse<T> Error(string message, int statusCode = 500, Dictionary<string, string[]>? validationErrors = null)
        => new() { IsSuccess = false, ErrorMessage = message, StatusCode = statusCode, ValidationErrors = validationErrors };
}

public class ApiResponse
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int StatusCode { get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    public static ApiResponse Success(int statusCode = 200)
        => new() { IsSuccess = true, StatusCode = statusCode };

    public static ApiResponse Error(string message, int statusCode = 500, Dictionary<string, string[]>? validationErrors = null)
        => new() { IsSuccess = false, ErrorMessage = message, StatusCode = statusCode, ValidationErrors = validationErrors };
}

public class ApiService : IApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ApiService> _logger;
    private readonly IConfigurationService _configService;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

    public ApiService(
        IHttpClientFactory httpClientFactory, 
        ILogger<ApiService> logger,
        IConfigurationService configService)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configService = configService;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = 
            { 
                new System.Text.Json.Serialization.JsonStringEnumConverter(),
                new FlexibleDateTimeConverter(),
                new FlexibleNullableDateTimeConverter()
            }
        };

        // Configuration de la politique de retry avec Polly
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && r.StatusCode != HttpStatusCode.Unauthorized)
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: _configService.Configuration.Api.RetryCount,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(
                    Math.Pow(2, retryAttempt) * _configService.Configuration.Api.RetryDelaySeconds),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Tentative {RetryCount} dans {Delay}ms pour {Endpoint}", 
                        retryCount, timespan.TotalMilliseconds, context.OperationKey);
                });
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        return await ExecuteWithRetryAsync<T>(async client =>
        {
            _logger.LogDebug("?? GET {Endpoint}", endpoint);
            return await client.GetAsync(endpoint);
        }, endpoint);
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
    {
        return await ExecuteWithRetryAsync<T>(async client =>
        {
            _logger.LogDebug("?? POST {Endpoint}", endpoint);
            return await client.PostAsJsonAsync(endpoint, data, _jsonOptions);
        }, endpoint);
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint)
    {
        return await ExecuteWithRetryAsync<T>(async client =>
        {
            _logger.LogDebug("?? POST {Endpoint} (sans body)", endpoint);
            return await client.PostAsync(endpoint, null);
        }, endpoint);
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
    {
        return await ExecuteWithRetryAsync<T>(async client =>
        {
            _logger.LogDebug("?? PUT {Endpoint}", endpoint);
            return await client.PutAsJsonAsync(endpoint, data, _jsonOptions);
        }, endpoint);
    }

    public async Task<ApiResponse> DeleteAsync(string endpoint)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Api");
            var context = new Context(endpoint);

            var response = await _retryPolicy.ExecuteAsync(async (ctx) =>
            {
                _logger.LogDebug("??? DELETE {Endpoint}", endpoint);
                return await client.DeleteAsync(endpoint);
            }, context);

            if (response.IsSuccessStatusCode)
            {
                return ApiResponse.Success((int)response.StatusCode);
            }

            var errorMessage = await HandleErrorResponseAsync(response);
            return ApiResponse.Error(errorMessage, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError("Erreur lors de DELETE {Endpoint}: {Message}", endpoint, ex.Message);
            return ApiResponse.Error($"Erreur de communication: {ex.Message}");
        }
    }

    public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint, object data)
    {
        return await ExecuteWithRetryAsync<T>(async client =>
        {
            _logger.LogDebug("??? DELETE {Endpoint} (avec body)", endpoint);
            var request = new HttpRequestMessage(HttpMethod.Delete, endpoint)
            {
                Content = JsonContent.Create(data, options: _jsonOptions)
            };
            return await client.SendAsync(request);
        }, endpoint);
    }

    private async Task<ApiResponse<T>> ExecuteWithRetryAsync<T>(
        Func<HttpClient, Task<HttpResponseMessage>> operation, 
        string endpoint)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Api");
            var context = new Context(endpoint);

            var response = await _retryPolicy.ExecuteAsync(async (ctx) =>
            {
                return await operation(client);
            }, context);

            return await ProcessResponseAsync<T>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError("Erreur lors de l'appel � {Endpoint}: {Message}", endpoint, ex.Message);
            return ApiResponse<T>.Error($"Erreur de communication: {ex.Message}");
        }
    }

    private async Task<ApiResponse<T>> ProcessResponseAsync<T>(HttpResponseMessage response)
    {
        var statusCode = (int)response.StatusCode;

        _logger.LogInformation("📥 Response Status: {StatusCode} pour type {Type}", statusCode, typeof(T).Name);

        if (response.IsSuccessStatusCode)
        {
            try
            {
                if (response.StatusCode == HttpStatusCode.NoContent || 
                    response.Content.Headers.ContentLength == 0)
                {
                    _logger.LogDebug("✅ NoContent response");
                    return ApiResponse<T>.Success(default, statusCode);
                }

                var content = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("📄 Response content length: {Length}", content?.Length ?? 0);
                _logger.LogDebug("📄 Response content preview: {Content}", content?.Substring(0, Math.Min(500, content?.Length ?? 0)));
                
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogDebug("✅ Empty content response");
                    return ApiResponse<T>.Success(default, statusCode);
                }

                // ✅ PRIORITÉ 1: Essayer désérialisation directe du DTO (backend retourne DTO direct)
                try
                {
                    _logger.LogDebug("🔄 Tentative désérialisation directe vers {Type}", typeof(T).Name);
                    var directData = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                    if (directData != null)
                    {
                        _logger.LogInformation("✅ Désérialisation directe réussie");
                        return ApiResponse<T>.Success(directData, statusCode);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogDebug("⚠️ Désérialisation directe échouée: {Message}", ex.Message);
                }

                // ✅ PRIORITÉ 2: Tenter wrapper backend ApiResponse
                try
                {
                    _logger.LogDebug("🔄 Tentative désérialisation wrapper backend");
                    var backendResponse = JsonSerializer.Deserialize<BackendApiResponse<T>>(content, _jsonOptions);
                    if (backendResponse != null && backendResponse.IsSuccess)
                    {
                        _logger.LogInformation("✅ Wrapper backend désérialisé avec succès");
                        return ApiResponse<T>.Success(backendResponse.Data, statusCode);
                    }
                    else if (backendResponse != null && !backendResponse.IsSuccess)
                    {
                        _logger.LogWarning("❌ Wrapper backend indique échec: {Error}", backendResponse.ErrorMessage);
                        return ApiResponse<T>.Error(backendResponse.ErrorMessage ?? "Erreur backend", statusCode);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogDebug("⚠️ Pas un wrapper backend: {Message}", ex.Message);
                }

                // ✅ PRIORITÉ 3: Pour List<T>, essayer wrapper avec array
                var typeT = typeof(T);
                if (typeT.IsGenericType && 
                    (typeT.GetGenericTypeDefinition() == typeof(List<>) || 
                     typeT.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                {
                    try
                    {
                        _logger.LogDebug("🔄 Tentative wrapper avec array pour {Type}", typeT.Name);
                        var elementType = typeT.GetGenericArguments()[0];
                        var wrapperType = typeof(BackendApiResponseWithArray<>).MakeGenericType(elementType);
                        var wrapperResponse = JsonSerializer.Deserialize(content, wrapperType, _jsonOptions);
                        
                        if (wrapperResponse != null)
                        {
                            var isSuccessProp = wrapperType.GetProperty("IsSuccess");
                            var dataProp = wrapperType.GetProperty("Data");
                            
                            if (isSuccessProp?.GetValue(wrapperResponse) is bool isSuccess && isSuccess)
                            {
                                var dataValue = dataProp?.GetValue(wrapperResponse);
                                if (dataValue != null)
                                {
                                    if (typeT.GetGenericTypeDefinition() == typeof(List<>))
                                    {
                                        var toListMethod = typeof(Enumerable).GetMethod("ToList")?.MakeGenericMethod(elementType);
                                        var listData = toListMethod?.Invoke(null, new[] { dataValue });
                                        _logger.LogInformation("✅ Wrapper array désérialisé vers List");
                                        return ApiResponse<T>.Success((T)listData!, statusCode);
                                    }
                                    
                                    _logger.LogInformation("✅ Wrapper array désérialisé");
                                    return ApiResponse<T>.Success((T)dataValue, statusCode);
                                }
                            }
                            else if (isSuccessProp?.GetValue(wrapperResponse) is bool isFail && !isFail)
                            {
                                var errorProp = wrapperType.GetProperty("ErrorMessage");
                                var errorMsg = errorProp?.GetValue(wrapperResponse)?.ToString() ?? "Erreur backend";
                                _logger.LogWarning("❌ Wrapper array indique échec: {Error}", errorMsg);
                                return ApiResponse<T>.Error(errorMsg, statusCode);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug("⚠️ Pas un wrapper array: {Message}", ex.Message);
                    }
                }

                // ✅ Fallback: Erreur de désérialisation
                _logger.LogError("❌ Toutes tentatives de désérialisation échouées pour {Type}", typeof(T).Name);
                return ApiResponse<T>.Error($"Impossible de désérialiser la réponse en {typeof(T).Name}", statusCode);
            }
            catch (JsonException ex)
            {
                _logger.LogError("❌ Erreur de désérialisation pour {Type}: {Message}", typeof(T).Name, ex.Message);
                return ApiResponse<T>.Error($"Erreur de format de réponse: {ex.Message}", statusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError("💥 Erreur inattendue lors du traitement de la réponse: {Message}", ex.Message);
                return ApiResponse<T>.Error($"Erreur inattendue: {ex.Message}", statusCode);
            }
        }

        var errorMessage = await HandleErrorResponseAsync(response);
        var validationErrors = await ExtractValidationErrorsAsync(response);
        
        _logger.LogError("❌ Réponse HTTP échec: {StatusCode} - {Error}", statusCode, errorMessage);
        return ApiResponse<T>.Error(errorMessage, statusCode, validationErrors);
    }

    private async Task<string> HandleErrorResponseAsync(HttpResponseMessage response)
    {
        var statusCode = response.StatusCode;
        var content = await response.Content.ReadAsStringAsync();

        return statusCode switch
        {
            HttpStatusCode.BadRequest => await ExtractProblemDetailsMessageAsync(content) ?? "Requ�te invalide",
            HttpStatusCode.Unauthorized => "Authentification requise",
            HttpStatusCode.Forbidden => "Accès interdit - permissions insuffisantes",
            HttpStatusCode.NotFound => "Ressource introuvable",
            HttpStatusCode.TooManyRequests => "Trop de requêtes - veuillez patienter",
            HttpStatusCode.InternalServerError => content.Length > 0 ? content : "Erreur serveur interne",
            _ => $"Erreur HTTP {(int)statusCode}: {content}"
        };
    }

    private async Task<Dictionary<string, string[]>?> ExtractValidationErrorsAsync(HttpResponseMessage response)
    {
        if (response.StatusCode != HttpStatusCode.BadRequest)
        {
            return null;
        }

        try
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            var problemDetails = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
            
            if (problemDetails.TryGetProperty("errors", out var errorsElement))
            {
                var errors = new Dictionary<string, string[]>();
                foreach (var error in errorsElement.EnumerateObject())
                {
                    var messages = error.Value.EnumerateArray()
                        .Select(x => x.GetString() ?? "")
                        .Where(x => !string.IsNullOrEmpty(x))
                        .ToArray();
                        
                    if (messages.Length > 0)
                    {
                        errors[error.Name] = messages;
                    }
                }
                return errors.Count > 0 ? errors : null;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning("Impossible d'extraire les erreurs de validation: {Message}", ex.Message);
        }

        return null;
    }

    private async Task<string?> ExtractProblemDetailsMessageAsync(string content)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            var problemDetails = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
            
            if (problemDetails.TryGetProperty("title", out var titleElement))
            {
                return titleElement.GetString();
            }
            
            if (problemDetails.TryGetProperty("detail", out var detailElement))
            {
                return detailElement.GetString();
            }
        }
        catch (JsonException)
        {
            // Si ce n'est pas du JSON valide, retourner le contenu brut
            return content;
        }

        return null;
    }

    private static bool IsIoTEndpoint(string? endpoint)
    {
        return endpoint?.Contains("/IoTHub/") == true;
    }
}

/// <summary>
/// ✅ DTO pour wrapper ApiResponse du backend (compatible C# backend)
/// </summary>
internal class BackendApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public int StatusCode { get; set; }
}

/// <summary>
/// ✅ DTO spécialisé pour wrapper backend retournant IEnumerable/array
/// </summary>
internal class BackendApiResponseWithArray<T>
{
    public bool IsSuccess { get; set; }
    public IEnumerable<T>? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public int StatusCode { get; set; }
}