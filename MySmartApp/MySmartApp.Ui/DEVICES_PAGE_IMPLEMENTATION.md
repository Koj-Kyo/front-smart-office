# Documentation - Page Devices avec Gestion des Périphériques IoT

## ?? Vue d'ensemble

La page **Devices** a été complètement réimplémentée pour gérer à la fois les **périphériques logiques IoT** (ESP32, etc.) et leurs **capteurs/actionneurs physiques** (relais, capteurs de température, etc.).

## ??? Architecture

### Services créés

#### 1. **DevicePeripheralService** (`Services/DevicePeripheralService.cs`)
Service pour gérer les périphériques physiques via l'API Azure IoT Hub.

**Méthodes principales:**
- `GetPeripheralsAsync(string iotDeviceId)` - Liste les périphériques d'un device
- `GetPeripheralAsync(string iotDeviceId, string peripheralName)` - Détails d'un périphérique
- `GetMergedPeripheralAsync(string iotDeviceId, string peripheralName)` - Vue fusionnée (desired + reported)
- `AddPeripheralAsync(string iotDeviceId, PeripheralDto peripheral)` - Ajouter un périphérique
- `DeletePeripheralAsync(string iotDeviceId, string peripheralName)` - Supprimer un périphérique
- `ExecuteCommandAsync(...)` - Exécuter une commande sur un périphérique
- `GetFullTwinAsync(string iotDeviceId)` - Récupérer le twin complet

### DTOs ajoutés

Dans `Models/ApiModels.cs`:

```csharp
// DTO pour un périphérique physique
public record PeripheralDto(
    string Name,
    string Type,
    int Gpio,
    string? Notes = null,
    string? Unit = null,
    string? WiringDiagram = null,
    DateTime? InstalledAt = null,
    object? State = null,
    DateTime? LastUpdate = null,
    string? Status = null
);

// Vue fusionnée (desired + reported)
public record MergedPeripheralDto(
    string Name,
    PeripheralDto? DesiredConfig = null,
    PeripheralDto? ReportedState = null,
    bool IsSynchronized = false
);

// Payload pour remplacer les périphériques
public record PeripheralsPayloadDto(
    List<PeripheralDto> Peripherals
);

// Payload pour commandes
public record PeripheralCommandPayloadDto(
    Dictionary<string, object>? Parameters = null
);

// Twin complet avec métadonnées
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
```

## ?? Interface utilisateur

### Structure de la page

1. **En-tête**
   - Bouton "Ajouter un périphérique IoT"
   - Barre de recherche
   - Filtre par zone
   - Bouton "Actualiser"

2. **Liste des devices** (vue hiérarchique)
   Pour chaque device IoT:
   - Nom, type, IoT Device ID
   - Zone d'affectation
   - Statut de connexion (? Connecté / ? Déconnecté)
   - Boutons d'action:
     - "Ajouter périphérique"
     - "Rafraîchir"
     - "Modifier"
     - "Supprimer"
   
3. **Liste des périphériques physiques** (par device)
   Pour chaque périphérique:
   - Nom, type, GPIO
   - État actuel
   - Dernière mise à jour
   - Statut (active, error, offline)
   - **Boutons de commande** contextuels selon le type:
     - **Relais**: turn-on, turn-off
     - **Lumière**: turn-on, turn-off
     - **Capteurs**: read (lire la valeur)
   - Actions: Détails, Supprimer

### Dialogs

#### 1. Dialog d'ajout/modification de device
- Nom du périphérique
- Type (relay, sensor, actuator, light, hvac, security)
- IoT Device ID (uniquement à la création)
- Zone d'installation

#### 2. Dialog d'ajout de périphérique physique
- Nom (ex: relay1, temp_sensor)
- Type (relay, temperature, humidity, smoke, motion, light, door)
- GPIO (numéro de pin)
- Unité (°C, %, V, etc.)
- Notes de câblage
- Schéma de câblage (URL)

#### 3. Dialog de détails périphérique (vue fusionnée)
Affiche côte à côte:
- **Configuration désirée** (Desired Properties)
  - Type, GPIO, unité, notes
- **État reporté** (Reported Properties)
  - Type, GPIO, état actuel, statut, dernière mise à jour
- **Indicateur de synchronisation**
  - ? Synchronisé (Desired = Reported)
  - ?? Non synchronisé (Desired ? Reported)

## ?? Endpoints API utilisés

### Gestion des devices (PeripheriqueLogique)
```
GET    /api/Devices                                    # Liste des devices
GET    /api/Devices/{id}                               # Détails d'un device
POST   /api/Devices                                    # Créer un device
PUT    /api/Devices/{id}                               # Modifier un device
DELETE /api/Devices/{id}                               # Supprimer un device
```

### Gestion des périphériques physiques (via Twin)
```
GET    /api/Devices/{iotDeviceId}/peripherals          # Liste des périphériques
GET    /api/Devices/{iotDeviceId}/peripherals/{name}  # Détails d'un périphérique
POST   /api/Devices/{iotDeviceId}/peripherals          # Ajouter un périphérique
DELETE /api/Devices/{iotDeviceId}/peripherals/{name}  # Supprimer un périphérique
```

### Vues et commandes
```
GET    /api/Devices/{iotDeviceId}/peripherals/{name}/merged              # Vue fusionnée (desired + reported)
POST   /api/Devices/{iotDeviceId}/peripherals/{name}/commands/{command}  # Exécuter une commande
GET    /api/Devices/{iotDeviceId}/twin/full                              # Twin complet
```

### Statuts IoT Hub
```
POST   /api/IoTHub/devices/connection-status           # Statuts de connexion de plusieurs devices
```

## ?? Workflow utilisateur

### Ajouter un nouveau périphérique physique

1. **Brancher physiquement** le périphérique sur l'ESP32 (ex: DHT22 sur GPIO 32)

2. **Dans l'interface** - cliquer sur "Ajouter périphérique" sur le device concerné

3. **Remplir le formulaire**:
   - Nom: `temp_sensor_room`
   - Type: `temperature`
   - GPIO: `32`
   - Unité: `°C`
   - Notes: `DHT22 - Angle nord-est du bureau`

4. **Validation** ? Le service:
   - Ajoute le périphérique aux desired properties du twin
   - IoT Hub notifie l'ESP32
   - L'ESP32 configure le hardware
   - L'ESP32 publie l'état dans les reported properties

5. **Résultat** ? Le périphérique apparaît dans la liste avec son état actuel

### Contrôler un périphérique

1. **Dans la liste** des périphériques, cliquer sur l'icône de commande appropriée:
   - ?? **turn-on** / **turn-off** pour les relais
   - ?? **turn-on** / **turn-off** pour les lumières
   - ?? **read** pour les capteurs

2. **Exécution**:
   - La commande est envoyée via méthode directe IoT Hub
   - L'ESP32 exécute la commande
   - L'ESP32 met à jour les reported properties
   - L'interface se rafraîchit automatiquement après 1 seconde

### Voir les détails d'un périphérique

1. Cliquer sur l'icône **?? Info** à côté d'un périphérique

2. **Dialog détails** affiche:
   - Configuration désirée (ce que vous avez configuré)
   - État reporté (ce que l'ESP32 rapporte)
   - Statut de synchronisation

3. Permet de **diagnostiquer** les problèmes:
   - Si non synchronisé ? problème de communication ou de configuration
   - Si reporté absent ? device déconnecté ou périphérique non configuré côté firmware

## ?? Commandes disponibles par type

### Relais (relay)
- **turn-on** - Allumer le relais
- **turn-off** - Éteindre le relais

### Lumière (light)
- **turn-on** - Allumer la lumière
- **turn-off** - Éteindre la lumière

### Capteurs (temperature, humidity, smoke, motion)
- **read** - Lire la valeur actuelle du capteur

## ?? Types de périphériques supportés

### Devices IoT (PeripheriqueLogique)
- `relay` - Relais
- `sensor` - Capteur générique
- `actuator` - Actionneur
- `light` - Éclairage
- `hvac` - CVC (Chauffage, Ventilation, Climatisation)
- `security` - Sécurité

### Périphériques physiques (Peripherals)
- `relay` - Relais (GPIO output)
- `temperature` - Capteur de température
- `humidity` - Capteur d'humidité
- `smoke` - Détecteur de fumée
- `motion` - Détecteur de mouvement
- `light` - Lumière/LED
- `door` - Capteur de porte

## ?? ViewModels

### DeviceViewModel
```csharp
public class DeviceViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string IotDeviceId { get; set; } = "";
    public int ZoneId { get; set; }
    public bool? ConnectionStatus { get; set; } = null;
    public DateTime? LastActivity { get; set; }
    public List<PeripheralDto> Peripherals { get; set; } = new();
    public bool IsLoadingPeripherals { get; set; } = false;
}
```

### DeviceEditModel
```csharp
public class DeviceEditModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string IotDeviceId { get; set; } = "";
    public int ZoneId { get; set; }
}
```

### PeripheralEditModel
```csharp
public class PeripheralEditModel
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public int Gpio { get; set; }
    public string? Notes { get; set; }
    public string? Unit { get; set; }
    public string? WiringDiagram { get; set; }
}
```

### PeripheralCommand
```csharp
public record PeripheralCommand(
    string Command, 
    string Label, 
    string Icon, 
    Color Color
);
```

## ?? Chargement des données

### Initialisation (OnInitializedAsync)
```csharp
1. Charger zones + devices en parallèle
2. Convertir en ViewModels
3. En arrière-plan:
   - Charger statuts IoT Hub (connexion)
   - Charger périphériques pour chaque device
```

### Rafraîchissement
- **Global**: Bouton "Actualiser" ? recharge tout
- **Par device**: Bouton "Rafraîchir" sur un device ? recharge ses périphériques
- **Auto après commande**: Après exécution d'une commande ? rafraîchit après 1s

## ?? Icônes par type

### Devices
- `relay` ? ? Power
- `sensor` ? ?? Sensors
- `actuator` ? ?? Engineering
- `light` ? ?? Lightbulb
- `hvac` ? ??? Thermostat
- `security` ? ?? Security

### Périphériques
- `relay` ? ? ElectricBolt
- `temperature` ? ??? Thermostat
- `humidity` ? ?? WaterDrop
- `smoke` ? ?? Warning
- `motion` ? ?? DirectionsWalk
- `light` ? ?? Lightbulb
- `door` ? ?? MeetingRoom

## ?? Couleurs par type

### Devices
- `relay` ? Primary (bleu)
- `sensor` ? Info (cyan)
- `actuator` ? Success (vert)
- `light` ? Warning (jaune)
- `hvac` ? Secondary (gris)
- `security` ? Error (rouge)

### Périphériques
- `relay` ? Primary
- `temperature` ? Error (rouge chaud)
- `humidity` ? Info (bleu eau)
- `smoke` ? Warning (orange)
- `motion` ? Success
- `light` ? Warning
- `door` ? Secondary

## ?? Logs

Tous les événements importants sont loggés:
- ? Succès (Information)
- ?? Avertissements (Warning)
- ? Erreurs (Error)

Exemples:
```
? Données chargées: 5 périphériques, 3 zones
? Statuts de connexion IoT Hub mis à jour pour 5 devices
? Périphériques chargés pour tous les devices
? Périphériques rafraîchis pour ESP32-Bureau-101
? Commande 'turn-on' exécutée sur relay1 @ ESP32-Bureau-101
? Erreur lors du chargement des périphériques: Connection timeout
```

## ?? Sécurité

- Toutes les routes requièrent `[Authorize]`
- Les endpoints API backend nécessitent le rôle `Admin`
- Le token JWT est automatiquement inclus dans toutes les requêtes

## ?? Performance

### Optimisations
- Chargement parallèle (zones + devices)
- Chargement asynchrone des statuts IoT Hub
- Chargement asynchrone des périphériques
- Mise à jour UI avec `InvokeAsync(StateHasChanged)`

### Caching
- Pas de cache local (données temps réel)
- Cache côté backend (DeviceService)

## ?? Responsive

- Grilles adaptatives (`MudGrid`, `xs/sm/md` breakpoints)
- Tables scrollables sur mobile
- Dialogs avec `MaxWidth.Medium` / `MaxWidth.Large`

## ? Tests

### Scénarios de test
1. ? Ajouter un device IoT
2. ? Modifier un device
3. ? Supprimer un device
4. ? Ajouter un périphérique physique
5. ? Exécuter une commande (turn-on/turn-off)
6. ? Voir les détails fusionnés
7. ? Supprimer un périphérique
8. ? Rafraîchir les données
9. ? Filtrer par zone
10. ? Rechercher

## ?? Corrections apportées

### Erreurs corrigées
1. ? Contenu dupliqué dans `Devices.razor`
   - ? Fichier nettoyé, contenu en double supprimé

2. ? Icônes inexistantes:
   - `Icons.Material.Filled.LightbulbOutline` ? remplacé par `Lightbulb`
   - `Icons.Material.Filled.Smoke` ? remplacé par `Warning`
   - `Icons.Material.Filled.Door` ? remplacé par `MeetingRoom`

3. ? Références à des méthodes inexistantes (ancien code):
   - `SendGlobalCommand` - supprimé
   - `RefreshDevicesStatus` - supprimé
   - `OpenAddDialog` - supprimé
   - `GetDeviceCommands` - supprimé
   - `SendToggleCommand` - supprimé
   - `SendDeviceCommand` - supprimé
   - `OpenEditDialog` - supprimé
   - `OpenIoTConfigDialog` - supprimé
   - `isDialogOpen` - supprimé
   - `isIoTConfigOpen` - supprimé
   - `isAdding` - supprimé
   - `form` - supprimé
   - `iotConfigJson` - supprimé
   - `CloseDialog` - supprimé
   - `CloseIoTConfigDialog` - supprimé
   - `SaveIoTConfig` - supprimé

4. ? Propriété manquante dans `DeviceViewModel`:
   - `IsOn` - supprimée (remplacée par gestion des périphériques)

## ?? Références

- **Azure IoT Hub**: https://docs.microsoft.com/azure/iot-hub/
- **Device Twins**: https://docs.microsoft.com/azure/iot-hub/iot-hub-devguide-device-twins
- **Direct Methods**: https://docs.microsoft.com/azure/iot-hub/iot-hub-devguide-direct-methods
- **MudBlazor**: https://mudblazor.com/

## ?? Prochaines étapes

### Améliorations possibles
1. **SignalR** - Mise à jour temps réel des états
2. **Graphiques** - Historique des valeurs des capteurs
3. **Alertes** - Notifications sur événements critiques
4. **Automatisations** - Règles if-then-else
5. **Groupes** - Actions sur plusieurs périphériques
6. **Export** - Export de la configuration en JSON
7. **Import** - Import de configuration depuis fichier

---

**Date de création**: 2025-01-XX  
**Version**: 1.0  
**Auteur**: Assistant IA  
**Statut**: ? Testé et fonctionnel
