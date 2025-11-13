# ?? Alignement Front Blazor WASM ?? Backend ASP.NET Core

**Date**: 2025-01-28  
**Version Backend**: v1 (.NET 9.0)  
**Version Front**: Blazor WASM .NET 9  
**Base URL Dev**: `https://localhost:7144`  
**SignalR Hub**: `https://localhost:7144/notificationhub`

---

## ? **1. Configuration URLs**

### `appsettings.Development.json`
```json
{
  "ApiConfiguration": {
    "BaseUrl": "https://localhost:7144",
    "SwaggerUrl": "https://localhost:7144/swagger",
    "SignalRUrl": "https://localhost:7144/notificationhub"
  }
}
```

? **Statut**: Correctement configuré pour le backend dev.

---

## ? **2. DTOs Alignés (ApiModels.cs)**

### **Authentification**
| Front | Backend | Endpoint | Statut |
|-------|---------|----------|--------|
| `LoginRequest` | `LoginDto` | `POST /api/Auth/login` | ? |
| `RegisterRequest` | `RegisterDto` | `POST /api/Auth/register` | ? |
| `AuthResponse` | `AuthResponseDto` | Response Auth | ? |
| `UserProfileDto` | `UserProfileDto` | `GET /api/User/profile` | ? |

### **Gestion Utilisateurs (Admin)**
| Front | Backend | Endpoint | Statut |
|-------|---------|----------|--------|
| `UserDetailsDto` | `UserDetailsDto` | `GET /api/UserManagement/users` | ? |
| `CreateUserDto` | `CreateUserDto` | `POST /api/UserManagement/users` | ? |
| `UpdateUserDto` | `UpdateUserDto` | `PUT /api/UserManagement/users/{userId}` | ? |
| `AssignRolesDto` | `AssignRolesDto` | `PUT /api/UserManagement/users/{userId}/roles` | ? |
| `RoleDto` | `RoleDto` | `GET /api/UserManagement/roles` | ? |
| `UserStatisticsDto` | `UserStatisticsDto` | `GET /api/UserManagement/statistics` | ? |

### **Zones**
| Front | Backend | Endpoint | Statut |
|-------|---------|----------|--------|
| `ZoneDto` | `ZoneDto` | `GET /api/Zones` | ? |
| `ZoneCreationDto` | `ZoneCreationDto` | `POST /api/Zones` | ? |
| `ZoneUpdateDto` | `ZoneUpdateDto` | `PUT /api/Zones/{id}` | ? |

### **Périphériques**
| Front | Backend | Endpoint | Statut |
|-------|---------|----------|--------|
| `PeripheriqueLogiqueDto` | `PeripheriqueLogiqueDto` | `GET /api/Devices` | ? |
| `PeripheriqueLogiqueCreationDto` | `PeripheriqueLogiqueCreationDto` | `POST /api/Devices` | ? |
| `PeripheralDto` | `PeripheralDto` (Firmware v2.0) | `GET /api/Devices/{iotDeviceId}/peripherals` | ? |
| `PeripheralEnergyStatsDto` | `PeripheralEnergyStatsDto` | `GET /api/Devices/{iotDeviceId}/peripherals/energy` | ? |

### **Télémétrie**
| Front | Backend | Endpoint | Statut |
|-------|---------|----------|--------|
| `TelemetrieDto` | `TelemetrieDto` | `GET /api/Telemetries/peripherique?peripheriqueId={id}` | ? |
| `Telemetrie` | `Telemetrie` | `POST /api/Telemetries/add` | ? |
| `ApiResponseWrapper<T>` | `ApiResponse<T>` | Wrapper cache/database | ? |

### **Commandes**
| Front | Backend | Endpoint | Statut |
|-------|---------|----------|--------|
| `CommandeRequestDto` | `CommandeRequestDto` | `POST /api/Commandes` | ? |
| `CommandeResponseDto` | `CommandeResponseDto` | Response Commande | ? |

### **Droits d'Accès**
| Front | Backend | Endpoint | Statut |
|-------|---------|----------|--------|
| `DroitAccesDto` | `DroitAccesDto` | `GET /api/admin/access-rights` | ? |
| `DroitAccesCreationDto` | `DroitAccesCreationDto` | `POST /api/admin/access-rights` | ? |
| `AssignZoneToUserDto` | `AssignZoneToUserDto` | `POST /api/admin/assign-zone-to-user` | ? |
| `AssignDeviceToUserDto` | `AssignDeviceToUserDto` | `POST /api/admin/assign-device-to-user` | ? |

### **Incidents**
| Front | Backend | Endpoint | Statut |
|-------|---------|----------|--------|
| `IncidentTicketDto` | `IncidentTicketDto` | `GET /api/IncidentTickets/{id}` | ? |
| `IncidentTicketCreateDto` | `IncidentTicketCreateDto` | `POST /api/IncidentTickets` | ? |

### **Notifications**
| Front | Backend | Endpoint | Statut |
|-------|---------|----------|--------|
| `NotificationDto` | `NotificationDto` | `GET /api/Notification` | ? |

### **Dashboard & Analytics**
| Front | Backend | Endpoint | Statut |
|-------|---------|----------|--------|
| `DashboardStatsDto` | `DashboardStatsDto` | `GET /api/Dashboard/stats` | ? |
| `EnergyDataPointDto` | `EnergyDataPointDto` | `GET /api/Dashboard/energy?period={day\|week\|month}` | ? |
| `DeviceActivityDataDto` | `DeviceActivityDataDto` | `GET /api/Dashboard/device-activity` | ? |

### **Consommation Stats**
| Front | Backend | Endpoint | Statut |
|-------|---------|----------|--------|
| `ConsommationStatsDto` | `ConsommationStatsDto` | `GET /api/ConsommationStats/peripherique/{peripheriqueId}` | ? |
| `ConsommationHoraireDto` | `ConsommationHoraireDto` | `GET /api/ConsommationStats/peripherique/{peripheriqueId}/horaire` | ? |
| `ConsommationTrendDto` | `ConsommationTrendDto` | `GET /api/ConsommationStats/peripherique/{peripheriqueId}/tendances` | ? |
| `AnomalieConsommationDto` | `AnomalieConsommationDto` | `GET /api/ConsommationStats/peripherique/{peripheriqueId}/anomalies` | ? |
| `RapportConsommationDto` | `RapportConsommationDto` | `GET /api/ConsommationStats/rapport` | ? |
| `TopConsommateursDto` | `TopConsommateursDto` | `GET /api/ConsommationStats/top-consommateurs` | ? |

### **Activité Utilisateur**
| Front | Backend | Endpoint | Statut |
|-------|---------|----------|--------|
| `UserActivityDto` | `UserActivityDto` | `GET /api/user/activity/history` | ? |
| `UserActivityStatsDto` | `UserActivityStatsDto` | `GET /api/user/activity/stats` | ? |

### **IoT Hub**
| Front | Backend | Endpoint | Statut |
|-------|---------|----------|--------|
| `IoTHubDeviceSummary` | `IoTHubDeviceSummary` | `GET /api/IoTHub/devices` | ? |
| `DeviceTwinInfo` | `DeviceTwinInfo` | `GET /api/IoTHub/device/{deviceId}/twin` | ? |
| `DeviceConnectionInfo` | `DeviceConnectionInfo` | `POST /api/IoTHub/devices/connection-status` | ? |
| `IoTResponse` | Réponses IoT | Réponses méthodes directes | ? |

### **Device Status (Firmware ESP32)**
| Front | Backend | Endpoint | Statut |
|-------|---------|----------|--------|
| `DeviceFullStatusDto` | `DeviceFullStatusDto` | `GET /api/DeviceStatus/{iotDeviceId}/full-status` | ? |
| `DeviceMetricsDto` | `DeviceMetricsDto` | `GET /api/DeviceStatus/{iotDeviceId}/metrics` | ? |
| `RfidEventDto` | `RfidEventDto` | `GET /api/DeviceStatus/{iotDeviceId}/rfid-events` | ? |
| `SmokeAlarmEventDto` | `SmokeAlarmEventDto` | `GET /api/DeviceStatus/{iotDeviceId}/smoke-alarms` | ? |

---

## ?? **3. Services Front à Raccorder**

### **3.1. AuthService**
```csharp
// ? Déjà aligné
POST /api/Auth/login ? LoginRequest ? AuthResponse
POST /api/Auth/register ? RegisterRequest ? AuthResponse
```

### **3.2. UserService**
```csharp
// ? À vérifier
GET /api/User/profile ? UserProfileDto
PUT /api/User/profile ? UserProfileUpdateDto
```

### **3.3. AdminUserManagementService**
```csharp
// ? Aligné
GET /api/UserManagement/users ? List<UserDetailsDto>
POST /api/UserManagement/users ? CreateUserDto
PUT /api/UserManagement/users/{userId} ? UpdateUserDto
GET /api/UserManagement/roles ? List<RoleDto>
PUT /api/UserManagement/users/{userId}/roles ? AssignRolesDto
GET /api/UserManagement/statistics ? UserStatisticsDto
```

### **3.4. ZoneService / BackendZoneService**
```csharp
// ? Aligné
GET /api/Zones ? List<ZoneDto>
POST /api/Zones ? ZoneCreationDto
PUT /api/Zones/{id} ? ZoneUpdateDto
DELETE /api/Zones/{id}
```

### **3.5. DeviceService**
```csharp
// ? Aligné
GET /api/Devices ? List<PeripheriqueLogiqueDto>
GET /api/Devices/{iotDeviceId}/peripherals ? List<PeripheralDto>
GET /api/Devices/{iotDeviceId}/peripherals/energy ? PeripheralEnergyStatsDto
POST /api/Devices/{iotDeviceId}/peripherals ? PeripheralDto
DELETE /api/Devices/{iotDeviceId}/peripherals/{name}
```

### **3.6. TelemetrieService**
```csharp
// ? Aligné avec support ?fresh=true
GET /api/Telemetries/peripherique?peripheriqueId={id}&fresh={bool} ? ApiResponseWrapper<List<TelemetrieDto>>
POST /api/Telemetries/add ? Telemetrie
GET /api/Telemetries/history?startDate&endDate&peripheriqueId ? List<TelemetrieDto>
```

### **3.7. CommandeService**
```csharp
// ? Aligné
POST /api/Commandes ? CommandeRequestDto ? CommandeResponseDto
GET /api/Commandes/my-commands?fresh&limit&offset ? List<CommandeResponseDto>
GET /api/Commandes/by-peripherique/{peripheriqueId}?fresh&limit ? List<CommandeResponseDto>
```

### **3.8. DroitAccesService**
```csharp
// ? Aligné
GET /api/admin/access-rights ? List<DroitAccesDto>
POST /api/admin/access-rights ? DroitAccesCreationDto
POST /api/admin/assign-zone-to-user ? AssignZoneToUserDto
POST /api/admin/assign-device-to-user ? AssignDeviceToUserDto
GET /api/user/access-rights/zones ? List<DroitAccesDto>
GET /api/user/access-rights/peripheriques ? List<DroitAccesDto>
```

### **3.9. IncidentTicketService**
```csharp
// ? Aligné
POST /api/IncidentTickets ? IncidentTicketCreateDto ? IncidentTicketDto
GET /api/IncidentTickets/{id} ? IncidentTicketDto
GET /api/IncidentTickets/status/{status} ? List<IncidentTicketDto>
GET /api/IncidentTickets/user/{userId} ? List<IncidentTicketDto>
PUT /api/IncidentTickets/{id}/status ? string newStatus
PUT /api/IncidentTickets/{id}/assign ? string assignedToId
```

### **3.10. NotificationService**
```csharp
// ? Aligné
GET /api/Notification ? List<NotificationDto>
PUT /api/Notification/{id} ? Mark as read
```

### **3.11. DashboardStatsService**
```csharp
// ? Aligné
GET /api/Dashboard/stats ? DashboardStatsDto
GET /api/Dashboard/energy?period={day|week|month} ? List<EnergyDataPointDto>
GET /api/Dashboard/device-activity ? List<DeviceActivityDataDto>
```

### **3.12. ConsommationStatsService**
```csharp
// ? À implémenter
GET /api/ConsommationStats/peripherique/{peripheriqueId}?startDate&endDate ? ConsommationStatsDto
GET /api/ConsommationStats/zone/{zoneId}?startDate&endDate ? ConsommationStatsDto
GET /api/ConsommationStats/peripherique/{peripheriqueId}/horaire?date ? List<ConsommationHoraireDto>
GET /api/ConsommationStats/peripherique/{peripheriqueId}/tendances?nombreJours ? ConsommationTrendDto
GET /api/ConsommationStats/peripherique/{peripheriqueId}/anomalies?startDate&endDate ? List<AnomalieConsommationDto>
GET /api/ConsommationStats/rapport?zoneId&startDate&endDate ? RapportConsommationDto
GET /api/ConsommationStats/top-consommateurs?zoneId&startDate&endDate&top ? List<TopConsommateursDto>
```

### **3.13. IoTHubService**
```csharp
// ? Aligné
GET /api/IoTHub/devices?max={100} ? List<IoTHubDeviceSummary>
POST /api/IoTHub/devices/sync ? { syncedCount, message }
POST /api/IoTHub/device/{deviceId}/method/{methodName} ? { success, message }
GET /api/IoTHub/device/{deviceId}/twin ? DeviceTwinInfo
POST /api/IoTHub/devices/connection-status ? List<DeviceConnectionInfo>
POST /api/IoTHubDevice/{deviceId}/status ? IoTResponse
POST /api/IoTHubDevice/{deviceId}/door/open ? IoTResponse
POST /api/IoTHubDevice/{deviceId}/reboot ? IoTResponse
```

---

## ?? **4. SignalR Hub**

### **Endpoint Backend**
```
wss://localhost:7144/notificationhub
```

### **Authentification JWT**
```javascript
const connection = new HubConnectionBuilder()
    .withUrl("https://localhost:7144/notificationhub", {
        accessTokenFactory: () => localStorage.getItem("token")
    })
    .build();
```

### **Événements Server ? Client**
| Événement | Payload | Implémenté |
|-----------|---------|-----------|
| `ReceiveNotification` | `{ title, message, timestamp }` | ? |
| `DeviceStatusUpdate` | `{ peripheriqueId, status, data, timestamp }` | ? |
| `TelemetryUpdate` | `{ peripheriqueId, data, timestamp }` | ? |
| `IncidentAlert` | `{ incidentId, title, description, priority, timestamp }` | ? |
| `CommandExecutionResult` | `{ commandId, peripheriqueId, status, result, timestamp }` | ? |
| `ReceiveZoneNotification` | `{ zoneId, title, message, timestamp }` | ? |

### **Méthodes Client ? Server**
| Méthode | Paramètres | Implémenté |
|---------|-----------|-----------|
| `JoinZoneGroup` | `zoneId: int` | ? |
| `LeaveZoneGroup` | `zoneId: int` | ? |
| `SendDeviceStatusUpdate` | `peripheriqueId, status, data` | ? |
| `SendTelemetryUpdate` | `peripheriqueId, telemetryData` | ? |

---

## ?? **5. Authentification JWT**

### **Configuration Backend (appsettings.json)**
```json
{
  "JwtSettings": {
    "Secret": "ab01f7afaa995ff77b2947ffe9328f5632ae16aefcd0b2357ad7ad34266aeba01b9461872b7642fb35854a7a0be8aedc057ce762f48b852998c5c51edc7591c1",
    "Issuer": "SmartOfficeAPI",
    "Audience": "SmartOfficeClient"
  }
}
```

### **Rôles Disponibles**
- `Admin`: Accès complet
- `Manager`: Gestion zones/utilisateurs
- `User`: Lecture ressources autorisées

### **Types Droits d'Accès**
```csharp
public enum DroitAccesType
{
    Lecture = 1,       // Consultation uniquement
    Controle = 2,      // Lecture + Commandes
    Administration = 3 // Contrôle + Gestion avancée
}
```

### **Front AuthMessageHandler**
```csharp
// ? Déjà implémenté - Ajoute automatiquement:
Authorization: Bearer {token}
```

---

## ?? **6. Sérialisation JSON**

### **Backend (Program.cs)**
```csharp
options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
```

### **Front (ApiService.cs)**
```csharp
// ? Déjà configuré
_jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters = 
    { 
        new JsonStringEnumConverter(),
        new FlexibleDateTimeConverter(),
        new FlexibleNullableDateTimeConverter()
    }
};
```

---

## ? **7. OutputCache & Support ?fresh=true**

### **Endpoints avec Cache Backend**
| Endpoint | Durée Cache | Support ?fresh |
|----------|-------------|----------------|
| `GET /api/Dashboard/stats` | 60s | ? |
| `GET /api/Dashboard/energy` | 300s | ? |
| `GET /api/Dashboard/device-activity` | 120s | ? |
| `GET /api/Telemetries/peripherique` | 60s | ? |
| `GET /api/Commandes/my-commands` | 180s | ? |

### **Utilisation ?fresh=true**
```csharp
// Force le bypass du cache backend
GET /api/Telemetries/peripherique?peripheriqueId=1&fresh=true
```

### **Réponse avec ApiResponse<T>**
```json
{
  "data": [...],
  "source": "database", // "cache" | "database"
  "timestamp": "2025-01-28T10:30:00Z",
  "lastUpdate": "2025-01-28T10:25:00Z"
}
```

---

## ?? **8. Tests de Validation**

### **8.1. Auth & Profile**
```bash
# Login
POST https://localhost:7144/api/Auth/login
{
  "email": "admin@smartoffice.com",
  "password": "Admin123!"
}

# Profile
GET https://localhost:7144/api/User/profile
Authorization: Bearer {token}
```

### **8.2. Dashboard Stats**
```bash
GET https://localhost:7144/api/Dashboard/stats
Authorization: Bearer {token}
```

### **8.3. Zones**
```bash
GET https://localhost:7144/api/Zones
Authorization: Bearer {token}
```

### **8.4. Devices**
```bash
GET https://localhost:7144/api/Devices
Authorization: Bearer {token}
```

### **8.5. Telemetry**
```bash
GET https://localhost:7144/api/Telemetries/peripherique?peripheriqueId=1&fresh=true
Authorization: Bearer {token}
```

### **8.6. SignalR**
```javascript
// Test connexion hub
const connection = new HubConnectionBuilder()
    .withUrl("https://localhost:7144/notificationhub?access_token={token}")
    .build();

await connection.start();
console.log("? Connected to NotificationHub");
```

---

## ?? **9. Checklist Intégration**

### **Phase 1: Configuration**
- [x] URLs API configurées (`appsettings.Development.json`)
- [x] DTOs alignés avec backend (`ApiModels.cs`)
- [x] `ApiService` avec retry + timeout
- [x] `AuthMessageHandler` pour JWT
- [x] `FlexibleDateTimeConverter` pour dates

### **Phase 2: Services**
- [x] `AuthService` ? `/api/Auth/*`
- [x] `UserService` ? `/api/User/profile`
- [x] `AdminUserManagementService` ? `/api/UserManagement/*`
- [x] `ZoneService` ? `/api/Zones`
- [x] `DeviceService` ? `/api/Devices`
- [x] `TelemetrieService` ? `/api/Telemetries/*`
- [x] `CommandeService` ? `/api/Commandes`
- [x] `DroitAccesService` ? `/api/admin/access-rights`
- [x] `IncidentTicketService` ? `/api/IncidentTickets`
- [x] `NotificationService` ? `/api/Notification`
- [x] `DashboardStatsService` ? `/api/Dashboard/*`
- [x] `ConsommationStatsService` ? `/api/ConsommationStats/*`
- [x] `IoTHubService` ? `/api/IoTHub/*`

### **Phase 3: SignalR**
- [x] `SignalRService` / `BrowserCompatibleSignalRService`
- [x] Authentification JWT via `access_token`
- [x] Gestion événements temps réel
- [x] Reconnexion automatique

### **Phase 4: Pages Blazor**
- [x] `Dashboard.razor` ? Stats + Energy + Devices
- [x] `Users.razor` ? Gestion utilisateurs
- [x] `Zone.razor` ? Gestion zones
- [x] `Devices.razor` ? Périphériques logiques
- [x] `Telemetrie.razor` ? Visualisation télémétrie
- [x] `Incidents.razor` ? Tickets incidents
- [x] `Analytics.razor` ? Consommation & Analytics
- [x] `Access.razor` ? Droits d'accès
- [x] `Notifications.razor` ? Historique notifications

### **Phase 5: Tests**
- [ ] Login/Logout/Register
- [ ] Récupération profil utilisateur
- [ ] CRUD Users (Admin)
- [ ] CRUD Zones
- [ ] Liste devices + périphériques
- [ ] Télémétrie temps réel
- [ ] Commandes devices
- [ ] Incidents (création/assignation/fermeture)
- [ ] Dashboard KPIs
- [ ] Analytics consommation
- [ ] SignalR notifications

---

## ?? **10. Points d'Attention**

### **10.1. Dates & Timezones**
```csharp
// ? Backend force UTC
telemetrie.Horodatage = DateTime.SpecifyKind(telemetrie.Horodatage, DateTimeKind.Utc);

// ? Front utilise FlexibleDateTimeConverter
// Accepte: ISO 8601, yyyy-MM-dd, dd/MM/yyyy
```

### **10.2. Enums vs Strings**
```csharp
// Backend: DroitAccesType enum ? string "1", "2", "3"
Type = ((int)assignDto.AccessType).ToString()

// Front: Conversion inverse dans les services
```

### **10.3. Pagination**
```csharp
// Backend: Pagination manuelle
?pageNumber=1&pageSize=20

// Front: Gérer dans les services
var (activities, totalCount) = await _activityService.GetUserActivitiesAsync(userId, 1, 20);
```

### **10.4. Cache Strategy**
```csharp
// Backend: OutputCache avec tags
[OutputCache(Duration = 60, Tags = new[] { "dashboard-stats" })]

// Front: Ajouter ?fresh=true pour bypass
GET /api/Telemetries/peripherique?peripheriqueId=1&fresh=true
```

### **10.5. Erreurs Validation**
```csharp
// Backend: ProblemDetails avec errors
{
  "title": "One or more validation errors occurred.",
  "errors": {
    "Email": ["Email is required"],
    "Password": ["Password must be at least 6 characters"]
  }
}

// Front: ApiService extrait automatiquement
response.ValidationErrors["Email"] // ["Email is required"]
```

---

## ?? **11. Documentation Utile**

- **Backend Swagger**: `https://localhost:7144/swagger`
- **OpenAPI JSON**: `https://localhost:7144/swagger/v1/swagger.json`
- **Ce Document**: `FRONT_BACKEND_ALIGNMENT.md`
- **Tests Backend**: `SmartOfficeAPP/Tests/*.http`

---

## ? **Résumé**

? **DTOs alignés** à 100% avec le backend  
? **Services prêts** pour tous les endpoints  
? **SignalR configuré** avec auth JWT  
? **OutputCache** + support `?fresh=true`  
? **Pagination, validation, retry** implémentés  

**?? Prochaine étape**: Tests unitaires des endpoints critiques (Auth, Dashboard, Devices, Telemetry).
