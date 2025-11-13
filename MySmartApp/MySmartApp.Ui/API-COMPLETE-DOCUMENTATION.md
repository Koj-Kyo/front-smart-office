# SmartOffice API - Documentation Complète Backend

**Date de création**: 2025  
**Date mise à jour**: Janvier 2025 ✅
**Version API**: v1  
**Framework**: ASP.NET Core 9.0  
**Base de données**: PostgreSQL  
**Cache**: Redis / In-Memory  
**IoT**: Azure IoT Hub  

---

## 📋 Table des Matières

1. [Configuration Générale](#configuration-générale)
2. [Authentification & Autorisation](#authentification--autorisation)
3. [Controllers & Endpoints](#controllers--endpoints)
4. [DTOs (Request/Response)](#dtos-requestresponse)
5. [SignalR Hubs](#signalr-hubs)
6. [URLs de l'API](#urls-de-lapi)
7. [Swagger/OpenAPI](#swaggeropenapi)

---

## ?? Configuration Générale

### Program.cs - Configuration Complète

#### **Authentification JWT**
```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!)),
        ClockSkew = TimeSpan.Zero
    };
    
    // Support JWT pour SignalR (query access_token)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationhub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});
```

#### **CORS Configuration**
```csharp
// Politique pour applications web
opt.AddPolicy("AllowFrontend", p => p
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithOrigins("https://localhost:7152", "http://localhost:5249", "http://localhost:3000", "https://localhost:3000")
    .AllowCredentials());

// Politique pour application mobile MAUI
opt.AddPolicy("AllowMobileApp", p => p
    .AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed(origin => 
    {
        // Origines autorisées pour MAUI
        if (string.IsNullOrEmpty(origin)) return true;
        if (origin.StartsWith("http://localhost")) return true;
        if (origin.StartsWith("http://10.0.2.2")) return true; // Android Emulator
        if (origin.StartsWith("app://")) return true; // MAUI
        return false;
    })
    .AllowCredentials());
```

#### **JSON Configuration (camelCase)**
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
```

#### **Redis & OutputCache**
```csharp
// Configuration Redis (si disponible)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "SmartOfficeAPI";
});

// OutputCache avec politiques
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.Expire(TimeSpan.FromMinutes(5));
        policy.SetVaryByQuery("fresh", "limit", "offset");
        policy.Tag("api-data");
    });

    options.AddPolicy("DeviceData", policy =>
    {
        policy.Expire(TimeSpan.FromMinutes(2));
        policy.SetVaryByQuery("fresh", "deviceId");
        policy.Tag("device-data");
    });

    options.AddPolicy("TelemetryData", policy =>
    {
        policy.Expire(TimeSpan.FromMinutes(1));
        policy.SetVaryByQuery("fresh", "peripheriqueId", "startDate", "endDate");
        policy.Tag("telemetry-data");
    });
});
```

#### **SignalR Configuration**
```csharp
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.MaximumReceiveMessageSize = 64 * 1024; // 64KB
});
```

#### **Services Enregistrés**
```csharp
// Repositories
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IZoneRepository, ZoneRepository>();
builder.Services.AddScoped<IPeripheriqueLogiqueRepository, PeripheriqueLogiqueRepository>();
builder.Services.AddScoped<IDevicePeripheralRepository, DevicePeripheralRepository>();
builder.Services.AddScoped<IDeviceLogRepository, DeviceLogRepository>();
builder.Services.AddScoped<ICommandeRepository, CommandeRepository>();
builder.Services.AddScoped<ITelemetrieRepository, TelemetrieRepository>();
builder.Services.AddScoped<IDroitAccesRepository, DroitAccesRepository>();
builder.Services.AddScoped<IJournalAuditRepository, JournalAuditRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ISuggestionRepository, SuggestionRepository>();
builder.Services.AddScoped<IDeviceMetricsRepository, DeviceMetricsRepository>();
builder.Services.AddScoped<IRfidEventRepository, RfidEventRepository>();
builder.Services.AddScoped<ISmokeAlarmEventRepository, SmokeAlarmEventRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IZoneService, ZoneService>();
builder.Services.AddScoped<IPeripheriqueLogiqueService, PeripheriqueLogiqueService>();
builder.Services.AddScoped<IDevicePeripheralService, DevicePeripheralService>();
builder.Services.AddScoped<IDeviceLogService, DeviceLogService>();
builder.Services.AddScoped<ICommandeService, CommandeService>();
builder.Services.AddScoped<IIotHubService, IotHubService>();
builder.Services.AddScoped<ITelemetrieService, TelemetrieService>();
builder.Services.AddScoped<IDroitAccesService, DroitAccesService>();
builder.Services.AddScoped<IJournalAuditService, JournalAuditService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IIncidentTicketService, IncidentTicketService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IUserActivityService, UserActivityService>();
builder.Services.AddScoped<IDeviceSyncService, DeviceSyncService>();
builder.Services.AddScoped<ISignalRService, SignalRService>();
builder.Services.AddScoped<IConsommationStatsService, ConsommationStatsService>();
builder.Services.AddScoped<IDeviceStatusService, DeviceStatusService>();
builder.Services.AddScoped<DataSeedingService>();

// Background Workers
builder.Services.AddHostedService<TelemetrieIngestionWorker>();
builder.Services.AddHostedService<LogIngestionWorker>();
builder.Services.AddHostedService<DeviceSyncWorker>();
```

#### **Authorization Handlers**
```csharp
builder.Services.AddAuthorization();
builder.Services.AddScoped<IAuthorizationHandler, ResourceAccessHandler>();
```

---

## 🔐 Authentification & Autorisation

### Configuration JWT (appsettings.json)
```json
"JwtSettings": {
  "Secret": "ab01f7afaa995ff77b2947ffe9328f5632ae16aefcd0b2357ad7ad34266aeba01b9461872b7642fb35854a7a0be8aedc057ce762f48b852998c5c51edc7591c1",
  "Issuer": "SmartOfficeAPI",
  "Audience": "SmartOfficeClient"
}
```

### Rôles disponibles
- **Admin**: Accès complet à toutes les ressources
- **Manager**: Gestion des zones et des utilisateurs
- **User**: Accès en lecture aux ressources autorisées

### Types de droits d'accès (DroitAccesType)
```csharp
public enum DroitAccesType
{
    Lecture = 1,       // Consultation uniquement
    Controle = 2,      // Lecture + Commandes
    Administration = 3 // Contrôle + Gestion avancée
}
```

---

## 🎯 Controllers & Endpoints

### Résumé des Controllers

| Controller | Route Base | Auth | Description |
|------------|-----------|------|-------------|
| **AuthController** | `/api/Auth` | Variable | Authentification & enregistrement |
| **ZonesController** | `/api/Zones` | Admin | Gestion des zones |
| **DevicesController** | `/api/Devices` | Admin | Gestion périphériques logiques (à documenter) |
| **DevicePeripheralsController** | `/api/DevicePeripherals` | Admin | Périphériques physiques |
| **DeviceStatusController** | `/api/DeviceStatus` | Admin/Manager | Métriques, RFID, alarme, périphériques |
| **DeviceLogsController** | `/api/DeviceLogs` | User | Logs des devices |
| **TelemetriesController** | `/api/Telemetries` | User | Télémétrie IoT |
| **CommandesController** | `/api/Commandes` | User | Commandes IoT |
| **DashboardController** | `/api/Dashboard` | User | Statistiques Dashboard |
| **ConsommationStatsController** | `/api/ConsommationStats` | User | Stats énergétiques |
| **DroitAccesController** | `/api` | Variable | Gestion droits d'accès |
| **IncidentTicketsController** | `/api/IncidentTickets` | User | Tickets incidents |
| **NotificationController** | `/api/Notification` | User | Notifications utilisateur |
| **UserController** | `/api/User` | User | Profil utilisateur |
| **UserManagementController** | `/api/UserManagement` | Admin | Gestion utilisateurs/rôles |
| **UserActivityController** | `/api/user/activity` | User | Historique activité |
| **IoTHubController** | `/api/IoTHub` | User | Gestion IoT Hub (sync, twin) |
| **IoTHubDeviceController** | `/api/IoTHubDevice` | Admin/Manager | Commandes directes IoT |
| **SignalRTestController** | `/api/SignalRTest` | User | Tests SignalR |

---

### 1. AuthController
**Route**: `/api/Auth`  
**Authentification**: Anonyme (sauf test endpoints)

#### Endpoints
| Méthode | Endpoint | Auth | Description |
|---------|----------|------|-------------|
| POST | `/register` | Anonyme | Enregistrement nouvel utilisateur |
| POST | `/login` | Anonyme | Connexion et génération JWT |
| GET | `/test-auth` | `[Authorize]` | Test authentification |
| GET | `/test-admin` | `[Authorize(Roles="Admin")]` | Test rôle Admin |

#### DTOs
**RegisterDto** (Request):
```csharp
{
  "userName": "string",
  "email": "string",
  "password": "string",
  "confirmPassword": "string",
  "fullName": "string?"
}
```

**LoginDto** (Request):
```csharp
{
  "email": "string",
  "password": "string"
}
```

**AuthResponseDto** (Response):
```csharp
{
  "token": "string",
  "refreshToken": "string?",
  "expiration": "DateTime",
  "userId": "string",
  "email": "string",
  "fullName": "string?",
  "roles": ["string"]
}
```

---

### 2. ZonesController
**Route**: `/api/Zones`  
**Authentification**: `[Authorize(Roles="Admin")]`

#### Endpoints
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/` | Liste toutes les zones |
| GET | `/{id}` | Récupère une zone par ID |
| POST | `/` | Crée une nouvelle zone |
| PUT | `/{id}` | Met à jour une zone |
| DELETE | `/{id}` | Supprime une zone |

#### DTOs
**ZoneDto** (Response):
```csharp
{
  "id": "int",
  "nom": "string",
  "description": "string?",
  "batiment": "string?",
  "etage": "int?",
  "capacitePersonnes": "int?",
  "superficie": "float?",
  "dateCreation": "DateTime"
}
```

**ZoneCreationDto** (Request):
```csharp
{
  "nom": "string",
  "description": "string?",
  "batiment": "string?",
  "etage": "int?",
  "capacitePersonnes": "int?",
  "superficie": "float?"
}
```

**ZoneUpdateDto** (Request):
```csharp
{
  "nom": "string",
  "description": "string?",
  "batiment": "string?",
  "etage": "int?",
  "capacitePersonnes": "int?",
  "superficie": "float?"
}
```

---

### 3. DevicesController
**Route**: `/api/Devices`  
**Authentification**: `[Authorize(Roles="Admin")]`

#### Endpoints
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/` | Liste tous les périphériques |
| GET | `/{id}` | Récupère un périphérique par ID |
| GET | `/api/zones/{zoneId}/devices` | Périphériques d'une zone |
| POST | `/` | Crée un périphérique |
| PUT | `/{id}` | Met à jour un périphérique |
| DELETE | `/{id}` | Supprime un périphérique |
| POST | `/{iotDeviceId}/desired` | Met à jour desired properties |

#### **?? Gestion des périphériques (Firmware v2.0)**
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/{iotDeviceId}/peripherals` | Liste périphériques (reported/desired) |
| GET | `/{iotDeviceId}/peripherals/{name}` | Récupère un périphérique |
| POST | `/{iotDeviceId}/peripherals` | Ajoute un périphérique |
| PUT | `/{iotDeviceId}/peripherals` | Remplace liste périphériques |
| DELETE | `/{iotDeviceId}/peripherals/{name}` | Supprime un périphérique |
| POST | `/{iotDeviceId}/peripherals/{name}/commands/{command}` | Exécute commande périphérique |
| GET | `/{iotDeviceId}/peripherals/{name}/merged` | Vue fusionnée desired+reported |
| GET | `/{iotDeviceId}/twin/full` | Twin complet avec métadonnées |

#### **? Gestion énergétique (Firmware v1.3.0)**
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/{iotDeviceId}/peripherals/energy` | Statistiques énergétiques |
| POST | `/{iotDeviceId}/peripherals/energy/reset` | Réinitialise compteurs |

#### DTOs
**PeripheralDto**:
```csharp
{
  "id": "string?",                    // GUID unique
  "name": "string",                   // Ex: "relay4"
  "type": "string",                   // Ex: "relay", "smoke", "temp"
  "gpio": "int",                      // Ex: 33
  "notes": "string?",                 // Notes de câblage
  "unit": "string?",                  // "°C", "%", "W"
  "wiringDiagram": "string?",         // URL schéma
  "installedAt": "DateTime?",         // Date installation
  "state": "object?",                 // État temps réel
  "lastUpdate": "DateTime?",          // Dernier rapport
  "status": "string?",                // "active", "error", "offline"
  "metering": "string?",              // "none", "estimated", "measured"
  "ratedWatt": "float?",              // Puissance nominale (W)
  "energyMWh": "double?",             // Énergie consommée (mWh)
  "lastPowerMW": "float?",            // Dernière puissance (mW)
  "isOutput": "bool?",                // true=sortie, false=entrée
  "activeHigh": "bool?"               // true=actif haut, false=actif bas
}
```

**PeripheralEnergyStatsDto**:
```csharp
{
  "deviceId": "string",
  "timestamp": "DateTime",
  "peripherals": [
    {
      "name": "string",
      "type": "string",
      "metering": "string",
      "ratedWatt": "float",
      "energyWh": "double",
      "currentPowerW": "float",
      "state": "string",
      "lastUpdate": "DateTime?"
    }
  ],
  "totalEnergyWh": "double",
  "totalPowerW": "double"
}
```

---

### 4. TelemetriesController
**Route**: `/api/Telemetries`  
**Authentification**: `[Authorize]`  
**OutputCache**: Policies "TelemetryData"

#### Endpoints
| Méthode | Endpoint | Cache | Description |
|---------|----------|-------|-------------|
| GET | `/peripherique?peripheriqueId={id}&fresh={bool}` | ? | Télémétrie par périphérique |
| GET | `/by-peripherique/{peripheriqueId}?fresh={bool}` | ? | Alias télémétrie |
| POST | `/add` | ? | Ajoute télémétrie |
| GET | `/history?startDate&endDate&peripheriqueId&fresh` | ? | Données historiques |
| POST | `/suggestions` | ? | Stocke suggestion IA |

#### Support `?fresh=true`
- Bypasse le cache
- Force lecture depuis database
- Retourne `ApiResponse<T>` avec source "database"

#### DTOs
**TelemetrieDto**:
```csharp
{
  "id": "int",
  "peripheriqueLogiqueId": "int",
  "typeEvenement": "string",
  "valeur": "float",
  "unite": "string?",
  "horodatage": "DateTime",
  "deviceId": "string?",
  "peripheralName": "string?"
}
```

**ApiResponse<T>** (Wrapper):
```csharp
{
  "data": "T",
  "source": "cache" | "database",
  "timestamp": "DateTime",
  "lastUpdate": "DateTime?"
}
```

---

### 5. CommandesController
**Route**: `/api/Commandes`  
**Authentification**: `[Authorize]`  
**OutputCache**: Policy "DefaultPolicy"

#### Endpoints
| Méthode | Endpoint | Cache | Description |
|---------|----------|-------|-------------|
| POST | `/` | ? | Envoie commande IoT |
| GET | `/my-commands?fresh&limit&offset` | ? | Historique utilisateur |
| GET | `/by-peripherique/{peripheriqueId}?fresh&limit` | ? | Historique périphérique |

#### DTOs
**CommandeRequestDto** (Request):
```csharp
{
  "peripheriqueLogiqueId": "int",
  "typeCommande": "string",
  "parametres": "string?"
}
```

**CommandeResponseDto** (Response):
```csharp
{
  "isSuccess": "bool",
  "message": "string",
  "commandeId": "int?",
  "peripheriqueLogiqueId": "int",
  "typeCommande": "string",
  "dateExecution": "DateTime?",
  "userId": "string?"
}
```

---

### 6. UserManagementController
**Route**: `/api/UserManagement`  
**Authentification**: `[Authorize(Roles="Admin")]`

#### Endpoints - Utilisateurs
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/users` | Liste tous les utilisateurs |
| GET | `/users/{userId}` | Récupère utilisateur par ID |
| GET | `/users/search?searchTerm={term}` | Recherche utilisateurs |
| POST | `/users` | Crée utilisateur |
| PUT | `/users/{userId}` | Met à jour utilisateur |
| PATCH | `/users/{userId}/toggle-status` | Active/Désactive utilisateur |
| DELETE | `/users/{userId}` | Supprime utilisateur |

#### Endpoints - Rôles
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/roles` | Liste tous les rôles |
| POST | `/roles` | Crée rôle |
| DELETE | `/roles/{roleId}` | Supprime rôle |

#### Endpoints - Association Utilisateurs-Rôles
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/users/{userId}/roles` | Rôles d'un utilisateur |
| PUT | `/users/{userId}/roles` | Remplace rôles utilisateur |
| POST | `/users/{userId}/roles/{roleName}` | Ajoute rôle |
| DELETE | `/users/{userId}/roles/{roleName}` | Retire rôle |
| GET | `/roles/{roleName}/users` | Utilisateurs d'un rôle |

#### Endpoints - Statistiques
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/statistics` | Statistiques utilisateurs/rôles |

#### DTOs
**CreateUserDto** (Request):
```csharp
{
  "userName": "string",
  "email": "string",
  "fullName": "string",
  "password": "string",
  "phoneNumber": "string?",
  "emailConfirmed": "bool",
  "roles": ["string"]
}
```

**UpdateUserDto** (Request):
```csharp
{
  "userName": "string",
  "email": "string",
  "fullName": "string",
  "phoneNumber": "string?",
  "emailConfirmed": "bool",
  "lockoutEnabled": "bool"
}
```

**UserDetailsDto** (Response):
```csharp
{
  "id": "string",
  "userName": "string",
  "email": "string",
  "fullName": "string",
  "phoneNumber": "string?",
  "emailConfirmed": "bool",
  "phoneNumberConfirmed": "bool",
  "twoFactorEnabled": "bool",
  "lockoutEnd": "DateTimeOffset?",
  "lockoutEnabled": "bool",
  "accessFailedCount": "int",
  "roles": ["string"],
  "createdDate": "DateTime",
  "lastLogin": "DateTime",
  "isActive": "bool"
}
```

**AssignRolesDto** (Request):
```csharp
{
  "userId": "string",
  "roleNames": ["string"]
}
```

**UserStatisticsDto** (Response):
```csharp
{
  "totalUsers": "int",
  "activeUsers": "int",
  "lockedUsers": "int",
  "unconfirmedEmails": "int",
  "totalRoles": "int",
  "usersByRole": {
    "roleName": "count"
  }
}
```

---

### 7. DroitAccesController
**Route**: `/api`  
**Authentification**: Variable selon endpoint

#### Endpoints - Administration (Admin only)
| Méthode | Endpoint | Auth | Description |
|---------|----------|------|-------------|
| GET | `/admin/access-rights` | Admin | Liste droits d'accès |
| GET | `/admin/access-rights/{id}` | Admin | Récupère droit par ID |
| POST | `/admin/access-rights` | Admin | Crée droit d'accès |
| PUT | `/admin/access-rights/{id}` | Admin | Met à jour droit |
| DELETE | `/admin/access-rights/{id}` | Admin | Supprime droit |
| POST | `/admin/assign-zone-to-user` | Admin | Affecte zone à utilisateur |
| POST | `/admin/assign-device-to-user` | Admin | Affecte périphérique à utilisateur |

#### Endpoints - Utilisateurs
| Méthode | Endpoint | Auth | Description |
|---------|----------|------|-------------|
| GET | `/user/access-rights/zones` | User | Droits zones utilisateur |
| GET | `/user/access-rights/peripheriques` | User | Droits périphériques utilisateur |

#### DTOs
**DroitAccesDto** (Response):
```csharp
{
  "id": "int",
  "userId": "string",
  "userName": "string?",
  "zoneId": "int?",
  "zoneName": "string?",
  "peripheriqueLogiqueId": "int?",
  "peripheriqueName": "string?",
  "type": "string",          // "1"=Lecture, "2"=Controle, "3"=Administration
  "dateOctroi": "DateTime",
  "dateExpiration": "DateTime?"
}
```

**DroitAccesCreationDto** (Request):
```csharp
{
  "userId": "string",
  "zoneId": "int?",
  "peripheriqueLogiqueId": "int?",
  "type": "string"           // "1", "2", ou "3"
}
```

**AssignZoneToUserDto** (Request):
```csharp
{
  "userId": "string",
  "zoneId": "int",
  "accessType": "DroitAccesType"  // Enum: Lecture, Controle, Administration
}
```

**AssignDeviceToUserDto** (Request):
```csharp
{
  "userId": "string",
  "peripheriqueLogiqueId": "int",
  "accessType": "DroitAccesType"
}
```

---

### 8. IncidentTicketsController
**Route**: `/api/IncidentTickets`  
**Authentification**: `[Authorize]`

#### Endpoints
| Méthode | Endpoint | Auth | Description |
|---------|----------|------|-------------|
| POST | `/` | User | Crée ticket incident |
| GET | `/{id}` | User | Récupère ticket par ID |
| GET | `/status/{status}` | User | Tickets par statut |
| GET | `/user/{userId}` | User | Tickets d'un utilisateur |
| PUT | `/{id}/status` | User | Met à jour statut |
| PUT | `/{id}/assign` | Manager/Admin | Assigne ticket |
| DELETE | `/{id}` | Admin | Supprime ticket |

#### DTOs
**IncidentTicketCreateDto** (Request):
```csharp
{
  "titre": "string",
  "description": "string",
  "peripheriqueLogiqueId": "int?",
  "zoneId": "int?",
  "priorite": "string",      // "Basse", "Moyenne", "Haute", "Critique"
  "categorie": "string?"     // "Électrique", "Réseau", "Sécurité", etc.
}
```

**IncidentTicketDto** (Response):
```csharp
{
  "id": "int",
  "titre": "string",
  "description": "string",
  "statut": "string",        // "Ouvert", "En cours", "Fermé"
  "priorite": "string",
  "categorie": "string?",
  "createdById": "string",
  "createdByName": "string?",
  "assignedToId": "string?",
  "assignedToName": "string?",
  "peripheriqueLogiqueId": "int?",
  "peripheriqueName": "string?",
  "zoneId": "int?",
  "zoneName": "string?",
  "dateCreation": "DateTime",
  "dateModification": "DateTime?",
  "dateFermeture": "DateTime?"
}
```

---

### 9. NotificationController
**Route**: `/api/Notification`  
**Authentification**: `[Authorize]`

#### Endpoints
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/` | Notifications utilisateur |
| PUT | `/{id}` | Marque notification lue |

#### DTOs
**NotificationDto**:
```csharp
{
  "id": "int",
  "userId": "string",
  "titre": "string",
  "message": "string",
  "type": "string",          // "Info", "Warning", "Error", "Success"
  "isRead": "bool",
  "dateCreation": "DateTime",
  "relatedEntityId": "int?",
  "relatedEntityType": "string?"
}
```

---

### 10. ConsommationStatsController
**Route**: `/api/ConsommationStats`  
**Authentification**: `[Authorize]`

#### Endpoints
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/peripherique/{peripheriqueId}?startDate&endDate` | Stats consommation périphérique |
| GET | `/zone/{zoneId}?startDate&endDate` | Stats consommation zone |
| GET | `/peripherique/{peripheriqueId}/horaire?date` | Consommation horaire |
| GET | `/peripherique/{peripheriqueId}/tendances?nombreJours` | Tendances consommation |
| GET | `/peripherique/{peripheriqueId}/anomalies?startDate&endDate` | Détection anomalies |
| GET | `/rapport?zoneId&startDate&endDate` | Rapport consommation détaillé |
| GET | `/peripherique/{peripheriqueId}/cout?startDate&endDate&tarifKwh` | Calcul coût énergétique |
| GET | `/top-consommateurs?zoneId&startDate&endDate&top` | Top consommateurs |
| GET | `/peripherique/{peripheriqueId}/comparaison?startDate1&endDate1&startDate2&endDate2` | Compare périodes |

#### DTOs
**ConsommationStatsDto**:
```csharp
{
  "peripheriqueLogiqueId": "int",
  "peripheriqueName": "string",
  "startDate": "DateTime",
  "endDate": "DateTime",
  "consommationTotaleKwh": "double",
  "consommationMoyenneKwh": "double",
  "consommationMaxKwh": "double",
  "consommationMinKwh": "double",
  "nombreMesures": "int",
  "coutEstime": "decimal?"
}
```

---

### 11. DashboardController
**Route**: `/api/Dashboard`  
**Authentification**: `[Authorize]`  
**OutputCache**: Policies avec tags

#### Endpoints
| Méthode | Endpoint | Cache | Tags | Description |
|---------|----------|-------|------|-------------|
| GET | `/stats` | 60s | dashboard-stats | Statistiques KPI |
| GET | `/energy?period={day|week|month}` | 300s | dashboard-energy | Données énergétiques |
| GET | `/device-activity` | 120s | dashboard-devices | Activité appareils |

#### DTOs
**DashboardStatsDto** (Response):
```csharp
{
  "activeDevices": "int",
  "totalDevices": "int",
  "devicesChangePercent": "double",
  "activeUsers": "int",
  "totalUsers": "int",
  "usersChangePercent": "double",
  "openIncidents": "int",
  "totalIncidents": "int",
  "incidentsChangePercent": "double",
  "energyConsumed": "double",
  "energyChangePercent": "double"
}
```

**EnergyDataPointDto**:
```csharp
{
  "label": "string",         // Format selon période: "HH:mm", "ddd", "dd/MM"
  "value": "double"          // Consommation en Wh
}
```

**DeviceActivityDataDto**:
```csharp
{
  "deviceType": "string",
  "count": "int"
}
```

---

### 12. UserActivityController
**Route**: `/api/user/activity`  
**Authentification**: `[Authorize]`

#### Endpoints
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/history?pageNumber&pageSize&startDate&endDate&activityType` | Historique activité utilisateur |
| GET | `/stats?startDate&endDate` | Statistiques activité |
| GET | `/recent` | Activités récentes (24h) |

#### DTOs
**UserActivityDto** (Response):
```csharp
{
  "id": "int",
  "userId": "string",
  "userName": "string?",
  "activityType": "string",      // "Commande", "Audit"
  "action": "string",
  "resource": "string?",
  "resourceId": "int?",
  "details": "string?",
  "timestamp": "DateTime",
  "ipAddress": "string?",
  "userAgent": "string?"
}
```

**UserActivityStatsDto** (Response):
```csharp
{
  "totalActivities": "int",
  "commandeCount": "int",
  "auditCount": "int",
  "mostActiveHour": "int",
  "mostActiveDay": "string",
  "activitiesByType": {
    "type": "count"
  }
}
```

---

### 13. IoTHubDeviceController
**Route**: `/api/IoTHubDevice`  
**Authentification**: `[Authorize(Roles="Admin,Manager")]`

#### ?? Commandes Directes
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| POST | `/{deviceId}/method/{methodName}` | Invoque méthode directe |
| POST | `/{deviceId}/status` | Statut complet device |
| POST | `/{deviceId}/door/open` | Ouvre porte |
| POST | `/{deviceId}/door/close` | Ferme porte |
| POST | `/{deviceId}/reboot` | Redémarre device |
| POST | `/{deviceId}/alarm/on` | Active alarme fumée |
| POST | `/{deviceId}/alarm/off` | Désactive alarme |
| POST | `/{deviceId}/heartbeat` | Force heartbeat |
| POST | `/{deviceId}/telemetry/send` | Force télémétrie |

#### ?? Contrôle Périphérique
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| POST | `/{deviceId}/peripheral/{name}/{command}` | Commande périphérique |

#### ? Gestion Énergétique
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| POST | `/{deviceId}/energy/reset` | Réinitialise compteurs |
| POST | `/{deviceId}/energy/report` | Rapport énergétique |

#### ?? Diagnostics
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| POST | `/{deviceId}/diagnostics` | Diagnostics complets |
| POST | `/{deviceId}/test/log` | Envoie log de test |

#### ?? Gestion Device Twin
| Méthode | Endpoint | Auth | Description |
|---------|----------|------|-------------|
| GET | `/{deviceId}/twin` | Manager | Récupère Device Twin |
| GET | `/list?maxCount={100}` | Manager | Liste devices IoT Hub |
| POST | `/connection-status` | Manager | Statut connexion devices |
| POST | `/{deviceId}/message` | Manager | Message Cloud-to-Device |
| PUT | `/{deviceId}` | Admin | Crée/Met à jour device |
| DELETE | `/{deviceId}` | Admin | Supprime device |

#### DTOs
**DirectMethodPayload**:
```csharp
{
  "payload": "string"          // JSON stringifié
}
```

**PeripheralControlParameters**:
```csharp
{
  "parameters": {
    "key": "value"
  }
}
```

**TestLogPayload**:
```csharp
{
  "level": "info" | "warn" | "error",
  "source": "string",
  "message": "string"
}
```

**CloudMessagePayload**:
```csharp
{
  "message": "string",
  "properties": {
    "key": "value"
  }
}
```

**DeviceUpdatePayload**:
```csharp
{
  "enabled": "bool"
}
```

---

## ?? SignalR Hubs

### NotificationHub
**Endpoint**: `/notificationhub`  
**Authentification**: `[Authorize]` avec support JWT via `access_token` query parameter

#### Méthodes Client ? Server

##### `OnConnectedAsync()`
- Connexion automatique au groupe personnel `User_{userId}`
- Log de connexion

##### `OnDisconnectedAsync()`
- Déconnexion automatique du groupe personnel
- Log de déconnexion

##### `SendNotificationToUser(userId, title, message)`
**Paramètres**:
```csharp
{
  "userId": "string",
  "title": "string",
  "message": "string"
}
```

##### `SendNotificationToZone(zoneId, title, message)`
**Paramètres**:
```csharp
{
  "zoneId": "int",
  "title": "string",
  "message": "string"
}
```

##### `JoinZoneGroup(zoneId)`
Rejoint le groupe `Zone_{zoneId}`

##### `LeaveZoneGroup(zoneId)`
Quitte le groupe `Zone_{zoneId}`

##### `SendDeviceStatusUpdate(peripheriqueId, status, data)`
**Paramètres**:
```csharp
{
  "peripheriqueId": "int",
  "status": "string",
  "data": "object?"
}
```

##### `SendTelemetryUpdate(peripheriqueId, telemetryData)`
**Paramètres**:
```csharp
{
  "peripheriqueId": "int",
  "telemetryData": "object"
}
```

##### `SendIncidentAlert(incidentId, title, description, priority)`
**Paramètres**:
```csharp
{
  "incidentId": "int",
  "title": "string",
  "description": "string",
  "priority": "string"
}
```

##### `SendCommandExecutionResult(commandId, peripheriqueId, status, result)`
**Paramètres**:
```csharp
{
  "commandId": "int",
  "peripheriqueId": "int",
  "status": "string",
  "result": "string"
}
```

#### Événements Server ? Client

##### `ReceiveNotification`
**Payload**:
```json
{
  "title": "string",
  "message": "string",
  "timestamp": "DateTime"
}
```

##### `ReceiveZoneNotification`
**Payload**:
```json
{
  "zoneId": "int",
  "title": "string",
  "message": "string",
  "timestamp": "DateTime"
}
```

##### `JoinedZoneGroup`
**Payload**: `zoneId: int`

##### `LeftZoneGroup`
**Payload**: `zoneId: int`

##### `DeviceStatusUpdate`
**Payload**:
```json
{
  "peripheriqueId": "int",
  "status": "string",
  "data": "object?",
  "timestamp": "DateTime"
}
```

##### `TelemetryUpdate`
**Payload**:
```json
{
  "peripheriqueId": "int",
  "data": "object",
  "timestamp": "DateTime"
}
```

##### `IncidentAlert`
**Payload**:
```json
{
  "incidentId": "int",
  "title": "string",
  "description": "string",
  "priority": "string",
  "timestamp": "DateTime"
}
```

##### `CommandExecutionResult`
**Payload**:
```json
{
  "commandId": "int",
  "peripheriqueId": "int",
  "status": "string",
  "result": "string",
  "timestamp": "DateTime"
}
```

---

## ?? URLs de l'API

### Développement
**HTTPS**: `https://localhost:7144`  
**HTTP**: `http://localhost:5290`

### IIS Express
**HTTP**: `http://localhost:53886/`  
**HTTPS**: `https://localhost:44387/`

### Docker
**HTTP**: `http://localhost:8080`  
**HTTPS**: `https://localhost:8081`

### SignalR Hub
**Endpoint**: `{BASE_URL}/notificationhub`  
**Exemple**: `https://localhost:7144/notificationhub`

**Connexion avec JWT**:
```javascript
const connection = new HubConnectionBuilder()
    .withUrl("https://localhost:7144/notificationhub", {
        accessTokenFactory: () => localStorage.getItem("token")
    })
    .build();
```

**Connexion avec Query Parameter** (Alternative):
```javascript
const token = localStorage.getItem("token");
const connection = new HubConnectionBuilder()
    .withUrl(`https://localhost:7144/notificationhub?access_token=${token}`)
    .build();
```

---

## ?? Swagger/OpenAPI

### Configuration
**Swagger UI**: `{BASE_URL}/swagger`  
**OpenAPI JSON**: `{BASE_URL}/swagger/v1/swagger.json`

**Exemples**:
- Dev: `https://localhost:7144/swagger`
- Dev: `https://localhost:7144/swagger/v1/swagger.json`

### Authentification dans Swagger
1. Cliquer sur **Authorize** ??
2. Entrer: `Bearer {votre_token_jwt}`
3. Cliquer sur **Authorize**
4. Tester les endpoints sécurisés

### Télécharger OpenAPI JSON
```bash
curl https://localhost:7144/swagger/v1/swagger.json -o smartoffice-api-spec.json
```

---

## ?? DTOs Principaux (Récapitulatif)

### Authentication
- `RegisterDto` (Request)
- `LoginDto` (Request)
- `AuthResponseDto` (Response)

### Zones
- `ZoneDto` (Response)
- `ZoneCreationDto` (Request)
- `ZoneUpdateDto` (Request)

### Périphériques
- `PeripheriqueLogiqueDto` (Response)
- `PeripheriqueLogiqueCreationDto` (Request)
- `PeripheriqueLogiqueUpdateDto` (Request)
- `PeripheralDto` (Firmware v2.0)
- `PeripheralEnergyDto` (Firmware v1.3.0)
- `PeripheralEnergyStatsDto` (Firmware v1.3.0)

### Télémétrie
- `TelemetrieDto` (Response)
- `Telemetrie` (Model)

### Commandes
- `CommandeRequestDto` (Request)
- `CommandeResponseDto` (Response)

### Droits d'Accès
- `DroitAccesDto` (Response)
- `DroitAccesCreationDto` (Request)
- `AssignZoneToUserDto` (Request)
- `AssignDeviceToUserDto` (Request)

### Gestion Utilisateurs
- `UserDetailsDto` (Response)
- `CreateUserDto` (Request)
- `UpdateUserDto` (Request)
- `AssignRolesDto` (Request)
- `RoleDto` (Response)
- `UserStatisticsDto` (Response)

### Incidents
- `IncidentTicketDto` (Response)
- `IncidentTicketCreateDto` (Request)

### Notifications
- `NotificationDto` (Response)

### Consommation & Analytics
- `ConsommationStatsDto` (Response)
- `DashboardStatsDto` (Response)
- `EnergyDataPointDto` (Response)
- `DeviceActivityDataDto` (Response)

### Activité Utilisateur
- `UserActivityDto` (Response)
- `UserActivityStatsDto` (Response)

### API Response Wrapper
- `ApiResponse<T>` (Generic wrapper pour cache/database)

---

## ?? Architecture Globale

### Couches de l'Application

```
???????????????????????????????????????????
?         Controllers (API Layer)         ?
???????????????????????????????????????????
?       Services (Business Logic)         ?
???????????????????????????????????????????
?     Repositories (Data Access)          ?
???????????????????????????????????????????
?   ApplicationDbContext (EF Core)        ?
???????????????????????????????????????????
?         PostgreSQL Database             ?
???????????????????????????????????????????

          ????????????????
          ?  Redis Cache ?  (OutputCache + DistributedCache)
          ????????????????

          ????????????????
          ? Azure IoT Hub?  (Device Management)
          ????????????????

          ????????????????
          ?  SignalR Hub ?  (Real-time notifications)
          ????????????????
```

### Background Workers
1. **TelemetrieIngestionWorker**: Ingestion télémétrie IoT Hub
2. **LogIngestionWorker**: Ingestion logs devices
3. **DeviceSyncWorker**: Synchronisation périodique Device Twins

### Middleware Pipeline
```
HTTP Request
    ?
CamelCaseEnforcerMiddleware (Force camelCase JSON)
    ?
CORS Middleware
    ?
OutputCache Middleware
    ?
Authentication Middleware (JWT)
    ?
Authorization Middleware
    ?
Activity Logging Middleware
    ?
Controllers
    ?
Response (JSON camelCase)
```

---

## ??? Informations Techniques

### Technologies
- **.NET**: 9.0
- **ASP.NET Core**: 9.0
- **Entity Framework Core**: 9.0
- **Database**: PostgreSQL
- **Cache**: Redis (StackExchange.Redis) / In-Memory fallback
- **IoT**: Azure IoT Hub SDK
- **Mapping**: AutoMapper
- **SignalR**: ASP.NET Core SignalR
- **Authentication**: JWT Bearer

### NuGet Packages Principaux
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Tools`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `StackExchange.Redis`
- `Microsoft.AspNetCore.SignalR`
- `Azure.Messaging.EventHubs`
- `AutoMapper.Extensions.Microsoft.DependencyInjection`

### Features Activées
- ✔️ **OutputCache** avec policies et tags
- ✔️ **DistributedCache** (Redis/Memory)
- ✔️ **JWT Authentication**
- ✔️ **Role-based Authorization**
- ✔️ **Resource-based Authorization** (Custom handlers)
- ✔️ **SignalR** avec authentification JWT
- ✔️ **CORS** pour Web + Mobile MAUI
- ✔️ **camelCase JSON** (Middleware + Options)
- ✔️ **Swagger/OpenAPI** avec sécurité JWT
- ✔️ **Health Checks** (`/health/live`, `/health/ready`)
- ✔️ **Rate Limiting** (Global: 100 req/10s)
- ✔️ **Activity Logging** (Audit trail)
- ✔️ **Background Workers** (Telemetry, Logs, Sync)

---

## ?? Notes Importantes

### Cache Strategy
- **OutputCache**: Utilisé pour GET endpoints avec `?fresh=true` support
- **DistributedCache**: Utilisé pour data access layer caching
- **Tags**: Permet invalidation ciblée (`dashboard-stats`, `device-data`, etc.)
- **VaryByQuery**: Cache distinct par paramètres query (`fresh`, `limit`, `deviceId`, etc.)

### Sécurité
- **JWT Expiration**: Configuré dans `JwtSettings:TokenExpirationMinutes`
- **Refresh Tokens**: Supportés via `ApplicationUser.RefreshToken`
- **Password Policy**: Identity default (6 caractères min, majuscule, chiffre)
- **Lockout**: Activable via `UpdateUserDto.LockoutEnabled`
- **HTTPS**: Forcé en production (`app.UseHttpsRedirection()`)

### IoT Hub Integration
- **Device Twin**: Synchronisation automatique via `DeviceSyncWorker`
- **Direct Methods**: Invocation via `IoTHubDeviceController`
- **Telemetry Ingestion**: Via Event Hub + `TelemetrieIngestionWorker`
- **Firmware Compatibility**: v1.3.0 (Energy metering) + v2.0.0 (Enhanced peripherals)

### Performance
- **Connection Pooling**: PostgreSQL (default: 100)
- **Redis Connection**: Multiplexer avec retry policy
- **SignalR**: Keep-alive 15s, timeout 30s
- **Rate Limiting**: 100 req/10s global
- **OutputCache**: Expiration 1-5 minutes selon endpoint

---

## 🚀 Démarrage Rapide

### 1. Cloner et Configurer
```bash
git clone <repo-url>
cd SmartOfficeAPP
dotnet restore
```

### 2. Configuration User Secrets (Dev)
```bash
# ConnectionString PostgreSQL
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=smartoffice;Username=postgres;Password=votrepassword"

# Redis (optionnel)
dotnet user-secrets set "Redis:ConnectionString" "localhost:6379"

# IoT Hub
dotnet user-secrets set "IotHubConnectionString" "HostName=....azure-devices.net;SharedAccessKeyName=...;SharedAccessKey=..."
dotnet user-secrets set "IotHubEventHub:ConnectionString" "Endpoint=sb://....servicebus.windows.net/;..."
```

### 3. Migrations & Seed
```bash
dotnet ef database update
dotnet run
# Le DataSeedingService créera les rôles par défaut
```

### 4. Créer un Admin
```http
POST https://localhost:7144/api/Auth/register
Content-Type: application/json

{
  "userName": "admin",
  "email": "admin@smartoffice.com",
  "password": "Admin123!",
  "confirmPassword": "Admin123!",
  "fullName": "Administrateur"
}
```

### 5. Assigner le Rôle Admin (via DbContext ou UI)
```sql
-- PostgreSQL
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT u."Id", r."Id" 
FROM "AspNetUsers" u, "AspNetRoles" r 
WHERE u."Email" = 'admin@smartoffice.com' AND r."Name" = 'Admin';
```

### 6. Login et Tester
```http
POST https://localhost:7144/api/Auth/login
Content-Type: application/json

{
  "email": "admin@smartoffice.com",
  "password": "Admin123!"
}
```

Copier le `token` de la réponse et l'utiliser dans Swagger ou dans les headers:
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 🆘 Support & Contact

Pour toute question sur l'API:
1. Consulter la documentation Swagger: `https://localhost:7144/swagger`
2. Télécharger le fichier OpenAPI: `https://localhost:7144/swagger/v1/swagger.json`
3. Consulter les tests HTTP: `SmartOfficeAPP/Tests/*.http`

---

**Fin de la documentation complète** ✅
