using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public class BrowserCompatibleSignalRService : ISignalRService, IAsyncDisposable
{
    private readonly ITokenProvider _tokenProvider;
    private readonly IConfiguration _configuration;
    private HubConnection? _hubConnection;

    public BrowserCompatibleSignalRService(ITokenProvider tokenProvider, IConfiguration configuration)
    {
        _tokenProvider = tokenProvider;
        _configuration = configuration;
    }

    public bool IsConnected => 
        _hubConnection?.State == HubConnectionState.Connected;
    
    public HubConnectionState ConnectionState => 
        _hubConnection?.State ?? HubConnectionState.Disconnected;

    public event Action<HubConnectionState>? ConnectionStateChanged;
    public event Action<TelemetryData>? OnTelemetryUpdate;
    public event Action<DeviceStatusUpdate>? OnDeviceStatusUpdate;
    public event Action<NotificationDto>? OnNotificationReceived;
    public event Action<string>? OnIncidentAlert;
    public event Action<string>? OnCommandExecutionResult;

    public async Task StartAsync()
    {
        try
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }

            var hubUrl = _configuration["SignalR:HubUrl"] ?? "https://localhost:7144/notificationhub";
            var token = await _tokenProvider.GetTokenAsync();

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        options.AccessTokenProvider = () => Task.FromResult<string?>(token);
                    }
                    // Configuration compatible navigateur - pas de manipulation SSL
                    options.Headers["User-Agent"] = "MySmartApp-Blazor";
                })
                .WithAutomaticReconnect(new[] { 
                    TimeSpan.Zero, 
                    TimeSpan.FromSeconds(2), 
                    TimeSpan.FromSeconds(10), 
                    TimeSpan.FromSeconds(30) 
                })
                .Build();

            // Événements de connexion
            _hubConnection.Closed += (error) =>
            {
                ConnectionStateChanged?.Invoke(HubConnectionState.Disconnected);
                return Task.CompletedTask;
            };

            _hubConnection.Reconnecting += (error) =>
            {
                ConnectionStateChanged?.Invoke(HubConnectionState.Reconnecting);
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += (connectionId) =>
            {
                ConnectionStateChanged?.Invoke(HubConnectionState.Connected);
                return Task.CompletedTask;
            };

            // Configuration des événements avec gestion d'erreur
            _hubConnection.On<TelemetryData>("TelemetryUpdate", data =>
            {
                try
                {
                    OnTelemetryUpdate?.Invoke(data);
                }
                catch (Exception)
                {
                    // Ignorer les erreurs pour éviter les cycles de reload
                }
            });

            _hubConnection.On<DeviceStatusUpdate>("DeviceStatusUpdate", update =>
            {
                try
                {
                    OnDeviceStatusUpdate?.Invoke(update);
                }
                catch (Exception)
                {
                    // Ignorer les erreurs pour éviter les cycles de reload
                }
            });

            _hubConnection.On<NotificationDto>("ReceiveNotification", notification =>
            {
                try
                {
                    OnNotificationReceived?.Invoke(notification);
                }
                catch (Exception)
                {
                    // Ignorer les erreurs pour éviter les cycles de reload
                }
            });

            _hubConnection.On<string>("IncidentAlert", message =>
            {
                try
                {
                    OnIncidentAlert?.Invoke(message);
                }
                catch (Exception)
                {
                    // Ignorer les erreurs pour éviter les cycles de reload
                }
            });

            _hubConnection.On<string>("CommandExecutionResult", result =>
            {
                try
                {
                    OnCommandExecutionResult?.Invoke(result);
                }
                catch (Exception)
                {
                    // Ignorer les erreurs pour éviter les cycles de reload
                }
            });

            await _hubConnection.StartAsync();
            
            // Notifier la connexion établie
            ConnectionStateChanged?.Invoke(HubConnectionState.Connected);
        }
        catch (Exception)
        {
            // Ne pas propager l'erreur - l'app peut fonctionner sans SignalR
            // Ignorer silencieusement pour éviter les cycles de reload
        }
    }

    public async Task StopAsync()
    {
        if (_hubConnection != null)
        {
            try
            {
                await _hubConnection.StopAsync();
            }
            catch (Exception)
            {
                // Ignorer les erreurs de déconnexion
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}