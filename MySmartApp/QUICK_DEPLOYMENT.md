# ? Démarrage Rapide - Déploiement CI/CD

## ?? Objectif
Déployer MySmartApp.Ui sur Azure Static Web Apps en 10 minutes.

---

## ?? Étape 1 : Créer Azure Static Web App (5 min)

### Via le Portail Azure

1. **Ouvrez** https://portal.azure.com
2. **Cliquez** sur "Create a resource" (bouton bleu en haut à gauche)
3. **Cherchez** "Static Web Apps" et sélectionnez
4. **Remplissez** le formulaire :

```
Subscription     : [Votre abonnement]
Resource Group   : Créer nouveau ? "rg-mysmartapp"
Name             : mysmartapp-ui
Region           : West Europe
Plan Type        : Free

Deployment Details:
?? Source        : GitHub
?? GitHub Account: Koj-Kyo
?? Repository    : front-smart-office
?? Branch        : ui-wasm-only

Build Details:
?? Build Presets : Blazor
?? App location  : /MySmartApp.Ui
?? Output location: wwwroot
```

5. **Cliquez** sur "Review + create" puis "Create"
6. **Attendez** ~2 minutes pour la création

---

## ?? Étape 2 : Récupérer le Token (2 min)

### Option A : Via le Portail

1. **Accédez** à votre Static Web App créée
2. **Cliquez** sur "Manage deployment token" (dans le menu Overview)
3. **Copiez** le token affiché
4. **Gardez-le** ouvert dans un onglet

### Option B : Via Azure CLI

```bash
az staticwebapp secrets list \
  --name mysmartapp-ui \
  --resource-group rg-mysmartapp \
  --query "properties.apiKey" -o tsv
```

---

## ?? Étape 3 : Configurer GitHub Secret (2 min)

1. **Ouvrez** https://github.com/Koj-Kyo/front-smart-office/settings/secrets/actions
2. **Cliquez** sur "New repository secret"
3. **Remplissez** :
   - Name: `AZURE_STATIC_WEB_APPS_API_TOKEN`
   - Value: [Collez le token de l'étape 2]
4. **Cliquez** sur "Add secret"

---

## ?? Étape 4 : Déployer (1 min)

```bash
# Depuis votre terminal local
cd D:\Memoire\FrontSmart\MySmartApp

# Ajouter les fichiers
git add .github/workflows/
git add MySmartApp.Ui/wwwroot/staticwebapp.config.json
git add DEPLOYMENT_GUIDE.md
git add QUICK_DEPLOYMENT.md

# Commit
git commit -m "?? Add CI/CD pipeline for Azure Static Web Apps"

# Push
git push origin ui-wasm-only
```

---

## ? Étape 5 : Vérifier (2 min)

### 1. Vérifier le workflow GitHub

```
?? Ouvrez : https://github.com/Koj-Kyo/front-smart-office/actions
? Le workflow "Azure Static Web Apps CI/CD" doit être en cours
?? Attendez 3-5 minutes
```

### 2. Obtenir l'URL de l'application

**Via le Portail Azure :**
1. Retournez dans votre Static Web App
2. Regardez l'**URL** en haut (Overview)
3. Format : `https://[nom-généré].azurestaticapps.net`

**Via Azure CLI :**
```bash
az staticwebapp show \
  --name mysmartapp-ui \
  --resource-group rg-mysmartapp \
  --query "defaultHostname" -o tsv
```

### 3. Tester l'application

```bash
# Ouvrez l'URL dans votre navigateur
https://[votre-url].azurestaticapps.net

# Vérifiez que l'application charge correctement
```

---

## ?? C'est fait !

Votre application est maintenant déployée et configurée en CI/CD !

### Prochaines étapes

1. **Chaque push** sur `ui-wasm-only` déclenchera un déploiement automatique
2. **Les Pull Requests** créeront des environnements de staging temporaires
3. **Configurez** votre backend dans `appsettings.json`

---

## ?? Problèmes courants

### ? Le workflow échoue

```bash
# Vérifiez les logs
https://github.com/Koj-Kyo/front-smart-office/actions

# Causes communes :
- Token GitHub expiré ? Regénérez-le
- Mauvais chemin app_location ? Vérifiez /MySmartApp.Ui
- Erreur de build ? Vérifiez que le projet compile localement
```

### ? L'application ne charge pas

1. **Vérifiez la console du navigateur** (F12)
2. **Erreur 404** ? Vérifiez `staticwebapp.config.json`
3. **Erreur CORS** ? Configurez votre backend API

### ? Le secret GitHub n'est pas reconnu

```bash
# Vérifiez le nom exact du secret
AZURE_STATIC_WEB_APPS_API_TOKEN

# Pas d'espaces, majuscules exactes
```

---

## ?? Commandes utiles

```bash
# Voir les workflows
gh workflow list

# Voir les runs
gh run list

# Voir les logs d'un run
gh run view <run-id> --log

# Déclencher manuellement un workflow
gh workflow run "Azure Static Web Apps CI/CD"
```

---

## ?? Documentation

- [Azure Static Web Apps](https://learn.microsoft.com/azure/static-web-apps/)
- [GitHub Actions](https://docs.github.com/actions)
- [Blazor WebAssembly](https://learn.microsoft.com/aspnet/core/blazor/)

---

## ? Améliorations optionnelles

```bash
# Ajouter un domaine personnalisé
az staticwebapp hostname set \
  --name mysmartapp-ui \
  --resource-group rg-mysmartapp \
  --hostname www.mondomaine.com

# Activer l'authentification
# Via le portail : Static Web App > Authentication
```

---

**?? Bravo ! Votre application est en production !**
