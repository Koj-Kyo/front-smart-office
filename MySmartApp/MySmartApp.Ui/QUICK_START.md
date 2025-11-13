# ?? Quick Start - Front Blazor WASM + Backend ASP.NET Core 9

**Dernière mise à jour**: 2025-01-28

---

## ? **Démarrage Ultra-Rapide (5 minutes)**

### **Prérequis**
- .NET 9 SDK installé
- Visual Studio 2022 ou VS Code + C# Extension
- Ports libres: `7144` (backend HTTPS), `7152` (front HTTPS)

---

### **Étape 1: Démarrer le Backend**

```bash
# Terminal 1 - Backend
cd D:\Memoire\BackendSmart\SmartOfficeAPP
dotnet run

# Vérifier que le serveur démarre:
# ? Now listening on: https://localhost:7144
# ? Now listening on: http://localhost:5290
```

**Test rapide**:
- Ouvrir: `https://localhost:7144/swagger`
- ? Si Swagger UI s'affiche ? Backend OK

---

### **Étape 2: Démarrer le Front Blazor WASM**

```bash
# Terminal 2 - Frontend
cd D:\Memoire\FrontSmart\MySmartApp\MySmartApp.Ui
dotnet run

# Vérifier que le serveur démarre:
# ? Now listening on: https://localhost:7152
# ? Now listening on: http://localhost:5249
```

**Test rapide**:
- Ouvrir: `https://localhost:7152`
- ? Si la page d'accueil s'affiche ? Front OK

---

### **Étape 3: Login Admin**

1. Cliquer sur **"Se connecter"**
2. Entrer:
   - **Email**: `admin@smartoffice.com`
   - **Password**: `Admin123!`
3. Cliquer sur **"Connexion"**
4. ? Si redirection vers `/dashboard` ? Auth OK

---

### **Étape 4: Test Dashboard**

1. Sur `/dashboard`, vérifier:
   - ? **KPIs** affichés (Active Devices, Users, Incidents, Energy)
   - ? **Graphique énergie** visible
   - ? **Device Activity Chart** visible
2. Si tout s'affiche ? ? **Intégration réussie !**

---

## ?? **Vérification Rapide des Endpoints**

### **Test 1: Auth (Login)**
```http
POST https://localhost:7144/api/Auth/login
Content-Type: application/json

{
  "email": "admin@smartoffice.com",
  "password": "Admin123!"
}

# ? Réponse attendue:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2025-01-29T10:30:00Z",
  "userId": "...",
  "email": "admin@smartoffice.com",
  "roles": ["Admin"]
}
```

### **Test 2: Dashboard Stats**
```http
GET https://localhost:7144/api/Dashboard/stats
Authorization: Bearer {votre_token_jwt}

# ? Réponse attendue:
{
  "activeDevices": 5,
  "totalDevices": 10,
  "devicesChangePercent": 0,
  "activeUsers": 3,
  "totalUsers": 5,
  "usersChangePercent": 0,
  "openIncidents": 2,
  "totalIncidents": 8,
  "incidentsChangePercent": 0,
  "energyConsumed": 1250.5,
  "energyChangePercent": 0
}
```

### **Test 3: SignalR (DevTools Console)**
```javascript
// Ouvrir DevTools > Console, vérifier:
? Connected to NotificationHub
? SignalR connection established
```

---

## ?? **Troubleshooting Express**

### **? Backend ne démarre pas**
```bash
# Vérifier les ports occupés
netstat -ano | findstr :7144
netstat -ano | findstr :5290

# Si occupé, tuer le processus:
taskkill /PID {PID} /F

# Ou changer les ports dans launchSettings.json
```

### **? CORS Error**
```
Access to fetch at 'https://localhost:7144/api/...' has been blocked by CORS policy
```

**Solution Backend (Program.cs)**:
```csharp
// Avant app.Build()
builder.Services.AddCors(opt => 
    opt.AddPolicy("AllowFrontend", p => p
        .WithOrigins("https://localhost:7152", "http://localhost:5249")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

// Après app.UseAuthentication() et app.UseAuthorization()
app.UseCors("AllowFrontend");
```

### **? 401 Unauthorized**
```
Failed to load resource: the server responded with a status of 401 (Unauthorized)
```

**Vérifications**:
1. Token JWT bien stocké dans LocalStorage ?
   - DevTools > Application > LocalStorage > `authToken`
2. Token expiré ?
   - Se reconnecter
3. Header `Authorization: Bearer {token}` présent ?
   - DevTools > Network > Cliquer sur la requête > Headers

### **? SignalR Not Connecting**
```
Error: Failed to complete negotiation with the server
```

**Vérifications**:
1. Backend SignalR activé ?
   ```csharp
   builder.Services.AddSignalR();
   app.MapHub<NotificationHub>("/notificationhub");
   ```
2. Authentification JWT configurée pour SignalR ?
   ```csharp
   options.Events = new JwtBearerEvents
   {
       OnMessageReceived = context =>
       {
           var accessToken = context.Request.Query["access_token"];
           if (!string.IsNullOrEmpty(accessToken) && 
               context.HttpContext.Request.Path.StartsWithSegments("/notificationhub"))
           {
               context.Token = accessToken;
           }
           return Task.CompletedTask;
       }
   };
   ```

---

## ?? **Tests des Pages Principales**

| Page | URL | Test Rapide |
|------|-----|-------------|
| Dashboard | `/dashboard` | ? KPIs + Graphiques affichés |
| Zones | `/zone` | ? Liste des zones visible |
| Devices | `/devices` | ? Liste devices + périphériques |
| Télémétrie | `/telemetrie` | ? Données télémétrie affichées |
| Incidents | `/incidents` | ? Liste tickets visible |
| Users (Admin) | `/users` | ? Liste utilisateurs + rôles |
| Droits d'Accès | `/access` | ? Liste droits affichée |
| Analytics | `/analytics` | ? Stats consommation visibles |
| Notifications | `/notifications` | ? Liste notifications affichée |

---

## ?? **Documentation Complète**

| Document | Description |
|----------|-------------|
| `API-COMPLETE-DOCUMENTATION.md` | Doc complète backend (endpoints, DTOs, config) |
| `FRONT_BACKEND_ALIGNMENT.md` | Mapping DTOs/Services/Endpoints |
| `QUICK_TEST_GUIDE.md` | Guide de test phase par phase (11 phases) |
| `README_ALIGNMENT.md` | Récapitulatif de l'alignement |
| `QUICK_START.md` | Ce guide de démarrage ultra-rapide |

---

## ?? **Checklist Démarrage Réussi**

- [ ] Backend démarre sur `https://localhost:7144`
- [ ] Swagger accessible sur `/swagger`
- [ ] Front démarre sur `https://localhost:7152`
- [ ] Login Admin réussi
- [ ] Dashboard affiche les KPIs
- [ ] SignalR connecté (vérifier console DevTools)
- [ ] Aucune erreur CORS dans la console
- [ ] Token JWT stocké dans LocalStorage

---

## ? **Next Steps**

1. ? Exécuter les tests du `QUICK_TEST_GUIDE.md` (Phases 1-6)
2. ? Tester CRUD sur Zones, Devices, Incidents
3. ? Vérifier SignalR temps réel
4. ? Tester les analytics de consommation
5. ?? Tests de charge (Postman)

---

**?? Bon développement !**
