# ?? CORRECTIONS URGENTES - BUILD DASHBOARD

**Total erreurs:** 20  
**Status:** ? Build échoue  
**Priority:** CRITIQUE

---

## ?? CORRECTIONS REQUISES

### ? 1. ActiveAlarmsWidget.razor
**Remplacer partout `DeviceId` ? `IotDeviceId`**

```razor
Line 48:  Device: @alarm.IotDeviceId
Line 115: <strong>Device:</strong> @_alarmToDeactivate.IotDeviceId
Line 235: _alarmToDeactivate.IotDeviceId, request
Line 241: Logger.LogInformation("? Alarme désactivée pour {IotDeviceId}", _alarmToDeactivate.IotDeviceId)
```

---

### ? 2. DeviceMetricsWidget.razor
**Corrections multiples:**

```csharp
// Line 44: Corriger typo
Color="@(_healthyCount > 0 ? Color.Success : Color.Default)"

// Line 83: IotDeviceId
@metric.IotDeviceId

// Line 90: Status au lieu de Label
@health.Status

// Line 200: Revoir API call (nécessite refonte logique)
// API attend deviceId string, pas maxResults int
```

---

### ? 3. RecentActivityWidget.razor
**Ajouter méthodes helpers en @code:**

```csharp
private Color GetActivityTypeColor(string activityType)
{
    return activityType switch
    {
        "Commande" => Color.Primary,
        "Login" => Color.Success,
        "Create" => Color.Success,
        "Update" => Color.Warning,
        "Delete" => Color.Error,
        "Audit" => Color.Info,
        _ => Color.Default
    };
}

private string GetActivityTypeIcon(string activityType)
{
    return activityType switch
    {
        "Commande" => Icons.Material.Filled.Send,
        "Login" => Icons.Material.Filled.Login,
        "Create" => Icons.Material.Filled.Add,
        "Update" => Icons.Material.Filled.Edit,
        "Delete" => Icons.Material.Filled.Delete,
        "Audit" => Icons.Material.Filled.Visibility,
        _ => Icons.Material.Filled.Info
    };
}
```

---

### ? 4. AccessRightsManagementComponent.razor
**Corrections majeures:**

#### 4.1 SearchUsers signature (line 327)
```csharp
private async Task<IEnumerable<UserDetailsDto>> SearchUsers(string searchTerm, CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
        return Enumerable.Empty<UserDetailsDto>();

    try
    {
        var response = await AdminUserService.SearchUsersAsync(searchTerm);
        return response.IsSuccess && response.Data != null 
            ? response.Data 
            : Enumerable.Empty<UserDetailsDto>();
    }
    catch
    {
        return Enumerable.Empty<UserDetailsDto>();
    }
}
```

#### 4.2 Ajouter _allUsers dans @code (après line 268)
```csharp
private List<UserDetailsDto> _allUsers = new();
```

#### 4.3 DTOs properties (markup)
```razor
<!-- Line 76: ZoneDto.Nom ? Name -->
<MudSelectItem Value="@zone.Id">@zone.Name</MudSelectItem>

<!-- Line 99: DeviceDto.Type ? DeviceType -->
@device.Name (@(device.DeviceType ?? "N/A"))

<!-- Line 187: DroitAccesDto.PeripheriqueName ? PeripheriqueLogiqueName -->
@right.PeripheriqueLogiqueName

<!-- Lines 206-207: DroitAccesDto.AccessType ? Type -->
Color="@(right.Type == "Controle" ? Color.Primary : Color.Info)">
@right.Type
```

#### 4.4 Services methods (LoadDataAsync)
```csharp
// Line 308: GetZonesAsync ? GetBackendZonesAsync
var zonesResponse = await ZoneService.GetBackendZonesAsync();
if (zonesResponse.IsSuccess && zonesResponse.Data != null)
{
    // Convertir BackendZoneDto[] ? ZoneDto[]
    _availableZones = zonesResponse.Data
        .Select(z => new ZoneDto(z.Id, z.Nom))
        .ToList();
}

// Line 315: GetDevicesAsync ? GetPeriphLogiquesAsync
var devicesResponse = await DeviceService.GetPeriphLogiquesAsync();
if (devicesResponse.IsSuccess && devicesResponse.Data != null)
{
    // Convertir PeripheriqueLogiqueDto[] ? DeviceDto[]
    _availableDevices = devicesResponse.Data
        .Select(d => new DeviceDto(d.Id, d.Name, null, d.Type))
        .ToList();
}
```

#### 4.5 ApiResponse ambiguïté (line 504)
```csharp
// Utiliser fully qualified name
MySmartApp.Ui.Models.ApiResponse<bool> response;
```

---

## ?? ORDRE D'EXÉCUTION

1. **ActiveAlarmsWidget** (4 find/replace simples)
2. **DeviceMetricsWidget** (3 corrections + 1 refonte API)
3. **RecentActivityWidget** (ajouter 2 méthodes)
4. **AccessRightsManagementComponent** (11 corrections complexes)

---

## ?? NOTES IMPORTANTES

### DeviceMetricsWidget - Problème API
**API Backend:**
```csharp
GET /api/DeviceStatus/metrics/latest?maxResults=100
```

**Mais DeviceStatusService.GetLatestMetricsAsync() attend:**
```csharp
Task<ApiResponse<List<DeviceMetricsDto>>> GetLatestMetricsAsync(string deviceId)
```

**Solution temporaire:** Créer nouvelle méthode ou utiliser endpoint différent

---

### AccessRightsManagementComponent - Services manquants
**IBackendZoneService n'a pas GetZonesAsync()**  
**Solution:** Utiliser `GetBackendZonesAsync()` + convertir DTOs

**IDeviceService n'a pas GetDevicesAsync()**  
**Solution:** Utiliser `GetPeriphLogiquesAsync()` + convertir DTOs

---

## ?? APRÈS CORRECTIONS

### Test Build
```bash
dotnet build MySmartApp.Ui
```

### Si succès ? Tests fonctionnels
1. Lancer app (`dotnet run`)
2. Naviguer `/dashboard`
3. Vérifier widgets affichés
4. Déclencher alarme test
5. Vérifier logs critiques
6. Tester droits d'accès

---

**Dernière mise à jour:** 2025-01-30 18:30 UTC  
**Priorité:** ?? CRITIQUE  
**Status:** ? En attente application corrections
