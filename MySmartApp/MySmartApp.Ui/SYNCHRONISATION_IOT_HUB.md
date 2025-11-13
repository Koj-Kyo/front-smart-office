# Synchronisation des Devices depuis Azure IoT Hub

## ?? Vue d'ensemble

Cette fonctionnalité permet de synchroniser automatiquement les périphériques IoT depuis Azure IoT Hub vers la base de données locale de l'application.

## ?? Fonctionnalités ajoutées

### 1. Nouveau DTO `IoTHubSyncResultDto`

Ajouté dans `MySmartApp.Ui\Models\ApiModels.cs`:

```csharp
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
```

### 2. Nouvelle méthode dans `IIoTHubService`

```csharp
Task<ApiResponse<IoTHubSyncResultDto>> SyncDevicesFromIoTHubAsync();
```

### 3. Implémentation dans `IoTHubService`

La méthode `SyncDevicesFromIoTHubAsync()`:
- Appelle l'endpoint `/api/IoTHub/devices/sync` (POST)
- Affiche une notification de progression
- Affiche un résumé de la synchronisation
- Invalide le cache des statuts de connexion
- Retourne les statistiques de synchronisation

### 4. Interface utilisateur

Sur la page **Devices** (`MySmartApp.Ui\Pages\Devices.razor`):

#### Nouveau bouton "Synchroniser depuis IoT Hub"
- Couleur: Secondary (violet)
- Icône: `CloudSync`
- Position: À côté du bouton "Ajouter un périphérique IoT"
- États:
  - Normal: "Synchroniser depuis IoT Hub"
  - En cours: Spinner + "Synchronisation..."
  - Désactivé pendant le chargement ou la synchronisation

## ?? Flux de synchronisation

```
????????????????????
?  Utilisateur     ?
?  clique sur      ?
?  "Synchroniser"  ?
????????????????????
         ?
         v
????????????????????????????????
?  isSyncing = true            ?
?  Notification: Info          ?
?  "Synchronisation en cours..." ?
????????????????????????????????
         ?
         v
????????????????????????????????
?  POST /api/IoTHub/           ?
?       devices/sync           ?
????????????????????????????????
         ?
         v
????????????????????????????????
?  Backend récupère tous       ?
?  les devices depuis IoT Hub  ?
?  et les synchronise avec     ?
?  la base de données          ?
????????????????????????????????
         ?
         v
????????????????????????????????
?  Retour des statistiques:    ?
?  - TotalDevices              ?
?  - CreatedCount              ?
?  - UpdatedCount              ?
?  - FailedCount               ?
????????????????????????????????
         ?
         v
????????????????????????????????
?  Notification de succès      ?
?  avec détails ou Warning     ?
?  si échecs                   ?
????????????????????????????????
         ?
         v
????????????????????????????????
?  Cache invalidé              ?
?  LoadData() appelé           ?
?  isSyncing = false           ?
????????????????????????????????
```

## ?? Statistiques retournées

Après la synchronisation, l'utilisateur reçoit un résumé:

```
"Synchronisation terminée: 
 X créé(s), 
 Y mis à jour, 
 Z échec(s)"
```

### Types de messages:
- ? **Success**: Si aucun échec (`FailedCount == 0`)
- ?? **Warning**: Si au moins un échec (`FailedCount > 0`)
- ? **Error**: Si la requête échoue complètement

## ?? Sécurité

- L'endpoint nécessite une authentification (Bearer Token)
- Seuls les administrateurs peuvent synchroniser les devices
- Le backend valide les permissions avant d'autoriser la synchronisation

## ?? Cas d'usage

### 1. Premier déploiement
Après avoir configuré Azure IoT Hub avec plusieurs devices, utilisez la synchronisation pour importer automatiquement tous les devices dans l'application.

### 2. Ajout de nouveaux devices dans IoT Hub
Lorsque de nouveaux devices sont ajoutés directement dans Azure IoT Hub (via portail Azure, CLI, ou autre), utilisez la synchronisation pour les importer dans l'application.

### 3. Mise à jour des devices existants
Si les propriétés des devices ont été modifiées dans IoT Hub, la synchronisation met à jour les informations locales.

### 4. Réconciliation après incident
En cas de désynchronisation entre IoT Hub et la base de données locale, cette fonctionnalité permet de remettre les données en cohérence.

## ?? Configuration backend requise

L'endpoint backend doit implémenter:

```http
POST /api/IoTHub/devices/sync
Authorization: Bearer {token}
Content-Type: application/json
```

### Réponse attendue:

```json
{
  "success": true,
  "message": "Synchronisation terminée avec succès",
  "totalDevices": 10,
  "createdCount": 3,
  "updatedCount": 5,
  "failedCount": 2,
  "failedDeviceIds": ["device-001", "device-002"],
  "timestamp": "2024-01-15T10:30:00Z"
}
```

## ?? Améliorations futures possibles

1. **Synchronisation automatique**: 
   - Timer pour synchroniser périodiquement
   - Webhook depuis IoT Hub pour synchronisation en temps réel

2. **Synchronisation sélective**:
   - Permettre de choisir quels devices synchroniser
   - Filtrer par type, zone, ou état de connexion

3. **Historique de synchronisation**:
   - Conserver un log des synchronisations
   - Afficher les changements détectés

4. **Résolution de conflits**:
   - Gérer les conflits entre données locales et IoT Hub
   - Permettre à l'utilisateur de choisir quelle version conserver

5. **Synchronisation bidirectionnelle**:
   - Pousser les modifications locales vers IoT Hub
   - Synchronisation complète dans les deux sens

## ?? Notes importantes

- ?? La synchronisation peut prendre du temps si beaucoup de devices sont présents dans IoT Hub
- ?? L'opération invalide le cache des statuts de connexion
- ?? Un rechargement complet des données est effectué après synchronisation
- ?? Les échecs de synchronisation n'empêchent pas les succès pour les autres devices
- ?? Les devices échoués sont listés dans `FailedDeviceIds` pour investigation

## ?? Tests suggérés

1. **Test de synchronisation vide**: 
   - IoT Hub sans devices ? Résultat: 0 créé, 0 mis à jour

2. **Test de création**: 
   - Devices dans IoT Hub mais pas en BDD ? Tous créés

3. **Test de mise à jour**: 
   - Devices existants avec modifications ? Tous mis à jour

4. **Test de gestion d'erreur**: 
   - Device mal formé dans IoT Hub ? Échec comptabilisé

5. **Test de performance**: 
   - 100+ devices ? Vérifier le temps de réponse

6. **Test de permissions**: 
   - Utilisateur non-admin ? Erreur 403 Forbidden

## ?? Fichiers modifiés

- ? `MySmartApp.Ui\Models\ApiModels.cs` - Ajout de `IoTHubSyncResultDto`
- ? `MySmartApp.Ui\Services\IoTHubService.cs` - Ajout de la méthode de synchronisation
- ? `MySmartApp.Ui\Pages\Devices.razor` - Ajout du bouton et de la méthode UI
- ? `MySmartApp.Ui\SYNCHRONISATION_IOT_HUB.md` - Documentation (ce fichier)
