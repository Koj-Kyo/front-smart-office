namespace MySmartApp.Ui.Models;

// ==========================================
// DTOs d'authentification (correspondant au backend)
// ==========================================

/// <summary>
/// DTO pour la connexion (correspond à LoginDto du backend)
/// </summary>
public record LoginDto(string Email, string Password);

/// <summary>
/// DTO de réponse d'authentification (correspond à AuthResponseDto du backend)
/// </summary>
public record AuthResponseDto(
    string AccessToken,
    string? RefreshToken = null,
    string? UserId = null,
    string? Email = null,
    List<string>? Roles = null
);

/// <summary>
/// DTO pour l'inscription (correspond à RegisterDto du backend)
/// </summary>
public record RegisterDto(
    string Email, 
    string Password, 
    string ConfirmPassword,
    string? FirstName = null,
    string? LastName = null
);

// ==========================================
// Wrapper pour les réponses API
// ==========================================

public record ApiResponse<T>(
    bool IsSuccess,
    T? Data,
    string? ErrorMessage = null,
    int StatusCode = 200,
    Dictionary<string, string[]>? ValidationErrors = null
)
{
    public static ApiResponse<T> Success(T data, int statusCode = 200) =>
        new(true, data, null, statusCode);

    public static ApiResponse<T> Error(string errorMessage, int statusCode = 400, Dictionary<string, string[]>? validationErrors = null) =>
        new(false, default, errorMessage, statusCode, validationErrors);
};

// ==========================================
// Modèles métier
// ==========================================

public record ZoneDto(
    int Id, 
    string Name, 
    string? Description = null,
    List<DeviceDto>? Devices = null
);

public record DeviceDto(
    int Id, 
    string Name, 
    string? Description = null,
    string? DeviceType = null,
    int? ZoneId = null,
    string? ZoneName = null,
    bool? IsConnected = null,
    DateTime? LastSeen = null
);

public record UserProfileDto(
    string Id,
    string Email,
    string? FirstName,
    string? LastName,
    List<string> Roles
);

public record AccessRightDto(
    int Id,
    string UserId,
    int? ZoneId,
    int? PeripheriqueId,
    string AccessType,
    DateTime CreatedAt
);

// ==========================================
// Modèles IoT Hub
// ==========================================

public record IoTResponse(
    bool Success,
    string Message
);

public record DeviceTwinDto(
    string DeviceId,
    object? Properties,
    object? Tags
);

// ==========================================
// Modèles SignalR
// ==========================================

public record TelemetryData(
    string DeviceId,
    double Value,
    string Unit,
    DateTime Timestamp
);

public record DeviceStatusUpdate(
    string DeviceId,
    string Status,
    DateTime Timestamp
);

public record NotificationDto(
    string Message,
    string Type,
    DateTime Timestamp
);

// ==========================================
// Énumérations pour UI
// ==========================================

public enum ErrorSeverity
{
    Info,
    Success,
    Warning,
    Error
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}