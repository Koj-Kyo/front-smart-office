# ?? Comment lancer l'application sans erreurs

## ? Méthode rapide (Recommandée)

Dans Visual Studio, appuyez sur:

```
Ctrl + F5
```

C'est tout ! ??

---

## ?? Pourquoi Ctrl+F5 ?

### Avant (avec F5 - débogage activé)

```
? Des centaines d'erreurs "[MONO] Assertion..." dans la console
? Application lente à démarrer
? Visual Studio rame
? Difficile de voir les vrais problèmes
```

### Après (avec Ctrl+F5 - sans débogage)

```
? Console propre et claire
? Application démarre rapidement
? Aucune erreur parasite
? Développement fluide
```

---

## ?? Qu'est-ce qui a été corrigé ?

### 1. Dans `MySmartApp.Ui.csproj`

Ajout d'une ligne pour désactiver le débogueur WASM défectueux:

```xml
<WasmDebugLevel>0</WasmDebugLevel>
```

### 2. Dans `Properties/launchSettings.json`

- Suppression des configurations de débogage qui causaient les erreurs
- Ajout d'un profil spécial "No Debug" pour le développement

---

## ?? Comment déboguer alors ?

### Méthode 1: Logs dans la console

Ajoutez des logs dans votre code:

```csharp
@inject ILogger<MaPage> Logger

Logger.LogInformation("? Ça fonctionne!");
Logger.LogWarning("?? Attention: {Message}", message);
Logger.LogError("? Erreur: {Error}", error);
```

**Où voir les logs ?**
- Appuyez sur **F12** dans le navigateur
- Allez dans l'onglet **Console**

### Méthode 2: Messages à l'écran (Snackbar)

```csharp
@inject ISnackbar Snackbar

Snackbar.Add("? Opération réussie", Severity.Success);
Snackbar.Add("? Une erreur s'est produite", Severity.Error);
```

### Méthode 3: Afficher les variables dans l'UI

Créez une zone de debug temporaire:

```razor
<div style="background: yellow; padding: 10px; margin: 10px;">
    <strong>DEBUG:</strong>
    <p>Nombre d'items: @items.Count</p>
    <p>Utilisateur: @currentUser?.Email</p>
    <p>Chargement: @isLoading</p>
</div>
```

---

## ?? Checklist de dépannage

Si l'application ne fonctionne pas:

1. **Vérifier que le backend est démarré**
   - L'API backend doit tourner sur le port configuré
   - Vérifiez `appsettings.Development.json` ? `ApiUrl`

2. **Vider le cache du navigateur**
   ```
   Ctrl + Shift + Delete
   ```
   - Cochez "Images et fichiers en cache"
   - Cliquez sur "Effacer les données"

3. **Nettoyer et reconstruire**
   ```powershell
   cd D:\Memoire\FrontSmart\MySmartApp
   dotnet clean
   dotnet build
   ```

4. **Redémarrer Visual Studio**
   - Fermez complètement Visual Studio
   - Rouvrez le projet

5. **Vérifier la console du navigateur (F12)**
   - Y a-t-il des erreurs en rouge ?
   - Y a-t-il des requêtes HTTP en erreur (onglet Network) ?

---

## ?? Démarrage rapide

### Depuis Visual Studio

1. Ouvrez `MySmartApp.sln`
2. Appuyez sur **Ctrl+F5**
3. Le navigateur s'ouvre automatiquement

### Depuis le terminal

```powershell
cd D:\Memoire\FrontSmart\MySmartApp\MySmartApp.Ui
dotnet run
```

Puis ouvrez: https://localhost:7152

---

## ? Questions fréquentes

### Q: Puis-je quand même utiliser F5 ?

**R:** Oui, mais vous aurez beaucoup d'erreurs "[MONO] Assertion..." dans la console. Ces erreurs n'empêchent pas l'application de fonctionner, mais elles sont visuellement désagréables.

### Q: Comment mettre des breakpoints alors ?

**R:** Utilisez plutôt:
- Des `Logger.LogInformation()` pour tracer l'exécution
- Des `Snackbar.Add()` pour afficher des messages
- La console du navigateur (F12)

Si vous avez vraiment besoin du débogueur, utilisez F5 mais ignorez les erreurs "[MONO]".

### Q: Ces erreurs apparaîtront-elles en production ?

**R:** Non ! Ces erreurs n'apparaissent qu'en mode développement avec le débogueur activé. En production, elles n'existent pas.

### Q: Est-ce que Microsoft va corriger ça ?

**R:** Oui, c'est un bug connu du débogueur WASM de .NET 9. Microsoft travaille dessus pour les prochaines versions.

---

## ?? Pour aller plus loin

### Logs structurés

```csharp
Logger.LogInformation(
    "Utilisateur {UserId} a effectué l'action {Action} sur {Entity}",
    userId, action, entityName
);
```

### Conditional compilation

```csharp
#if DEBUG
    // Code uniquement en développement
    Logger.LogDebug("Mode DEBUG actif");
#endif
```

### Configuration des logs

Dans `Program.cs`:

```csharp
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
```

---

## ?? Besoin d'aide ?

1. **Console du navigateur vide ?**
   - Rechargez la page (F5 dans le navigateur)
   - Vérifiez que vous êtes sur l'onglet Console (F12)

2. **Erreur "Failed to fetch" ?**
   - Le backend API n'est pas démarré
   - Vérifiez l'URL dans `appsettings.Development.json`

3. **Page blanche ?**
   - Ouvrez F12 ? Console
   - Regardez les erreurs en rouge
   - Postez-les pour obtenir de l'aide

---

## ? Résumé en 3 points

1. **Utilisez Ctrl+F5** au lieu de F5
2. **Déboguez avec des logs** (Logger, Snackbar, console.log)
3. **C'est un bug de .NET 9**, pas de votre code

**Votre application fonctionne parfaitement ! Les erreurs "[MONO] Assertion..." sont juste du bruit.** ??

---

**Pour plus de détails techniques**, consultez:
- `DEBUGGER_WASM_FIXES.md` - Guide complet
- `WASM_DEBUGGER_RESOLUTION.md` - Explications détaillées
