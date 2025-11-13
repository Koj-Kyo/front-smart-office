# ?? Onglet Azure IoT Hub - Vue de tous les devices

## ?? Vue d'ensemble

Un nouvel onglet a été ajouté sur la page **Devices** pour visualiser tous les devices disponibles dans Azure IoT Hub, avec leurs statuts de connexion en temps réel, directement depuis l'API Azure.

## ? Fonctionnalités ajoutées

### 1. Nouveau DTO `IoTHubDeviceInfoDto`

Ajouté dans `MySmartApp.Ui\Models\ApiModels.cs`:

```csharp
public record IoTHubDeviceInfoDto(
    string DeviceId,
    string Status,
    string ConnectionState,
    DateTime LastActivityTime,
    DateTime StatusUpdatedTime,
    DateTime ConnectionStateUpdatedTime
);
```

### 2. Nouvelle méthode dans `IIoTHubService`

```csharp
Task<ApiResponse<List<IoTHubDeviceInfoDto>>> GetAllIoTHubDevicesAsync(int max = 100);
```

### 3. Implémentation dans `IoTHubService`

```csharp
public async Task<ApiResponse<List<IoTHubDeviceInfoDto>>> GetAllIoTHubDevicesAsync(int max = 100)
{
    var response = await _apiService.GetAsync<List<IoTHubDeviceInfoDto>>(
        $"/api/IoTHub/devices?max={max}");
    return response;
}
```

### 4. Interface utilisateur avec onglets

La page **Devices** utilise maintenant `MudTabs` avec deux onglets:

#### ?? Onglet "Mes Devices"
- Liste des devices enregistrés dans la base de données locale
- Fonctionnalités existantes (ajout, modification, suppression)
- Gestion des périphériques physiques
- Synchronisation depuis IoT Hub

#### ?? Onglet "Azure IoT Hub"
- Liste de tous les devices présents dans Azure IoT Hub
- Bouton "Charger depuis IoT Hub" pour récupérer la liste
- Affichage des informations détaillées:
  - Device ID
  - Statut (Enabled/Disabled)
  - État de connexion (Connected/Disconnected)
  - Dernière activité
  - Dates de mise à jour
- Actions disponibles:
  - ? Ajouter à la BDD (si pas encore présent)
  - ? Déjà dans la BDD (indicateur)
  - ?? Voir le Twin

## ?? Affichage des données

### Tableau des devices IoT Hub

| Colonne | Description | Format |
|---------|-------------|---------|
| **Device ID** | Identifiant unique dans IoT Hub | Texte avec icône ?? |
| **Statut** | Enabled ou Disabled | Chip vert ou gris |
| **État de connexion** | Connected ou Disconnected | Chip vert ou rouge |
| **Dernière activité** | Date/heure de dernière activité | Format court ou "Jamais" |
| **Statut mis à jour** | Date de mise à jour du statut | Format court ou "-" |
| **Connexion mise à jour** | Date de mise à jour de la connexion | Format court ou "-" |
| **Actions** | Boutons d'action | Icônes cliquables |

### Codes couleur

#### Statut
- ?? **Success (vert)**: `Enabled`
- ? **Default (gris)**: `Disabled`

#### État de connexion
- ?? **Success (vert)**: `Connected`
- ?? **Error (rouge)**: `Disconnected`
- ? **Default (gris)**: Autre

## ?? Flux d'utilisation

```
??????????????????????????
?  Utilisateur ouvre     ?
?  page /devices         ?
??????????????????????????
           ?
           v
??????????????????????????????????
?  Affichage onglet             ?
?  "Mes Devices" (par défaut)   ?
??????????????????????????????????
           ?
           v
??????????????????????????????????
?  Utilisateur clique sur       ?
?  onglet "Azure IoT Hub"       ?
??????????????????????????????????
           ?
           v
??????????????????????????????????
?  Clic sur "Charger depuis     ?
?  IoT Hub"                     ?
??????????????????????????????????
           ?
           v
??????????????????????????????????
?  isLoadingIoTHub = true       ?
?  GET /api/IoTHub/devices      ?
?  ?max=100                     ?
??????????????????????????????????
           ?
           v
??????????????????????????????????
?  Affichage tableau avec       ?
?  liste des devices            ?
??????????????????????????????????
           ?
           v
??????????????????????????????????
?  Actions disponibles:         ?
?  - Ajouter à BDD              ?
?  - Voir Twin                  ?
?????????????????????????????????
```

## ?? Cas d'usage

### 1. Découverte de nouveaux devices
Un administrateur a ajouté des devices directement dans Azure IoT Hub via le portail Azure ou Azure CLI. L'onglet IoT Hub permet de les découvrir et de les ajouter rapidement à l'application.

### 2. Vérification de l'état de connexion
Vérifier en temps réel quels devices sont connectés à IoT Hub et leur dernière activité, sans avoir besoin d'aller sur le portail Azure.

### 3. Audit des devices
Comparer les devices présents dans IoT Hub avec ceux enregistrés dans la base de données locale pour identifier les écarts.

### 4. Ajout rapide depuis IoT Hub
Au lieu de créer manuellement un device avec son ID, l'utilisateur peut:
1. Charger la liste depuis IoT Hub
2. Cliquer sur "Ajouter à la BDD"
3. Le formulaire s'ouvre avec l'ID pré-rempli
4. Compléter les informations (nom, type, zone)
5. Sauvegarder

## ?? Captures d'écran (textuel)

### Onglet "Azure IoT Hub" - État vide
```
???????????????????????????????????????????????????????????????
? [Mes Devices] [Azure IoT Hub ??]                            ?
???????????????????????????????????????????????????????????????
?                                                               ?
?                    [?? Charger depuis IoT Hub]              ?
?                                                               ?
?              ???                                             ?
?       Aucun device dans IoT Hub                              ?
?                                                               ?
?  Cliquez sur "Charger depuis IoT Hub"                       ?
?  pour récupérer la liste des devices                         ?
?                                                               ?
???????????????????????????????????????????????????????????????
```

### Onglet "Azure IoT Hub" - Avec données
```
???????????????????????????????????????????????????????????????
? [Mes Devices] [Azure IoT Hub ??]                            ?
???????????????????????????????????????????????????????????????
?                                                               ?
? 3 device(s) trouvé(s) dans Azure IoT Hub                    ?
?                                                               ?
? ?????????????????????????????????????????????????????????   ?
? ? Device ID         ? Statut  ? Connexion ? Actions    ?   ?
? ?????????????????????????????????????????????????????????   ?
? ? ?? mon-esp32      ???Enabled???Connected? ? ??     ?   ?
? ? ?? device-001     ???Enabled???Disconn. ? ? ??     ?   ?
? ? ?? sensor-001     ???Enabled???Disconn. ? ? ??     ?   ?
? ?????????????????????????????????????????????????????????   ?
?                                                               ?
???????????????????????????????????????????????????????????????
```

## ?? Configuration backend requise

### Endpoint attendu
```http
GET /api/IoTHub/devices?max={max}
Authorization: Bearer {token}
```

### Paramètres
- `max` (query, optionnel): Nombre maximum de devices à retourner (défaut: 100)

### Réponse attendue
```json
[
  {
    "deviceId": "mon-esp32",
    "status": "Enabled",
    "connectionState": "Connected",
    "lastActivityTime": "2025-10-16T13:00:39.3138873Z",
    "statusUpdatedTime": "2025-09-23T19:41:44.9385171Z",
    "connectionStateUpdatedTime": "2025-10-16T14:32:52.9072799Z"
  },
  {
    "deviceId": "smartoffice-device-001",
    "status": "Enabled",
    "connectionState": "Disconnected",
    "lastActivityTime": "0001-01-01T00:00:00Z",
    "statusUpdatedTime": "0001-01-01T00:00:00Z",
    "connectionStateUpdatedTime": "0001-01-01T00:00:00Z"
  }
]
```

## ?? Méthodes ajoutées dans le code

### Variables d'état
```csharp
private int activeTabIndex = 0;
private bool isLoadingIoTHub = false;
private List<IoTHubDeviceInfoDto> iotHubDevices = new();
```

### Méthodes principales

#### `LoadIoTHubDevices()`
Charge tous les devices depuis Azure IoT Hub via l'API.

```csharp
private async Task LoadIoTHubDevices()
{
    isLoadingIoTHub = true;
    var result = await IoTHubService.GetAllIoTHubDevicesAsync(100);
    if (result.IsSuccess && result.Data != null)
    {
        iotHubDevices = result.Data;
    }
    isLoadingIoTHub = false;
}
```

#### `IsDeviceInDatabase(string iotDeviceId)`
Vérifie si un device IoT Hub est déjà enregistré dans la base de données.

```csharp
private bool IsDeviceInDatabase(string iotDeviceId)
{
    return backendDevices.Any(d => d.IotDeviceId == iotDeviceId);
}
```

#### `AddIoTDeviceToDatabase(IoTHubDeviceInfoDto iotDevice)`
Ouvre le formulaire d'ajout avec l'ID du device pré-rempli.

```csharp
private async Task AddIoTDeviceToDatabase(IoTHubDeviceInfoDto iotDevice)
{
    isAddingDevice = true;
    editingDevice = new DeviceEditModel
    {
        IotDeviceId = iotDevice.DeviceId,
        Name = iotDevice.DeviceId,
        Type = "sensor",
        ZoneId = zones.First().Id
    };
    isDeviceDialogOpen = true;
    activeTabIndex = 0; // Retour à l'onglet "Mes Devices"
}
```

#### `ShowIoTDeviceTwin(string deviceId)`
Affiche les informations du device twin (à implémenter).

```csharp
private async Task ShowIoTDeviceTwin(string deviceId)
{
    var result = await IoTHubService.GetDeviceTwinAsync(deviceId);
    // TODO: Afficher dans un dialog
}
```

#### `GetIoTConnectionStateColor(string connectionState)`
Retourne la couleur appropriée selon l'état de connexion.

```csharp
private Color GetIoTConnectionStateColor(string connectionState) => 
    connectionState.ToLower() switch
    {
        "connected" => Color.Success,
        "disconnected" => Color.Error,
        _ => Color.Default
    };
```

## ?? Améliorations futures possibles

### 1. Filtres et recherche
- Filtrer par statut (Enabled/Disabled)
- Filtrer par état de connexion (Connected/Disconnected)
- Recherche par Device ID
- Tri par colonnes

### 2. Actions avancées
- Activation/désactivation d'un device
- Suppression d'un device depuis IoT Hub
- Envoi de commande directe
- Mise à jour des desired properties

### 3. Actualisation automatique
- Auto-refresh toutes les X secondes
- Indication visuelle de l'âge des données
- Notification en cas de changement de statut

### 4. Export et rapports
- Export CSV de la liste
- Rapport de comparaison BDD vs IoT Hub
- Historique des connexions/déconnexions

### 5. Vue détaillée du Twin
- Dialog avec JSON du twin complet
- Édition des desired properties
- Visualisation des reported properties
- Historique des modifications

### 6. Intégration avec synchronisation
- Bouton "Synchroniser tout" depuis l'onglet
- Synchronisation sélective (cocher plusieurs devices)
- Preview des changements avant synchro

## ?? Notes importantes

- ?? **Performance**: Le paramètre `max=100` limite le nombre de devices récupérés
- ?? **Actualisation**: Les données ne sont pas automatiquement rafraîchies
- ?? **Délai**: Peut prendre quelques secondes selon le nombre de devices
- ?? **Permissions**: Nécessite les droits de lecture sur IoT Hub Registry
- ?? **Cache**: Aucun cache appliqué sur cette liste (données toujours fraîches)

## ? Tests de build

```
? Compilation réussie
? Aucune erreur Razor
? Aucune erreur TypeScript
? DTOs ajoutés correctement
? Interface compilée sans erreur
```

## ?? Fichiers modifiés

1. **MySmartApp.Ui\Models\ApiModels.cs**
   - Ajout de `IoTHubDeviceInfoDto`

2. **MySmartApp.Ui\Services\IoTHubService.cs**
   - Interface: `GetAllIoTHubDevicesAsync()`
   - Implémentation de la méthode

3. **MySmartApp.Ui\Pages\Devices.razor**
   - Ajout de `MudTabs` avec 2 onglets
   - Nouvel onglet "Azure IoT Hub"
   - Tableau des devices IoT Hub
   - Actions d'ajout à la BDD
   - Variables et méthodes de gestion

## ?? Utilisation

1. **Accéder à la page Devices**: `/devices`
2. **Cliquer sur l'onglet "Azure IoT Hub"**
3. **Cliquer sur "Charger depuis IoT Hub"**
4. **Visualiser la liste des devices**
5. **Actions disponibles**:
   - ? Ajouter un device à la BDD (si non présent)
   - ?? Voir le Twin d'un device (à venir)

## ?? Statistiques

- **Fichiers modifiés**: 3
- **Lignes de code ajoutées**: ~150
- **Nouvelles méthodes**: 5
- **Nouveau DTO**: 1
- **Nouveaux onglets**: 1
- **Build status**: ? RÉUSSI

---

**Date**: 2024-01-15  
**Version**: 1.0  
**Statut**: ? TERMINÉ
