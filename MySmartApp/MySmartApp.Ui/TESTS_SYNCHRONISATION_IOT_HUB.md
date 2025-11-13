# ?? Checklist de test - Synchronisation IoT Hub

## ? Tests de build
- [x] Compilation réussie
- [x] Pas d'erreurs Razor
- [x] Pas d'erreurs TypeScript
- [x] DTOs ajoutés correctement

## ?? Tests fonctionnels à effectuer

### 1. Interface utilisateur
- [ ] Le bouton "Synchroniser depuis IoT Hub" est visible
- [ ] Le bouton est positionné à côté du bouton "Ajouter un périphérique IoT"
- [ ] Le bouton a l'icône `CloudSync`
- [ ] Le bouton a la couleur Secondary (violet)

### 2. États du bouton
- [ ] État normal: Texte "Synchroniser depuis IoT Hub"
- [ ] État en cours: Spinner visible + texte "Synchronisation..."
- [ ] État désactivé pendant le chargement de la page
- [ ] État désactivé pendant la synchronisation

### 3. Fonctionnement de la synchronisation
- [ ] Clic sur le bouton démarre la synchronisation
- [ ] Notification "Synchronisation en cours..." s'affiche (Info)
- [ ] Appel API POST /api/IoTHub/devices/sync effectué
- [ ] Token Bearer envoyé dans l'en-tête Authorization

### 4. Résultats de synchronisation

#### Cas de succès complet (aucun échec)
- [ ] Notification de succès affichée (Success - vert)
- [ ] Message: "Synchronisation terminée: X créé(s), Y mis à jour, 0 échec(s)"
- [ ] Les nouveaux devices apparaissent dans la liste
- [ ] Les devices mis à jour affichent les nouvelles informations

#### Cas de succès partiel (avec échecs)
- [ ] Notification Warning affichée (orange)
- [ ] Message: "Synchronisation terminée: X créé(s), Y mis à jour, Z échec(s)"
- [ ] Les devices réussis sont visibles
- [ ] Les IDs des devices échoués sont loggués

#### Cas d'erreur complète
- [ ] Notification d'erreur affichée (Error - rouge)
- [ ] Message: "Erreur lors de la synchronisation: [message d'erreur]"
- [ ] Le bouton redevient actif après l'erreur
- [ ] Aucune modification n'est appliquée

### 5. Rechargement des données
- [ ] LoadData() est appelé après synchronisation réussie
- [ ] Les statuts de connexion IoT Hub sont rafraîchis
- [ ] Le cache des statuts de connexion est invalidé
- [ ] Les périphériques de chaque device sont rechargés

### 6. Logs
- [ ] Log au début: "Démarrage de la synchronisation..."
- [ ] Log en cas de succès avec statistiques
- [ ] Log en cas d'erreur avec détails
- [ ] Logs visibles dans la console du navigateur

## ?? Tests de sécurité

### Authentification
- [ ] Utilisateur non authentifié: Redirection vers login
- [ ] Token expiré: Message d'erreur approprié
- [ ] Token invalide: Message d'erreur 401

### Autorisation
- [ ] Utilisateur admin: Synchronisation autorisée
- [ ] Utilisateur standard: Vérifier les permissions backend
- [ ] Erreur 403 si permissions insuffisantes

## ?? Tests de performance

### Charge légère (1-10 devices)
- [ ] Synchronisation en moins de 5 secondes
- [ ] Interface reste réactive
- [ ] Pas de freeze de l'UI

### Charge moyenne (10-50 devices)
- [ ] Synchronisation en moins de 15 secondes
- [ ] Spinner visible pendant tout le processus
- [ ] Notification de résultat affichée

### Charge importante (50+ devices)
- [ ] Synchronisation possible (peut prendre du temps)
- [ ] Interface reste réactive
- [ ] Timeout géré correctement

## ?? Tests de scénarios

### Scénario 1: Premier déploiement
**Setup**: Base de données vide, 10 devices dans IoT Hub
- [ ] Tous les devices sont créés (CreatedCount = 10)
- [ ] UpdatedCount = 0
- [ ] FailedCount = 0

### Scénario 2: Ajout de nouveaux devices
**Setup**: 5 devices en BDD, 10 devices dans IoT Hub
- [ ] 5 nouveaux devices créés (CreatedCount = 5)
- [ ] 5 devices existants mis à jour (UpdatedCount = 5)
- [ ] FailedCount = 0

### Scénario 3: Mise à jour uniquement
**Setup**: 10 devices en BDD, 10 devices dans IoT Hub (avec modifications)
- [ ] CreatedCount = 0
- [ ] UpdatedCount = 10
- [ ] FailedCount = 0

### Scénario 4: Devices problématiques
**Setup**: 10 devices dans IoT Hub dont 2 mal formés
- [ ] 8 devices synchronisés avec succès
- [ ] FailedCount = 2
- [ ] FailedDeviceIds contient les 2 IDs problématiques
- [ ] Notification Warning affichée

### Scénario 5: IoT Hub vide
**Setup**: Base de données avec devices, IoT Hub vide
- [ ] TotalDevices = 0
- [ ] CreatedCount = 0
- [ ] UpdatedCount = 0
- [ ] Notification appropriée

### Scénario 6: Erreur de connexion IoT Hub
**Setup**: IoT Hub indisponible ou credentials invalides
- [ ] Message d'erreur explicite
- [ ] Pas de modification en BDD
- [ ] Bouton redevient actif

## ?? Tests d'ergonomie

### Responsive design
- [ ] Bouton visible sur desktop
- [ ] Bouton visible sur tablette
- [ ] Bouton visible sur mobile (peut nécessiter scroll)

### Accessibilité
- [ ] Texte du bouton lisible
- [ ] Icône visible et compréhensible
- [ ] États du bouton clairement distinguables
- [ ] Notifications lisibles

### UX
- [ ] Feedback immédiat au clic (spinner)
- [ ] Notifications non intrusives
- [ ] Messages d'erreur compréhensibles
- [ ] Progression visible (spinner)

## ?? Tests de régression

### Page Devices
- [ ] Autres boutons fonctionnent toujours
- [ ] Ajout de device fonctionne
- [ ] Modification de device fonctionne
- [ ] Suppression de device fonctionne
- [ ] Ajout de périphérique fonctionne
- [ ] Filtres fonctionnent
- [ ] Recherche fonctionne

### Navigation
- [ ] Accès à la page Devices
- [ ] Retour à la page précédente
- [ ] Navigation vers autres pages

## ?? Tests de données

### Intégrité des données
- [ ] Pas de doublons créés
- [ ] IotDeviceId unique respecté
- [ ] Relations Zone-Device préservées
- [ ] Données existantes non corrompues

### Cohérence
- [ ] Données BDD = Données IoT Hub après sync
- [ ] Statuts de connexion corrects
- [ ] Métadonnées à jour

## ?? Tests techniques

### Cache
- [ ] Cache invalidé après synchronisation
- [ ] Nouveaux statuts de connexion chargés
- [ ] Pas de données obsolètes affichées

### Concurrence
- [ ] Synchronisation pendant modification: Géré correctement
- [ ] Double clic sur bouton: Un seul appel API
- [ ] Synchronisation simultanée par plusieurs utilisateurs: OK

### Logs et monitoring
- [ ] Logs backend visibles
- [ ] Logs frontend visibles
- [ ] Erreurs capturées et loggées
- [ ] Métriques disponibles (temps de sync, etc.)

## ?? Notes de test

### Configuration de test
- URL Backend: _______________
- IoT Hub: _______________
- Utilisateur test: _______________
- Nombre de devices test: _______________

### Résultats de test
- Date: _______________
- Testeur: _______________
- Version: _______________
- Résultat global: ? Pass ? Fail

### Bugs trouvés
1. _______________________________________________
2. _______________________________________________
3. _______________________________________________

### Améliorations suggérées
1. _______________________________________________
2. _______________________________________________
3. _______________________________________________

## ? Validation finale

- [ ] Tous les tests critiques passés
- [ ] Aucun bug bloquant
- [ ] Performance acceptable
- [ ] Documentation à jour
- [ ] Code reviewé
- [ ] Prêt pour déploiement
