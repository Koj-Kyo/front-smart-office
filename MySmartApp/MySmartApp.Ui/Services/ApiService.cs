using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MySmartApp.Ui.Configuration;
using MySmartApp.Ui.Models;
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
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
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
            _logger.LogError(ex, "? Erreur lors de DELETE {Endpoint}", endpoint);
            return ApiResponse.Error($"Erreur de communication: {ex.Message}");
        }
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
            _logger.LogError(ex, "? Erreur lors de l'appel à {Endpoint}", endpoint);
            return ApiResponse<T>.Error($"Erreur de communication: {ex.Message}");
        }
    }

    private async Task<ApiResponse<T>> ProcessResponseAsync<T>(HttpResponseMessage response)
    {
        var statusCode = (int)response.StatusCode;

        if (response.IsSuccessStatusCode)
        {
            try
            {
                if (response.StatusCode == HttpStatusCode.NoContent || 
                    response.Content.Headers.ContentLength == 0)
                {
                    return ApiResponse<T>.Success(default, statusCode);
                }

                var content = await response.Content.ReadAsStringAsync();
                
                if (string.IsNullOrWhiteSpace(content))
                {
                    return ApiResponse<T>.Success(default, statusCode);
                }

                // Gestion spéciale pour les réponses IoT (wrapper success/message)
                if (IsIoTEndpoint(response.RequestMessage?.RequestUri?.ToString()))
                {
                    var iotResponse = JsonSerializer.Deserialize<IoTResponse>(content, _jsonOptions);
                    if (iotResponse != null)
                    {
                        if (iotResponse.Success)
                        {
                            // Si T est IoTResponse, retourner directement
                            if (typeof(T) == typeof(IoTResponse))
                            {
                                return ApiResponse<T>.Success((T)(object)iotResponse, statusCode);
                            }
                            // Sinon, essayer de désérialiser le message comme T
                            return ApiResponse<T>.Success(default, statusCode);
                        }
                        else
                        {
                            return ApiResponse<T>.Error(iotResponse.Message, statusCode);
                        }
                    }
                }

                var data = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                return ApiResponse<T>.Success(data, statusCode);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "?? Erreur de désérialisation pour {Type}", typeof(T).Name);
                return ApiResponse<T>.Error($"Erreur de format de réponse: {ex.Message}", statusCode);
            }
        }

        var errorMessage = await HandleErrorResponseAsync(response);
        var validationErrors = await ExtractValidationErrorsAsync(response);
        
        return ApiResponse<T>.Error(errorMessage, statusCode, validationErrors);
    }

    private async Task<string> HandleErrorResponseAsync(HttpResponseMessage response)
    {
        var statusCode = response.StatusCode;
        var content = await response.Content.ReadAsStringAsync();

        return statusCode switch
        {
            HttpStatusCode.BadRequest => await ExtractProblemDetailsMessageAsync(content) ?? "Requête invalide",
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
            _logger.LogWarning(ex, "Impossible d'extraire les erreurs de validation");
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