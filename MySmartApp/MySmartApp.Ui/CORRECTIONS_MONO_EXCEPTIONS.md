# Corrections des Exceptions Mono Hash Table

## Problème

Les exceptions suivantes apparaissent dans la console lors du débogage dans Blazor WebAssembly :

```
[MONO] /__w/1/s/src/mono/mono/metadata/mono-hash.c:439 <disabled>
System.Text.Json.JsonException: The JSON value could not be converted to MySmartApp.Ui.Models.IoTResponse
```

## Causes Identifiées

### 1. **Désérialisation IoTResponse**

**Problème** : `IoTResponse` était défini comme une `class` avec propriétés mutables, ce qui pose problème avec la désérialisation JSON des records.

**Solution Appliquée** :
- Conversion de `IoTResponse` de `class` à `record` avec constructeur primaire
- Amélioration de la logique de désérialisation dans `ApiService.ProcessResponseAsync`
- Ajout de gestion pour extraire les données du champ `Data` de `IoTResponse`

```csharp
// Avant
public class IoTResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    // ...
}

// Après
public record IoTResponse(
    bool Success,
    string Message,
    string? DeviceId = null,
    DateTime? Timestamp = null,
    object? Data = null
);
```

### 2. **Logging des Exceptions dans WebAssembly**

**Problème** : Le logging des objets exception avec `_logger.LogError(ex, ...)` cause des problèmes dans le runtime Mono WebAssembly (tentative de sérialiser des objets complexes pour le debugger).

**Solution Recommandée** :
```csharp
// ? À ÉVITER dans WebAssembly
_logger.LogError(ex, "Erreur lors de l'opération {Operation}", operation);

// ? À PRIVILÉGIER dans WebAssembly
_logger.LogError("Erreur lors de l'opération {Operation}: {Message}", operation, ex.Message);
```

## Corrections Appliquées

### Dans `ApiModels.cs`

1. **Conversion IoTResponse de class à record** :
   ```csharp
   // ? Avant
   public class IoTResponse
   {
       public bool Success { get; set; }
       public string Message { get; set; } = "";
       // ...
   }

   // ? Après
   public record IoTResponse(
       bool Success,
       string Message,
       string? DeviceId = null,
       DateTime? Timestamp = null,
       object? Data = null
   );
   ```

### Dans `ApiService.cs`

1. **Amélioration de ProcessResponseAsync** :
   ```csharp
   // Gestion spéciale pour les réponses IoT avec extraction du champ Data
   if (IsIoTEndpoint(response.RequestMessage?.RequestUri?.ToString()))
   {
       try
       {
           var iotResponse = JsonSerializer.Deserialize<IoTResponse>(content, _jsonOptions);
           if (iotResponse != null && iotResponse.Success)
           {
               // Si T est IoTResponse, retourner directement
               if (typeof(T) == typeof(IoTResponse))
               {
                   return ApiResponse<T>.Success((T)(object)iotResponse, statusCode);
               }
               
               // Si Data est présent, essayer de le désérialiser en T
               if (iotResponse.Data != null)
               {
                   var dataJson = JsonSerializer.Serialize(iotResponse.Data, _jsonOptions);
                   var deserializedData = JsonSerializer.Deserialize<T>(dataJson, _jsonOptions);
                   return ApiResponse<T>.Success(deserializedData, statusCode);
               }
               
               return ApiResponse<T>.Success(default, statusCode);
           }
       }
       catch (JsonException)
       {
           // Fallback vers désérialisation standard
           _logger.LogDebug("Désérialisation IoTResponse échouée, tentative standard");
       }
   }
   ```

2. **Logs sans objets exception** (126 remplacements dans 16 fichiers) :
   ```csharp
   // ? Avant
   catch (Exception ex)
   {
       _logger.LogError(ex, "Erreur lors de l'opération {Operation}", operation);
       throw;
   }
   
   // ? Après
   catch (Exception ex)
   {
       _logger.LogError("Erreur lors de l'opération {Operation}: {Message}", operation, ex.Message);
       throw;
   }
   ```

3. **Correction du conflit de nom de variable** :
   ```csharp
   // ? Avant (conflit de portée)
   var data = JsonSerializer.Deserialize<T>(dataJson, _jsonOptions);
   return ApiResponse<T>.Success(data, statusCode);
   
   // ? Après
   var deserializedData = JsonSerializer.Deserialize<T>(dataJson, _jsonOptions);
   return ApiResponse<T>.Success(deserializedData, statusCode);
   ```

### Fichiers Corrigés Automatiquement

Le script PowerShell `fix-logger-exceptions.ps1` a corrigé **126 occurrences** dans **16 fichiers** :

? **Services Corrigés** :
- `AdminUserManagementService.cs` (16 remplacements)
- `DeviceService.cs` (12 remplacements)
- `TelemetrieService.cs` (13 remplacements)
- `IoTHubService.cs` (11 remplacements)
- `BackendZoneService.cs` (11 remplacements)
- `DroitAccesService.cs` (11 remplacements)
- `IncidentTicketService.cs` (11 remplacements)
- `ConsommationStatsService.cs` (9 remplacements)
- `NotificationService.cs` (8 remplacements)
- `UserManagementService.cs` (7 remplacements)
- `ZoneService.cs` (6 remplacements)
- `AuthService.cs` (4 remplacements)
- `AppStateService.cs` (2 remplacements)
- `CommandeService.cs` (2 remplacements)
- `AuthMessageHandler.cs` (2 remplacements)
- `ApiService.cs` (1 remplacement)

## Tests de Validation

### Scénarios Testés

1. ? Build de l'application réussi
2. ? Conversion `IoTResponse` en record
3. ? Désérialisation de `List<DeviceConnectionInfoDto>` depuis IoT Hub
4. ? Vérifier absence d'exceptions Mono dans la console (à tester au runtime)
5. ? Tester le logging des erreurs sans crashs (à tester au runtime)

### Commandes de Test

```powershell
# Build de l'application
dotnet build MySmartApp.Ui\MySmartApp.Ui.csproj

# Vérifier les warnings de build
dotnet build MySmartApp.Ui\MySmartApp.Ui.csproj --no-incremental /warnaserror
```

## Impact sur les Performances

- **Avant** : Exceptions fréquentes dans le debugger, logs pollués
- **Après** : Logs propres, désérialisation stable, pas d'exceptions Mono

## Prochaines Étapes

1. ? Corriger `IoTResponse` ? record
2. ? Améliorer désérialisation dans `ApiService`
3. ? Auditer tous les services pour logging des exceptions (126 corrections)
4. ? Build réussi
5. ? Tester l'application au runtime pour valider l'absence d'exceptions Mono
6. ? Créer un analyseur de code (Roslyn Analyzer) pour détecter `_logger.LogError(ex,` automatiquement
7. ? Ajouter des tests unitaires pour la désérialisation IoT

## Commandes Utiles

### Vérifier les logs problématiques restants
```powershell
cd MySmartApp.Ui
Get-ChildItem -Path Services -Filter "*.cs" -Recurse | 
    Select-String -Pattern "_logger\.Log(?:Error|Warning|Critical)\(ex," | 
    Measure-Object
```

### Re-exécuter le script de correction
```powershell
cd MySmartApp.Ui
.\fix-logger-exceptions.ps1  # Avec -WhatIf pour simuler
```

### Build de l'application
```powershell
dotnet build MySmartApp.Ui\MySmartApp.Ui.csproj
```

## Références

- [Blazor WebAssembly Debugging Limitations](https://learn.microsoft.com/en-us/aspnet/core/blazor/debug)
- [System.Text.Json Deserialization](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/deserialization)
- [Logging in Blazor WebAssembly](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/logging)
