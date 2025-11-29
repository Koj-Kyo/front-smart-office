using System.Text.Json.Serialization;

namespace MySmartApp.Ui.Models;

// ==========================================
// DTOs d'authentification (correspondant au backend)
// ==========================================

/// <summary>
/// DTO pour la connexion (correspond � LoginDto du backend)
/// </summary>
public record LoginDto(string Email, string Password);

/// <summary>
/// DTO de r�ponse d'authentification (correspond � AuthResponseDto du backend)
/// </summary>
public record AuthResponseDto(
    string AccessToken,
    string? RefreshToken = null,
    string? UserId = null,
    string? Email = null,
    List<string>? Roles = null
);

/// <summary>
/// DTO pour l'inscription (correspond � RegisterDto du backend)
/// </summary>
public record RegisterDto(
    string Email, 
    string Password, 
    string ConfirmPassword,
    string? FirstName = null,
    string? LastName = null
);

// ==========================================
// DTOs Dashboard (correspondant au backend DashboardController)
// ==========================================

/// <summary>
/// DTO pour les statistiques KPI du Dashboard
/// </summary>
public record DashboardStats
{
    // Appareils
    public int ActiveDevices { get; set; }
    public int TotalDevices { get; set; }
    public double DevicesChangePercent { get; set; }
    
    // Utilisateurs
    public int ActiveUsers { get; set; }
  public int TotalUsers { get; set; }
    public double UsersChangePercent { get; set; }
    
    // Incidents
    public int OpenIncidents { get; set; }
  public int TotalIncidents { get; set; }
    public double IncidentsChangePercent { get; set; }
    
    // �nergie
    public double EnergyConsumed { get; set; }
    public double EnergyChangePercent { get; set; }
}

/// <summary>
/// DTO pour les points de donn�es de consommation �nerg�tique
/// </summary>
public record EnergyDataPoint
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
}

/// <summary>
/// DTO pour l'activit� des appareils par type
/// </summary>
public record DeviceActivityData
{
public string DeviceType { get; set; } = string.Empty;
    public int Count { get; set; }
}

/// <summary>
/// DTO pour le profil utilisateur actuel (Dashboard)
/// </summary>
public record CurrentUserDto
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// Wrapper g�n�rique pour les r�ponses API backend Dashboard
/// </summary>
public record DashboardApiResponse<T>
{
    public T? Data { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// DTO pour r�sultat d'envoi de commande
/// </summary>
public record CommandeResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? CommandeId { get; set; }
}

// ==========================================
// Wrapper pour les r�ponses API
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
// Mod�les m�tier
// ==========================================

public record ZoneDto(
    int Id, 
    string Name, 
    string? Description = null,
    List<DeviceDto>? Devices = null
);

// ==========================================
// DTOs Zones (correspondant au backend ZonesController)
// ==========================================

/// <summary>
/// DTO Zone principal du backend (avec Nom au lieu de Name)
/// </summary>
public record BackendZoneDto(
    int Id,
    string Nom
);

/// <summary>
/// DTO pour cr�ation de zone
/// </summary>
public record ZoneCreationDto(
    string Nom
);

/// <summary>
/// DTO pour mise � jour de zone
/// </summary>
public record ZoneUpdateDto(
    string Nom
);

/// <summary>
/// DTO �tendu pour les zones avec statistiques
/// </summary>
public record ZoneExtendedDto(
    int Id,
    string Nom,
    int PeripheriquesCount,
    int UtilisateursCount,
    DateTime? DerniereActivite = null,
    List<PeripheriqueLogiqueDto>? Peripheriques = null,
    List<DroitAccesDto>? DroitsAcces = null
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

/// <summary>
/// DTO pour le profil utilisateur (correspondant au backend UserController)
/// </summary>
public record BackendUserProfileDto(
    string Email,
    string? FullName = null
);

/// <summary>
/// DTO pour mise � jour du profil utilisateur
/// </summary>
public record UserProfileUpdateDto(
    string Email,
    string? FullName = null
);

/// <summary>
/// DTO pour mise � jour des r�les utilisateur
/// </summary>
public record RoleUpdateDto(
    string RoleName
);

/// <summary>
/// DTO pour r�ponse d'op�ration sur les r�les
/// </summary>
public record RoleOperationResultDto(
    bool Success,
    string Message,
    string? UserId = null,
    string? RoleName = null
);

// ==========================================
// DTOs UserManagementController (nouvelle API compl�te)
// ==========================================

/// <summary>
/// DTO d�taill� pour un utilisateur (UserManagementController)
/// </summary>
public record UserDetailsDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public int AccessFailedCount { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public DateTime LastLogin { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO pour cr�er un utilisateur
/// </summary>
public record CreateUserDto(
    string UserName,
    string Email,
    string FullName,
    string Password,
    string? PhoneNumber = null,
    bool EmailConfirmed = true,
    IList<string>? Roles = null
);

/// <summary>
/// DTO pour mettre � jour un utilisateur
/// </summary>
public record UpdateUserDto(
    string UserName,
    string Email,
    string FullName,
    string? PhoneNumber = null,
    bool EmailConfirmed = true,
    bool LockoutEnabled = false
);

/// <summary>
/// DTO pour un r�le avec statistiques
/// </summary>
public record RoleDto(
    string Id,
    string Name,
    string? Description,
    int UserCount,
    DateTime CreatedDate
);

/// <summary>
/// DTO pour cr�er un r�le
/// </summary>
public record CreateRoleDto(
    string Name,
    string? Description = null
);

/// <summary>
/// DTO pour assigner des r�les � un utilisateur
/// </summary>
public record AssignRolesDto(
    string UserId,
    IList<string> RoleNames
);

/// <summary>
/// DTO pour les statistiques des utilisateurs
/// </summary>
public record UserStatisticsDto(
    int TotalUsers,
    int ActiveUsers,
    int LockedUsers,
    int UnconfirmedEmails,
    int TotalRoles,
    Dictionary<string, int> UsersByRole,
    Dictionary<string, int> NewUsersLast30Days
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
// Mod�les IoT Hub
// ==========================================

/// <summary>
/// R�ponse standard IoT Hub avec propri�t�s optionnelles
/// </summary>
public record IoTResponse(
    bool Success,
    string Message,
    string? DeviceId = null,
    DateTime? Timestamp = null,
    object? Data = null
);

public record DeviceTwinDto(
    string DeviceId,
    object? Properties,
    object? Tags
);

// ==========================================
// Mod�les SignalR
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

public record SignalRNotificationDto(
    string Message,
    string Type,
    DateTime Timestamp
);

// ==========================================
// �num�rations pour UI
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

// ==========================================
// DTOs Commandes IoT (correspondant au backend)
// ==========================================

/// <summary>
/// DTO pour envoyer une commande � un p�riph�rique IoT
/// </summary>
public record CommandeRequestDto(
    int PeripheriqueLogiqueId,
    string NomCommande,
    string? Valeur = null
);

/// <summary>
/// DTO de r�ponse apr�s envoi d'une commande IoT
/// </summary>
public record CommandeResponseDto(
    bool IsSuccess,
    string Message,
    int? CommandeId = null
);

// ==========================================
// DTOs Statistiques de Consommation (correspondant au backend)
// ==========================================

/// <summary>
/// Statistiques de consommation �nerg�tique
/// </summary>
public record ConsommationStatsDto(
    int PeripheriqueId,
    string? PeripheriqueName,
    int? ZoneId,
    string? ZoneName,
    DateTime StartDate,
    DateTime EndDate,
    double ConsommationTotaleMwh,
    double PuissanceMoyenneMw,
    double PuissanceMaximaleMw,
    double PuissanceMinimaleMw,
    double TempsFonctionnementHeures,
    double EfficaciteEnergetique,
    int NombreCycles,
    double TempsMoyenCycleMinutes
);

/// <summary>
/// Donn�es de consommation horaire pour graphiques
/// </summary>
public record ConsommationHoraireDto(
    DateTime Heure,
    double ConsommationMwh,
    double PuissanceMoyenneMw,
    bool EtatActif,
    int NombreActivations
);

/// <summary>
/// Tendances de consommation
/// </summary>
public record ConsommationTrendDto(
    int PeripheriqueId,
    string? PeripheriqueName,
    int NombreJours,
    double ConsommationQuotidienneMoyenne,
    double TendancePourcentage,
    double ConsommationPrediteJour,
    List<ConsommationQuotidienneDto> DonneesQuotidiennes
);

/// <summary>
/// Consommation quotidienne
/// </summary>
public record ConsommationQuotidienneDto(
    DateTime Date,
    double ConsommationMwh,
    double TempsFonctionnementHeures,
    double PuissanceMoyenne
);

/// <summary>
/// Anomalie de consommation d�tect�e
/// </summary>
public record AnomalieConsommationDto(
    int PeripheriqueId,
    string? PeripheriqueName,
    DateTime DateDetection,
    string TypeAnomalie,
    string Description,
    double ValeurMesuree,
    double ValeurAttendue,
    double EcartPourcentage,
    string Severite
);

/// <summary>
/// Rapport de consommation d�taill�
/// </summary>
public record RapportConsommationDto(
    int? ZoneId,
    string? ZoneName,
    DateTime StartDate,
    DateTime EndDate,
    DateTime DateGeneration,
    double ConsommationTotaleMwh,
    decimal CoutTotalEstime,
    double EconomiesPotentiellesMwh,
    decimal EconomiesPotentiellesEuros,
    List<ConsommationStatsDto> StatistiquesParPeripherique,
    List<RecommandationOptimisationDto> Recommandations,
    ComparaisonPeriodeDto? ComparaisonPeriodePrecedente
);

/// <summary>
/// Recommandation d'optimisation �nerg�tique
/// </summary>
public record RecommandationOptimisationDto(
    int PeripheriqueId,
    string? PeripheriqueName,
    string TypeRecommandation,
    string Description,
    double EconomieEstimeeMwh,
    decimal EconomieEstimeeEuros,
    string Priorite
);

/// <summary>
/// Comparaison entre p�riodes
/// </summary>
public record ComparaisonPeriodeDto(
    double ConsommationPrecedenteMwh,
    double ConsommationActuelleMwh,
    double VariationMwh,
    double VariationPourcentage,
    string Tendance
);

/// <summary>
/// Co�t �nerg�tique calcul�
/// </summary>
public record CoutEnergetiqueDto(
    int PeripheriqueId,
    string? PeripheriqueName,
    DateTime StartDate,
    DateTime EndDate,
    double ConsommationMwh,
    decimal TarifKwh,
    decimal CoutTotal,
    decimal CoutMoyen,
    decimal CoutJournalier,
    List<CoutHoraireDto> CoutsHoraires
);

/// <summary>
/// Co�t horaire
/// </summary>
public record CoutHoraireDto(
    DateTime Heure,
    double ConsommationMwh,
    decimal TarifKwh,
    decimal Cout
);

/// <summary>
/// Top des consommateurs
/// </summary>
public record TopConsommateursDto(
    int PeripheriqueId,
    string PeripheriqueName,
    string PeripheriqueType,
    int ZoneId,
    string ZoneName,
    double ConsommationMwh,
    double PourcentageTotal,
    decimal CoutEstime,
    int Rang
);

/// <summary>
/// Comparaison de consommation entre deux p�riodes
/// </summary>
public record ComparaisonConsommationDto(
    int PeripheriqueId,
    string? PeripheriqueName,
    DateTime StartDate1,
    DateTime EndDate1,
    double ConsommationPeriode1Mwh,
    DateTime StartDate2,
    DateTime EndDate2,
    double ConsommationPeriode2Mwh,
    double DifferenceMwh,
    double DifferencePourcentage,
    string Tendance,
    string Interpretation
);

// ==========================================
// DTOs Droits d'Acc�s (correspondant au backend)
// ==========================================

/// <summary>
/// DTO principal pour les droits d'acc�s
/// </summary>
public record DroitAccesDto
{
    public int Id { get; set; }
    
    // ?? Utilisateur
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? UserFullName { get; set; }
    
    // ?? Zone
    public int? ZoneId { get; set; }
    public string? ZoneName { get; set; }
    public string? ZoneDescription { get; set; }
    
    // ?? P�riph�rique
    public int? PeripheriqueLogiqueId { get; set; }
    public string? PeripheriqueLogiqueName { get; set; }
    public string? PeripheriqueLogiqueType { get; set; }
    
    // ?? Type d'acc�s
    public string Type { get; set; } = string.Empty;  // "Lecture" ou "Controle"
    
    // ?? M�tadonn�es
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO pour cr�ation/modification de droits d'acc�s
/// </summary>
public record DroitAccesCreationDto(
    string UserId,
    int? ZoneId,
    int? PeripheriqueLogiqueId,
    string Type
);

/// <summary>
/// DTO pour les droits d'acc�s aux zones d'un utilisateur
/// </summary>
public record UserZoneAccessRightDto(
    int ZoneId,
    string ZoneName,
    string AccessType,
    DateTime GrantedAt
);

/// <summary>
/// DTO pour les droits d'acc�s aux p�riph�riques d'un utilisateur
/// </summary>
public record UserPeripheriqueAccessRightDto(
    int PeripheriqueId,
    string PeripheriqueName,
    string PeripheriqueType,
    int ZoneId,
    string ZoneName,
    string AccessType,
    DateTime GrantedAt
);

// ==========================================
// DTOs Devices/PeripheriqueLogique (correspondant au backend)
// ==========================================

/// <summary>
/// DTO P�riph�rique Logique principal
/// </summary>
public record PeripheriqueLogiqueDto(
    int Id,
    string Name,
    string Type,
    string IotDeviceId,
    int ZoneId
);

/// <summary>
/// DTO pour cr�ation de p�riph�rique logique
/// </summary>
public record PeripheriqueLogiqueCreationDto(
    string Name,
    string Type,
    string IotDeviceId,
    int ZoneId
);

/// <summary>
/// DTO pour mise � jour de p�riph�rique logique
/// </summary>
public record PeripheriqueLogiqueUpdateDto(
    string Name,
    string Type
);

// ==========================================
// DTOs Tickets d'Incidents (correspondant au backend)
// ==========================================

/// <summary>
/// DTO principal pour les tickets d'incidents
/// </summary>
public record IncidentTicketDto(
    int Id,
    string Sujet,
    string Description,
    string Statut,
    int? ZoneId,
    int? PeripheriqueLogiqueId,
    string CreatedById,
    string? AssignedToId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? Priorite = null,  // ? AJOUT: Priorit� du ticket
    string? CreatedByName = null,
    string? AssignedToName = null,
    string? ZoneName = null,
    string? PeripheriqueName = null
);

/// <summary>
/// DTO pour cr�ation de tickets d'incidents
/// </summary>
public class IncidentTicketCreateDto
{
    public string Sujet { get; set; } = "";
    public string Description { get; set; } = "";
    public string Statut { get; set; } = "";
    public string Priorite { get; set; } = "Moyenne";  // ? AJOUT: Priorit� par d�faut "Moyenne"
    public int? ZoneId { get; set; }
    public int? PeripheriqueLogiqueId { get; set; }
}

/// <summary>
/// DTO pour mise � jour du statut d'un ticket
/// </summary>
public record TicketStatusUpdateDto(
    string NewStatus
);

/// <summary>
/// DTO pour assignation d'un ticket
/// </summary>
public record TicketAssignmentDto(
    string AssignedToId
);

// ==========================================
// DTOs IoT Hub Management (correspondant au backend)
// ==========================================

/// <summary>
/// DTO pour invocation de m�thode directe sur un device
/// </summary>
public record DeviceMethodPayloadDto(
    string? Payload = "{}"
);

/// <summary>
/// DTO pour mise � jour des desired properties
/// </summary>
public record DesiredPropertiesPayloadDto(
    string DesiredProperties = "{}"
);

/// <summary>
/// DTO pour message Cloud-to-Device
/// </summary>
public record CloudToDeviceMessagePayloadDto(
    string Message,
    Dictionary<string, string>? Properties = null
);

/// <summary>
/// DTO pour cr�ation/mise � jour de device
/// </summary>
public record CreateDevicePayloadDto(
    bool Enabled = true
);

/// <summary>
/// DTO pour requ�te de statut de devices
/// </summary>
public record DeviceStatusRequestDto(
    List<string> DeviceIds
);

/// <summary>
/// DTO pour commandes intelligentes
/// </summary>
public record SmartCommandPayloadDto(
    double? Value = null,
    Dictionary<string, object>? Parameters = null
);

/// <summary>
/// DTO pour informations de device twin
/// </summary>
public record DeviceTwinInfoDto(
    string DeviceId,
    string Status,
    DateTime LastActivityTime,
    string? ReportedProperties,
    string? DesiredProperties,
    Dictionary<string, object>? Tags = null
);

/// <summary>
/// DTO pour informations de connexion de device
/// </summary>
public record DeviceConnectionInfoDto(
    string DeviceId,
    bool IsConnected,
    DateTime LastActivityTime,
    string ConnectionState
);

/// <summary>
/// DTO pour informations d�taill�es d'un device IoT Hub
/// </summary>
public record IoTHubDeviceInfoDto(
    string DeviceId,
    string Status,
    string ConnectionState,
    DateTime LastActivityTime,
    DateTime StatusUpdatedTime,
    DateTime ConnectionStateUpdatedTime
);

/// <summary>
/// DTO pour r�ponse d'op�ration IoT Hub
/// </summary>
public record IoTHubOperationResultDto(
    bool Success,
    string Message,
    string? DeviceId = null,
    DateTime Timestamp = default
);

/// <summary>
/// DTO pour r�sultat de synchronisation des devices depuis IoT Hub
/// </summary>
public record IoTHubSyncResultDto(
    bool Success,
    string Message,
    int TotalDevices,
    int CreatedCount,
    int UpdatedCount,
    int FailedCount,
    List<string>? FailedDeviceIds = null,
    DateTime Timestamp = default
);

// ==========================================
// DTOs Peripherals (IoT Twin Peripherals)
// ==========================================

/// <summary>
/// DTO pour un p�riph�rique physique (twin peripheral)
/// </summary>
public record PeripheralDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Gpio { get; set; }
    public string? Notes { get; set; }
    public string? Unit { get; set; }
    public string? WiringDiagram { get; set; }
    public DateTime? InstalledAt { get; set; }
    public object? State { get; set; }
    public DateTime? LastUpdate { get; set; }
    public string? Status { get; set; }
    public string? Metering { get; set; }
    public float? RatedWatt { get; set; }
    public double? EnergyMWh { get; set; }
    public float? LastPowerMW { get; set; }
    public bool? IsOutput { get; set; }
    public bool? ActiveHigh { get; set; }
}

/// <summary>
/// Payload pour mettre � jour/remplacer des p�riph�riques
/// </summary>
public record PeripheralsPayloadDto(
    List<PeripheralDto> Peripherals
);

/// <summary>
/// Payload pour commande sur un p�riph�rique
/// </summary>
public record PeripheralCommandPayloadDto(
    Dictionary<string, object>? Parameters = null
);

/// <summary>
/// Vue fusionn�e d'un p�riph�rique (desired + reported)
/// </summary>
public record MergedPeripheralDto(
    string Name,
    PeripheralDto? DesiredConfig = null,
    PeripheralDto? ReportedState = null,
    bool IsSynchronized = false
);

/// <summary>
/// Twin complet d'un device avec m�tadonn�es
/// </summary>
public record DeviceTwinFullDto(
    string DeviceId,
    string? Status = null,
    string? ConnectionState = null,
    DateTime? LastActivityTime = null,
    long Version = 0,
    List<PeripheralDto>? DesiredPeripherals = null,
    List<PeripheralDto>? ReportedPeripherals = null,
    string RawDesiredProperties = "{}",
    string RawReportedProperties = "{}"
);

// ==========================================
// DTOs Notifications (correspondant au backend)
// ==========================================

/// <summary>
/// DTO principal pour les notifications utilisateur
/// </summary>
public record UserNotificationDto(
    int Id,
    string UserId,
    string Titre,
    string Message,
    DateTime Horodatage,
    bool EstLue
);

/// <summary>
/// DTO pour cr�ation de notification
/// </summary>
public record UserNotificationCreateDto(
    string UserId,
    string Titre,
    string Message
);

/// <summary>
/// DTO pour statistiques de notifications
/// </summary>
public record NotificationStatsDto(
    int TotalNotifications,
    int NotificationsNonLues,
    int NotificationsLues,
    DateTime? DerniereNotification
);

// ==========================================
// DTOs T�l�m�trie (correspondant au backend)
// ==========================================

/// <summary>
/// DTO principal pour les donn�es de t�l�m�trie
/// </summary>
public record TelemetrieDto(
    int Id,
    int PeripheriqueLogiqueId,
    DateTime Horodatage,
    string TypeEvenement,
    double Valeur,
    string? DonneesJson = null,
    string? PeripheriqueName = null,
    string? PeripheriqueType = null,
    int? ZoneId = null,
    string? ZoneName = null
);

/// <summary>
/// DTO pour cr�ation de t�l�m�trie
/// </summary>
public class TelemetrieCreateDto
{
    public int PeripheriqueLogiqueId { get; set; }
    public string TypeEvenement { get; set; } = "";
    public double Valeur { get; set; }
    public string? DonneesJson { get; set; }
}

/// <summary>
/// DTO pour suggestions IA
/// </summary>
public record SuggestionDto(
    int Id,
    int UtilisateurId,
    string ContenuSuggestion,
    DateTime DateCreation
);

/// <summary>
/// DTO pour cr�ation de suggestion
/// </summary>
public record SuggestionCreateDto(
    int UtilisateurId,
    string ContenuSuggestion
);

/// <summary>
/// DTO pour requ�te d'historique de t�l�m�trie
/// </summary>
public record TelemetrieHistoryRequestDto(
    DateTime StartDate,
    DateTime EndDate,
    int? PeripheriqueId = null
);

/// <summary>
/// DTO pour statistiques de t�l�m�trie
/// </summary>
public record TelemetrieStatsDto(
    int PeripheriqueId,
    string? PeripheriqueName,
    DateTime StartDate,
    DateTime EndDate,
    int TotalEvents,
    double TotalValue,
    double AverageValue,
    double MaxValue,
    double MinValue,
    Dictionary<string, int> EventsByType,
    List<TelemetrieHourlyDto> HourlyData
);

/// <summary>
/// DTO pour donn�es de t�l�m�trie par heure
/// </summary>
public record TelemetrieHourlyDto(
    DateTime Hour,
    string TypeEvenement,
    double TotalValue,
    int EventCount,
    double AverageValue
);

/// <summary>
/// DTO pour tendances de t�l�m�trie
/// </summary>
public record TelemetrieTrendDto(
    int PeripheriqueId,
    string? PeripheriqueName,
    string TypeEvenement,
    int DaysAnalyzed,
    double DailyAverage,
    double TrendPercentage,
    string TrendDirection,
    List<TelemetrieDailyDto> DailyData
);

/// <summary>
/// DTO pour donn�es quotidiennes de t�l�m�trie
/// </summary>
public record TelemetrieDailyDto(
    DateTime Date,
    double TotalValue,
    int EventCount,
    Dictionary<string, double> ValuesByType
);

/// <summary>
/// DTO pour alertes bas�es sur la t�l�m�trie
/// </summary>
public record TelemetrieAlertDto(
    int PeripheriqueId,
    string? PeripheriqueName,
    string TypeEvenement,
    DateTime DetectedAt,
    string AlertType,
    string Description,
    double ThresholdValue,
    double ActualValue,
    string Severity
);

// ==========================================
// DTOs DeviceStatus (Nouveau - Phase 1)
// ==========================================

/// <summary>
/// DTO pour m�triques syst�me d'un device IoT
/// </summary>
public record DeviceMetricsDto(
    int Id,
    string IotDeviceId,
    DateTime Timestamp,
    int? Rssi,
    int? HeapFree,
    int? NvsQueueSize,
    long? UptimeSeconds,
    string? FirmwareVersion,
    string? FirmwareCommit,
    string? AdditionalData
);

/// <summary>
/// DTO pour �v�nement RFID
/// </summary>
public record RfidEventDto(
    int Id,
    string IotDeviceId,
    DateTime Timestamp,
    string CardUid,
    bool AccessGranted,
    string? DenialReason,
    string? UserId,
    string? Action
);

/// <summary>
/// DTO pour �v�nement alarme fum�e
/// </summary>
public record SmokeAlarmEventDto(
    int Id,
    string IotDeviceId,
    DateTime StartTime,
    DateTime? EndTime,
    bool IsActive,
    int? ZoneId,
    string? ZoneName,
    string? DeactivatedBy,
    string? DeactivationReason
);

/// <summary>
/// DTO pour statut complet d'un device (m�triques + RFID + alarmes + p�riph�riques)
/// </summary>
public record DeviceFullStatusDto(
    string IotDeviceId,
    DeviceMetricsDto? LatestMetrics,
    List<RfidEventDto>? RecentRfidEvents,
    SmokeAlarmEventDto? ActiveAlarm,
    List<PeripheralDto>? Peripherals,
    DateTime RetrievedAt
);

// ==========================================
// DTOs DeviceLogs (Nouveau - Phase 2)
// ==========================================

/// <summary>
/// DTO pour log syst�me d'un device
/// </summary>
public record DeviceLogDto(
    int Id,
    string DeviceId,
    DateTime Timestamp,
    string Level,
    string Source,
    string Message,
    string? Peripheral,
    string? AdditionalData
);

// ==========================================
// DTOs UserActivity (Nouveau - Phase 2)
// ==========================================

/// <summary>
/// DTO pour activit� utilisateur (audit trail)
/// </summary>
public record UserActivityDto(
    int Id,
    string UserId,
    string ActivityType,
    string Description,
    DateTime Timestamp,
    string? IpAddress,
    string? UserAgent,
    string? ResourceType,
    int? ResourceId,
    string? OldValue,
    string? NewValue
);

/// <summary>
/// DTO pour statistiques d'activit� utilisateur
/// </summary>
public record UserActivityStatsDto(
    int TotalActivities,
    int CommandesCount,
    int AuditsCount,
    DateTime? LastActivity,
    Dictionary<string, int> ActivitiesByType,
    List<UserActivityDto> RecentActivities
);

// ==========================================
// DTOs Pagination (Nouveau - Phase 2)
// ==========================================

/// <summary>
/// Wrapper g�n�rique pour r�ponses pagin�es
/// </summary>
public record PaginatedResponse<T>(
    List<T> Data,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages,
    bool HasPrevious,
    bool HasNext
);

// ==========================================
// Support ApiResponse Sans Data (Nouveau - Phase 1)
// ==========================================

/// <summary>
/// ApiResponse sans donn�es (pour DELETE, POST sans retour, etc.)
/// </summary>
public class ApiResponse
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public int StatusCode { get; init; }
    public Dictionary<string, string[]>? ValidationErrors { get; init; }

    public static ApiResponse Success(int statusCode = 200) =>
        new() { IsSuccess = true, StatusCode = statusCode };

    public static ApiResponse Error(string errorMessage, int statusCode = 400, Dictionary<string, string[]>? validationErrors = null) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage, StatusCode = statusCode, ValidationErrors = validationErrors };
}