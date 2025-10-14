using System.Collections.Concurrent;
using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

/// <summary>
/// Service d'état global de l'application avec gestion des erreurs et notifications
/// </summary>
public interface IAppStateService
{
    // États généraux
    bool IsLoading { get; }
    bool IsOnline { get; }
    string? CurrentError { get; }
    
    // Données utilisateur
    UserProfileDto? CurrentUser { get; }
    List<string> UserRoles { get; }
    bool IsAdmin { get; }
    
    // Données métier
    IReadOnlyList<ZoneDto> Zones { get; }
    IReadOnlyList<DeviceDto> Devices { get; }
    
    // Événements
    event Action? OnStateChanged;
    event Action<string>? OnError;
    event Action<string>? OnNotification;
    
    // Actions
    Task InitializeAsync();
    Task SetLoadingStateAsync(bool isLoading);
    Task SetErrorAsync(string? error);
    Task SetUserAsync(UserProfileDto? user);
    Task UpdateZonesAsync(IEnumerable<ZoneDto> zones);
    Task UpdateDevicesAsync(IEnumerable<DeviceDto> devices);
    Task ShowNotificationAsync(string message, NotificationType type = NotificationType.Info);
    Task RefreshDataAsync();
    void ClearState();
}

public class AppStateService : IAppStateService, IDisposable
{
    private readonly ILogger<AppStateService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ConcurrentDictionary<string, object> _cache = new();
    
    // États internes
    private bool _isLoading;
    private bool _isOnline = true;
    private string? _currentError;
    private UserProfileDto? _currentUser;
    private List<ZoneDto> _zones = new();
    private List<DeviceDto> _devices = new();

    public AppStateService(ILogger<AppStateService> logger)
    {
        _logger = logger;
    }

    // Propriétés publiques
    public bool IsLoading => _isLoading;
    public bool IsOnline => _isOnline;
    public string? CurrentError => _currentError;
    public UserProfileDto? CurrentUser => _currentUser;
    public List<string> UserRoles => _currentUser?.Roles ?? new List<string>();
    public bool IsAdmin => UserRoles.Contains("Admin");
    public IReadOnlyList<ZoneDto> Zones => _zones.AsReadOnly();
    public IReadOnlyList<DeviceDto> Devices => _devices.AsReadOnly();

    // Événements
    public event Action? OnStateChanged;
    public event Action<string>? OnError;
    public event Action<string>? OnNotification;

    public async Task InitializeAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            _logger.LogInformation("?? Initialisation de l'état de l'application...");
            
            // Initialisation des états par défaut
            _isLoading = false;
            _isOnline = true;
            _currentError = null;
            
            // Vérification de la connectivité
            await CheckConnectivityAsync();
            
            _logger.LogInformation("? État de l'application initialisé");
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erreur lors de l'initialisation de l'état");
            await SetErrorAsync($"Erreur d'initialisation: {ex.Message}");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SetLoadingStateAsync(bool isLoading)
    {
        if (_isLoading != isLoading)
        {
            _isLoading = isLoading;
            _logger.LogDebug("État de chargement: {IsLoading}", isLoading);
            
            await Task.Run(() => NotifyStateChanged());
        }
    }

    public async Task SetErrorAsync(string? error)
    {
        _currentError = error;
        
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogError("? Erreur dans l'état: {Error}", error);
            OnError?.Invoke(error);
        }
        
        await Task.Run(() => NotifyStateChanged());
    }

    public async Task SetUserAsync(UserProfileDto? user)
    {
        await _semaphore.WaitAsync();
        try
        {
            _currentUser = user;
            
            if (user != null)
            {
                _logger.LogInformation("?? Utilisateur connecté: {Email} (Rôles: {Roles})", 
                    user.Email, string.Join(", ", user.Roles));
            }
            else
            {
                _logger.LogInformation("?? Utilisateur déconnecté");
                ClearUserData();
            }
            
            NotifyStateChanged();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task UpdateZonesAsync(IEnumerable<ZoneDto> zones)
    {
        await _semaphore.WaitAsync();
        try
        {
            _zones = zones.ToList();
            _cache["zones_updated"] = DateTime.UtcNow;
            
            _logger.LogDebug("?? {Count} zones mises à jour", _zones.Count);
            NotifyStateChanged();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task UpdateDevicesAsync(IEnumerable<DeviceDto> devices)
    {
        await _semaphore.WaitAsync();
        try
        {
            _devices = devices.ToList();
            _cache["devices_updated"] = DateTime.UtcNow;
            
            _logger.LogDebug("?? {Count} appareils mis à jour", _devices.Count);
            NotifyStateChanged();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ShowNotificationAsync(string message, NotificationType type = NotificationType.Info)
    {
        var emoji = type switch
        {
            NotificationType.Success => "?",
            NotificationType.Warning => "??",
            NotificationType.Error => "?",
            _ => "??"
        };

        _logger.LogInformation("{Emoji} Notification {Type}: {Message}", emoji, type, message);
        
        await Task.Run(() => OnNotification?.Invoke($"{emoji} {message}"));
    }

    public async Task RefreshDataAsync()
    {
        await SetLoadingStateAsync(true);
        try
        {
            // Cette méthode sera implémentée pour recharger les données depuis l'API
            _logger.LogInformation("?? Actualisation des données...");
            
            // Ici, vous pourrez appeler vos services pour recharger zones, devices, etc.
            
            await ShowNotificationAsync("Données actualisées", NotificationType.Success);
        }
        catch (Exception ex)
        {
            await SetErrorAsync($"Erreur lors de l'actualisation: {ex.Message}");
        }
        finally
        {
            await SetLoadingStateAsync(false);
        }
    }

    public void ClearState()
    {
        _logger.LogInformation("?? Nettoyage de l'état de l'application");
        
        _currentUser = null;
        _zones.Clear();
        _devices.Clear();
        _cache.Clear();
        _currentError = null;
        
        NotifyStateChanged();
    }

    private void ClearUserData()
    {
        _zones.Clear();
        _devices.Clear();
        _cache.Clear();
    }

    private async Task CheckConnectivityAsync()
    {
        try
        {
            // Simulation d'une vérification de connectivité
            // En WebAssembly, on peut vérifier via une requête HTTP simple
            _isOnline = true;
        }
        catch
        {
            _isOnline = false;
            await ShowNotificationAsync("Connexion réseau indisponible", NotificationType.Warning);
        }
    }

    private void NotifyStateChanged()
    {
        try
        {
            OnStateChanged?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la notification de changement d'état");
        }
    }

    public void Dispose()
    {
        _semaphore?.Dispose();
        GC.SuppressFinalize(this);
    }
}