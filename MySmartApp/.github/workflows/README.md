# ?? GitHub Actions Workflows

Ce dossier contient les workflows CI/CD pour MySmartApp.

## ?? Workflows disponibles

### 1. `azure-static-web-apps.yml` - Déploiement Azure (PRODUCTION)

**Déclenchement :**
- Push sur `ui-wasm-only` ou `main`
- Pull Request vers ces branches

**Actions :**
- ? Build de l'application .NET 9 / Blazor WASM
- ? Exécution des tests
- ? Publication sur Azure Static Web Apps
- ? Création d'environnements de staging pour les PR

**Prérequis :**
- Secret GitHub : `AZURE_STATIC_WEB_APPS_API_TOKEN`
- Azure Static Web App créée

**URL de production :** Fournie par Azure après déploiement

---

### 2. `github-pages.yml` - Déploiement GitHub Pages (ALTERNATIVE)

**Déclenchement :**
- Push sur `ui-wasm-only` ou `main`
- Déclenchement manuel

**Actions :**
- ? Build de l'application
- ? Configuration du base path pour GitHub Pages
- ? Publication sur GitHub Pages

**Prérequis :**
- GitHub Pages activé dans les settings du repository
- Source : GitHub Actions

**URL de production :** `https://koj-kyo.github.io/front-smart-office/`

---

### 3. `ci.yml` - Intégration Continue

**Déclenchement :**
- Tous les push et PR sur `ui-wasm-only` et `main`

**Actions :**
- ? Vérification du build
- ? Exécution des tests
- ? Rapport de build dans les PR

**Prérequis :** Aucun

---

## ?? Démarrage Rapide

### Option 1 : Azure Static Web Apps (Recommandé)

```bash
# 1. Créer l'infrastructure Azure
./deploy-azure.ps1

# 2. Configurer le secret GitHub
# (Le script vous guide)

# 3. Commiter et pusher
git add .github/workflows/
git commit -m "?? Add CI/CD workflows"
git push origin ui-wasm-only
```

### Option 2 : GitHub Pages (Gratuit)

```bash
# 1. Configurer GitHub Pages
./deploy-github-pages.ps1

# 2. Activer GitHub Pages dans les settings
# Repository > Settings > Pages > Source: GitHub Actions

# 3. Le script commit et push automatiquement
```

---

## ?? Surveillance des Déploiements

### Via GitHub

**Interface Web :**
```
https://github.com/Koj-Kyo/front-smart-office/actions
```

**Via GitHub CLI :**
```bash
# Lister les workflows
gh workflow list

# Voir les exécutions récentes
gh run list

# Voir les détails d'une exécution
gh run view <run-id>

# Voir les logs
gh run view <run-id> --log
```

---

## ?? Configuration

### Variables d'environnement

Les workflows utilisent ces variables :

```yaml
# Azure Static Web Apps
AZURE_STATIC_WEB_APPS_API_TOKEN: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }} # Automatique

# Chemins
working-directory: ./MySmartApp.Ui
app_location: /MySmartApp.Ui
output_location: wwwroot
```

### Secrets GitHub requis

| Secret | Workflow | Description |
|--------|----------|-------------|
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | azure-static-web-apps.yml | Token de déploiement Azure |
| `GITHUB_TOKEN` | Tous | Token automatique GitHub |

---

## ?? Dépannage

### Le workflow Azure échoue

**1. Vérifier le secret :**
```bash
# Le secret doit être exactement :
AZURE_STATIC_WEB_APPS_API_TOKEN
```

**2. Vérifier les logs :**
```bash
gh run view --log
```

**3. Causes communes :**
- ? Token expiré ? Regénérez dans Azure Portal
- ? Mauvais chemin `app_location` ? Vérifiez `/MySmartApp.Ui`
- ? Erreur de build ? Testez localement : `dotnet build`

### Le workflow GitHub Pages échoue

**1. Vérifier que GitHub Pages est activé :**
```
Settings > Pages > Source: GitHub Actions
```

**2. Vérifier le base path :**
```html
<!-- Dans index.html -->
<base href="/front-smart-office/" />
```

**3. Vérifier les permissions :**
```yaml
permissions:
  contents: read
  pages: write
  id-token: write
```

### L'application déployée ne fonctionne pas

**1. Erreur 404 sur les routes :**
- ? Vérifiez `staticwebapp.config.json`
- ? Configurez la navigation fallback

**2. Erreur CORS :**
- ? Configurez votre API backend
- ? Vérifiez `appsettings.json`

**3. Erreur de chargement des ressources :**
- ? Vérifiez le base path
- ? Vérifiez la console du navigateur (F12)

---

## ?? Optimisations

### Cache des dépendances .NET

```yaml
- uses: actions/cache@v3
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
    restore-keys: |
      ${{ runner.os }}-nuget-
```

### Tests parallèles

```yaml
- name: Run tests
  run: dotnet test --parallel
```

### Notifications

Ajoutez dans le workflow :

```yaml
- name: Notify on failure
  if: failure()
  uses: 8398a7/action-slack@v3
  with:
    status: ${{ job.status }}
    webhook_url: ${{ secrets.SLACK_WEBHOOK }}
```

---

## ?? Environnements Multiples

### Configuration par branche

```yaml
on:
  push:
    branches:
      - main        # Production
      - develop     # Staging
      - ui-wasm-only # Development
```

### Secrets par environnement

```yaml
environment:
  name: production
  url: https://mysmartapp.azurestaticapps.net
```

---

## ?? Ressources

- [GitHub Actions Documentation](https://docs.github.com/actions)
- [Azure Static Web Apps](https://learn.microsoft.com/azure/static-web-apps/)
- [GitHub Pages](https://docs.github.com/pages)
- [Blazor WebAssembly](https://learn.microsoft.com/aspnet/core/blazor/)

---

## ? Checklist de Production

- [ ] Secrets GitHub configurés
- [ ] Workflows testés avec succès
- [ ] URL de production vérifiée
- [ ] Configuration backend mise à jour
- [ ] SSL/HTTPS actif
- [ ] Tests de déploiement réussis
- [ ] Monitoring configuré
- [ ] Documentation à jour

---

**Dernière mise à jour :** 2024
**Auteur :** Koj-Kyo
**Repository :** https://github.com/Koj-Kyo/front-smart-office
