using System.Net.Http;
using System.Text.Json;
using System.Text;
using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public interface ICommandeService
{
    Task<CommandeResult> SendCommandAsync(int peripheriqueId, string nomCommande, string? valeur = null);
    Task<CommandeResult> ToggleDeviceAsync(int peripheriqueId, bool isOn);
    Task<CommandeResult> SetDeviceValueAsync(int peripheriqueId, string parametre, string valeur);
    Task<CommandeResult> SetTemperatureAsync(int peripheriqueId, int temperature);
    Task<CommandeResult> SetBrightnessAsync(int peripheriqueId, int brightness);
}

public class CommandeService : ICommandeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CommandeService> _logger;
    private readonly ITokenProvider _tokenProvider;
    private readonly IAppStateService _appState;

    public CommandeService(
        HttpClient httpClient, 
        ILogger<CommandeService> logger, 
        ITokenProvider tokenProvider,
        IAppStateService appState)
    {
        _httpClient = httpClient;
        _logger = logger;
        _tokenProvider = tokenProvider;
        _appState = appState;
    }

    public async Task<CommandeResult> SendCommandAsync(int peripheriqueId, string nomCommande, string? valeur = null)
    {
        try
        {
            _logger.LogInformation("?? Envoi commande: {NomCommande} vers p�riph�rique {Id} avec valeur {Valeur}", 
                nomCommande, peripheriqueId, valeur);

            await EnsureAuthenticatedAsync();

            var request = new CommandeRequestDto(peripheriqueId, nomCommande, valeur);
            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // ✅ Endpoint corrigé: /api/Commande (singulier, pas pluriel)
            var response = await _httpClient.PostAsync("/api/Commande", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var commandeResponse = JsonSerializer.Deserialize<CommandeResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (commandeResponse?.IsSuccess == true)
                {
                    _logger.LogInformation("✅ Commande envoyée avec succès: {Message}", commandeResponse.Message);
                    await _appState.ShowNotificationAsync($"Commande '{nomCommande}' exécutée avec succès", NotificationType.Success);
                    
                    // ✅ Retourner le bon format pour le Dashboard
                    return new CommandeResult
                    {
                        IsSuccess = true,
                        Message = commandeResponse.Message,
                        CommandeId = commandeResponse.CommandeId
                    };
                }
                else
                {
                    _logger.LogWarning("⚠️ Commande échouée: {Message}", commandeResponse?.Message);
                    await _appState.ShowNotificationAsync($"Échec de la commande: {commandeResponse?.Message}", NotificationType.Warning);
 
                    return new CommandeResult
                    {
                        IsSuccess = false,
                        Message = commandeResponse?.Message ?? "Réponse invalide",
                        CommandeId = commandeResponse?.CommandeId
                    };
                }
            }
            else
            {
                _logger.LogWarning("? �chec envoi commande: {StatusCode} - {Error}", response.StatusCode, responseContent);

                // Essayer de parser comme CommandeResponseDto d'erreur du backend
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<CommandeResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (errorResponse != null)
                    {
                        await _appState.ShowNotificationAsync(errorResponse.Message, NotificationType.Error);
                        return new CommandeResult
                        {
                            IsSuccess = errorResponse.IsSuccess,
                            Message = errorResponse.Message,
                            CommandeId = errorResponse.CommandeId
                        };
                    }
                }
                catch
                {
                    // Pas un CommandeResponseDto, traiter comme erreur HTTP standard
                }

                var errorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized => "Non autorisé. Veuillez vous reconnecter.",
                    System.Net.HttpStatusCode.BadRequest => responseContent,
                    System.Net.HttpStatusCode.NotFound => "Périphérique non trouvé",
                    System.Net.HttpStatusCode.InternalServerError => "Erreur serveur",
                    _ => $"Erreur HTTP {response.StatusCode}"
                };

                await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
                return new CommandeResult
                {
                    IsSuccess = false,
                    Message = errorMessage
                };
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("❌ Erreur réseau lors de l'envoi de commande: {Message}", ex.Message);
            var errorMessage = "Erreur de connexion au serveur";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return new CommandeResult
            {
                IsSuccess = false,
                Message = errorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Exception lors de l'envoi de commande: {Message}", ex.Message);
            var errorMessage = $"Erreur technique: {ex.Message}";
            await _appState.ShowNotificationAsync(errorMessage, NotificationType.Error);
            return new CommandeResult
            {
                IsSuccess = false,
                Message = errorMessage
            };
        }
    }

    public async Task<CommandeResult> ToggleDeviceAsync(int peripheriqueId, bool isOn)
    {
        var commande = isOn ? "TurnOn" : "TurnOff";
        return await SendCommandAsync(peripheriqueId, commande);
    }

    public async Task<CommandeResult> SetDeviceValueAsync(int peripheriqueId, string parametre, string valeur)
    {
        return await SendCommandAsync(peripheriqueId, $"Set{parametre}", valeur);
    }

    public async Task<CommandeResult> SetTemperatureAsync(int peripheriqueId, int temperature)
    {
        return await SendCommandAsync(peripheriqueId, "SetTemperature", temperature.ToString());
    }

    public async Task<CommandeResult> SetBrightnessAsync(int peripheriqueId, int brightness)
    {
        return await SendCommandAsync(peripheriqueId, "SetBrightness", brightness.ToString());
    }

    private async Task EnsureAuthenticatedAsync()
    {
        var token = await _tokenProvider.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _logger.LogWarning("?? Aucun token disponible pour l'authentification");
        }
    }
}