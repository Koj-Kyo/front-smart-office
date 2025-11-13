# ?? GUIDE DE TEST - DASHBOARD BACKEND INTÉGRÉ

## ? **INTÉGRATION COMPLÈTE TERMINÉE**

### **Modifications réalisées:**

1. **? DTOs Dashboard ajoutés** (`ApiModels.cs`)
   - `DashboardStats` - Statistiques KPI
   - `EnergyDataPoint` - Points données énergétiques  
   - `DeviceActivityData` - Activité appareils
   - `CurrentUserDto` - Profil utilisateur enrichi
   - `DashboardApiResponse<T>` - Wrapper réponses
   - `CommandeResult` - Résultat commande

2. **? Services mis à jour**
   - `DashboardStatsService` ? Appels directs endpoints backend
   - `AuthService` ? Ajout `GetCurrentUserIdAsync()`
   - `CommandeService` ? Retour `CommandeResult` + endpoint corrigé
   - `IncidentTicketService` ? Utilisation vrais endpoints + AuthService

3. **? Configuration corrigée**
   - `appsettings.json` ? URL backend `http://localhost:5210`
   - Clé API OpenWeather ajoutée

---

## ?? **TESTS À EFFECTUER**

### **Étape 1: Démarrer le backend**

```bash
cd SmartOfficeAPP
dotnet run
```

**? Vérifier:** `Now listening on: http://localhost:5210`

### **Étape 2: Démarrer le frontend**

```bash
cd MySmartApp.Ui
dotnet run
```

**? Noter l'URL** (ex: `http://localhost:5088`)

### **Étape 3: Tests fonctionnels**

#### **A. Connexion**
1. Aller sur `http://localhost:5088`
2. Se connecter avec `admin@example.com` / `Admin@123`
3. **? Vérifier:** Redirection vers dashboard

#### **B. Header Dashboard** 
1. **? Vérifier:** "Bonjour, {FirstName}!" s'affiche
2. **? Vérifier:** Météo Lomé s'affiche
3. **? Vérifier:** Illustration visible

#### **C. KPIs (4 cartes)**
1. **? Vérifier:** Valeurs non nulles dans les 4 cartes
   - Appareils Actifs: X/Y
   - Utilisateurs Actifs: X/Y  
   - Tickets Ouverts: X/Y
   - Consommation: X.X kWh
2. **? Vérifier:** Badges de changement avec % et couleurs

#### **D. Graphique énergétique**
1. **? Vérifier:** Graphique ligne visible
2. **? Tester:** Changement période (Mensuel/Hebdomadaire/Quotidien)
3. **? Vérifier:** Labels et données changent

#### **E. Graphique activité (donut)**
1. **? Vérifier:** Donut visible avec couleurs
2. **? Vérifier:** Légendes types appareils
3. **? Tester:** Bouton actualiser

#### **F. Commandes IoT (DeviceCards)**
1. **? Tester:** Toggle DeviceCard (ex: Climatisation)
2. **? Vérifier:** Snackbar vert "Commande envoyée avec succès"
3. **? Vérifier:** Log console navigateur (F12)

#### **G. Widgets colonne droite**
1. **? Mes Appareils:** DeviceCards fonctionnent
2. **? Télémétrie:** Widget se charge (même vide)
3. **? Mes Tickets:** Liste ou "Aucun ticket ouvert"
4. **? Notifications:** Liste ou "Aucune notification"
5. **? Zones:** Liste zones ou message par défaut

---

## ?? **VÉRIFICATIONS TECHNIQUES (F12)**

### **Console Navigateur**
**Logs attendus:**
```
?? Récupération des statistiques du Dashboard depuis le backend
? Statistiques récupérées: 12 devices actifs, 8 users actifs
? Récupération des données énergétiques pour la période: month
? 30 points de données énergétiques récupérés
?? Récupération de l'activité des appareils
? 4 types d'appareils récupérés
```

### **Network Tab (F12)**
**Requêtes attendues:**

#### **GET /api/User/profile**
- Status: 200 OK
- Headers: Authorization: Bearer {token}
- Response: `{ "id": "...", "firstName": "...", "roles": [...] }`

#### **GET /api/Dashboard/stats**  
- Status: 200 OK
- Response: `{ "data": { "activeDevices": 12, ... }, "timestamp": "..." }`

#### **GET /api/Dashboard/energy?period=month**
- Status: 200 OK
- Response: `{ "data": [{ "label": "Jan", "value": 45.2 }], ... }`

#### **GET /api/Dashboard/device-activity**
- Status: 200 OK  
- Response: `{ "data": [{ "deviceType": "Climatisation", "count": 5 }], ... }`

#### **POST /api/Commande**
- Status: 200 OK
- Request: `{ "peripheriqueLogiqueId": 1, "nomCommande": "TurnOn" }`
- Response: `{ "isSuccess": true, "message": "...", "commandeId": 42 }`

---

## ?? **DÉPANNAGE**

### **Problème 1: "? Erreur réseau"**
**Cause:** Backend non démarré  
**Solution:** Vérifier `http://localhost:5210/swagger`

### **Problème 2: "?? Aucun token disponible"**
**Cause:** Session expirée  
**Solution:** Se reconnecter via `/login`

### **Problème 3: Données simulées affichées**
**Cause:** Endpoints backend retournent vide  
**Solution:** Normal, les services ont des **fallbacks automatiques**

### **Problème 4: Erreur CORS**
**Cause:** Backend refuse requêtes depuis frontend  
**Solution:** Vérifier configuration CORS backend

---

## ?? **ENDPOINTS BACKEND UTILISÉS**

| **Endpoint** | **Méthode** | **Service Frontend** |
|-------------|------------|---------------------|
| `/api/User/profile` | GET | `AuthService.GetCurrentUserAsync()` |
| `/api/Dashboard/stats` | GET | `DashboardStatsService.GetDashboardStatsAsync()` |
| `/api/Dashboard/energy` | GET | `DashboardStatsService.GetEnergyConsumptionDataAsync()` |
| `/api/Dashboard/device-activity` | GET | `DashboardStatsService.GetDeviceActivityDataAsync()` |
| `/api/Commande` | POST | `CommandeService.SendCommandAsync()` |
| `/api/IncidentTickets/my-tickets` | GET | `IncidentTicketService.GetMyTicketsAsync()` |
| `/api/Notification` | GET | `NotificationService.GetUserNotificationsAsync()` |
| `/api/Zones` | GET | `BackendZoneService.GetAllZonesAsync()` |
| `/api/Telemetries/history` | GET | `TelemetrieService.GetHistoricalDataAsync()` |

---

## ? **CHECKLIST DE VALIDATION**

### **Backend**
- [ ] Backend démarre sur port 5210
- [ ] Swagger accessible: `http://localhost:5210/swagger`  
- [ ] Endpoints Dashboard répondent
- [ ] Authentification JWT fonctionne

### **Frontend**
- [ ] Frontend démarre sans erreur
- [ ] Page Dashboard charge complètement
- [ ] 4 KPIs affichent des valeurs
- [ ] Graphiques visibles (ligne + donut)
- [ ] Toggle DeviceCard fonctionne  
- [ ] Snackbar succès apparaît
- [ ] Profil utilisateur s'affiche
- [ ] Widgets colonne droite se chargent

### **Intégration**
- [ ] Aucune erreur console rouge
- [ ] Requêtes HTTP 200 dans Network
- [ ] Logs avec emojis dans console
- [ ] Fallbacks activés si données vides
- [ ] JWT envoyé automatiquement

---

## ?? **RÉSULTATS ATTENDUS**

### **Succès complet** ?
- Dashboard charge en ~2-3 secondes
- Toutes les sections affichent des données  
- Commandes IoT fonctionnent
- Aucune erreur critique

### **Succès partiel** ?
- Dashboard charge avec données simulées
- Certains widgets vides (normal si DB vide)
- Commandes IoT échouent (périphériques inexistants)

### **Échec** ?
- Page blanche ou erreur 500
- "? Erreur réseau" répétés
- Token invalide permanent

---

**Temps de test estimé:** ?? 10-15 minutes  
**Difficulté:** ????? Facile  
**Support:** Console navigateur (F12) + logs backend