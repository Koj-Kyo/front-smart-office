# ?? DIAGNOSTIC FRONTEND MySmartApp.Ui

**Date:** 2025-01-30 19:30 UTC  
**Status Build:** ? **SUCCÈS** (warnings seulement)

---

## ? BUILD STATUS

```
? Build réussi: MySmartApp.Ui
? Blazor output généré
?? 95 warnings (non-bloquants)
? 0 erreurs
```

---

## ?? ANALYSE DES WARNINGS

### Catégories de Warnings

| Catégorie | Count | Gravité | Action |
|-----------|-------|---------|--------|
| **Nullable Reference** (CS8xxx) | ~40 | ?? Moyenne | Optionnel |
| **MudBlazor Analyzer** (MUD0002) | ~50 | ?? Faible | Cosmétique |
| **Unused Code** (CS0067, CS0414) | ~5 | ?? Faible | Nettoyage |

---

## ?? PROBLÈMES POTENTIELS

### 1. Configuration API Backend

**Fichier:** `wwwroot/appsettings.json`

Vérifiez que l'URL du backend est correcte :

```json
{
  "Api": {
    "BaseUrl": "https://localhost:7xxx",  // ?? PORT CORRECT?
    "Timeout": 30,
    "RetryCount": 3
  }
}
```

**Action:** Vérifier que le backend tourne sur ce port.

---

### 2. Problèmes de Connexion Backend

**Symptômes possibles:**
- ? Erreurs 404 / 500 dans console navigateur
- ? "Failed to fetch" errors
- ? CORS errors

**Vérification:**

1. **Backend lancé?**
```bash
# Terminal 1 - Backend
cd MySmartApp.API
dotnet run
```

2. **Port correct?**
```bash
# Vérifier launchSettings.json backend
# applicationUrl doit matcher Api.BaseUrl frontend
```

---

### 3. Problèmes CORS

**Erreur console:** `Access-Control-Allow-Origin`

**Solution Backend (Program.cs):**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorWasm", policy =>
    {
        policy.WithOrigins("https://localhost:5001", "http://localhost:5000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

app.UseCors("AllowBlazorWasm");
```

---

### 4. Authentification JWT

**Fichier:** `wwwroot/appsettings.json`

```json
{
  "Auth": {
    "AuthorityUrl": "https://localhost:7xxx",
    "TokenStorageKey": "accessToken"
  }
}
```

**Vérifications:**
- [ ] Backend AuthorityUrl correct?
- [ ] Token stocké dans localStorage?
- [ ] Token valide (non expiré)?

**Test localStorage:**
```javascript
// Console navigateur F12
localStorage.getItem('accessToken')
// Doit retourner un token JWT si connecté
```

---

## ?? TESTS DE DIAGNOSTIC

### Test 1: Vérifier API Backend

**Console navigateur (F12 ? Network):**

```javascript
fetch('https://localhost:7xxx/api/Dashboard/stats')
  .then(r => r.json())
  .then(console.log)
  .catch(console.error);
```

**Attendu:** Retourne stats ou erreur 401 (non authentifié)  
**Si erreur:** Backend non accessible

---

### Test 2: Vérifier Authentification

**Page:** `/diagnostics` ou `/login`

**Vérifier:**
- [ ] Login fonctionne?
- [ ] Token stocké après login?
- [ ] Redirect vers dashboard après login?

---

### Test 3: Vérifier Composants Dashboard

**Naviguer:** `/dashboard`

**Vérifier chaque widget:**
- [ ] **DashboardStats** - KPI affichés?
- [ ] **RecentActivityWidget** - Activités visibles?
- [ ] **ActiveAlarmsWidget** - Message "Aucune alarme"?
- [ ] **DeviceMetricsWidget** - Message "En développement"?
- [ ] **CriticalLogsWidget** - Logs critiques?

---

## ??? CHECKLIST DÉMARRAGE

### 1. Backend Running

```bash
cd MySmartApp.API
dotnet run
```

**Vérifier logs:**
```
Now listening on: https://localhost:7xxx
```

---

### 2. Frontend Running

```bash
cd MySmartApp.Ui
dotnet run
```

**Vérifier logs:**
```
Now listening on: https://localhost:5001
```

---

### 3. Tester Accès

**Navigateur:** `https://localhost:5001`

**Attendu:**
- ? Page login affichée
- ? Pas d'erreurs console F12
- ? Connexion backend OK

---

## ?? ERREURS COMMUNES

### Erreur 1: "Failed to fetch"

**Cause:** Backend non accessible  
**Solution:**
1. Vérifier backend lancé (`dotnet run`)
2. Vérifier port dans `appsettings.json`
3. Désactiver antivirus/firewall temporairement

---

### Erreur 2: CORS Policy

**Cause:** Backend refuse requêtes frontend  
**Solution:** Configurer CORS backend (voir section 3)

---

### Erreur 3: 401 Unauthorized

**Cause:** Token absent ou invalide  
**Solution:**
1. Se déconnecter et reconnecter
2. Vérifier token localStorage
3. Vérifier JWT secret backend/frontend match

---

### Erreur 4: Widgets vides

**Cause:** API backend non implémentée  
**Solutions:**
- **RecentActivityWidget:** ? Backend OK
- **ActiveAlarmsWidget:** ? Backend OK
- **DeviceMetricsWidget:** ?? `GetAllLatestMetricsAsync()` manquante
- **CriticalLogsWidget:** ? Backend OK

---

## ?? RECOMMANDATIONS

### Priorité HAUTE ??

1. **Vérifier Configuration API**
   - Fichier: `wwwroot/appsettings.json`
   - Valider: `Api.BaseUrl` = URL backend

2. **Tester Backend Accessible**
   ```bash
   curl https://localhost:7xxx/api/Dashboard/stats
   ```

3. **Tester Login**
   - Page: `/login`
   - Credentials: admin@example.com / password

---

### Priorité MOYENNE ??

4. **Implémenter API Manquante**
   - `GET /api/DeviceStatus/metrics/latest/all`
   - Pour: DeviceMetricsWidget

5. **Corriger Warnings Nullable**
   - Ajouter `#nullable enable` dans fichiers
   - Ou désactiver dans `.csproj`

---

### Priorité BASSE ??

6. **Corriger MudBlazor Warnings**
   - Remplacer `IsVisible` ? `@bind-Visible`
   - Remplacer `Title` ? `Tooltip` (MudIconButton)

7. **Nettoyer Code Inutilisé**
   - Events `DevicesUpdated` / `DeviceStatusChanged`
   - Variables `_isLoadingUsers`, `isLoading`

---

## ?? QUICK START

**Lancement complet en 3 étapes:**

```bash
# Terminal 1 - Backend
cd D:\Memoire\FrontSmart\MySmartApp\MySmartApp.API
dotnet run

# Terminal 2 - Frontend
cd D:\Memoire\FrontSmart\MySmartApp\MySmartApp.Ui
dotnet run

# Navigateur
https://localhost:5001
```

**Login:**
```
Email: admin@example.com
Password: Admin123!
```

---

## ?? SI PROBLÈMES PERSISTENT

**Fournir ces infos:**

1. **Erreurs console navigateur (F12)**
2. **Logs backend** (terminal où `dotnet run`)
3. **URL testée** (ex: `/dashboard`)
4. **Comportement attendu vs réel**

**Fichiers de logs:**
- Frontend: Console navigateur F12
- Backend: Terminal `dotnet run`
- Configuration: `wwwroot/appsettings.json`

---

**Status:** ? Build OK - Diagnostic complet fourni  
**Prochaine étape:** Vérifier backend accessible + tester login
