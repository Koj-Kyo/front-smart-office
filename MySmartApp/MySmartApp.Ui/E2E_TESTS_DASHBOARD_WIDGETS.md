# ?? GUIDE TESTS E2E - DASHBOARD WIDGETS

**Version:** 1.0  
**Date:** 2025-01-30  
**Scope:** Tests End-to-End Dashboard Phase 3.5  
**Status:** Ready for Execution

---

## ?? OBJECTIFS TESTS E2E

### Validation Complète
1. ? **ActiveAlarmsWidget** - Alarmes fumée + Désactivation
2. ? **CriticalLogsWidget** - Logs critiques 24h
3. ? **DeviceMetricsWidget** - Métriques IoT + Santé
4. ? **RecentActivityWidget** - Timeline activités

### Critères de Succès
- Build sans erreurs
- Widgets affichés dans Dashboard
- Données chargées depuis backend
- Interactions fonctionnelles (boutons, dialogs)
- Responsive design (desktop/mobile)

---

## ?? PRÉ-REQUIS

### 1. Backend Running
```bash
cd MySmartApp.Api
dotnet run

# Vérifier Swagger
https://localhost:7182/swagger
```

### 2. Frontend Running
```bash
cd MySmartApp.Ui
dotnet run

# Vérifier application
https://localhost:7000
```

### 3. Authentification
**Créer compte admin si nécessaire:**
```http
POST https://localhost:7182/api/Auth/register
Content-Type: application/json

{
  "email": "admin@test.com",
  "password": "Admin@123",
  "firstName": "Admin",
  "lastName": "Test"
}
```

**Login:**
```
Email: admin@test.com
Password: Admin@123
```

---

## ?? TEST SUITE 1: ACTIVEALARMS WIDGET

### Test 1.1: Sans Alarme (Empty State)

**Étapes:**
1. Se connecter à l'application
2. Naviguer vers Dashboard (`/dashboard`)
3. Localiser **ActiveAlarmsWidget** (row 1 gauche)

**Vérifications:**
- ? Widget affiché avec header "Alarmes Fumée Actives"
- ? Icône Warning (rouge)
- ? Message "Aucune alarme active"
- ? Icône verte CheckCircle
- ? Texte "Tous les systèmes fonctionnent normalement"
- ? **Pas de** bouton "Voir toutes les alarmes"

**Screenshot:** `test-1.1-no-alarms.png`

---

### Test 1.2: Déclencher Alarme

**Étapes Backend:**
```http
POST https://localhost:7182/api/DeviceStatus/ESP32_TEST/smoke-alarms
Authorization: Bearer <token>
Content-Type: application/json

{
  "zoneId": 1
}
```

**Vérifications Frontend:**
1. **Auto-refresh** (attendre max 60s ou cliquer refresh)
2. Widget affiche alarme:
   - ? Card alarme visible
   - ? Icône ?? + Nom zone
   - ? Device ID affiché
   - ? Heure déclenchement (dd/MM HH:mm)
   - ? Durée écoulée calculée
   - ? Bouton "Désactiver" présent
   - ? Chip compteur "1" dans header (rouge)

**Screenshot:** `test-1.2-alarm-triggered.png`

---

### Test 1.3: Désactiver Alarme

**Étapes:**
1. Cliquer bouton **"Désactiver"** sur alarme
2. Dialog s'ouvre

**Vérifications Dialog:**
- ? Titre "Désactiver l'alarme"
- ? Détails alarme affichés (zone, device, timestamp)
- ? Alert warning visible
- ? Champ "Raison de la désactivation" (textarea 3 lignes)
- ? Bouton "Annuler"
- ? Bouton "Confirmer" (désactivé initialement)

**Étapes (suite):**
3. Saisir raison: "Test E2E - Fausse alerte"
4. Bouton "Confirmer" activé
5. Cliquer "Confirmer"

**Vérifications:**
- ? Progress circular affiché pendant traitement
- ? Dialog se ferme après succès
- ? Snackbar succès affiché
- ? Alarme disparaît du widget
- ? Compteur header revient à 0 ou diminue
- ? Si 0 alarmes ? Message "Aucune alarme active"

**Screenshot:** `test-1.3-alarm-deactivated.png`

---

### Test 1.4: Plusieurs Alarmes

**Étapes Backend:**
```bash
# Déclencher 6 alarmes
for i in 1 2 3 4 5 6; do
  curl -X POST https://localhost:7182/api/DeviceStatus/ESP32_TEST_$i/smoke-alarms \
    -H "Authorization: Bearer <token>" \
    -H "Content-Type: application/json" \
    -d "{\"zoneId\": $i}"
done
```

**Vérifications:**
- ? Max 5 alarmes affichées (MaxAlarmsDisplayed=5)
- ? Message "+1 alarme(s) supplémentaire(s)"
- ? Chip compteur "6" dans header
- ? Bouton "Voir toutes les alarmes" visible
- ? Clic bouton ? Navigation `/devices?filter=alarms`

**Screenshot:** `test-1.4-multiple-alarms.png`

---

## ?? TEST SUITE 2: CRITICALLOGS WIDGET

### Test 2.1: Sans Logs Critiques

**Étapes:**
1. Dashboard affiché
2. Localiser **CriticalLogsWidget** (row 2 gauche)

**Vérifications:**
- ? Widget affiché avec header "Logs Critiques (24h)"
- ? Icône BugReport (rouge)
- ? Message "Aucun log critique"
- ? Icône verte CheckCircle
- ? Texte "Aucune erreur critique détectée dans les dernières 24h"

**Screenshot:** `test-2.1-no-critical-logs.png`

---

### Test 2.2: Générer Logs Critiques

**Étapes Backend:**
```http
POST https://localhost:7182/api/DeviceLogs
Authorization: Bearer <token>
Content-Type: application/json

{
  "deviceId": "ESP32_TEST",
  "level": "CRITICAL",
  "source": "TestE2E",
  "message": "Test log critique E2E - Simulation erreur",
  "timestamp": "2025-01-30T17:30:00Z"
}
```

**Répéter 8 fois** (créer 8 logs critiques)

**Vérifications Frontend:**
- ? Widget affiche logs (max 5)
- ? Border-left rouge pour chaque log
- ? Chip niveau "CRITICAL" (rouge) avec icône Cancel
- ? Chip source "TestE2E" (outlined)
- ? Message log affiché
- ? Timestamp HH:mm:ss + dd/MM
- ? Chip compteur "8" dans header (rouge)
- ? Message "+3 log(s) critique(s) supplémentaire(s)"

**Screenshot:** `test-2.2-critical-logs.png`

---

### Test 2.3: Filtrage par Niveau

**Étapes Backend:**
```http
# Ajouter logs ERROR (non-CRITICAL)
POST https://localhost:7182/api/DeviceLogs
...
"level": "ERROR"
```

**Vérifications:**
- ? Logs ERROR également affichés (widget affiche CRITICAL + ERROR)
- ? Border-left rouge pour ERROR
- ? Chip "ERROR" (rouge) avec icône Error
- ? Colorisation cohérente

**Screenshot:** `test-2.3-error-logs.png`

---

### Test 2.4: Navigation

**Étapes:**
1. Cliquer bouton **"Voir tous les logs"**

**Vérifications:**
- ? Navigation vers `/logs?level=critical`
- ? Page Logs filtrée sur critiques

**Screenshot:** `test-2.4-logs-navigation.png`

---

## ?? TEST SUITE 3: DEVICEMETRICS WIDGET

### Test 3.1: Sans Devices

**Étapes:**
1. Dashboard affiché
2. Localiser **DeviceMetricsWidget** (row 2 droite)

**Vérifications:**
- ? Widget affiché avec header "Métriques Devices IoT"
- ? Icône Sensors (bleue)
- ? Message "Aucune métrique disponible"
- ? Icône DevicesOther

**Screenshot:** `test-3.1-no-metrics.png`

---

### Test 3.2: Devices avec Métriques

**Prérequis Backend:**
- Avoir devices actifs avec métriques temps réel
- Si besoin, insérer métriques manuellement:

```http
POST https://localhost:7182/api/DeviceStatus/ESP32_001/metrics
...
{
  "deviceId": "ESP32_001",
  "rssi": -65,
  "heapFree": 125000,
  "uptimeSeconds": 86400,
  "firmwareVersion": "1.2.3"
}
```

**Répéter pour 8 devices** (différents états santé)

**Vérifications:**
- ? **Résumé 3 cards:**
  - Card "Bon état" (vert) avec compteur correct
  - Card "Attention" (orange) avec compteur correct
  - Card "Critique" (rouge) avec compteur correct
- ? **Liste devices:**
  - Max 5 devices affichés
  - Device ID + Firmware version
  - Chip statut santé coloré
- ? **Métriques par device:**
  - Signal WiFi (RSSI dBm + progress bar colorée)
  - Mémoire (KB/MB + progress bar colorée)
  - Uptime formaté lisible
  - Dernière MAJ timestamp

**Validation Colorisation:**
- RSSI < -80 ? Rouge
- RSSI < -70 ? Orange
- RSSI ? -70 ? Vert
- Heap < 50KB ? Rouge
- Heap < 100KB ? Orange
- Heap ? 100KB ? Vert

**Screenshot:** `test-3.2-device-metrics.png`

---

### Test 3.3: Calcul Santé

**Créer scénarios:**
1. **Device Bon:** RSSI -60, Heap 150KB
2. **Device Attention:** RSSI -75, Heap 80KB
3. **Device Critique:** RSSI -85, Heap 40KB

**Vérifications:**
- ? Compteurs résumé corrects
- ? Chips statut appropriés
- ? Colorisation progress bars

**Screenshot:** `test-3.3-health-calculation.png`

---

### Test 3.4: Navigation

**Étapes:**
1. Cliquer **"Voir tous les devices"**

**Vérifications:**
- ? Navigation vers `/devices`
- ? Page Devices affichée

---

## ?? TEST SUITE 4: RECENTACTIVITY WIDGET

### Test 4.1: Sans Activités

**Étapes:**
1. Dashboard affiché
2. Localiser **RecentActivityWidget** (row 1 droite)

**Vérifications:**
- ? Widget affiché avec header "Activité Récente"
- ? Icône History (cyan)
- ? Message "Aucune activité récente"
- ? Icône HourglassEmpty

**Screenshot:** `test-4.1-no-activities.png`

---

### Test 4.2: Générer Activités

**Étapes Backend:**
```http
# Login (génère activité "Login")
POST https://localhost:7182/api/Auth/login
...

# Créer user (génère "Create")
POST https://localhost:7182/api/UserManagement
...

# Envoyer commande (génère "Commande")
POST https://localhost:7182/api/IoTHub/devices/ESP32_001/method
...

# Mettre à jour (génère "Update")
PUT https://localhost:7182/api/UserManagement/{id}
...

# Supprimer (génère "Delete")
DELETE https://localhost:7182/api/UserManagement/{id}
...
```

**Vérifications:**
- ? Timeline verticale affichée
- ? Max 10 activités visibles
- ? **Par activité:**
  - Chip type coloré (Login?Vert, Delete?Rouge, etc.)
  - Chip ressource (Device, User, etc.)
  - Description claire
  - Email utilisateur
  - Timestamp relatif correct:
    - < 1min ? "À l'instant"
    - < 1h ? "Il y a Xmin"
    - < 24h ? "Il y a Xh"
    - < 7j ? "Il y a Xj"
    - > 7j ? "dd/MM HH:mm"

**Screenshot:** `test-4.2-recent-activities.png`

---

### Test 4.3: Colorisation par Type

**Vérifier colorisation:**
- ? **Commande** ? Bleu (Primary) + Icône Send
- ? **Login** ? Vert (Success) + Icône Login
- ? **Create** ? Vert (Success) + Icône Add
- ? **Update** ? Orange (Warning) + Icône Edit
- ? **Delete** ? Rouge (Error) + Icône Delete
- ? **Audit** ? Cyan (Info) + Icône Visibility

**Screenshot:** `test-4.3-activity-colors.png`

---

### Test 4.4: Plus de 10 Activités

**Générer 15 activités**

**Vérifications:**
- ? Max 10 affichées
- ? Chip compteur "15" dans header
- ? Message "+5 activité(s) supplémentaire(s)"
- ? Bouton "Voir tout l'historique" visible

**Étapes Navigation:**
1. Cliquer **"Voir tout l'historique"**

**Vérifications:**
- ? Navigation vers `/audit`
- ? Page Audit affichée avec toutes activités

**Screenshot:** `test-4.4-more-activities.png`

---

## ?? TEST SUITE 5: RESPONSIVE DESIGN

### Test 5.1: Desktop (?960px)

**Résolution:** 1920x1080

**Vérifications:**
- ? Widgets 2x2 grid (50% each)
- ? Row 1: ActiveAlarms (gauche) + RecentActivity (droite)
- ? Row 2: CriticalLogs (gauche) + DeviceMetrics (droite)
- ? Pas d'overflow horizontal
- ? Espacement cohérent (Spacing="3")
- ? Layout 2 colonnes existant (70%/30%) conservé

**Screenshot:** `test-5.1-desktop.png`

---

### Test 5.2: Tablet (600-959px)

**Résolution:** 768x1024

**Vérifications:**
- ? Widgets 2x2 grid conservé
- ? Layout 2 colonnes passe en 1 colonne (100%)
- ? Lisibilité préservée

**Screenshot:** `test-5.2-tablet.png`

---

### Test 5.3: Mobile (<600px)

**Résolution:** 375x667

**Vérifications:**
- ? Widgets stack vertical (100% width each)
- ? Ordre: Alarmes ? Activité ? Logs ? Métriques
- ? Toutes sections lisibles
- ? Pas de scroll horizontal
- ? Boutons accessibles

**Screenshot:** `test-5.3-mobile.png`

---

## ?? TEST SUITE 6: PERFORMANCE

### Test 6.1: Temps Chargement Initial

**Étapes:**
1. Ouvrir Developer Tools (F12)
2. Onglet Network ? Clear
3. Naviguer Dashboard (`/dashboard`)
4. Mesurer temps chargement

**Critères:**
- ? Temps total < 3s
- ? API calls parallèles
- ? Pas d'erreurs console

**Metrics:**
- Time to First Byte (TTFB)
- DOMContentLoaded
- Load event

**Screenshot:** `test-6.1-performance.png`

---

### Test 6.2: Auto-Refresh

**Étapes:**
1. Dashboard affiché
2. Attendre 60s (auto-refresh ActiveAlarms)
3. Observer console

**Vérifications:**
- ? Refresh automatique déclenché
- ? Pas de freeze UI
- ? Timestamp "Mis à jour" change
- ? Pas de memory leaks

**Screenshot:** `test-6.2-auto-refresh.png`

---

### Test 6.3: Memory Usage

**Étapes:**
1. Developer Tools ? Performance Monitor
2. Naviguer Dashboard
3. Rester 5 minutes
4. Observer mémoire

**Critères:**
- ? Mémoire stable (pas de fuite)
- ? CPU < 10% en idle
- ? Dispose() appelé sur navigation

**Screenshot:** `test-6.3-memory.png`

---

## ?? MATRICE TESTS

### Résumé Résultats

| Test Suite | Tests | Status | Notes |
|------------|-------|--------|-------|
| ActiveAlarmsWidget | 4 | ? | Sans alarme, Déclencher, Désactiver, Plusieurs |
| CriticalLogsWidget | 4 | ? | Sans logs, Générer, Filtrage, Navigation |
| DeviceMetricsWidget | 4 | ? | Sans devices, Métriques, Santé, Navigation |
| RecentActivityWidget | 4 | ? | Sans activités, Générer, Colorisation, Plus de 10 |
| Responsive Design | 3 | ? | Desktop, Tablet, Mobile |
| Performance | 3 | ? | Chargement, Auto-refresh, Mémoire |

**Total:** 22 tests E2E

---

## ?? CHECKLIST VALIDATION

### Avant Tests
- [ ] Backend running (`https://localhost:7182`)
- [ ] Frontend running (`https://localhost:7000`)
- [ ] Compte admin créé
- [ ] Devices actifs dans backend
- [ ] Developer Tools prêt

### Pendant Tests
- [ ] Capturer screenshots
- [ ] Noter erreurs console
- [ ] Mesurer temps chargement
- [ ] Vérifier responsive

### Après Tests
- [ ] Tous widgets fonctionnels
- [ ] Build sans erreurs
- [ ] Performance acceptable (< 3s)
- [ ] Pas de memory leaks
- [ ] Rapport tests rédigé

---

## ?? RAPPORT TESTS (Template)

### Exécution

**Date:** YYYY-MM-DD  
**Environnement:** Dev/Staging/Prod  
**Testeur:** Nom

### Résultats

| Test Suite | Pass | Fail | Skip | Notes |
|------------|------|------|------|-------|
| ActiveAlarms | X/4 | X/4 | X/4 | ... |
| CriticalLogs | X/4 | X/4 | X/4 | ... |
| DeviceMetrics | X/4 | X/4 | X/4 | ... |
| RecentActivity | X/4 | X/4 | X/4 | ... |
| Responsive | X/3 | X/3 | X/3 | ... |
| Performance | X/3 | X/3 | X/3 | ... |

**Total:** X/22 ?

### Issues Détectées

1. **Issue #1**
   - **Sévérité:** Critique/Majeure/Mineure
   - **Description:** ...
   - **Screenshot:** `issue-1.png`
   - **Steps to Reproduce:** ...

### Recommandations

1. ...
2. ...

---

## ?? AUTOMATISATION (Futur)

### Playwright Tests (C#)

```csharp
[Test]
public async Task ActiveAlarmsWidget_NoAlarms_ShowsEmptyState()
{
    // Navigate to dashboard
    await Page.GotoAsync("https://localhost:7000/dashboard");
    
    // Wait for widget
    var widget = Page.Locator(".active-alarms-widget");
    await widget.WaitForAsync();
    
    // Verify empty state
    await Expect(widget.Locator("text=Aucune alarme active")).ToBeVisibleAsync();
    await Expect(widget.Locator(".mud-icon-success")).ToBeVisibleAsync();
}

[Test]
public async Task ActiveAlarmsWidget_TriggerAlarm_ShowsAlarm()
{
    // Trigger alarm via API
    await TriggerAlarmAsync("ESP32_TEST", zoneId: 1);
    
    // Navigate to dashboard
    await Page.GotoAsync("https://localhost:7000/dashboard");
    
    // Wait for alarm to appear
    await Page.Locator("text=ESP32_TEST").WaitForAsync();
    
    // Verify alarm card
    await Expect(Page.Locator("text=Désactiver")).ToBeVisibleAsync();
}
```

---

## ?? RESSOURCES

### Documentation
- [PHASE_3.5_VALIDATION_COMPLETE.md](./PHASE_3.5_VALIDATION_COMPLETE.md)
- [INTEGRATION_WIDGETS_DASHBOARD.md](./INTEGRATION_WIDGETS_DASHBOARD.md)
- [API_REFERENCE_COMPLETE.md](./API_REFERENCE_COMPLETE.md)

### Screenshots Location
```
MySmartApp.Ui/TestResults/E2E/Screenshots/
```

### Logs
```
MySmartApp.Ui/TestResults/E2E/Logs/
```

---

**Dernière mise à jour:** 2025-01-30 17:45 UTC  
**Auteur:** GitHub Copilot Workspace Agent  
**Version:** 1.0  
**Status:** ? GUIDE E2E READY
