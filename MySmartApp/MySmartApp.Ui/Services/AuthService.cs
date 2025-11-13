using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MySmartApp.Ui.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace MySmartApp.Ui.Services;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string email, string password);
    Task<RegisterResult> RegisterAsync(RegisterDto request);
    Task<bool> ValidateTokenAsync();
    Task<UserProfileDto?> GetCurrentUserAsync();
    Task<string?> GetCurrentUserIdAsync();
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<bool> IsInRoleAsync(string role);
}

public class AuthService : IAuthService
{
    private readonly IApiService _apiService;
    private readonly ITokenProvider _tokenProvider;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILogger<AuthService> _logger;
    private readonly IAppStateService _appState;
    private readonly IConfiguration _configuration;

    public AuthService(
        IApiService apiService,
        ITokenProvider tokenProvider, 
        AuthenticationStateProvider authStateProvider,
        ILogger<AuthService> logger,
        IAppStateService appState,
        IConfiguration configuration)
    {
        _apiService = apiService;
        _tokenProvider = tokenProvider;
        _authStateProvider = authStateProvider;
        _logger = logger;
        _appState = appState;
        _configuration = configuration;
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation("?? Tentative de connexion pour {Email}", email);
            
            // Utiliser LoginDto qui correspond exactement au backend
            var loginDto = new LoginDto(email, password);
            
            // Appel direct à l'API sans wrapper ApiService pour plus de contrôle
            using var httpClient = await GetHttpClientAsync();
            
            _logger.LogDebug("?? URL de connexion: {BaseAddress}/api/Auth/login", httpClient.BaseAddress);
            
            var jsonContent = JsonSerializer.Serialize(loginDto);
            _logger.LogDebug("?? Données envoyées: {JsonContent}", jsonContent);
            
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            var httpResponse = await httpClient.PostAsync("/api/Auth/login", content);
            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            
            _logger.LogDebug("?? Réponse API: Status={StatusCode}, Content={Content}", 
                httpResponse.StatusCode, responseContent);
            
            if (httpResponse.IsSuccessStatusCode)
            {
                var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (authResponse != null && !string.IsNullOrEmpty(authResponse.AccessToken))
                {
                    // Sauvegarder le token
                    await _tokenProvider.SetTokenAsync(authResponse.AccessToken);
                    
                    // Mettre à jour l'état d'authentification
                    if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
                    {
                        await customProvider.MarkUserAsAuthenticated(authResponse.AccessToken);
                    }
                    
                    // Charger le profil utilisateur
                    var userProfile = await GetCurrentUserAsync();
                    if (userProfile != null)
                    {
                        await _appState.SetUserAsync(userProfile);
                    }
                    
                    _logger.LogInformation("? Connexion réussie pour {Email}", email);
                    await _appState.ShowNotificationAsync("Connexion réussie", NotificationType.Success);
                    
                    return new LoginResult(true, null, authResponse);
                }
                else
                {
                    _logger.LogWarning("?? Réponse de connexion invalide: AuthResponse null ou AccessToken vide");
                    var invalidResponseMessage = "Réponse du serveur invalide";
                    await _appState.ShowNotificationAsync(invalidResponseMessage, NotificationType.Error);
                    return new LoginResult(false, invalidResponseMessage);
                }
            }
            
            // Gérer les erreurs selon le code de statut
            var errorMessage = httpResponse.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Email ou mot de passe incorrect",
                HttpStatusCode.BadRequest => await ExtractBadRequestMessage(httpResponse),
                HttpStatusCode.InternalServerError => "Erreur serveur (500). Vérifiez que l'API backend est démarrée.",
                HttpStatusCode.NotFound => "Endpoint non trouvé (404). Vérifiez l'URL de l'API.",
                HttpStatusCode.Forbidden => "Accès interdit (403)",
                _ => $"Erreur de connexion ({httpResponse.StatusCode}): {responseContent}"
            };
            
            _logger.LogWarning("? Échec de connexion pour {Email}: {Error} (Status: {StatusCode})", 
                email, errorMessage, httpResponse.StatusCode);
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            
            return new LoginResult(false, errorMessage);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "?? Erreur de réseau lors de la connexion pour {Email}", email);
            var errorMessage = "Impossible de joindre le serveur. Vérifiez que l'API backend est démarrée sur https://localhost:7144";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return new LoginResult(false, errorMessage);
        }
        catch (TaskCanceledException tcEx) when (tcEx.InnerException is TimeoutException)
        {
            _logger.LogError(tcEx, "?? Timeout de connexion pour {Email}", email);
            var errorMessage = "Délai d'attente dépassé. Le serveur met trop de temps à répondre.";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return new LoginResult(false, errorMessage);
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError("?? Erreur de désérialisation JSON pour {Email}: {Message}", email, jsonEx.Message);
            var errorMessage = "Erreur de format de réponse du serveur";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return new LoginResult(false, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError("? Erreur inattendue lors de la connexion pour {Email}: {ExceptionType} - {Message}", 
                email, ex.GetType().Name, ex.Message);
            var errorMessage = $"Erreur inattendue: {ex.GetType().Name} - {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return new LoginResult(false, errorMessage);
        }
    }

    public async Task<RegisterResult> RegisterAsync(RegisterDto request)
    {
        try
        {
            _logger.LogInformation("?? Tentative d'inscription pour {Email}", request.Email);
            
            // Appel direct à l'API backend
            var httpClient = await GetHttpClientAsync();
            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            var httpResponse = await httpClient.PostAsync("/api/Auth/register", content);
            
            if (httpResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("? Inscription réussie pour {Email}", request.Email);
                await _appState.ShowNotificationAsync("Compte créé avec succès. Vous pouvez maintenant vous connecter.", NotificationType.Success);
                return new RegisterResult(true, "Compte créé avec succès");
            }
            
            var errorMessage = httpResponse.StatusCode switch
            {
                HttpStatusCode.BadRequest => await ExtractBadRequestMessage(httpResponse),
                HttpStatusCode.Conflict => "Un compte avec cet email existe déjà",
                _ => "Erreur lors de la création du compte"
            };
            
            _logger.LogWarning("? Échec d'inscription pour {Email}: {Error} (Status: {StatusCode})", 
                request.Email, errorMessage, httpResponse.StatusCode);
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            
            return new RegisterResult(false, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de l'inscription pour {Email}", request.Email);
            var errorMessage = "Erreur lors de la création du compte. Veuillez réessayer.";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return new RegisterResult(false, errorMessage);
        }
    }

    public async Task<bool> ValidateTokenAsync()
    {
        try
        {
            var httpClient = await GetHttpClientAsync();
            var httpResponse = await httpClient.GetAsync("/api/Auth/test-auth");
            
            _logger.LogDebug("Token validation response: {StatusCode}", httpResponse.StatusCode);
            return httpResponse.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Token validation failed: {Error}", ex.Message);
            return false;
        }
    }

    public async Task<UserProfileDto?> GetCurrentUserAsync()
    {
        try
        {
            // ? Utiliser le nouvel endpoint /api/User/profile du backend
            var httpClient = await GetHttpClientAsync();
            var httpResponse = await httpClient.GetAsync("/api/User/profile");
            
            if (httpResponse.IsSuccessStatusCode)
            {
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                var userProfile = JsonSerializer.Deserialize<CurrentUserDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (userProfile != null)
                {
                    // Convertir CurrentUserDto vers UserProfileDto
                    var profile = new UserProfileDto(
                        userProfile.Id,
                        userProfile.Email ?? "",
                        userProfile.FirstName,
                        userProfile.LastName,
                        userProfile.Roles ?? new List<string>()
                    );
                
                    _logger.LogDebug("?? Profil utilisateur récupéré: {Email}, Nom: {FirstName} {LastName}, Rôles: {Roles}", 
                        userProfile.Email, userProfile.FirstName, userProfile.LastName, string.Join(", ", userProfile.Roles));

                    return profile;
                }
            }
            
            if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Token expiré, déconnecter l'utilisateur
                await LogoutAsync();
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la récupération du profil utilisateur");
            return null;
        }
    }

    /// <summary>
    /// Récupère l'ID de l'utilisateur connecté depuis le JWT
    /// </summary>
    public async Task<string?> GetCurrentUserIdAsync()
    {
        try
        {
            var user = await GetCurrentUserAsync();
            return user?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de la récupération de l'ID utilisateur");
            return null;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            _logger.LogInformation("?? Déconnexion de l'utilisateur");
            
            // Nettoyer les tokens
            await _tokenProvider.ClearTokenAsync();
            
            // Mettre à jour l'état d'authentification
            if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
            {
                await customProvider.MarkUserAsLoggedOut();
            }
            
            // Nettoyer l'état de l'application
            await _appState.SetUserAsync(null);
            _appState.ClearState();
            
            await _appState.ShowNotificationAsync("Déconnexion réussie", NotificationType.Info);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la déconnexion");
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User.Identity?.IsAuthenticated == true;
    }

    public async Task<bool> IsInRoleAsync(string role)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User.IsInRole(role);
    }

    private async Task<HttpClient> GetHttpClientAsync()
    {
        var httpClient = new HttpClient();
        
        // Configuration du timeout
        httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        // URL de votre API backend depuis la configuration
        var apiBaseUrl = _configuration["ApiConfiguration:BaseUrl"] ?? "https://localhost:7144/api";
        // Retirer /api car on l'ajoute dans les endpoints
        var baseUrl = apiBaseUrl.Replace("/api", "");
        httpClient.BaseAddress = new Uri(baseUrl);
        
        // Headers par défaut
        httpClient.DefaultRequestHeaders.Add("User-Agent", "SmartOffice-Blazor/1.0");
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        
        // Ajouter le token d'autorisation si disponible (pour les appels authentifiés)
        var token = await _tokenProvider.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            _logger.LogDebug("?? Token ajouté aux headers d'autorisation");
        }
        else
        {
            _logger.LogDebug("?? Aucun token disponible pour l'autorisation");
        }
        
        _logger.LogDebug("?? HttpClient configuré pour {BaseAddress}", httpClient.BaseAddress);
        
        return httpClient;
    }

    private static async Task<string> ExtractBadRequestMessage(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            
            // Essayer de parser comme JSON d'abord
            if (content.StartsWith("[") || content.StartsWith("{"))
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(content);
                
                // Si c'est un tableau d'erreurs (comme retourné par le backend)
                if (jsonElement.ValueKind == JsonValueKind.Array)
                {
                    var errors = jsonElement.EnumerateArray()
                        .Where(e => e.ValueKind == JsonValueKind.String)
                        .Select(e => e.GetString())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
                    
                    return errors.Any() ? string.Join(", ", errors) : "Données invalides";
                }
                
                // Si c'est un objet avec des erreurs de validation
                if (jsonElement.TryGetProperty("errors", out var errorsProperty))
                {
                    var errorMessages = new List<string>();
                    foreach (var property in errorsProperty.EnumerateObject())
                    {
                        if (property.Value.ValueKind == JsonValueKind.Array)
                        {
                            var messages = property.Value.EnumerateArray()
                                .Where(e => e.ValueKind == JsonValueKind.String)
                                .Select(e => e.GetString())
                                .Where(s => !string.IsNullOrEmpty(s));
                            errorMessages.AddRange(messages!);
                        }
                    }
                    return errorMessages.Any() ? string.Join(", ", errorMessages) : "Données invalides";
                }
            }
            
            // Si ce n'est pas du JSON, retourner le contenu tel quel
            return !string.IsNullOrEmpty(content) ? content : "Données invalides";
        }
        catch
        {
            return "Erreur de validation";
        }
    }
}

// Résultats d'authentification
public record LoginResult(bool Success, string? ErrorMessage, AuthResponseDto? Data = null);

public record RegisterResult(
    bool Success, 
    string? ErrorMessage, 
    Dictionary<string, string[]>? ValidationErrors = null);