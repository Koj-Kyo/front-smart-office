# ?? Guide de Déploiement CI/CD - MySmartApp

## ?? Vue d'ensemble

Ce guide vous accompagne pour déployer votre application Blazor WebAssembly avec GitHub Actions.

---

## ? Prérequis

- [x] Repository GitHub : https://github.com/Koj-Kyo/front-smart-office
- [ ] Compte Azure (gratuit possible)
- [ ] Droits d'administration sur le repository

---

## ?? Options de Déploiement

### Option 1 : Azure Static Web Apps (RECOMMANDÉ)

**Avantages :**
- ? **Gratuit** pour petits projets (100 GB bande passante/mois)
- ? SSL/HTTPS automatique
- ? CDN mondial intégré
- ? Environnements de staging automatiques
- ? Parfait pour Blazor WebAssembly

**Étapes :**

#### 1. Créer une Azure Static Web App

```bash
# Option A : Via Azure Portal (Interface Web)
1. Allez sur https://portal.azure.com
2. Cliquez sur "Create a resource"
3. Cherchez "Static Web Apps"
4. Cliquez sur "Create"
5. Remplissez :
   - Subscription : Votre abonnement Azure
   - Resource Group : Créer nouveau "rg-mysmartapp"
   - Name : "mysmartapp-ui"
   - Region : "West Europe"
   - Source : GitHub
   - GitHub Account : Koj-Kyo
   - Repository : front-smart-office
   - Branch : ui-wasm-only
   - Build Details :
     * Build Presets : Blazor
     * App location : /MySmartApp.Ui
     * Output location : wwwroot
6. Cliquez sur "Review + create"
7. Cliquez sur "Create"
```

```bash
# Option B : Via Azure CLI
az login
az group create --name rg-mysmartapp --location westeurope
az staticwebapp create \
  --name mysmartapp-ui \
  --resource-group rg-mysmartapp \
  --source https://github.com/Koj-Kyo/front-smart-office \
  --branch ui-wasm-only \
  --app-location "/MySmartApp.Ui" \
  --output-location "wwwroot" \
  --login-with-github
```

#### 2. Récupérer le token de déploiement

```bash
# Via Azure CLI
az staticwebapp secrets list \
  --name mysmartapp-ui \
  --resource-group rg-mysmartapp \
  --query "properties.apiKey" -o tsv
```

Ou via le portail :
1. Allez dans votre Static Web App
2. Cliquez sur "Manage deployment token"
3. Copiez le token

#### 3. Ajouter le secret GitHub

1. Allez sur : https://github.com/Koj-Kyo/front-smart-office/settings/secrets/actions
2. Cliquez sur "New repository secret"
3. Name : `AZURE_STATIC_WEB_APPS_API_TOKEN`
4. Value : Collez le token copié
5. Cliquez sur "Add secret"

#### 4. Déclencher le déploiement

```bash
# Commit et push les workflows
git add .github/workflows/
git commit -m "?? Add CI/CD workflows"
git push origin ui-wasm-only
```

#### 5. Vérifier le déploiement

1. Allez sur : https://github.com/Koj-Kyo/front-smart-office/actions
2. Vérifiez que le workflow "Azure Static Web Apps CI/CD" est en cours
3. Attendez que le déploiement soit vert ?
4. L'URL sera visible dans le portail Azure

---

### Option 2 : GitHub Pages (GRATUIT & SIMPLE)

**Avantages :**
- ? 100% gratuit
- ? Très simple à configurer
- ? Pas de compte Azure nécessaire

**Inconvénients :**
- ?? URL : `https://koj-kyo.github.io/front-smart-office/`
- ?? Pas de backend possible
- ?? Repositories publics seulement (gratuit)

**Étapes :**

#### 1. Activer GitHub Pages

1. Allez sur : https://github.com/Koj-Kyo/front-smart-office/settings/pages
2. Source : "GitHub Actions"
3. Cliquez sur "Save"

#### 2. Activer le workflow

```bash
# Le workflow est déjà créé, activez-le
git add .github/workflows/github-pages.yml
git commit -m "? Enable GitHub Pages deployment"
git push origin ui-wasm-only
```

#### 3. Configurer l'application

Modifiez `MySmartApp.Ui/wwwroot/index.html` :

```html
<!-- Changez cette ligne : -->
<base href="/" />

<!-- En : -->
<base href="/front-smart-office/" />
```

Ou le workflow le fera automatiquement !

#### 4. Déclencher le déploiement

```bash
git add .
git commit -m "?? Deploy to GitHub Pages"
git push origin ui-wasm-only
```

#### 5. Vérifier le déploiement

Votre app sera disponible à :
https://koj-kyo.github.io/front-smart-office/

---

## ?? Configuration du Backend

### Variables d'environnement

Modifiez `MySmartApp.Ui/wwwroot/appsettings.json` :

```json
{
  "ApiBaseUrl": "https://votre-backend-api.azurewebsites.net",
  "Environment": "Production"
}
```

### Secrets GitHub pour différents environnements

Ajoutez ces secrets dans GitHub :

1. `AZURE_STATIC_WEB_APPS_API_TOKEN` - Token de déploiement
2. `API_BASE_URL_PROD` - URL du backend en production
3. `API_BASE_URL_STAGING` - URL du backend en staging

---

## ?? Surveillance du Déploiement

### Vérifier les workflows

```bash
# Via GitHub CLI
gh workflow list
gh run list
gh run view <run-id> --log
```

### Vérifier l'application déployée

```bash
# Test de l'application
curl https://your-app.azurestaticapps.net
curl https://koj-kyo.github.io/front-smart-office/
```

---

## ?? Dépannage

### Problème : Le workflow échoue

```bash
# Vérifiez les logs
1. Allez sur https://github.com/Koj-Kyo/front-smart-office/actions
2. Cliquez sur le workflow qui a échoué
3. Lisez les logs d'erreur
```

### Problème : L'application ne charge pas

1. **Vérifiez la console du navigateur** (F12)
2. **Vérifiez le base path** dans `index.html`
3. **Vérifiez l'URL de l'API** dans `appsettings.json`

### Problème : Erreur 404 sur les routes

Ajoutez `staticwebapp.config.json` :

```json
{
  "navigationFallback": {
    "rewrite": "/index.html",
    "exclude": ["/images/*.{png,jpg,gif}", "/css/*"]
  },
  "routes": [
    {
      "route": "/api/*",
      "allowedRoles": ["authenticated"]
    }
  ]
}
```

---

## ?? Améliorations futures

- [ ] Ajouter des tests automatisés
- [ ] Configurer des environnements de staging
- [ ] Ajouter des notifications Slack/Discord
- [ ] Configurer le rollback automatique
- [ ] Ajouter des métriques de performance

---

## ?? Support

- **Issues GitHub :** https://github.com/Koj-Kyo/front-smart-office/issues
- **Documentation Azure :** https://learn.microsoft.com/azure/static-web-apps/
- **Documentation GitHub Pages :** https://docs.github.com/pages

---

## ?? Checklist finale

- [ ] Workflows créés (`.github/workflows/`)
- [ ] Secret GitHub configuré (`AZURE_STATIC_WEB_APPS_API_TOKEN`)
- [ ] Application déployée
- [ ] URL de production vérifiée
- [ ] Backend configuré
- [ ] Tests de déploiement réussis

**Vous êtes prêt à déployer ! ??**
