# Guide de Correction de l'Encodage UTF-8 pour Azure Static Web Apps

## ?? Problème Résolu
Les accents et caractères spéciaux ne s'affichaient pas correctement sur le site Azure en ligne.

## ? Corrections Appliquées

### 1. **staticwebapp.config.json**
- ? Ajout d'en-têtes `Content-Type: charset=utf-8` pour tous les types de fichiers
- ? Configuration des `mimeTypes` avec `charset=utf-8`
- ? Routes spécifiques pour HTML, JS, CSS, JSON avec encodage UTF-8
- ? Suppression de `globalHeaders.Content-Type` qui causait des conflits

### 2. **web.config**
- ? Configuration complète pour IIS/Azure App Service
- ? MIME types avec `charset=utf-8` pour JSON
- ? En-têtes HTTP personnalisés avec UTF-8
- ? Support Blazor WebAssembly (.wasm, .dll, .dat, .blat)
- ? Règles de réécriture pour SPA

### 3. **MySmartApp.Ui.csproj**
- ? Ajout de `<Utf8Output>true</Utf8Output>`
- ? Ajout de `<FileEncoding>UTF-8</FileEncoding>`

### 4. **index.html**
- ? Déjà configuré avec `<meta charset="utf-8" />`
- ? Balise `lang="fr"` présente

## ?? Déploiement

### Méthode 1 : Via GitHub Actions (Recommandé)

```bash
# 1. Commit et push des modifications
git add .
git commit -m "fix: Correction encodage UTF-8 pour Azure"
git push origin ui-wasm-only

# 2. Le workflow GitHub Actions déploiera automatiquement
# Vérifiez dans : https://github.com/Koj-Kyo/front-smart-office/actions
```

### Méthode 2 : Via Azure CLI

```bash
# 1. Build du projet
cd D:\Memoire\FrontSmart\MySmartApp\MySmartApp.Ui
dotnet publish -c Release -o ./bin/publish

# 2. Déploiement (remplacez <APP_NAME> par le nom de votre Azure Static Web App)
az staticwebapp deploy --name <APP_NAME> --resource-group <RESOURCE_GROUP> --source ./bin/publish/wwwroot
```

## ?? Tests à Effectuer Après Déploiement

### Test 1 : Page Register
1. Ouvrir https://votre-site.azurestaticapps.net/register
2. Vérifier l'affichage correct de :
   - ? "Créer votre compte"
   - ? "Prénom"
   - ? "Adresse email"
   - ? "Confirmer le mot de passe"
   - ? "J'accepte les conditions d'utilisation"
   - ? "Vous avez déjà un compte ?"

### Test 2 : Page Login
1. Ouvrir https://votre-site.azurestaticapps.net/login
2. Vérifier :
   - ? "Se connecter à votre compte"
   - ? Tous les textes avec accents

### Test 3 : Console Navigateur
```javascript
// Ouvrir la console (F12) et exécuter :
console.log(document.characterSet);
// Doit afficher : "UTF-8"

// Vérifier les en-têtes HTTP
fetch(window.location.href).then(r => {
  console.log('Content-Type:', r.headers.get('content-type'));
});
// Doit contenir : "charset=utf-8"
```

### Test 4 : Vérification des En-têtes HTTP
Dans Chrome/Edge DevTools (F12) :
1. Onglet **Network**
2. Rafraîchir la page (F5)
3. Cliquer sur **index.html**
4. Vérifier dans **Headers** ? **Response Headers** :
   ```
   Content-Type: text/html; charset=utf-8
   ```

### Test 5 : Navigation Complète
Tester toutes les pages contenant des accents :
- ? /register ? "Créer votre compte"
- ? /login ? "Se connecter"
- ? /dashboard ? "Tableau de bord"
- ? /devices ? "Périphériques"
- ? /users ? "Utilisateurs"
- ? /telemetrie ? "Télémétrie"
- ? /notifications ? Messages avec accents

## ?? Dépannage

### Problème : Les accents persistent après déploiement

**Solution 1 : Vider le cache Azure CDN**
```bash
# Via Azure CLI
az cdn endpoint purge --resource-group <RESOURCE_GROUP> \
  --name <ENDPOINT_NAME> --profile-name <PROFILE_NAME> \
  --content-paths "/*"
```

**Solution 2 : Forcer le rechargement côté client**
- Ouvrir le site en navigation privée (Ctrl+Shift+N)
- Vider le cache du navigateur (Ctrl+Shift+Delete)
- Hard refresh (Ctrl+F5)

**Solution 3 : Vérifier le fichier déployé**
```bash
# Télécharger index.html depuis Azure et vérifier l'encodage
file -i index.html
# Doit afficher : charset=utf-8
```

### Problème : Certains accents OK, d'autres KO

**Cause possible** : Fichiers sources non encodés en UTF-8

**Solution** :
```powershell
# Utiliser le script fix-encoding.ps1 existant
.\fix-encoding.ps1

# Puis rebuild et redéployer
dotnet clean
dotnet publish -c Release
```

## ?? Checklist de Validation

Avant de valider la correction :

- [ ] Commit et push des modifications
- [ ] Déploiement réussi (GitHub Actions ou Azure CLI)
- [ ] Test page /register ? accents corrects
- [ ] Test page /login ? accents corrects
- [ ] Console navigateur ? charset=UTF-8
- [ ] Network DevTools ? Content-Type avec charset=utf-8
- [ ] Test en navigation privée
- [ ] Test sur mobile (si applicable)

## ?? Validation Finale

Tous les tests passent ? **Correction réussie !** ??

Les accents et caractères spéciaux doivent maintenant s'afficher correctement sur :
- ? Toutes les pages du site
- ? Tous les navigateurs (Chrome, Edge, Firefox, Safari)
- ? Tous les appareils (Desktop, Mobile, Tablet)

## ?? Notes Importantes

1. **Délai de propagation** : Les modifications peuvent prendre 5-10 minutes pour se propager sur Azure CDN
2. **Cache navigateur** : Toujours tester en navigation privée la première fois
3. **Fichiers sources** : Assurez-vous que tous les fichiers .razor/.cs sont en UTF-8 sans BOM
4. **Build** : Toujours faire un `dotnet clean` avant `dotnet publish` après modification du .csproj

## ?? Liens Utiles

- [Azure Static Web Apps Configuration](https://learn.microsoft.com/en-us/azure/static-web-apps/configuration)
- [Character Encoding in HTML](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/meta#charset)
- [UTF-8 Best Practices](https://www.w3.org/International/questions/qa-html-encoding-declarations)
