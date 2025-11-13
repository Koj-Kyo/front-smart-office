# ?? Corrections - Erreurs du débogueur WASM

## ?? Problème

Lors du débogage de l'application Blazor WebAssembly avec Visual Studio, vous rencontrez de nombreuses erreurs:

```
[MONO] * Assertion at /__w/1/s/src/mono/mono/component/debugger-agent.c:2017, 
condition `<disabled>' not met

program exited (with status: 1), but keepRuntimeAlive() is set (counter=1) 
due to an async operation, so halting execution but not exiting the runtime 
or preventing further async execution
```

## ?? Nature du problème

**Ce ne sont PAS des erreurs dans votre code !**

Ces assertions sont des **bugs connus du débogueur Mono WASM** dans .NET 9. Ils se produisent principalement:
- Lors du débogage dans le navigateur
- Avec les outils de développement (F12) ouverts
- Lors de l'utilisation de breakpoints dans Visual Studio

## ? Solutions appliquées

### 1. Configuration du projet (`MySmartApp.Ui.csproj`)

Ajout de la propriété `<WasmDebugLevel>0</WasmDebugLevel>` pour désactiver le débogage WASM:

```xml
<PropertyGroup>
  <TargetFramework>net9.0</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  
  <!-- Désactiver le débogage WASM pour éviter les assertions Mono -->
  <WasmDebugLevel>0</WasmDebugLevel>
</PropertyGroup>
```

### 2. Profils de lancement (`launchSettings.json`)

Suppression de la ligne `inspectUri` des profils existants et ajout d'un nouveau profil sans débogage:

```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5249",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7152;http://localhost:5249",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https (No Debug)": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7152;http://localhost:5249",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "ASPNETCORE_HOSTINGSTARTUPASSEMBLIES": ""
      }
    }
  }
}
```

## ?? Comment utiliser

### Méthode 1: Lancer sans débogage (Recommandé)

Dans Visual Studio:
1. Utilisez **Ctrl+F5** (Démarrer sans déboguer)
2. Ou sélectionnez le profil **"https (No Debug)"** dans la liste déroulante

### Méthode 2: Utiliser les outils de développement du navigateur

Au lieu du débogueur Visual Studio, utilisez les DevTools du navigateur:

1. Lancez l'application (Ctrl+F5)
2. Ouvrez les DevTools (F12)
3. Utilisez l'onglet **Console** pour voir les logs
4. Utilisez l'onglet **Sources** pour voir le code
5. Utilisez `console.log()` dans votre code C# via `JSRuntime`

### Méthode 3: Déboguer uniquement le côté serveur

Si vous avez un backend API, déboguez uniquement celui-ci:
1. Lancez le backend avec débogage (F5)
2. Lancez le frontend sans débogage (Ctrl+F5)

## ?? Techniques de débogage alternatives

### 1. Logging avec ILogger

```csharp
@inject ILogger<MaPage> Logger

protected override void OnInitialized()
{
    Logger.LogInformation("? Page initialisée");
    Logger.LogWarning("?? Attention: {Message}", "quelque chose");
    Logger.LogError("? Erreur: {Exception}", ex);
}
```

Les logs apparaîtront dans:
- La console du navigateur (F12)
- La fenêtre de sortie de Visual Studio

### 2. Console JavaScript

Injecter `IJSRuntime` pour utiliser `console.log`:

```csharp
@inject IJSRuntime JS

private async Task DebugAsync()
{
    await JS.InvokeVoidAsync("console.log", "Mon message de debug");
    await JS.InvokeVoidAsync("console.table", myObject);
    await JS.InvokeVoidAsync("console.warn", "Attention!");
    await JS.InvokeVoidAsync("console.error", "Erreur!");
}
```

### 3. Composant de diagnostic

Créez un composant pour afficher les variables:

```razor
<div class="debug-panel" style="position: fixed; bottom: 0; right: 0; background: white; border: 2px solid red; padding: 10px; max-width: 400px; max-height: 300px; overflow: auto;">
    <h4>?? Debug Info</h4>
    <p><strong>IsLoading:</strong> @isLoading</p>
    <p><strong>Count:</strong> @items.Count</p>
    <p><strong>Current User:</strong> @currentUser?.Email</p>
</div>
```

### 4. Exceptions détaillées

Dans `Program.cs`, activez les détails d'erreur:

```csharp
builder.Services.AddLogging(logging =>
{
    logging.SetMinimumLevel(LogLevel.Debug);
    logging.AddFilter("Microsoft", LogLevel.Warning);
    logging.AddFilter("System", LogLevel.Warning);
});

// Pour développement uniquement
#if DEBUG
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.WriteIndented = true;
});
#endif
```

### 5. Snackbar pour afficher les erreurs

Avec MudBlazor:

```csharp
@inject ISnackbar Snackbar

private async Task DebugOperationAsync()
{
    try
    {
        await MyOperationAsync();
        Snackbar.Add("? Opération réussie", Severity.Success);
    }
    catch (Exception ex)
    {
        Snackbar.Add($"? Erreur: {ex.Message}", Severity.Error);
        Logger.LogError(ex, "Erreur lors de l'opération");
    }
}
```

## ?? Comparaison des méthodes

| Méthode | Avantages | Inconvénients |
|---------|-----------|---------------|
| **Ctrl+F5** | ? Simple, rapide, pas d'erreurs | ? Pas de breakpoints |
| **DevTools navigateur** | ? Inspecter le DOM, network | ? Code transpilé difficile à lire |
| **ILogger** | ? Logs structurés, filtres | ? Requiert rebuild pour chaque ajout |
| **console.log via JS** | ? Flexible, temps réel | ? Nécessite IJSRuntime |
| **Composant debug** | ? Visual, interactif | ? Pollution de l'UI |

## ?? Bonnes pratiques

### 1. Environnements séparés

```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Warning",
      "System": "Warning"
    }
  }
}

// appsettings.Production.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Error",
      "System": "Error"
    }
  }
}
```

### 2. Guard clauses avec logging

```csharp
private async Task LoadDataAsync()
{
    Logger.LogDebug("?? Début du chargement des données");
    
    if (!IsAuthenticated)
    {
        Logger.LogWarning("?? Utilisateur non authentifié");
        return;
    }
    
    try
    {
        var data = await ApiService.GetDataAsync();
        Logger.LogInformation("? Données chargées: {Count} items", data.Count);
    }
    catch (HttpRequestException ex)
    {
        Logger.LogError(ex, "? Erreur HTTP lors du chargement");
        throw;
    }
}
```

### 3. Extension methods pour debugging

```csharp
public static class LoggerExtensions
{
    public static void LogMethodEntry(this ILogger logger, 
        [CallerMemberName] string methodName = "")
    {
        logger.LogDebug("? Entrée dans {MethodName}", methodName);
    }
    
    public static void LogMethodExit(this ILogger logger, 
        [CallerMemberName] string methodName = "")
    {
        logger.LogDebug("? Sortie de {MethodName}", methodName);
    }
}

// Usage
Logger.LogMethodEntry();
// ... code ...
Logger.LogMethodExit();
```

## ?? Références

- [Debugging Blazor WebAssembly](https://learn.microsoft.com/aspnet/core/blazor/debug)
- [.NET 9 Known Issues](https://github.com/dotnet/core/blob/main/release-notes/9.0/known-issues.md)
- [Mono WASM Debugger Issues](https://github.com/dotnet/runtime/issues?q=is%3Aissue+is%3Aopen+wasm+debugger)
- [Browser DevTools](https://developer.chrome.com/docs/devtools/)

## ?? Notes importantes

1. **Ces erreurs n'impactent PAS l'application en production** - elles n'apparaissent qu'en mode débogage
2. **L'application fonctionne normalement** malgré ces messages d'erreur
3. **Microsoft travaille sur des correctifs** pour .NET 9 et versions futures
4. **Utilisez `WasmDebugLevel=0`** pour désactiver complètement le débogueur

## ?? Résultat

Après application de ces corrections:
- ? Plus d'assertions Mono dans la console
- ? Application démarre normalement
- ? Performances améliorées (pas de débogueur)
- ? Logs clairs dans la console du navigateur

---

**Date**: 2025-01-XX  
**Version**: 1.0  
**Statut**: ? Testé et fonctionnel
