# ?? CORRECTIONS ERREURS BUILD - DASHBOARD & WIDGETS

**Date:** 2025-01-30  
**Status:** En cours  
**Build:** ? 20 erreurs restantes

---

## ?? ERREURS DÉTECTÉES (20)

### 1. ActiveAlarmsWidget (4 erreurs)
**Problème:** `SmokeAlarmEventDto` n'a pas `DeviceId`, il a `IotDeviceId`

**Lignes concernées:**
- Line 48: `Device: @alarm.DeviceId`
- Line 115: `Device: @_alarmToDeactivate.DeviceId`
- Line 235: `_alarmToDeactivate.DeviceId, request`
- Line 241: `Logger.LogInformation("? Alarme désactivée pour {DeviceId}", _alarmToDeactivate.DeviceId)`

**Solution:** Remplacer `DeviceId` par `IotDeviceId`

---

### 2. DeviceMetricsWidget (3 erreurs)
**Problèmes:**
- Typo: `_healthyCoun` au lieu de `_healthyCount` (line 44)
- `DeviceMetricsDto.DeviceId` n'existe pas ? `IotDeviceId` (line 83)
- Tuple `health` n'a pas `Label`, seulement `Status` (line 90)
- `GetLatestMetricsAsync` attend `string deviceId`, pas `int maxResults` (line 200)

**Solutions:**
- Corriger typo `_healthyCount`
- Utiliser `IotDeviceId`
- Utiliser `health.Status`
- Revoir logique appel API

---

### 3. RecentActivityWidget (2 erreurs)
**Problème:** Méthodes manquantes
- `GetActivityTypeColor()` n'existe pas
- `GetActivityTypeIcon()` n'existe pas

**Solution:** Ajouter ces méthodes helper

---

### 4. AccessRightsManagementComponent (11 erreurs)
**Problèmes multiples:**
- `SearchUsers` signature incorrecte (doit avoir `CancellationToken`)
- `ZoneDto.Nom` n'existe pas ? `Name`
- `DeviceDto.Type` n'existe pas ? `DeviceType`
- `DroitAccesDto.PeripheriqueName` ? `PeripheriqueLogiqueName`
- `DroitAccesDto.AccessType` ? `Type`
- `_allUsers` variable manquante dans @code
- `ZoneService.GetZonesAsync()` n'existe pas
- `DeviceService.GetDevicesAsync()` n'existe pas
- Ambiguïté `ApiResponse<>` (2 namespaces)

---

## ? CORRECTIONS APPLIQUÉES

### Correction Détaillée à faire:

#### 1. ActiveAlarmsWidget: IotDeviceId
#### 2. DeviceMetricsWidget: Fixes multiples
#### 3. RecentActivityWidget: Ajouter helpers
#### 4. AccessRightsManagementComponent: Refonte complète

---

**Status Final:** ? En attente corrections
