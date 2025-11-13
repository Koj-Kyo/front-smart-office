# ============================================
# Script de Déploiement GitHub Pages
# MySmartApp - Alternative gratuite
# ============================================

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  ?? MySmartApp - Déploiement GitHub Pages" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Vérifier si on est dans le bon répertoire
$currentPath = Get-Location
if (-not (Test-Path "MySmartApp.Ui\MySmartApp.Ui.csproj")) {
    Write-Host "? Ce script doit être exécuté depuis la racine du projet" -ForegroundColor Red
    Write-Host "?? Répertoire actuel : $currentPath" -ForegroundColor Yellow
    Write-Host "?? Répertoire attendu : D:\Memoire\FrontSmart\MySmartApp" -ForegroundColor Yellow
    exit 1
}

Write-Host "? Répertoire correct détecté" -ForegroundColor Green
Write-Host ""

# Vérifier si gh CLI est installé
Write-Host "?? Vérification de GitHub CLI..." -ForegroundColor Yellow
$ghVersion = gh --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "?? GitHub CLI n'est pas installé (optionnel)" -ForegroundColor Yellow
    Write-Host "?? Pour l'installer : https://cli.github.com/" -ForegroundColor Cyan
    $useGhCli = $false
} else {
    Write-Host "? GitHub CLI est installé" -ForegroundColor Green
    $useGhCli = $true
}
Write-Host ""

# Configuration de la base path
Write-Host "?? Configuration de l'application..." -ForegroundColor Yellow
$indexPath = "MySmartApp.Ui\wwwroot\index.html"
if (Test-Path $indexPath) {
    $content = Get-Content $indexPath -Raw
    if ($content -match '<base href="/" />') {
        Write-Host "?? Le base path doit être configuré pour GitHub Pages" -ForegroundColor Yellow
        Write-Host "?? Modification de index.html..." -ForegroundColor Cyan
        
        $content = $content -replace '<base href="/" />', '<base href="/front-smart-office/" />'
        Set-Content -Path $indexPath -Value $content -NoNewline
        
        Write-Host "? Base path configuré : /front-smart-office/" -ForegroundColor Green
    } else {
        Write-Host "? Base path déjà configuré" -ForegroundColor Green
    }
} else {
    Write-Host "? Fichier index.html introuvable" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Activer GitHub Pages via gh CLI
if ($useGhCli) {
    Write-Host "?? Configuration de GitHub Pages..." -ForegroundColor Yellow
    
    # Vérifier si l'utilisateur est connecté
    $ghAuth = gh auth status 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "?? Connexion à GitHub..." -ForegroundColor Yellow
        gh auth login
        if ($LASTEXITCODE -ne 0) {
            Write-Host "? Échec de la connexion" -ForegroundColor Red
            exit 1
        }
    }
    
    Write-Host "? Authentifié sur GitHub" -ForegroundColor Green
    Write-Host ""
}

# Instructions pour activer GitHub Pages manuellement
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  ?? Configuration GitHub Pages (MANUEL)" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Allez sur :" -ForegroundColor White
Write-Host "   https://github.com/Koj-Kyo/front-smart-office/settings/pages" -ForegroundColor Cyan
Write-Host ""
Write-Host "2. Dans 'Build and deployment' :" -ForegroundColor White
Write-Host "   - Source : Sélectionnez 'GitHub Actions'" -ForegroundColor Green
Write-Host ""
Write-Host "3. Cliquez sur 'Save'" -ForegroundColor White
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Commit et push
Write-Host "?? Préparation du déploiement..." -ForegroundColor Yellow
Write-Host ""

$response = Read-Host "Voulez-vous commiter et pusher maintenant ? (O/N)"
if ($response -eq 'O' -or $response -eq 'o' -or $response -eq 'Y' -or $response -eq 'y') {
    Write-Host ""
    Write-Host "?? Ajout des fichiers..." -ForegroundColor Cyan
    
    git add .github/workflows/github-pages.yml
    git add MySmartApp.Ui/wwwroot/index.html
    git add DEPLOYMENT_GUIDE.md
    git add QUICK_DEPLOYMENT.md
    git add deploy-github-pages.ps1
    
    Write-Host "? Fichiers ajoutés" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "?? Création du commit..." -ForegroundColor Cyan
    git commit -m "?? Add GitHub Pages deployment workflow"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Commit créé" -ForegroundColor Green
        Write-Host ""
        
        Write-Host "?? Push vers GitHub..." -ForegroundColor Cyan
        git push origin ui-wasm-only
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "? Push réussi !" -ForegroundColor Green
            Write-Host ""
            Write-Host "================================================" -ForegroundColor Cyan
            Write-Host "  ? Déploiement en cours !" -ForegroundColor Green
            Write-Host "================================================" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "?? Suivez le déploiement :" -ForegroundColor Yellow
            Write-Host "   https://github.com/Koj-Kyo/front-smart-office/actions" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "?? Durée estimée : 3-5 minutes" -ForegroundColor Yellow
            Write-Host ""
            Write-Host "?? Votre application sera disponible à :" -ForegroundColor Yellow
            Write-Host "   https://koj-kyo.github.io/front-smart-office/" -ForegroundColor Cyan
            Write-Host ""
        } else {
            Write-Host "? Échec du push" -ForegroundColor Red
        }
    } else {
        Write-Host "? Échec du commit (peut-être aucun changement ?)" -ForegroundColor Yellow
    }
} else {
    Write-Host ""
    Write-Host "?? Déploiement annulé" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "?? Commandes à exécuter manuellement :" -ForegroundColor Cyan
    Write-Host "   git add .github/workflows/github-pages.yml" -ForegroundColor Gray
    Write-Host "   git add MySmartApp.Ui/wwwroot/index.html" -ForegroundColor Gray
    Write-Host "   git commit -m '?? Add GitHub Pages deployment'" -ForegroundColor Gray
    Write-Host "   git push origin ui-wasm-only" -ForegroundColor Gray
    Write-Host ""
}

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  ?? Documentation complète" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? Consultez les guides :" -ForegroundColor White
Write-Host "   - QUICK_DEPLOYMENT.md (Guide rapide)" -ForegroundColor Gray
Write-Host "   - DEPLOYMENT_GUIDE.md (Guide complet)" -ForegroundColor Gray
Write-Host ""
Write-Host "?? Bonne chance avec votre déploiement !" -ForegroundColor Green
Write-Host ""
