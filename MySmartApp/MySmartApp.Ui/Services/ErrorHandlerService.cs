using System.Net;
using MySmartApp.Ui.Configuration;
using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

/// <summary>
/// Service global de gestion d'erreurs avec logging et notifications
/// </summary>
public interface IErrorHandlerService
{
    Task HandleExceptionAsync(Exception exception, string? context = null);
    Task HandleHttpErrorAsync(HttpStatusCode statusCode, string? message = null, string? context = null);
    Task HandleValidationErrorsAsync(Dictionary<string, string[]> validationErrors, string? context = null);
    void SetGlobalErrorBoundary(Action<Exception> errorBoundary);
}

public class ErrorHandlerService : IErrorHandlerService
{
    private readonly ILogger<ErrorHandlerService> _logger;
    private readonly IAppStateService _appState;
    private readonly IConfigurationService _configService;
    private Action<Exception>? _globalErrorBoundary;
    
    public ErrorHandlerService(
        ILogger<ErrorHandlerService> logger,
        IAppStateService appState,
        IConfigurationService configService)
    {
        _logger = logger;
        _appState = appState;
        _configService = configService;
    }

    public async Task HandleExceptionAsync(Exception exception, string? context = null)
    {
        var contextInfo = !string.IsNullOrEmpty(context) ? $" dans {context}" : "";
        
        switch (exception)
        {
            case HttpRequestException httpEx:
                _logger.LogError(httpEx, "? Erreur de requête HTTP{Context}", contextInfo);
                await _appState.ShowNotificationAsync(
                    "Problème de connexion au serveur", 
                    NotificationType.Error);
                break;

            case TaskCanceledException when exception.InnerException is TimeoutException:
                _logger.LogWarning("?? Timeout de requête{Context}", contextInfo);
                await _appState.ShowNotificationAsync(
                    "Délai d'attente dépassé - veuillez réessayer", 
                    NotificationType.Warning);
                break;

            case OperationCanceledException:
                _logger.LogDebug("?? Opération annulée{Context}", contextInfo);
                // Pas de notification pour les annulations
                break;

            case UnauthorizedAccessException:
                _logger.LogWarning("?? Accès non autorisé{Context}", contextInfo);
                await HandleUnauthorizedAsync();
                break;

            case ArgumentException argEx:
                _logger.LogWarning(argEx, "?? Argument invalide{Context}", contextInfo);
                await _appState.ShowNotificationAsync(
                    $"Données invalides: {argEx.Message}", 
                    NotificationType.Warning);
                break;

            case InvalidOperationException invOpEx:
                _logger.LogError(invOpEx, "? Opération invalide{Context}", contextInfo);
                await _appState.ShowNotificationAsync(
                    "Opération non autorisée dans l'état actuel", 
                    NotificationType.Error);
                break;

            case System.Text.Json.JsonException jsonEx:
                _logger.LogError(jsonEx, "? Erreur de format JSON{Context}", contextInfo);
                await _appState.ShowNotificationAsync(
                    "Erreur de format des données reçues", 
                    NotificationType.Error);
                break;

            default:
                _logger.LogError(exception, "? Erreur inattendue{Context}: {ExceptionType}", 
                    contextInfo, exception.GetType().Name);
                
                var userMessage = _configService.Configuration.App.Environment == "Development" 
                    ? $"Erreur: {exception.Message}"
                    : "Une erreur inattendue s'est produite";
                    
                await _appState.ShowNotificationAsync(userMessage, NotificationType.Error);
                
                // Déclencher le boundary global si configuré
                _globalErrorBoundary?.Invoke(exception);
                break;
        }

        // Mettre à jour l'état d'erreur global
        await _appState.SetErrorAsync($"Erreur{contextInfo}: {GetUserFriendlyMessage(exception)}");
    }

    public async Task HandleHttpErrorAsync(HttpStatusCode statusCode, string? message = null, string? context = null)
    {
        var contextInfo = !string.IsNullOrEmpty(context) ? $" dans {context}" : "";
        
        switch (statusCode)
        {
            case HttpStatusCode.Unauthorized:
                _logger.LogWarning("?? Non autorisé (401){Context}", contextInfo);
                await HandleUnauthorizedAsync();
                break;

            case HttpStatusCode.Forbidden:
                _logger.LogWarning("?? Accès interdit (403){Context}", contextInfo);
                await _appState.ShowNotificationAsync(
                    "Vous n'avez pas les permissions nécessaires", 
                    NotificationType.Warning);
                break;

            case HttpStatusCode.NotFound:
                _logger.LogWarning("?? Ressource introuvable (404){Context}", contextInfo);
                await _appState.ShowNotificationAsync(
                    message ?? "Ressource introuvable", 
                    NotificationType.Warning);
                break;

            case HttpStatusCode.BadRequest:
                _logger.LogWarning("?? Requête invalide (400){Context}: {Message}", contextInfo, message);
                await _appState.ShowNotificationAsync(
                    message ?? "Données de requête invalides", 
                    NotificationType.Warning);
                break;

            case HttpStatusCode.TooManyRequests:
                _logger.LogWarning("?? Trop de requêtes (429){Context}", contextInfo);
                await _appState.ShowNotificationAsync(
                    "Trop de requêtes - veuillez patienter", 
                    NotificationType.Warning);
                break;

            case HttpStatusCode.InternalServerError:
                _logger.LogError("?? Erreur serveur (500){Context}: {Message}", contextInfo, message);
                await _appState.ShowNotificationAsync(
                    "Erreur du serveur - veuillez réessayer plus tard", 
                    NotificationType.Error);
                break;

            case HttpStatusCode.ServiceUnavailable:
                _logger.LogError("?? Service indisponible (503){Context}", contextInfo);
                await _appState.ShowNotificationAsync(
                    "Service temporairement indisponible", 
                    NotificationType.Error);
                break;

            default:
                _logger.LogWarning("?? Erreur HTTP {StatusCode}{Context}: {Message}", 
                    (int)statusCode, contextInfo, message);
                await _appState.ShowNotificationAsync(
                    message ?? $"Erreur HTTP {(int)statusCode}", 
                    NotificationType.Warning);
                break;
        }
    }

    public async Task HandleValidationErrorsAsync(Dictionary<string, string[]> validationErrors, string? context = null)
    {
        var contextInfo = !string.IsNullOrEmpty(context) ? $" dans {context}" : "";
        _logger.LogWarning("?? Erreurs de validation{Context}: {Errors}", 
            contextInfo, string.Join(", ", validationErrors.Keys));

        // Créer un message convivial
        var errorMessages = validationErrors
            .SelectMany(kvp => kvp.Value.Select(error => $"{kvp.Key}: {error}"))
            .Take(3) // Limiter à 3 erreurs pour éviter la surcharge
            .ToList();

        var message = errorMessages.Count > 0 
            ? string.Join("\n", errorMessages)
            : "Données invalides";

        if (validationErrors.Count > 3)
        {
            message += $"\n... et {validationErrors.Count - 3} autres erreurs";
        }

        await _appState.ShowNotificationAsync(message, NotificationType.Warning);
    }

    public void SetGlobalErrorBoundary(Action<Exception> errorBoundary)
    {
        _globalErrorBoundary = errorBoundary;
        _logger.LogDebug("??? Error boundary global configuré");
    }

    private async Task HandleUnauthorizedAsync()
    {
        await _appState.ShowNotificationAsync(
            "Session expirée - reconnexion nécessaire", 
            NotificationType.Warning);
        
        // Ici, vous pourriez déclencher une redirection vers la page de login
        // ou tenter un refresh automatique du token
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            HttpRequestException => "Problème de connexion",
            TaskCanceledException => "Opération annulée",
            UnauthorizedAccessException => "Accès non autorisé",
            ArgumentException => "Données invalides",
            InvalidOperationException => "Opération non autorisée",
            System.Text.Json.JsonException => "Format de données incorrect",
            _ => "Erreur inattendue"
        };
    }
}

/// <summary>
/// Extension methods pour simplifier la gestion d'erreurs
/// </summary>
public static class ErrorHandlerExtensions
{
    public static async Task HandleAsync(this IErrorHandlerService errorHandler, 
        Func<Task> operation, string? context = null)
    {
        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            await errorHandler.HandleExceptionAsync(ex, context);
        }
    }

    public static async Task<T?> HandleAsync<T>(this IErrorHandlerService errorHandler, 
        Func<Task<T>> operation, string? context = null, T? defaultValue = default)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            await errorHandler.HandleExceptionAsync(ex, context);
            return defaultValue;
        }
    }
}