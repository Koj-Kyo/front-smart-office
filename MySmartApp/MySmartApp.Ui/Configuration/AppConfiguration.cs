namespace MySmartApp.Ui.Configuration;

/// <summary>
/// Configuration centralis�e de l'application avec validation et bonnes pratiques
/// </summary>
public class AppConfiguration
{
    public ApiConfiguration Api { get; set; } = new();
    public AppSettings App { get; set; } = new();
    public FeatureFlags Features { get; set; } = new();
    public LoggingConfiguration Logging { get; set; } = new();
}

public class ApiConfiguration
{
    public string BaseUrl { get; set; } = "https://localhost:7144/api";
    public string SwaggerUrl { get; set; } = "https://localhost:7144/swagger";
    public string SignalRUrl { get; set; } = "https://localhost:7144/notificationhub";
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 2;
    
    // Validation des URLs
    public bool IsValid()
    {
        return Uri.TryCreate(BaseUrl, UriKind.Absolute, out _) &&
               Uri.TryCreate(SwaggerUrl, UriKind.Absolute, out _) &&
               Uri.TryCreate(SignalRUrl, UriKind.Absolute, out _) &&
               TimeoutSeconds > 0 && RetryCount >= 0;
    }
}

public class AppSettings
{
    public string AppName { get; set; } = "Smart Office";
    public string Version { get; set; } = "1.0.0";
    public string Environment { get; set; } = "Production";
    public bool EnableAnalytics { get; set; } = true;
    public int CacheTimeoutMinutes { get; set; } = 10;
    public int SessionTimeoutMinutes { get; set; } = 60;
}

public class FeatureFlags
{
    public bool EnableRealTimeUpdates { get; set; } = true;
    public bool EnableOfflineMode { get; set; } = false;
    public bool EnableDarkModeToggle { get; set; } = true;
    public bool EnableDeveloperTools { get; set; } = false;
}

public class LoggingConfiguration
{
    public Dictionary<string, string> LogLevel { get; set; } = new();
}

/// <summary>
/// Service de configuration avec cache et validation
/// </summary>
public interface IConfigurationService
{
    AppConfiguration Configuration { get; }
    T GetSection<T>(string sectionName) where T : class, new();
    bool IsFeatureEnabled(string featureName);
    Task RefreshConfigurationAsync();
}

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationService> _logger;
    private AppConfiguration? _cachedConfiguration;
    private readonly object _lock = new();

    public ConfigurationService(IConfiguration configuration, ILogger<ConfigurationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public AppConfiguration Configuration
    {
        get
        {
            if (_cachedConfiguration == null)
            {
                lock (_lock)
                {
                    _cachedConfiguration ??= LoadConfiguration();
                }
            }
            return _cachedConfiguration;
        }
    }

    public T GetSection<T>(string sectionName) where T : class, new()
    {
        try
        {
            var section = _configuration.GetSection(sectionName);
            var result = new T();
            section.Bind(result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erreur lors du chargement de la section {SectionName}, utilisation des valeurs par d�faut", sectionName);
            return new T();
        }
    }

    public bool IsFeatureEnabled(string featureName)
    {
        try
        {
            var featureSection = _configuration.GetSection("FeatureFlags");
            return featureSection.GetValue<bool>(featureName, false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erreur lors de la v�rification du feature flag {FeatureName}", featureName);
            return false;
        }
    }

    public async Task RefreshConfigurationAsync()
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                _cachedConfiguration = null;
                _logger.LogInformation("Configuration rafra�chie");
            }
        });
    }

    private AppConfiguration LoadConfiguration()
    {
        try
        {
            var config = new AppConfiguration();
            
            _configuration.GetSection("ApiConfiguration").Bind(config.Api);
            _configuration.GetSection("AppConfiguration").Bind(config.App);
            _configuration.GetSection("FeatureFlags").Bind(config.Features);
            _configuration.GetSection("Logging").Bind(config.Logging);

            // Validation
            if (!config.Api.IsValid())
            {
                _logger.LogWarning("Configuration API invalide, utilisation des valeurs par d�faut");
                config.Api = new ApiConfiguration();
            }

            _logger.LogInformation("Configuration charg�e: {AppName} v{Version} ({Environment})", 
                config.App.AppName, config.App.Version, config.App.Environment);

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du chargement de la configuration, utilisation des valeurs par d�faut");
            return new AppConfiguration();
        }
    }
}