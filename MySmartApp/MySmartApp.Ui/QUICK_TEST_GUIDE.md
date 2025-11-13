# ?? Guide de Test Rapide - Front Blazor WASM ?? Backend

**Date**: 2025-01-28  
**Prérequis**: Backend ASP.NET Core 9 en cours d'exécution sur `https://localhost:7144`

---

## ?? **Démarrage**

### **1. Démarrer le Backend**
```bash
cd SmartOfficeAPP
dotnet run
# Vérifier: https://localhost:7144/swagger
```

### **2. Démarrer le Front Blazor WASM**
```bash
cd MySmartApp.Ui
dotnet run
# Naviguer vers: https://localhost:7152 ou http://localhost:5249
```

---

## ? **Tests Prioritaires**

### **Phase 1: Authentification**

#### **Test 1.1 - Login Admin**
1. Aller sur `/login`
2. Entrer:
   - Email: `admin@smartoffice.com`
   - Password: `Admin123!`
3. ? **Attendu**: Token JWT stocké, redirection vers `/dashboard`
4. Vérifier dans **DevTools > Application > LocalStorage**:
   - `authToken` présent
   - Format JWT valide

#### **Test 1.2 - Profile Utilisateur**
1. Cliquer sur le bouton profil (en haut à droite)
2. ? **Attendu**: Affichage des infos utilisateur
   - `FullName`: "Administrateur"
   - `Email`: "admin@smartoffice.com"
   - `Roles`: ["Admin"]

---

### **Phase 2: Dashboard**

#### **Test 2.1 - KPIs Dashboard**
1. Aller sur `/dashboard`
2. ? **Attendu**: Affichage des cartes KPI
   - **Active Devices** / Total Devices
   - **Active Users** / Total Users
   - **Open Incidents** / Total Incidents
   - **Energy Consumed** (en Wh)

#### **Test 2.2 - Graphique Énergie**
1. Sur `/dashboard`, vérifier le graphique de consommation
2. Tester les filtres:
   - Période: `day`, `week`, `month`
3. ? **Attendu**: Mise à jour du graphique avec les bonnes données

#### **Test 2.3 - Device Activity Chart**
1. Vérifier le graphique "Activité des Appareils"
2. ? **Attendu**: Groupement par type (relay, sensor, etc.)

---

### **Phase 3: Gestion Zones**

#### **Test 3.1 - Liste des Zones**
1. Aller sur `/zone`
2. ? **Attendu**: Affichage de toutes les zones
   - Nom, Description, Bâtiment, Étage, Capacité

#### **Test 3.2 - Créer une Zone**
1. Cliquer sur "Ajouter une zone"
2. Remplir:
   - Nom: "Bureau Test"
   - Description: "Zone de test"
   - Bâtiment: "A"
   - Étage: 1
   - Capacité: 10
   - Superficie: 25.5
3. Cliquer sur "Créer"
4. ? **Attendu**: Zone ajoutée dans la liste

#### **Test 3.3 - Modifier une Zone**
1. Cliquer sur l'icône "Modifier" d'une zone
2. Modifier le nom: "Bureau Test Modifié"
3. Cliquer sur "Enregistrer"
4. ? **Attendu**: Nom mis à jour dans la liste

#### **Test 3.4 - Supprimer une Zone**
1. Cliquer sur l'icône "Supprimer"
2. Confirmer la suppression
3. ? **Attendu**: Zone supprimée de la liste

---

### **Phase 4: Gestion Devices**

#### **Test 4.1 - Liste des Devices**
1. Aller sur `/devices`
2. ? **Attendu**: Affichage des périphériques IoT
   - Nom, Type, IoT Device ID, Zone
   - Statut de connexion (si disponible)

#### **Test 4.2 - Synchronisation IoT Hub**
1. Cliquer sur "Synchroniser depuis IoT Hub"
2. ? **Attendu**: Message de succès avec le nombre de devices synchronisés

#### **Test 4.3 - Ajouter un Périphérique Physique**
1. Cliquer sur "Ajouter périphérique" d'un device
2. Remplir:
   - Nom: "relay1"
   - Type: "relay"
   - GPIO: 33
   - Notes: "Test relay"
3. Cliquer sur "Ajouter"
4. ? **Attendu**: Périphérique ajouté dans la liste

#### **Test 4.4 - Commandes Périphérique**
1. Dans la liste des périphériques physiques
2. Cliquer sur "Allumer" (icône power)
3. ? **Attendu**: État du périphérique mis à jour après 1s

---

### **Phase 5: Télémétrie**

#### **Test 5.1 - Liste Télémétrie**
1. Aller sur `/telemetrie`
2. Sélectionner un périphérique dans le filtre
3. ? **Attendu**: Affichage des données de télémétrie
   - Type Événement, Valeur, Unité, Horodatage

#### **Test 5.2 - Filtre de Dates**
1. Sélectionner une plage de dates
2. Cliquer sur "Filtrer"
3. ? **Attendu**: Données filtrées par dates

#### **Test 5.3 - Bypass Cache (?fresh=true)**
1. Ouvrir DevTools > Network
2. Cocher "Force refresh data" (si disponible)
3. ? **Attendu**: Requête avec `?fresh=true` dans l'URL

---

### **Phase 6: Incidents**

#### **Test 6.1 - Liste des Incidents**
1. Aller sur `/incidents`
2. ? **Attendu**: Affichage de tous les tickets
   - Titre, Statut, Priorité, Date création

#### **Test 6.2 - Créer un Incident**
1. Cliquer sur "Créer un ticket"
2. Remplir:
   - Titre: "Test Incident"
   - Description: "Description du test"
   - Priorité: "Moyenne"
   - Catégorie: "Électrique"
3. Cliquer sur "Créer"
4. ? **Attendu**: Ticket ajouté dans la liste

#### **Test 6.3 - Changer le Statut**
1. Cliquer sur un ticket
2. Changer le statut: "Ouvert" ? "En cours"
3. ? **Attendu**: Statut mis à jour

---

### **Phase 7: Gestion Utilisateurs (Admin)**

#### **Test 7.1 - Liste des Utilisateurs**
1. Aller sur `/users`
2. ? **Attendu**: Affichage de tous les utilisateurs
   - UserName, Email, Rôles, IsActive

#### **Test 7.2 - Créer un Utilisateur**
1. Cliquer sur "Ajouter un utilisateur"
2. Remplir:
   - UserName: "testuser"
   - Email: "test@smartoffice.com"
   - FullName: "Test User"
   - Password: "Test123!"
   - Roles: ["User"]
3. Cliquer sur "Créer"
4. ? **Attendu**: Utilisateur ajouté dans la liste

#### **Test 7.3 - Modifier les Rôles**
1. Cliquer sur "Modifier" pour un utilisateur
2. Changer les rôles: ["User"] ? ["User", "Manager"]
3. Cliquer sur "Enregistrer"
4. ? **Attendu**: Rôles mis à jour

#### **Test 7.4 - Désactiver un Utilisateur**
1. Cliquer sur "Désactiver" pour un utilisateur
2. ? **Attendu**: `IsActive = false`, utilisateur grisé

---

### **Phase 8: Droits d'Accès**

#### **Test 8.1 - Liste des Droits**
1. Aller sur `/access`
2. ? **Attendu**: Affichage de tous les droits d'accès
   - User, Zone/Device, Type (Lecture/Controle/Administration)

#### **Test 8.2 - Affecter une Zone à un Utilisateur**
1. Cliquer sur "Affecter zone"
2. Sélectionner:
   - Utilisateur: "testuser"
   - Zone: "Bureau Test"
   - Type: "Controle" (2)
3. Cliquer sur "Affecter"
4. ? **Attendu**: Droit ajouté dans la liste

#### **Test 8.3 - Affecter un Device à un Utilisateur**
1. Cliquer sur "Affecter device"
2. Sélectionner:
   - Utilisateur: "testuser"
   - Device: périphérique de la liste
   - Type: "Lecture" (1)
3. Cliquer sur "Affecter"
4. ? **Attendu**: Droit ajouté dans la liste

---

### **Phase 9: Analytics & Consommation**

#### **Test 9.1 - Consommation par Périphérique**
1. Aller sur `/analytics`
2. Sélectionner un périphérique
3. Définir une plage de dates
4. Cliquer sur "Afficher stats"
5. ? **Attendu**: Affichage des statistiques
   - Consommation totale (kWh)
   - Consommation moyenne
   - Nombre de mesures

#### **Test 9.2 - Consommation Horaire**
1. Dans Analytics, cliquer sur "Voir horaire"
2. ? **Attendu**: Graphique des consommations par heure

#### **Test 9.3 - Top Consommateurs**
1. Cliquer sur "Top consommateurs"
2. ? **Attendu**: Liste des périphériques triés par consommation décroissante

---

### **Phase 10: Notifications**

#### **Test 10.1 - Liste des Notifications**
1. Aller sur `/notifications`
2. ? **Attendu**: Affichage de toutes les notifications
   - Titre, Message, Type (Info/Warning/Error), IsRead

#### **Test 10.2 - Marquer comme Lu**
1. Cliquer sur une notification non lue
2. ? **Attendu**: `IsRead = true`, notification grisée

---

### **Phase 11: SignalR (Temps Réel)**

#### **Test 11.1 - Connexion SignalR**
1. Ouvrir DevTools > Console
2. Vérifier les logs:
   - `? Connected to NotificationHub`
3. ? **Attendu**: Connexion établie sans erreur

#### **Test 11.2 - Réception Notification**
1. Via Postman ou Swagger, envoyer:
   ```http
   POST https://localhost:7144/api/SignalRTest/test-user-notification
   {
     "userId": "{votre_userId}",
     "title": "Test SignalR",
     "message": "Message de test temps réel"
   }
   ```
2. ? **Attendu**: Notification apparaît en haut à droite (toast/snackbar)

#### **Test 11.3 - Mise à Jour Device Status**
1. Via Swagger, déclencher une commande device
2. ? **Attendu**: Badge notification mis à jour en temps réel

---

## ?? **Troubleshooting**

### **Erreur 401 Unauthorized**
```
? Solution:
- Vérifier que le token JWT est bien stocké dans LocalStorage
- Se reconnecter si le token a expiré
- Vérifier les headers Authorization dans DevTools > Network
```

### **CORS Error**
```
Access to fetch at 'https://localhost:7144/api/...' from origin 'https://localhost:7152' 
has been blocked by CORS policy

? Solution Backend (Program.cs):
builder.Services.AddCors(opt => 
    opt.AddPolicy("AllowFrontend", p => p
        .WithOrigins("https://localhost:7152", "http://localhost:5249")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

app.UseCors("AllowFrontend");
```

### **SignalR Not Connecting**
```
Error: Failed to complete negotiation with the server

? Solution:
1. Vérifier que le hub est bien à /notificationhub
2. Vérifier l'authentification JWT:
   - Passer le token via accessTokenFactory
   - Ou via query string ?access_token={token}
3. Vérifier les logs backend pour les erreurs de négociation
```

### **Dates Mal Formatées**
```
Cannot deserialize DateTime

? Solution:
- Vérifier que FlexibleDateTimeConverter est bien enregistré
- Le backend envoie des dates en ISO 8601 UTC
- Exemple: "2025-01-28T10:30:00Z"
```

---

## ?? **Vérification des Endpoints**

### **Avec DevTools > Network**
Vérifier pour chaque appel:
1. ? **Request Headers**:
   - `Authorization: Bearer {token}`
   - `Content-Type: application/json`
2. ? **Response**:
   - Status: `200 OK`, `201 Created`, `204 No Content`
   - Body: Format JSON camelCase
3. ? **Cache**:
   - Présence du paramètre `?fresh=true` si forcé

### **Avec Swagger Backend**
1. Aller sur `https://localhost:7144/swagger`
2. Cliquer sur "Authorize" ??
3. Entrer: `Bearer {votre_token_jwt}`
4. Tester les endpoints directement

---

## ?? **Checklist Complète**

### **Authentification**
- [ ] Login Admin
- [ ] Register User
- [ ] Profil Utilisateur
- [ ] Logout

### **Dashboard**
- [ ] KPIs (Devices, Users, Incidents, Energy)
- [ ] Graphique Énergie (day/week/month)
- [ ] Device Activity Chart

### **Zones**
- [ ] Liste Zones
- [ ] Créer Zone
- [ ] Modifier Zone
- [ ] Supprimer Zone

### **Devices**
- [ ] Liste Devices
- [ ] Sync IoT Hub
- [ ] Ajouter Périphérique Physique
- [ ] Commandes Périphérique
- [ ] Voir Détails (Merged Twin)

### **Télémétrie**
- [ ] Liste Télémétrie
- [ ] Filtres (Date, Périphérique)
- [ ] Support ?fresh=true

### **Incidents**
- [ ] Liste Incidents
- [ ] Créer Incident
- [ ] Changer Statut
- [ ] Assigner Incident

### **Users (Admin)**
- [ ] Liste Utilisateurs
- [ ] Créer Utilisateur
- [ ] Modifier Rôles
- [ ] Désactiver Utilisateur

### **Droits d'Accès**
- [ ] Liste Droits
- [ ] Affecter Zone à User
- [ ] Affecter Device à User

### **Analytics**
- [ ] Stats Consommation
- [ ] Consommation Horaire
- [ ] Tendances
- [ ] Anomalies
- [ ] Top Consommateurs

### **Notifications**
- [ ] Liste Notifications
- [ ] Marquer comme Lu
- [ ] Badge temps réel

### **SignalR**
- [ ] Connexion Hub
- [ ] Réception Notifications
- [ ] Mises à Jour Temps Réel

---

## ?? **Résumé des Tests Critiques**

### **Must-Have (P0)**
1. ? Login/Logout
2. ? Dashboard KPIs
3. ? Liste Zones
4. ? Liste Devices
5. ? Liste Télémétrie
6. ? SignalR Connexion

### **Should-Have (P1)**
7. ? CRUD Zones
8. ? CRUD Devices
9. ? CRUD Incidents
10. ? CRUD Users (Admin)
11. ? Commandes Périphériques

### **Nice-to-Have (P2)**
12. ? Analytics Avancées
13. ? Droits d'Accès Granulaires
14. ? Notifications Temps Réel
15. ? Consommation Énergétique

---

## ?? **Next Steps**

1. ? Exécuter les tests **Phase 1 à Phase 6** (prioritaires)
2. ?? Corriger les bugs bloquants
3. ? Tests SignalR (Phase 11)
4. ?? Tests de charge (Postman/k6)
5. ?? Polish UI/UX (animations, messages d'erreur)

---

**Bon testing ! ??**
