using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public class SignalRService : ISignalRService, IAsyncDisposable
{
    private readonly ITokenProvider _tokenProvider;
    private readonly IConfiguration _configuration;
    private HubConnection? _hubConnection;

    public SignalRService(ITokenProvider tokenProvider, IConfiguration configuration)
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
                        // Note: HttpMessageHandlerFactory avec ServerCertificateCustomValidationCallback 
                        // n'est pas supporté dans Blazor WebAssembly (navigateur)
                        // La validation SSL est gérée automatiquement par le navigateur
                    })
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
                .Build();

            // Configuration des événements de connexion
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

            // Configuration des événements de données
            _hubConnection.On<TelemetryData>("TelemetryUpdate", data =>
            {
                try
                {
                    OnTelemetryUpdate?.Invoke(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur TelemetryUpdate: {ex.Message}");
                }
            });

            _hubConnection.On<DeviceStatusUpdate>("DeviceStatusUpdate", update =>
            {
                try
                {
                    OnDeviceStatusUpdate?.Invoke(update);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur DeviceStatusUpdate: {ex.Message}");
                }
            });

            _hubConnection.On<NotificationDto>("ReceiveNotification", notification =>
            {
                try
                {
                    OnNotificationReceived?.Invoke(notification);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur ReceiveNotification: {ex.Message}");
                }
            });

            _hubConnection.On<string>("IncidentAlert", message =>
            {
                try
                {
                    OnIncidentAlert?.Invoke(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur IncidentAlert: {ex.Message}");
                }
            });

            _hubConnection.On<string>("CommandExecutionResult", result =>
            {
                try
                {
                    OnCommandExecutionResult?.Invoke(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur CommandExecutionResult: {ex.Message}");
                }
            });

            await _hubConnection.StartAsync();
            Console.WriteLine("SignalR connecté avec succès");
            
            // Notifier la connexion établie
            ConnectionStateChanged?.Invoke(HubConnectionState.Connected);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur de connexion SignalR: {ex.Message}");
            // Ne pas propager l'erreur pour éviter de casser l'application
        }
    }

    public async Task StopAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
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