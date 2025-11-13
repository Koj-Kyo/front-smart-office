using Microsoft.AspNetCore.SignalR.Client;
using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public interface ISignalRService
{
    Task StartAsync();
    Task StopAsync();
    bool IsConnected { get; }
    HubConnectionState ConnectionState { get; }
    
    // Événements de connexion
    event Action<HubConnectionState>? ConnectionStateChanged;
    
    // Événements de données
    event Action<TelemetryData>? OnTelemetryUpdate;
    event Action<DeviceStatusUpdate>? OnDeviceStatusUpdate;
    event Action<SignalRNotificationDto>? OnNotificationReceived;
    event Action<string>? OnIncidentAlert;
    event Action<string>? OnCommandExecutionResult;
}