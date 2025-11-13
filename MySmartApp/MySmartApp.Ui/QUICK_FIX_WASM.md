# ? Guide Ultra-Rapide - Résolution Erreurs WASM

## ?? Problème

Des centaines de lignes d'erreurs `[MONO] * Assertion at ...` dans la console.

## ? Solution (30 secondes)

### Dans Visual Studio

Appuyez sur:
```
Ctrl + F5
```

**C'est tout !** ?

---

## ?? Pourquoi ?

- C'est un **bug de .NET 9**, pas de votre code
- L'application **fonctionne normalement**
- Ces erreurs **n'apparaissent qu'en mode debug**

## ?? Ce qui a été fait

### 1. `MySmartApp.Ui.csproj`
```xml
<WasmDebugLevel>0</WasmDebugLevel>
```

### 2. `launchSettings.json`
- ? Supprimé `inspectUri`
- ? Ajouté profil "No Debug"

## ?? Comment déboguer alors ?

### Méthode 1: Logs
```csharp
Logger.LogInformation("? Mon message");
```
Puis F12 ? Console dans le navigateur

### Méthode 2: Snackbar
```csharp
Snackbar.Add("? Ça marche!", Severity.Success);
```

### Méthode 3: Afficher les variables
```razor
<div style="background:yellow">
    Debug: IsLoading = @isLoading
</div>
```

## ?? Documentation complète

- **Guide simple**: [GUIDE_SIMPLE_DEMARRAGE.md](./GUIDE_SIMPLE_DEMARRAGE.md)
- **Guide technique**: [DEBUGGER_WASM_FIXES.md](./DEBUGGER_WASM_FIXES.md)
- **Résolution**: [WASM_DEBUGGER_RESOLUTION.md](./WASM_DEBUGGER_RESOLUTION.md)
- **Historique**: [HISTORIQUE_CORRECTIONS_DEBUGGER.md](./HISTORIQUE_CORRECTIONS_DEBUGGER.md)

## ? Checklist

- [ ] Utilisez **Ctrl+F5** (pas F5)
- [ ] Nettoyez si nécessaire: `dotnet clean && dotnet build`
- [ ] Videz le cache du navigateur (Ctrl+Shift+Delete)
- [ ] Regardez F12 ? Console pour les vrais problèmes

---

**Résultat**: Console propre, app rapide, développement fluide ! ??
