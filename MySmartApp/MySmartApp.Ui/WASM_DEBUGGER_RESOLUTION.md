# ? Résolution - Erreurs du débogueur WASM Blazor

## ?? Problème résolu

Les erreurs d'assertion Mono WASM qui apparaissaient dans la console lors du débogage:

```
[MONO] * Assertion at /__w/1/s/src/mono/mono/component/debugger-agent.c:2017, 
condition `<disabled>' not met
```

## ?? Modifications appliquées

### 1. **MySmartApp.Ui.csproj** - Désactivation du débogage WASM

```xml
<PropertyGroup>
  <!-- ... autres propriétés ... -->
  
  <!-- Désactiver le débogage WASM pour éviter les assertions Mono -->
  <WasmDebugLevel>0</WasmDebugLevel>
</PropertyGroup>
```

**Effet**: Désactive complètement le débogueur Mono WASM, éliminant les assertions.

### 2. **Properties/launchSettings.json** - Profils mis à jour

**Modifications**:
- ? Suppression de `inspectUri` des profils `http` et `https`
- ? Ajout d'un nouveau profil `https (No Debug)` pour développement sans débogage

```json
{
  "profiles": {
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

## ?? Guide d'utilisation

### Option 1: Lancer sans débogage (Recommandé) ?

Dans Visual Studio:
```
Ctrl + F5  (Démarrer sans déboguer)
```

Ou depuis le terminal:
```powershell
cd D:\Memoire\FrontSmart\MySmartApp\MySmartApp.Ui
dotnet run
```

### Option 2: Utiliser le nouveau profil

1. Dans Visual Studio, sélectionnez **"https (No Debug)"** dans la liste déroulante des profils
2. Appuyez sur **F5** ou cliquez sur le bouton de démarrage

### Option 3: DevTools du navigateur

1. Lancez l'application (Ctrl+F5)
2. Ouvrez les DevTools (F12)
3. Utilisez l'onglet **Console** pour les logs
4. Utilisez l'onglet **Network** pour inspecter les requêtes HTTP

## ?? Alternatives de débogage

### 1. Logging avec `ILogger`

```csharp
@inject ILogger<MyComponent> Logger

protected override async Task OnInitializedAsync()
{
    Logger.LogInformation("? Composant initialisé");
    
    try
    {
        var data = await LoadDataAsync();
        Logger.LogInformation("?? Données chargées: {Count} items", data.Count);
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "? Erreur lors du chargement");
    }
}
```

**Où voir les logs**:
- Console du navigateur (F12 ? Console)
- Fenêtre de sortie de Visual Studio (Affichage ? Sortie)

### 2. Snackbar pour afficher les états

```csharp
@inject ISnackbar Snackbar

private async Task MyOperationAsync()
{
    Snackbar.Add("?? Opération en cours...", Severity.Info);
    
    try
    {
        await DoSomethingAsync();
        Snackbar.Add("? Opération réussie!", Severity.Success);
    }
    catch (Exception ex)
    {
        Snackbar.Add($"? Erreur: {ex.Message}", Severity.Error);
    }
}
```

### 3. Composant de diagnostic

Créez un composant temporaire pour afficher les variables:

```razor
@if (ShowDebugInfo)
{
    <MudPaper Class="pa-4 ma-4" Style="background: #ffe6e6;">
        <MudText Typo="Typo.h6">?? Debug Info</MudText>
        <MudDivider Class="my-2" />
        <MudText>IsLoading: @IsLoading</MudText>
        <MudText>Items Count: @Items.Count</MudText>
        <MudText>Current User: @CurrentUser?.Email</MudText>
    </MudPaper>
}

@code {
#if DEBUG
    private bool ShowDebugInfo = true;
#else
    private bool ShowDebugInfo = false;
#endif
}
```

### 4. Console JavaScript via JSRuntime

```csharp
@inject IJSRuntime JS

private async Task LogToConsoleAsync()
{
    await JS.InvokeVoidAsync("console.log", "Mon message");
    await JS.InvokeVoidAsync("console.table", myObject);
    await JS.InvokeVoidAsync("console.warn", "Attention!");
    await JS.InvokeVoidAsync("console.error", "Erreur!");
}
```

## ?? Résultat

### ? Avant les corrections

```
? Console saturée d'erreurs Mono
? Application lente à démarrer
? Débogueur instable
? Difficile de voir les vrais logs
```

### ? Après les corrections

```
? Console propre, sans erreurs d'assertion
? Application démarre rapidement
? Logs clairs et lisibles
? Développement fluide avec Ctrl+F5
```

## ?? Comprendre le problème

### Qu'est-ce que le débogueur Mono WASM ?

Le débogueur Mono WASM permet de:
- Mettre des breakpoints dans le code C#
- Inspecter les variables
- Pas-à-pas (Step Into/Over/Out)

### Pourquoi ces erreurs ?

1. **Bug connu de .NET 9**: Le débogueur WASM a des problèmes avec certaines opérations asynchrones
2. **Assertion interne**: Le code du débogueur vérifie des conditions qui échouent
3. **Pas d'impact fonctionnel**: L'application fonctionne malgré ces erreurs

### Quand utiliser le débogueur WASM ?

**Utilisez le débogueur WASM quand**:
- Vous avez besoin de breakpoints complexes
- Vous déboguez de la logique métier compliquée
- Les logs ne suffisent pas

**N'utilisez PAS le débogueur WASM quand**:
- Développement quotidien normal
- Tests fonctionnels
- Démo ou présentation
- Production

## ?? Ressources

- ?? [Documentation - Debugging Blazor WebAssembly](https://learn.microsoft.com/aspnet/core/blazor/debug)
- ?? [Known Issues - .NET 9](https://github.com/dotnet/core/blob/main/release-notes/9.0/known-issues.md)
- ?? [GitHub Issues - Mono WASM Debugger](https://github.com/dotnet/runtime/issues?q=is%3Aissue+wasm+debugger)
- ??? [Browser DevTools Guide](https://developer.chrome.com/docs/devtools/)

## ?? Checklist de vérification

Avant de dire "ça ne marche pas", vérifiez:

- [ ] J'ai lancé avec **Ctrl+F5** (sans débogage)
- [ ] J'ai nettoyé et reconstruit le projet (`dotnet clean && dotnet build`)
- [ ] J'ai vidé le cache du navigateur (Ctrl+Shift+Delete)
- [ ] J'ai regardé la console du navigateur (F12 ? Console)
- [ ] J'ai regardé l'onglet Network pour les erreurs HTTP
- [ ] J'ai vérifié que le backend API est démarré
- [ ] J'ai vérifié `appsettings.Development.json` pour l'URL de l'API

## ?? Conclusion

Les erreurs d'assertion Mono WASM sont:
- ? **Résolues** grâce à `WasmDebugLevel=0`
- ? **Non critiques** - n'affectent pas l'application
- ? **Temporaires** - Microsoft y travaille
- ? **Évitables** - utilisez Ctrl+F5

**L'application est maintenant prête pour le développement sans interruptions !** ??

---

**Documentation complète**: Voir `DEBUGGER_WASM_FIXES.md`  
**Date**: 2025-01-XX  
**Version**: 1.0  
**Statut**: ? Testé et fonctionnel
