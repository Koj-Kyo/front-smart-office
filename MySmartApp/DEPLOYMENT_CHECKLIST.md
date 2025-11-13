# ? Checklist de Déploiement CI/CD

## ?? Vue d'ensemble

Cette checklist vous guide étape par étape pour déployer MySmartApp avec CI/CD.

---

## ?? Phase 1 : Préparation (5 min)

### Infrastructure Cloud

- [ ] **Compte Azure créé** (ou GitHub pour GitHub Pages)
  - ?? Azure : https://azure.microsoft.com/free/
  - ?? Pas de compte Azure ? ? Utilisez GitHub Pages (gratuit)

- [ ] **Azure CLI installé** (pour Azure uniquement)
  - ?? https://aka.ms/installazurecliwindows
  - ? Vérifier : `az --version`

- [ ] **GitHub CLI installé** (optionnel mais recommandé)
  - ?? https://cli.github.com/
  - ? Vérifier : `gh --version`

### Repository

- [ ] **Repository cloné localement**
  - ? D:\Memoire\FrontSmart\MySmartApp

- [ ] **Branche correcte**
  - ? `git branch` ? ui-wasm-only

- [ ] **Git configuré**
  - ? `git status` fonctionne

---

## ?? Phase 2 : Choix de la Plateforme (2 min)

### Option A : Azure Static Web Apps (RECOMMANDÉ)

**Avantages :**
- ? Gratuit (100 GB/mois)
- ? SSL automatique
- ? CDN mondial
- ? Environnements de staging
- ? Production-ready

**Checklist Azure :**

- [ ] **1. Créer l'infrastructure**
  ```powershell
  cd D:\Memoire\FrontSmart\MySmartApp
  .\deploy-azure.ps1
  ```

- [ ] **2. Noter le token de déploiement**
  - ? Copié automatiquement dans le presse-papier
  - ? Ou récupéré manuellement depuis Azure Portal

- [ ] **3. Configurer le secret GitHub**
  - ?? https://github.com/Koj-Kyo/front-smart-office/settings/secrets/actions
  - Name : `AZURE_STATIC_WEB_APPS_API_TOKEN`
  - Value : [Token copié]

- [ ] **4. Workflow configuré**
  - ? Fichier : `.github/workflows/azure-static-web-apps.yml`

---

### Option B : GitHub Pages (ALTERNATIVE GRATUITE)

**Avantages :**
- ? 100% gratuit
- ? Très simple
- ? Pas de compte Azure requis

**Limitations :**
- ?? Pas de backend possible
- ?? URL : koj-kyo.github.io/front-smart-office/

**Checklist GitHub Pages :**

- [ ] **1. Exécuter le script de configuration**
  ```powershell
  cd D:\Memoire\FrontSmart\MySmartApp
  .\deploy-github-pages.ps1
  ```

- [ ] **2. Activer GitHub Pages**
  - ?? https://github.com/Koj-Kyo/front-smart-office/settings/pages
  - Source : **GitHub Actions**

- [ ] **3. Vérifier le base path**
  - ? Fichier : `MySmartApp.Ui/wwwroot/index.html`
  - ? `<base href="/front-smart-office/" />`

- [ ] **4. Workflow configuré**
  - ? Fichier : `.github/workflows/github-pages.yml`

---

## ?? Phase 3 : Déploiement (3 min)

### Commit et Push

- [ ] **1. Vérifier les fichiers ajoutés**
  ```bash
  git status
  ```
  
  Fichiers attendus :
  - ? `.github/workflows/azure-static-web-apps.yml`
  - ? `.github/workflows/github-pages.yml`
  - ? `.github/workflows/ci.yml`
  - ? `MySmartApp.Ui/wwwroot/staticwebapp.config.json`
  - ? `DEPLOYMENT_GUIDE.md`
  - ? `QUICK_DEPLOYMENT.md`
  - ? Scripts PowerShell

- [ ] **2. Commit**
  ```bash
  git add .github/workflows/
  git add MySmartApp.Ui/wwwroot/staticwebapp.config.json
  git add *.md
  git add *.ps1
  git commit -m "?? Add CI/CD pipeline for Azure Static Web Apps"
  ```

- [ ] **3. Push**
  ```bash
  git push origin ui-wasm-only
  ```

---

## ?? Phase 4 : Vérification (5 min)

### Workflow GitHub Actions

- [ ] **1. Ouvrir les Actions**
  - ?? https://github.com/Koj-Kyo/front-smart-office/actions

- [ ] **2. Vérifier le workflow en cours**
  - ? "Azure Static Web Apps CI/CD" ou "Deploy to GitHub Pages"
  - ? Status : ?? En cours ? ?? Success

- [ ] **3. Lire les logs si erreur**
  ```bash
  gh run list
  gh run view <run-id> --log
  ```

### Application Déployée

#### Pour Azure :

- [ ] **1. Récupérer l'URL**
  - ?? Azure Portal ? Static Web App ? Overview
  - ? Format : `https://[nom-généré].azurestaticapps.net`

- [ ] **2. Tester l'application**
  - [ ] L'application charge
  - [ ] Les routes fonctionnent (/dashboard, /devices, etc.)
  - [ ] Pas d'erreur 404
  - [ ] Pas d'erreur CORS
  - [ ] SSL/HTTPS actif ??

#### Pour GitHub Pages :

- [ ] **1. URL fixe**
  - ?? https://koj-kyo.github.io/front-smart-office/

- [ ] **2. Tester l'application**
  - [ ] L'application charge
  - [ ] Les routes fonctionnent
  - [ ] Les ressources se chargent correctement

---

## ?? Phase 5 : Configuration Backend (5 min)

### Configuration de l'API

- [ ] **1. Mettre à jour `appsettings.json`**
  ```json
  {
    "ApiBaseUrl": "https://votre-backend.azurewebsites.net",
    "Environment": "Production"
  }
  ```

- [ ] **2. Configurer CORS sur le backend**
  - Autoriser l'URL de production dans le backend
  - Azure : `https://*.azurestaticapps.net`
  - GitHub Pages : `https://koj-kyo.github.io`

- [ ] **3. Tester l'intégration**
  - [ ] Login fonctionne
  - [ ] Les données se chargent
  - [ ] Pas d'erreur CORS
  - [ ] JWT tokens fonctionnent

---

## ?? Phase 6 : Monitoring (En continu)

### Surveillance des Déploiements

- [ ] **Workflow CI**
  - ? S'exécute sur chaque push
  - ? Builds réussis
  - ? Tests passent

- [ ] **Notifications** (optionnel)
  - [ ] Email sur échec
  - [ ] Slack/Discord webhook
  - [ ] GitHub notifications actives

### Performance

- [ ] **Tests de charge**
  - [ ] Page d'accueil < 3 sec
  - [ ] Dashboard < 5 sec
  - [ ] Navigation fluide

- [ ] **Monitoring Azure** (Azure uniquement)
  - ?? Portal ? Static Web App ? Monitoring
  - Métriques à surveiller :
    - Requests
    - Data out
    - Response time

---

## ?? Phase 7 : Production Ready

### Checklist Finale

- [ ] **Infrastructure**
  - [ ] ? Azure/GitHub configuré
  - [ ] ? DNS configuré (si domaine personnalisé)
  - [ ] ? SSL/HTTPS actif

- [ ] **CI/CD**
  - [ ] ? Workflows GitHub Actions fonctionnels
  - [ ] ? Déploiements automatiques
  - [ ] ? Secrets configurés

- [ ] **Application**
  - [ ] ? Build réussi
  - [ ] ? Tests passent
  - [ ] ? Pas d'erreurs en console
  - [ ] ? Toutes les pages fonctionnent

- [ ] **Backend**
  - [ ] ? API accessible
  - [ ] ? CORS configuré
  - [ ] ? Authentication fonctionne

- [ ] **Documentation**
  - [ ] ? README.md à jour
  - [ ] ? Guides de déploiement créés
  - [ ] ? Équipe informée

---

## ?? Dépannage Rapide

### ? Le workflow échoue

```bash
# 1. Vérifier les logs
gh run view --log

# 2. Causes communes
- Token GitHub expiré ? Regénérer
- Erreur de build ? Tester localement : dotnet build
- Secret manquant ? Vérifier les secrets GitHub
```

### ? L'application ne charge pas

```bash
# 1. Console du navigateur (F12)
- Erreur 404 ? Vérifier staticwebapp.config.json
- Erreur CORS ? Configurer le backend
- Erreur de chargement ? Vérifier le base path
```

### ? Le backend ne répond pas

```bash
# 1. Vérifier CORS
- Le backend doit autoriser l'origine frontend

# 2. Vérifier l'URL API
- Vérifier appsettings.json
- Tester l'API avec Postman
```

---

## ?? Support

### Documentation

- ?? **Guide complet :** DEPLOYMENT_GUIDE.md
- ? **Guide rapide :** QUICK_DEPLOYMENT.md
- ?? **Workflows :** .github/workflows/README.md

### Ressources

- **Azure :** https://learn.microsoft.com/azure/static-web-apps/
- **GitHub Actions :** https://docs.github.com/actions
- **Blazor WASM :** https://learn.microsoft.com/aspnet/core/blazor/

### Issues

- ?? https://github.com/Koj-Kyo/front-smart-office/issues

---

## ? Félicitations !

Si toutes les cases sont cochées, votre application est en production ! ??

**Prochaines étapes :**
- Surveiller les déploiements
- Optimiser les performances
- Ajouter des tests
- Documenter pour l'équipe

**Bon déploiement ! ??**
