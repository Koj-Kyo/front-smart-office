# ?? VALIDATION RAPIDE - ENDPOINTS DASHBOARD

## ?? **Base URL Backend**
```
http://localhost:5210
```

## ?? **1. Authentification (obtenir token)**

```http
POST http://localhost:5210/api/Auth/login
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "Admin@123"
}
```

**Réponse attendue:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "...",
  "userId": "user-guid"
}
```

## ?? **2. Profil utilisateur**

```http
GET http://localhost:5210/api/User/profile
Authorization: Bearer {accessToken}
```

**Réponse attendue:**
```json
{
  "id": "user-guid",
  "email": "admin@example.com",
  "fullName": "Admin User",
  "firstName": "Admin",
  "lastName": "User",
  "roles": ["Admin"]
}
```

## ?? **3. Statistiques KPI Dashboard**

```http
GET http://localhost:5210/api/Dashboard/stats
Authorization: Bearer {accessToken}
```

**Réponse attendue:**
```json
{
  "data": {
    "activeDevices": 12,
    "totalDevices": 15,
    "devicesChangePercent": 5.2,
    "activeUsers": 8,
    "totalUsers": 10,
    "usersChangePercent": 3.1,
    "openIncidents": 3,
    "totalIncidents": 25,
    "incidentsChangePercent": -2.4,
    "energyConsumed": 1245.5,
    "energyChangePercent": -8.5
  },
  "source": "Database",
  "timestamp": "2025-01-13T..."
}
```

## ? **4. Données énergétiques**

```http
GET http://localhost:5210/api/Dashboard/energy?period=month
Authorization: Bearer {accessToken}
```

**Réponse attendue:**
```json
{
  "data": [
    { "label": "Jan", "value": 45.2 },
    { "label": "Fév", "value": 48.7 },
    { "label": "Mar", "value": 42.1 }
  ],
  "source": "Database",
  "timestamp": "2025-01-13T..."
}
```

## ?? **5. Activité des appareils**

```http
GET http://localhost:5210/api/Dashboard/device-activity
Authorization: Bearer {accessToken}
```

**Réponse attendue:**
```json
{
  "data": [
    { "deviceType": "Climatisation", "count": 5 },
    { "deviceType": "Éclairage", "count": 8 },
    { "deviceType": "Capteur", "count": 2 }
  ],
  "source": "Database",
  "timestamp": "2025-01-13T..."
}
```

## ??? **6. Commande IoT**

```http
POST http://localhost:5210/api/Commande
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "peripheriqueLogiqueId": 1,
"nomCommande": "TurnOn",
  "valeur": null
}
```

**Réponse attendue:**
```json
{
  "isSuccess": true,
  "message": "Commande envoyée avec succès",
  "commandeId": 42
}
```

## ?? **7. Mes tickets**

```http
GET http://localhost:5210/api/IncidentTickets/my-tickets
Authorization: Bearer {accessToken}
```

**Réponse attendue:**
```json
[
  {
 "id": 1,
    "sujet": "Problème climatisation",
    "description": "La climatisation ne fonctionne plus",
    "statut": "Ouvert",
    "createdAt": "2025-01-13T10:00:00Z"
  }
]
```

## ?? **8. Notifications**

```http
GET http://localhost:5210/api/Notification
Authorization: Bearer {accessToken}
```

**Réponse attendue:**
```json
[
  {
    "id": 1,
    "userId": "user-guid",
    "titre": "Alerte température",
    "message": "Température élevée détectée",
    "horodatage": "2025-01-13T14:20:00Z",
    "estLue": false
  }
]
```

## ?? **9. Zones**

```http
GET http://localhost:5210/api/Zones
Authorization: Bearer {accessToken}
```

**Réponse attendue:**
```json
[
  {
 "id": 1,
    "nom": "Bureau Principal"
  },
  {
    "id": 2,
    "nom": "Salle de Réunion"
  }
]
```

## ?? **10. Télémétrie**

```http
GET http://localhost:5210/api/Telemetries/history?startDate=2025-01-12T00:00:00Z&endDate=2025-01-13T23:59:59Z
Authorization: Bearer {accessToken}
```

**Réponse attendue:**
```json
[
  {
    "id": 1,
    "peripheriqueLogiqueId": 1,
    "horodatage": "2025-01-13T14:25:00Z",
 "typeEvenement": "temperature",
    "valeur": 23.5,
    "peripheriqueName": "Capteur Bureau"
  }
]
```

---

## ? **VALIDATION RAPIDE (5 min)**

### **Option 1: VS Code REST Client**
1. Installer extension "REST Client"
2. Copier-coller les requêtes ci-dessus
3. Cliquer "Send Request" pour chaque

### **Option 2: Postman**
1. Importer collection depuis ces requêtes
2. Définir variable `{{token}}` avec accessToken
3. Exécuter toutes les requêtes

### **Option 3: Frontend Dashboard**
1. Démarrer backend + frontend
2. Se connecter et aller sur `/dashboard`
3. Ouvrir F12 ? Network
4. Actualiser la page
5. Vérifier que toutes les requêtes sont 200 OK

---

## ?? **Codes d'erreur attendus**

| Code | Signification | Action |
|------|---------------|--------|
| 200 | ? Succès | Parfait |
| 401 | ? Token invalide | Se reconnecter |
| 404 | ?? Endpoint introuvable | Vérifier URL backend |
| 500 | ? Erreur serveur | Vérifier logs backend |

---

## ?? **Checklist validation**

- [ ] **Login** ? Token JWT obtenu
- [ ] **Profile** ? FirstName, LastName, Roles présents
- [ ] **Stats** ? 8 valeurs KPI retournées
- [ ] **Energy** ? Array de points avec label/value
- [ ] **Activity** ? Array types appareils avec count
- [ ] **Commande** ? isSuccess: true
- [ ] **Tickets** ? Array (vide ou avec données)
- [ ] **Notifications** ? Array (vide ou avec données)  
- [ ] **Zones** ? Array (vide ou avec données)
- [ ] **Télémétrie** ? Array (vide ou avec données)

**Si 5+ endpoints répondent correctement:** ? **Intégration réussie**
**Si <5 endpoints répondent:** ?? **Problème configuration/backend**