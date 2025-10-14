using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using MySmartApp.Ui;
using MySmartApp.Ui.Services;
using MySmartApp.Ui.Configuration;
using Polly;
using Polly.Extensions.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// ?? 1. COMPOSANTS RACINE
builder.RootComponents.Add<App>("#app");

// ?? 2. LOGGING ET CONFIGURATION
ConfigureLogging(builder);
ConfigureConfiguration(builder);

// ?? 3. SERVICES UI
ConfigureUIServices(builder);

// ?? 4. SERVICES MÉTIER
ConfigureBusinessServices(builder);

// ?? 5. SERVICES HTTP ET API
ConfigureHttpServices(builder);

// ?? 6. SERVICES D'AUTHENTIFICATION
ConfigureAuthenticationServices(builder);

// ?? 7. SERVICES TEMPS RÉEL
ConfigureSignalRServices(builder);

var app = builder.Build();

// ?? DÉMARRAGE AVEC GESTION D'ERREURS
try
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("?? Démarrage de {AppName}...", "Smart Office");
    
    await app.RunAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"? Erreur critique de démarrage: {ex.Message}");
    Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
    throw;
}

// ?? MÉTHODES DE CONFIGURATION

static void ConfigureLogging(WebAssemblyHostBuilder builder)
{
    // Le logging console est automatiquement configuré dans WebAssembly
    // Configuration du niveau de log selon l'environnement
    if (builder.HostEnvironment.IsDevelopment())
    {
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
    }
    else
    {
        builder.Logging.SetMinimumLevel(LogLevel.Information);
    }
}

static void ConfigureConfiguration(WebAssemblyHostBuilder builder)
{
    // Service de configuration centralisé
    builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
    
    // Configuration basique sans validation avancée pour WebAssembly
    builder.Services.Configure<AppConfiguration>(builder.Configuration);
}

static void ConfigureUIServices(WebAssemblyHostBuilder builder)
{
    // MudBlazor avec configuration optimisée
    builder.Services.AddMudServices(config =>
    {
        config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;
        config.SnackbarConfiguration.PreventDuplicates = true;
        config.SnackbarConfiguration.NewestOnTop = false;
        config.SnackbarConfiguration.ShowCloseIcon = true;
        config.SnackbarConfiguration.VisibleStateDuration = 5000;
        config.SnackbarConfiguration.HideTransitionDuration = 500;
        config.SnackbarConfiguration.ShowTransitionDuration = 500;
    });

    // Services UI
    builder.Services.AddScoped<IThemeService, ThemeService>();
    builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
}

static void ConfigureBusinessServices(WebAssemblyHostBuilder builder)
{
    // Service d'état global de l'application
    builder.Services.AddSingleton<IAppStateService, AppStateService>();
    
    // Service de gestion d'erreurs global
    builder.Services.AddScoped<IErrorHandlerService, ErrorHandlerService>();
    
    // Services métier spécialisés
    builder.Services.AddScoped<IZoneService, ZoneService>();
    builder.Services.AddScoped<IDeviceService, DeviceService>();
    // builder.Services.AddScoped<ITelemetryService, TelemetryService>();
}

static void ConfigureHttpServices(WebAssemblyHostBuilder builder)
{
    // HttpClientFactory de base
    builder.Services.AddHttpClient();
    
    // Client HTTP principal avec Polly pour la résilience
    builder.Services.AddHttpClient("Api", (serviceProvider, client) =>
    {
        var config = serviceProvider.GetRequiredService<IConfigurationService>();
        var apiConfig = config.Configuration.Api;
        
        client.BaseAddress = new Uri(apiConfig.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(apiConfig.TimeoutSeconds);
        
        // Headers par défaut
        client.DefaultRequestHeaders.Add("User-Agent", $"SmartOffice-WebApp/{config.Configuration.App.Version}");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    })
    .AddHttpMessageHandler<AuthMessageHandler>();
    // Note: AddPolicyHandler n'est pas disponible dans WebAssembly
    // Les policies Polly sont gérées directement dans ApiService

    // Service API robuste
    builder.Services.AddScoped<IApiService, ApiService>();
}

static void ConfigureAuthenticationServices(WebAssemblyHostBuilder builder)
{
    // Services d'authentification dans l'ordre correct
    builder.Services.AddAuthorizationCore();
    
    // Token management
    builder.Services.AddScoped<ITokenProvider, TokenProvider>();
    builder.Services.AddScoped<AuthMessageHandler>();
    
    // Authentication state provider personnalisé
    builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
    
    // Service d'authentification
    builder.Services.AddScoped<IAuthService, AuthService>();
}

static void ConfigureSignalRServices(WebAssemblyHostBuilder builder)
{
    // Service SignalR compatible navigateur
    builder.Services.AddScoped<ISignalRService, BrowserCompatibleSignalRService>();
}

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .Or<TaskCanceledException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Console.WriteLine($"?? Retry {retryCount} dans {timespan.TotalSeconds}s");
            });
}

static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
{
    return Policy.TimeoutAsync<HttpResponseMessage>(10); // Timeout individuel par requête
}
