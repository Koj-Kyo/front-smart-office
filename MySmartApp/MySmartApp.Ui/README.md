# ?? MySmartApp - Application Smart Home

## ?? Vue d'ensemble

**MySmartApp** est une application de gestion de maison intelligente développée avec **Blazor WebAssembly** et **.NET 9**. Elle permet de contrôler et surveiller des appareils IoT connectés, gérer des zones de la maison, et administrer les utilisateurs avec un système d'authentification et d'autorisation complet.

### ?? Fonctionnalités principales

- ?? **Gestion des zones** de la maison
- ?? **Contrôle des appareils IoT** (éclairage, température, etc.)
- ?? **Gestion des utilisateurs** et des rôles
- ?? **Authentification JWT** sécurisée
- ?? **Tableau de bord** interactif avec analytics
- ?? **Temps réel** via SignalR
- ?? **Interface moderne** avec MudBlazor

---

## ??? Architecture technique

### Stack technologique

- **Framework** : Blazor WebAssembly (.NET 9)
- **UI Library** : MudBlazor 8.13.0
- **Authentification** : JWT avec Microsoft.AspNetCore.Components.Authorization
- **Temps réel** : SignalR Client
- **HTTP Client** : Microsoft.Extensions.Http avec intercepteur d'authentification

### ?? Packages NuGet

| Package | Version | Usage |
|---------|---------|-------|
| `Microsoft.AspNetCore.Components.WebAssembly` | 9.0.9 | Framework Blazor WASM |
| `Microsoft.AspNetCore.Components.Authorization` | 9.0.9 | Système d'autorisation |
| `Microsoft.AspNetCore.SignalR.Client` | 9.0.9 | Communication temps réel |
| `MudBlazor` | 8.13.0 | Composants UI Material Design |
| `System.IdentityModel.Tokens.Jwt` | 8.14.0 | Parsing des tokens JWT |

---

## ??? Structure du projet

```
MySmartApp.Ui/
??? ?? Components/
?   ??? RedirectToLogin.razor          # Redirection vers login
??? ?? Layout/
?   ??? MainLayout.razor               # Layout principal
?   ??? NavMenu.razor                  # Menu de navigation
?   ??? DashBoardHeader.razor          # En-tête du dashboard
?   ??? DeviceCard.razor               # Carte d'appareil
?   ??? TemperatureGauge.razor         # Jauge de température
??? ?? Pages/
?   ??? Home.razor                     # Page d'accueil publique
?   ??? Login.razor                    # Page de connexion
?   ??? Index.razor                    # Dashboard principal (??)
?   ??? Devices.razor                  # Gestion appareils (?? Admin)
?   ??? Zone.razor                     # Gestion zones (?? Admin)
?   ??? Users.razor                    # Gestion utilisateurs (?? Admin)
?   ??? Analytics.razor                # Analytiques (??)
?   ??? Profiles.razor                 # Profil utilisateur (??)
?   ??? Access.razor                   # Droits d'accès (??)
??? ?? Services/
?   ??? IAuthService.cs / AuthService.cs                    # Authentification
?   ??? CustomAuthenticationStateProvider.cs               # État d'authentification
?   ??? ITokenProvider.cs / TokenProvider.cs               # Gestion tokens
?   ??? ILocalStorageService.cs / LocalStorageService.cs   # Stockage local
?   ??? AuthMessageHandler.cs                              # Intercepteur HTTP
?   ??? ISignalRService.cs / SignalRService.cs            # SignalR
?   ??? IDeviceService.cs / DeviceService.cs               # API Appareils
?   ??? IZoneService.cs / ZoneService.cs                   # API Zones
?   ??? IUserService.cs / UserService.cs                   # API Utilisateurs
??? ?? Models/
?   ??? ApiModels.cs                   # Modèles de données API
??? ?? Themes/
?   ??? MyTheme.cs                     # Thème MudBlazor personnalisé
??? ?? wwwroot/
    ??? appsettings.json               # Configuration production
    ??? appsettings.Development.json   # Configuration développement
    ??? index.html                     # Point d'entrée HTML
```

**Légende :** ?? = Authentification requise | ?? = Admin uniquement

---

## ?? Système d'authentification

### Architecture de sécurité

#### 1. **Flux d'authentification**
1. **Login** ? API `/Auth/login` avec email/password
2. **Réception JWT** ? Stockage en localStorage
3. **Parsing du token** ? Extraction des claims (nom, rôles, etc.)
4. **Mise à jour état** ? `CustomAuthenticationStateProvider`
5. **Navigation** ? Redirection vers dashboard

#### 2. **Gestion des autorisations**

**Pages protégées par authentification :**
- `/dashboard` - Tableau de bord principal
- `/analytics` - Analyses et statistiques
- `/profile` - Profil utilisateur
- `/access` - Gestion des droits d'accès

**Pages réservées aux administrateurs :**
- `/devices` - Gestion des appareils IoT
- `/zones` - Gestion des zones de la maison
- `/utilisateurs` - Administration des utilisateurs

#### 3. **Composants de sécurité**

```csharp
// Protection au niveau route
[Authorize]                    // Authentification requise
[Authorize(Roles = "Admin")]   // Rôle Admin requis

// Protection au niveau UI
<AuthorizeView>
    <Authorized>/* Contenu pour utilisateurs connectés */</Authorized>
    <NotAuthorized>/* Contenu pour visiteurs */</NotAuthorized>
</AuthorizeView>

<AuthorizeView Roles="Admin">
    <Authorized>/* Contenu réservé aux admins */</Authorized>
</AuthorizeView>
```

---

## ?? API et endpoints

### Configuration des services

```json
{
  "Api": {
    "BaseUrl": "https://localhost:7144/api"
  },
  "SignalR": {
    "HubUrl": "https://localhost:7144/notificationhub"
  }
}
```

### ?? Authentification

| Méthode | Endpoint | Description | Auth |
|---------|----------|-------------|------|
| `POST` | `/Auth/login` | Connexion utilisateur | ? |
| `POST` | `/Auth/register` | Inscription utilisateur | ? |
| `GET` | `/Auth/test-auth` | Validation token | ?? |
| `GET` | `/Auth/test-admin` | Test rôle admin | ?? |

**Format de réponse login :**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "def50200c1a9...",
  "userId": "12345"
}
```

### ?? Gestion des zones

| Méthode | Endpoint | Description | Auth |
|---------|----------|-------------|------|
| `GET` | `/Zones` | Liste des zones | ?? |
| `GET` | `/Zones/{id}` | Détails d'une zone | ?? |
| `POST` | `/Zones` | Créer une zone | ?? |
| `PUT` | `/Zones/{id}` | Modifier une zone | ?? |
| `DELETE` | `/Zones/{id}` | Supprimer une zone | ?? |

### ?? Gestion des appareils

| Méthode | Endpoint | Description | Auth |
|---------|----------|-------------|------|
| `GET` | `/Devices` | Liste des appareils | ?? |
| `GET` | `/Devices/{id}` | Détails d'un appareil | ?? |
| `GET` | `/zones/{zoneId}/devices` | Appareils d'une zone | ?? |
| `POST` | `/Devices` | Créer un appareil | ?? |
| `PUT` | `/Devices/{id}` | Modifier un appareil | ?? |
| `DELETE` | `/Devices/{id}` | Supprimer un appareil | ?? |

### ?? IoT Hub

| Méthode | Endpoint | Description | Auth |
|---------|----------|-------------|------|
| `POST` | `/IoTHub/device/{deviceId}/method/{methodName}` | Invoquer méthode | ?? |
| `POST` | `/IoTHub/device/{deviceId}/smart-commands/{command}` | Commande intelligente | ?? |
| `GET` | `/IoTHub/device/{deviceId}/twin` | Récupérer device twin | ?? |
| `PUT` | `/IoTHub/device/{deviceId}/twin/desired` | Mettre à jour propriétés | ?? |
| `POST` | `/IoTHub/device/{deviceId}/message` | Envoyer message C2D | ?? |

**Commandes intelligentes disponibles :**
- `turn-on` - Allumer l'appareil
- `turn-off` - Éteindre l'appareil  
- `set-brightness` - Régler luminosité
- `set-temperature` - Régler température
- `get-status` - Obtenir statut
- `reset` - Réinitialiser

### ?? Gestion des utilisateurs

| Méthode | Endpoint | Description | Auth |
|---------|----------|-------------|------|
| `GET` | `/User/profile` | Profil utilisateur | ?? |
| `PUT` | `/User/profile` | Modifier profil | ?? |
| `POST` | `/User/admin/users/{id}/roles` | Attribuer rôles | ?? |
| `DELETE` | `/User/admin/users/{id}/roles` | Supprimer rôles | ?? |

### ?? Droits d'accès

| Méthode | Endpoint | Description | Auth |
|---------|----------|-------------|------|
| `GET` | `/admin/access-rights` | Liste droits (admin) | ?? |
| `POST` | `/admin/access-rights` | Créer droit d'accès | ?? |
| `GET` | `/user/access-rights/zones` | Zones accessibles | ?? |
| `GET` | `/user/access-rights/peripheriques` | Appareils accessibles | ?? |

---

## ?? SignalR - Temps réel

### Hub de notifications
- **URL** : `https://localhost:7144/notificationhub`
- **Authentification** : Token JWT via query parameter `access_token`

### Événements écoutés

| Événement | Description | Données |
|-----------|-------------|---------|
| `TelemetryUpdate` | Nouvelle donnée de télémétrie | `{ deviceId, value, unit, timestamp }` |
| `DeviceStatusUpdate` | Changement de statut d'appareil | `{ deviceId, status, timestamp }` |
| `ReceiveNotification` | Notification générale | `{ message, type, timestamp }` |
| `IncidentAlert` | Alerte d'incident | `{ deviceId, alertType, message, severity }` |
| `CommandExecutionResult` | Résultat d'exécution de commande | `{ commandId, success, result }` |

### Configuration du service

```csharp
// Connexion automatique avec reconnexion
await _connection.StartAsync();

// Écoute des événements
_connection.On<TelemetryData>("TelemetryUpdate", OnTelemetryReceived);
_connection.On<DeviceStatusUpdate>("DeviceStatusUpdate", OnDeviceStatusChanged);
```

---

## ?? Interface utilisateur

### Design System - MudBlazor

**Thème personnalisé :**
- **Palette principale** : Violet (#7A40F2)
- **Mode sombre** : Activé par défaut
- **Typography** : Roboto, system fonts
- **Composants** : Cards, DataGrids, Forms, Navigation

### Pages principales

#### ?? **Page d'accueil** (`/`)
- Présentation de l'application
- Boutons d'accès : Dashboard / Se connecter
- Accessible sans authentification

#### ?? **Login** (`/login`)
- Formulaire de connexion (email/password)
- Validation côté client
- Gestion des erreurs d'authentification
- Redirection automatique après connexion

#### ?? **Dashboard** (`/dashboard`)
- **Authentification requise**
- Accueil personnalisé avec nom d'utilisateur
- Météo et informations environnementales
- Vue d'ensemble des appareils par zone
- Contrôles rapides (interrupteurs, température)
- Graphique de consommation électrique
- Section membres avec avatars

#### ?? **Gestion des appareils** (`/devices`) - **?? Admin**
- Table complète des appareils IoT
- Fonctions CRUD (Créer, Lire, Modifier, Supprimer)
- Filtrage et recherche
- Modal d'édition avec sélection d'icônes et couleurs
- Affectation aux zones

#### ?? **Gestion des zones** (`/zones`) - **?? Admin**
- Vue par zones de la maison
- Ajout/modification/suppression de zones
- Visualisation des appareils par zone

#### ?? **Gestion des utilisateurs** (`/utilisateurs`) - **?? Admin**
- Liste des utilisateurs avec expansion panels
- Gestion des rôles (Admin, Membre, Visiteur)
- Activation/désactivation des comptes
- Dialog de création/modification

#### ?? **Analytics** (`/analytics`)
- **Authentification requise**
- Statistiques de consommation
- Graphiques et tendances
- Rapports d'utilisation

#### ?? **Profil** (`/profile`)
- **Authentification requise**
- Informations personnelles
- Modification du profil
- Préférences utilisateur

### Navigation adaptative

**Menu de navigation** (sidebar) :
- **Utilisateur connecté** : Toutes les pages autorisées
- **Administrateur** : Accès complet + pages d'administration
- **Visiteur** : Seulement Home et Login
- Indicateur visuel de la page active
- Toggle mode sombre/clair
- Bouton de déconnexion

---

## ?? Installation et configuration

### Prérequis

- **.NET 9 SDK** ou supérieur
- **Visual Studio 2022** ou **Visual Studio Code**
- **API Backend** configurée sur `https://localhost:7144`

### Installation

1. **Cloner le repository**
```bash
git clone https://github.com/Koj-Kyo/front-smart-office.git
cd MySmartApp/MySmartApp.Ui
```

2. **Restaurer les packages**
```bash
dotnet restore
```

3. **Configurer l'API** (optionnel)
   
   Modifier `wwwroot/appsettings.json` :
```json
{
  "Api": {
    "BaseUrl": "https://votre-api.com/api"
  },
  "SignalR": {
    "HubUrl": "https://votre-api.com/notificationhub"
  }
}
```

4. **Lancer l'application**
```bash
dotnet run
```

L'application sera disponible sur `https://localhost:5001`

---

## ?? Gestion des rôles

### Rôles disponibles

| Rôle | Description | Permissions |
|------|-------------|-------------|
| **Admin** | Administrateur système | Accès complet à toutes les fonctionnalités |
| **Membre** | Utilisateur standard | Dashboard, Analytics, Profil, Contrôle appareils |
| **Visiteur** | Accès restreint | Dashboard en lecture seule |

### Matrice des permissions

| Fonctionnalité | Admin | Membre | Visiteur |
|----------------|-------|---------|----------|
| Dashboard | ? | ? | ?? |
| Contrôle appareils | ? | ? | ? |
| Analytics | ? | ? | ?? |
| Profil utilisateur | ? | ? | ? |
| Gestion appareils | ? | ? | ? |
| Gestion zones | ? | ? | ? |
| Gestion utilisateurs | ? | ? | ? |
| Configuration système | ? | ? | ? |

---

## ?? Gestion des erreurs

### Codes de réponse API

| Code | Type | Description | Action frontend |
|------|------|-------------|-----------------|
| **200** | Succès | Opération réussie | Afficher les données |
| **201** | Créé | Ressource créée | Confirmer création |
| **204** | No Content | Modification/suppression réussie | Actualiser la vue |
| **400** | Bad Request | Erreurs de validation | Afficher erreurs par champ |
| **401** | Unauthorized | Token invalide/expiré | Rediriger vers login |
| **403** | Forbidden | Permissions insuffisantes | Afficher message d'erreur |
| **404** | Not Found | Ressource introuvable | Message "non trouvé" |
| **429** | Rate Limited | Trop de requêtes | Ralentir les requêtes |
| **500** | Server Error | Erreur serveur | Message d'erreur générique |

### Gestion des erreurs d'authentification

```csharp
// Interception automatique via AuthMessageHandler
if (response.StatusCode == HttpStatusCode.Unauthorized)
{
    // Redirection automatique vers login
    await _authStateProvider.MarkUserAsLoggedOut();
    _navigationManager.NavigateTo("/login", forceLoad: true);
}
```

### Rate Limiting

- **Limite globale** : ~100 requêtes par 10 secondes
- **Recommandation** : Privilégier SignalR pour les mises à jour temps réel
- **Stratégie** : Éviter le polling agressif

---

## ?? Monitoring et télémétrie

### Données de télémétrie reçues

**Via SignalR :**
- Température en temps réel
- État des appareils (ON/OFF)
- Consommation électrique
- Alertes et incidents
- Résultats de commandes

**Format des données :**
```json
{
  "deviceId": "sensor_salon_01",
  "value": 22.5,
  "unit": "°C",
  "timestamp": "2024-01-15T14:30:00Z"
}
```

### Graphiques et visualisations

- **Graphique de consommation** : Chart.js via MudBlazor
- **Jauges de température** : Composants personnalisés
- **Indicateurs d'état** : Temps réel via SignalR
- **Historiques** : API `/Telemetrie/historical`

---

## ?? Développement et maintenance

### Structure des services

**Pattern Repository/Service :**
- `IAuthService` ? Authentification et autorisation
- `IDeviceService` ? CRUD appareils via API
- `IZoneService` ? CRUD zones via API  
- `IUserService` ? Gestion profils utilisateurs
- `ISignalRService` ? Communication temps réel
- `ITokenProvider` ? Gestion sécurisée des tokens JWT
- `ILocalStorageService` ? Stockage local (localStorage)

### Injection de dépendances

```csharp
// Program.cs - Configuration des services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();
```

### Tests et debugging

**Outils de développement :**
- Hot Reload activé
- Browser Developer Tools
- Network tab pour debug API
- Console pour erreurs JavaScript/Blazor

**Logs disponibles :**
- Authentification (login/logout)
- Erreurs API (400, 401, 500)
- Événements SignalR
- Navigation et routing

---

## ?? TODO / Améliorations futures

### Fonctionnalités en cours de développement

- [ ] **Refresh Token** automatique
- [ ] **Notifications push** navigateur  
- [ ] **Mode hors ligne** avec synchronisation
- [ ] **Thèmes** personnalisables par utilisateur
- [ ] **Tableau de bord** personnalisable (widgets drag & drop)
- [ ] **Historique des commandes** et audit trail
- [ ] **Géolocalisation** et géofencing
- [ ] **API de règles** et automatisations
- [ ] **Sauvegarde/restauration** de configuration

### Améliorations techniques

- [ ] **Lazy loading** des modules
- [ ] **Service Worker** pour le cache
- [ ] **Compression Brotli** pour les assets
- [ ] **Tests unitaires** Blazor
- [ ] **Tests d'intégration** API
- [ ] **CI/CD Pipeline** automatisé
- [ ] **Monitoring** et métriques (Application Insights)
- [ ] **Documentation API** Swagger/OpenAPI

---

## ?? Résolution de problèmes

### Problèmes courants

**? Erreur "401 Unauthorized"**
- Vérifier que l'API est démarrée
- Contrôler la validité du token JWT
- Vérifier l'URL de base de l'API

**? SignalR ne se connecte pas**
- Vérifier l'URL du hub SignalR
- Contrôler que le token est passé correctement
- Vérifier les CORS sur l'API

**? Pages d'admin non accessibles**
- Vérifier que l'utilisateur a le rôle "Admin"
- Contrôler les claims dans le JWT token
- Vérifier l'attribut `[Authorize(Roles = "Admin")]`

**? Erreurs de build**
- `dotnet clean` puis `dotnet restore`
- Vérifier les versions des packages NuGet
- Contrôler la syntaxe Razor

### Debugging avancé

**Network inspection :**
```bash
# Chrome DevTools -> Network
# Filtrer par "Fetch/XHR"
# Vérifier les headers Authorization
```

**Blazor debugging :**
```bash
# F12 -> Console
# Rechercher les erreurs Blazor
# Utiliser les breakpoints dans le code C#
```

---

## ?? Contact et support

- **Repository** : https://github.com/Koj-Kyo/front-smart-office
- **Branch actuelle** : `ui-wasm-only`
- **Documentation API** : Voir les contrôleurs du backend
- **Issues** : Utiliser le système GitHub Issues

---

## ?? Licence

Ce projet est développé dans le cadre d'un mémoire de recherche.

---

*Dernière mise à jour : Janvier 2024*
*Version de l'application : 1.0.0*
*Framework : .NET 9 / Blazor WebAssembly*