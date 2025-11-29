using Microsoft.AspNetCore.SignalR.Client;
using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public interface ISignalRService
{
    Task StartAsync();
    Task StopAsync();
    bool IsConnected { get; }
    HubConnectionState ConnectionState { get; }
    
    // �v�nements de connexion
    event Action<HubConnectionState>? ConnectionStateChanged;
    
    // �v�nements de donn�es
    event Action<TelemetryData>? OnTelemetryUpdate;
    event Action<DeviceStatusUpdate>? OnDeviceStatusUpdate;
    event Action<SignalRNotificationDto>? OnNotificationReceived;
    event Action<string>? OnIncidentAlert;
    event Action<string>? OnCommandExecutionResult;
}