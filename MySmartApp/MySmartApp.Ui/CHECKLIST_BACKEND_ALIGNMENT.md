# ? CHECKLIST BACKEND-FRONTEND ALIGNMENT

**Date de création:** 2025-01-30  
**Status:** ?? En cours - Phase 1  
**Prochaine révision:** Après chaque phase

---

## ?? PHASE 1 - CRITIQUE (Aujourd'hui)

### ?? 1. DeviceStatusService - PRIORITÉ ABSOLUE

- [ ] **Créer fichier** `Services/DeviceStatusService.cs`
- [ ] **Copier code** depuis `CORRECTIONS_PRIORITAIRES_BACKEND_ALIGNMENT.md` (ligne 85-600)
- [ ] **Ajouter DTOs** dans `Models/ApiModels.cs`:
  - [ ] `DeviceMetricsDto`
  - [ ] `RfidEventDto`
  - [ ] `SmokeAlarmEventDto`
  - [ ] `DeviceFullStatusDto`
  - [ ] `RecordMetricsRequest`
  - [ ] `RecordRfidEventRequest`
  - [ ] `RecordSmokeAlarmRequest`
  - [ ] `DeactivateSmokeAlarmRequest`
- [ ] **Enregistrer service** dans `Program.cs`:
  ```csharp
  builder.Services.AddScoped<IDeviceStatusService, DeviceStatusService>();
  ```
- [ ] **Compiler** et vérifier aucune erreur
- [ ] **Tester endpoint** `GET /api/DeviceStatus/{id}/full-status` avec Postman
- [ ] **Tester endpoint** `POST /api/DeviceStatus/{id}/metrics` avec Postman
- [ ] **Vérifier logs** dans console backend
- [ ] **Intégrer dans page** Devices.razor pour affichage métriques

**?? Durée estimée:** 30-45 minutes  
**?? Impact:** ????? CRITIQUE

---

### ? 2. Support `?fresh=true` - TelemetrieService

- [x] ? **Modifier interface** `ITelemetrieService`
- [x] ? **Ajouter paramètre `bool fresh`** dans méthodes:
  - [x] `GetTelemetriesByPeripheriqueAsync()`
  - [x] `GetHistoricalDataAsync()`
  - [x] `GetRecentTelemetriesAsync()`
- [x] ? **Ajouter query param** `?fresh=true` dans URLs
- [x] ? **Compiler** et vérifier

**?? Durée:** ? FAIT  
**?? Impact:** ????

---

### ? 3. Support `?fresh=true` - CommandeService

- [ ] **Modifier interface** `ICommandeService`
- [ ] **Ajouter méthode** `GetMyCommandsAsync(bool fresh, int limit, int offset)`
- [ ] **Ajouter méthode** `GetCommandsByPeripheriqueAsync(int id, bool fresh, int limit)`
- [ ] **Implémenter avec** query params conditionnels
- [ ] **Compiler** et vérifier
- [ ] **Tester** dans page Incidents.razor:
  ```csharp
  var commands = await CommandeService.GetMyCommandsAsync(fresh: true);
  ```

**?? Durée estimée:** 15 minutes  
**?? Impact:** ????

---

### ? 4. Correction URLs ConsommationStatsService

- [x] ? **Ajouter `/api/`** dans tous les endpoints:
  - [x] `/ConsommationStats/...` ? `/api/ConsommationStats/...`
- [x] ? **Compiler** et vérifier
- [x] ? **Tester** page Analytics.razor

**?? Durée:** ? FAIT  
**?? Impact:** ????

---

### ?? 5. Ajouter DTOs Manquants

- [ ] **Ouvrir** `Models/ApiModels.cs`
- [ ] **Ajouter section DeviceStatus** (ligne ~1445):
  - [ ] `DeviceMetricsDto`
  - [ ] `RfidEventDto`
  - [ ] `SmokeAlarmEventDto`
  - [ ] `DeviceFullStatusDto`
- [ ] **Ajouter section Pagination** (ligne ~1485):
  - [ ] `PaginatedResponse<T>`
- [ ] **Ajouter section UserActivity** (ligne ~1500):
  - [ ] `UserActivityDto`
  - [ ] `UserActivityStatsDto`
- [ ] **Ajouter section DeviceLogs** (ligne ~1520):
  - [ ] `DeviceLogDto`
- [ ] **Ajouter classe** `ApiResponse` sans Data (ligne ~1540)
- [ ] **Compiler** et vérifier aucune erreur

**?? Durée estimée:** 10 minutes  
**?? Impact:** ????? BLOQUANT

---

### ?? 6. Enregistrement Services Program.cs

- [ ] **Ouvrir** `Program.cs`
- [ ] **Localiser** section `// Services` (ligne ~40)
- [ ] **Ajouter**:
  ```csharp
  // === NOUVEAUX SERVICES CRITIQUES ===
  builder.Services.AddScoped<IDeviceStatusService, DeviceStatusService>();
  ```
- [ ] **Compiler** projet complet

**?? Durée estimée:** 2 minutes  
**?? Impact:** ????? BLOQUANT

---

### ?? 7. Tests Phase 1

#### Test 1: DeviceStatus Full Flow
- [ ] **Démarrer backend** API
- [ ] **Postman:** POST `/api/DeviceStatus/ESP32_TEST/metrics`
  ```json
  {
    "Rssi": -50,
    "HeapFree": 200000,
    "UptimeSeconds": 3600
  }
  ```
- [ ] **Vérifier** réponse `201 Created`
- [ ] **Postman:** GET `/api/DeviceStatus/ESP32_TEST/metrics/latest`
- [ ] **Vérifier** réponse contient données envoyées
- [ ] **Postman:** GET `/api/DeviceStatus/ESP32_TEST/full-status`
- [ ] **Vérifier** réponse complète

#### Test 2: Fresh vs Cache (Télémétrie)
- [ ] **Blazor UI:** Ouvrir page Telemetrie
- [ ] **DevTools Network:** Observer appel API
- [ ] **Vérifier** `Source: "cache"` dans réponse
- [ ] **Ajouter** paramètre `?fresh=true` manuellement dans URL
- [ ] **Vérifier** `Source: "database"` dans réponse
- [ ] **Mesurer** différence temps réponse

#### Test 3: Fresh dans Commandes
- [ ] **Page Incidents:** Ouvrir
- [ ] **Modifier code** pour appeler `GetMyCommandsAsync(fresh: true)`
- [ ] **Hot Reload** application
- [ ] **Vérifier** nouvelles commandes apparaissent immédiatement

#### Test 4: Build Final
- [ ] **Terminal:** `dotnet clean`
- [ ] **Terminal:** `dotnet build`
- [ ] **Vérifier** 0 erreurs, 0 warnings critiques

**?? Durée estimée:** 30 minutes  
**?? Impact:** ????? VALIDATION

---

## ?? PROGRESSION PHASE 1

| Tâche | Status | Durée | Assigné |
|-------|--------|-------|---------|
| 1. DeviceStatusService | ? TODO | 45min | - |
| 2. Fresh TelemetrieService | ? FAIT | - | - |
| 3. Fresh CommandeService | ? TODO | 15min | - |
| 4. URLs ConsommationStats | ? FAIT | - | - |
| 5. DTOs Manquants | ? TODO | 10min | - |
| 6. Program.cs Registration | ? TODO | 2min | - |
| 7. Tests | ? TODO | 30min | - |
| **TOTAL** | **2/7** | **1h42** | - |

**?? Objectif:** Terminer Phase 1 aujourd'hui

---

## ?? PHASE 2 - IMPORTANTE (Cette semaine)

### ?? 8. Compléter DroitAccesService

- [ ] **Ajouter interface** méthodes admin:
  ```csharp
  Task<ApiResponse<DroitAccesDto>> AssignZoneToUserAsync(string userId, int zoneId, DroitAccesType accessType);
  Task<ApiResponse<DroitAccesDto>> AssignDeviceToUserAsync(string userId, int deviceId, DroitAccesType accessType);
  Task<ApiResponse<List<DroitAccesDto>>> GetAllAccessRightsAsync();
  Task<ApiResponse> DeleteAccessRightAsync(int id);
  ```
- [ ] **Implémenter méthodes** (voir `CORRECTIONS_PRIORITAIRES_BACKEND_ALIGNMENT.md` ligne 850)
- [ ] **Tester** page Access.razor avec assignation zones
- [ ] **Vérifier** droits admin fonctionnent

**?? Durée estimée:** 1h  
**?? Impact:** ???

---

### ?? 9. Créer UserActivityService

- [ ] **Créer fichier** `Services/UserActivityService.cs`
- [ ] **Copier interface** depuis `CORRECTIONS_PRIORITAIRES_BACKEND_ALIGNMENT.md` ligne 950
- [ ] **Implémenter méthodes**:
  - [ ] `GetUserActivitiesAsync()` avec pagination
  - [ ] `GetUserActivityStatsAsync()`
  - [ ] `GetRecentActivityAsync()`
- [ ] **Enregistrer** dans Program.cs:
  ```csharp
  builder.Services.AddScoped<IUserActivityService, UserActivityService>();
  ```
- [ ] **Créer page** `Pages/UserActivity.razor` pour affichage
- [ ] **Tester** historique et stats

**?? Durée estimée:** 1h30  
**?? Impact:** ???

---

### ?? 10. Créer DeviceLogService

- [ ] **Créer fichier** `Services/DeviceLogService.cs`
- [ ] **Ajouter DTO** `DeviceLogDto` dans ApiModels.cs
- [ ] **Implémenter méthodes**:
  - [ ] `GetRecentLogsAsync(int limit)`
  - [ ] `GetLogsByDeviceIdAsync(string deviceId, int limit)`
  - [ ] `GetLogsByLevelAsync(string level, int limit)`
  - [ ] `GetLogsByDateRangeAsync()`
- [ ] **Enregistrer** dans Program.cs
- [ ] **Créer composant** `Components/DeviceLogsComponent.razor`
- [ ] **Intégrer** dans page Devices
- [ ] **Tester** filtres et pagination

**?? Durée estimée:** 1h  
**?? Impact:** ???

---

## ?? PROGRESSION PHASE 2

| Tâche | Status | Durée | Assigné |
|-------|--------|-------|---------|
| 8. DroitAccesService | ? TODO | 1h | - |
| 9. UserActivityService | ? TODO | 1h30 | - |
| 10. DeviceLogService | ? TODO | 1h | - |
| **TOTAL** | **0/3** | **3h30** | - |

**?? Objectif:** Terminer Phase 2 fin de semaine

---

## ?? PHASE 3 - AMÉLIORATIONS (Sprint suivant)

### ?? 11. UserManagementService Complet

- [ ] **Ajouter méthode** `SearchUsersAsync(string searchTerm)`
- [ ] **Ajouter méthode** `ToggleUserActiveStatusAsync(string userId)`
- [ ] **Ajouter méthode** `GetUserStatisticsAsync()`
- [ ] **Intégrer** dans page Users.razor
- [ ] **Tester** recherche et toggle

**?? Durée estimée:** 2h  
**?? Impact:** ??

---

### ?? 12. IoTHubService Extensions

- [ ] **Ajouter méthodes** smart commands:
  ```csharp
  Task<ApiResponse> ExecuteSmartCommandAsync(string deviceId, string command, SmartCommandPayloadDto? payload);
  ```
- [ ] **Ajouter méthodes** peripherals sync:
  ```csharp
  Task<ApiResponse<int>> SyncPeripheralsAsync(string iotDeviceId);
  Task<ApiResponse<int>> SyncAllPeripheralsAsync();
  ```
- [ ] **Ajouter méthodes** ESP32 firmware:
  - [ ] `OpenDoorAsync()`
  - [ ] `CloseDoorAsync()`
  - [ ] `RebootDeviceAsync()`
  - [ ] `AlarmOnAsync()` / `AlarmOffAsync()`
- [ ] **Créer composant** `Components/IoTHubControlComponent.razor` étendu
- [ ] **Tester** toutes les commandes

**?? Durée estimée:** 2h  
**?? Impact:** ???

---

### ?? 13. DevicePeripheralService Complet

- [ ] **Ajouter méthodes** CRUD:
  - [ ] `GetAllAsync()`
  - [ ] `GetByIdAsync(int id)`
  - [ ] `CreateAsync(DevicePeripheralCreationDto dto)`
  - [ ] `UpdateAsync(int id, DevicePeripheralUpdateDto dto)`
  - [ ] `DeleteAsync(int id)`
- [ ] **Ajouter méthode** `SyncWithDesiredPropertiesAsync(string iotDeviceId)`
- [ ] **Intégrer** dans page Devices
- [ ] **Tester** CRUD complet

**?? Durée estimée:** 1h  
**?? Impact:** ??

---

## ?? PROGRESSION PHASE 3

| Tâche | Status | Durée | Assigné |
|-------|--------|-------|---------|
| 11. UserManagement Complet | ? TODO | 2h | - |
| 12. IoTHub Extensions | ? TODO | 2h | - |
| 13. DevicePeripheral CRUD | ? TODO | 1h | - |
| **TOTAL** | **0/3** | **5h** | - |

**?? Objectif:** Terminer Phase 3 début février

---

## ?? MÉTRIQUES GLOBALES

### Conformité Endpoints Backend

```
Avant:  ?????????????????? 40% (7/18 contrôleurs)
Phase 1: ?????????????????? 55% (10/18)
Phase 2: ?????????????????? 75% (13/18)
Phase 3: ???????????????????? 100% (18/18) ??
```

### Support `?fresh=true`

```
Avant:  ?????????????????? 20% (1/5 services)
Phase 1: ?????????????????? 60% (3/5)
Phase 2: ?????????????????? 80% (4/5)
Phase 3: ???????????????????? 100% (5/5) ??
```

### DTOs Complets

```
Avant:  ?????????????????? 70%
Phase 1: ?????????????????? 85%
Phase 2: ??????????????????? 95%
Phase 3: ???????????????????? 100% ??
```

---

## ?? DOCUMENTATION DE RÉFÉRENCE

- ?? **Analyse complète:** `BACKEND_FRONTEND_ALIGNMENT_REPORT.md`
- ?? **Code complet:** `CORRECTIONS_PRIORITAIRES_BACKEND_ALIGNMENT.md`
- ?? **Vue exécutive:** `RESUME_EXECUTIF_ALIGNMENT.md`
- ? **Quick Start:** `QUICK_START_BACKEND_ALIGNMENT.md`
- ? **Checklist:** Ce fichier

---

## ?? AIDE RAPIDE

### Erreur: Service not registered
```csharp
// Ajouter dans Program.cs
builder.Services.AddScoped<IYourService, YourService>();
```

### Erreur: 404 Not Found
```csharp
// Vérifier URL commence par /api/
await _apiService.GetAsync<T>("/api/ControllerName/...");
```

### Erreur: DTO not found
```csharp
// Vérifier ApiModels.cs contient le record
public record YourDto(...);
```

### Cache ne se rafraîchit pas
```csharp
// Utiliser fresh=true
await Service.GetDataAsync(fresh: true);
```

---

## ? VALIDATION FINALE

Avant de marquer le projet comme terminé, vérifier :

- [ ] ? Build réussit sans erreurs
- [ ] ? Tous les tests Postman passent
- [ ] ? Toutes les pages UI fonctionnent
- [ ] ? Notifications SignalR s'affichent
- [ ] ? Performance acceptable (<500ms endpoints)
- [ ] ? Documentation à jour
- [ ] ? Code reviewé
- [ ] ? Git commit/push effectué

---

**Dernière mise à jour:** 2025-01-30 14:30 UTC  
**Prochaine révision:** Après Phase 1  
**Responsable:** Équipe Frontend
