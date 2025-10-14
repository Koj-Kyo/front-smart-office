using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

namespace MySmartApp.Ui.Services;

/// <summary>
/// Handler pour ajouter automatiquement le token d'authentification aux requêtes HTTP
/// </summary>
public class AuthMessageHandler : DelegatingHandler
{
    private readonly ITokenProvider _tokenProvider;
    private readonly ILogger<AuthMessageHandler> _logger;

    public AuthMessageHandler(ITokenProvider tokenProvider, ILogger<AuthMessageHandler> logger)
    {
        _tokenProvider = tokenProvider;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Ajouter le token d'authentification si disponible
            var token = await _tokenProvider.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("?? Token ajouté à la requête {Method} {Uri}", 
                    request.Method, request.RequestUri?.ToString());
            }
            else
            {
                _logger.LogDebug("?? Pas de token disponible pour {Method} {Uri}", 
                    request.Method, request.RequestUri?.ToString());
            }

            // Ajouter des headers de correlation et tracing
            if (!request.Headers.Contains("X-Correlation-ID"))
            {
                request.Headers.Add("X-Correlation-ID", Guid.NewGuid().ToString());
            }

            if (!request.Headers.Contains("X-Client-Version"))
            {
                request.Headers.Add("X-Client-Version", "1.0.0");
            }

            var response = await base.SendAsync(request, cancellationToken);

            // Log des réponses non-succès pour debug
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("?? Réponse HTTP {StatusCode} pour {Method} {Uri}", 
                    (int)response.StatusCode, request.Method, request.RequestUri?.ToString());

                // Si 401, le token est peut-être expiré
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("?? Token possiblement expiré, nettoyage recommandé");
                }
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "? Erreur de requête HTTP pour {Method} {Uri}", 
                request.Method, request.RequestUri?.ToString());
            throw;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning("?? Timeout de la requête {Method} {Uri}", 
                request.Method, request.RequestUri?.ToString());
            throw;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug("?? Requête annulée {Method} {Uri}", 
                request.Method, request.RequestUri?.ToString());
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur inattendue dans AuthMessageHandler pour {Method} {Uri}", 
                request.Method, request.RequestUri?.ToString());
            throw;
        }
    }
}