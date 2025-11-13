# ============================================
# Script de Déploiement Automatique
# MySmartApp - Azure Static Web Apps
# ============================================

param(
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroupName = "rg-mysmartapp",
    
    [Parameter(Mandatory=$false)]
    [string]$StaticWebAppName = "mysmartapp-ui",
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "westeurope",
    
    [Parameter(Mandatory=$false)]
    [string]$Branch = "ui-wasm-only"
)

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  ?? MySmartApp - Déploiement Azure Static Web Apps" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Vérifier si Azure CLI est installé
Write-Host "?? Vérification d'Azure CLI..." -ForegroundColor Yellow
$azVersion = az version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Azure CLI n'est pas installé." -ForegroundColor Red
    Write-Host "?? Installez-le depuis : https://aka.ms/installazurecliwindows" -ForegroundColor Yellow
    exit 1
}
Write-Host "? Azure CLI est installé" -ForegroundColor Green
Write-Host ""

# Vérifier si l'utilisateur est connecté
Write-Host "?? Vérification de la connexion Azure..." -ForegroundColor Yellow
$account = az account show 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Vous n'êtes pas connecté à Azure." -ForegroundColor Red
    Write-Host "?? Connexion en cours..." -ForegroundColor Yellow
    az login
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? Échec de la connexion" -ForegroundColor Red
        exit 1
    }
}
Write-Host "? Connecté à Azure" -ForegroundColor Green
Write-Host ""

# Afficher l'abonnement actuel
$subscriptionName = az account show --query "name" -o tsv
Write-Host "?? Abonnement actuel : $subscriptionName" -ForegroundColor Cyan
Write-Host ""

# Créer le resource group
Write-Host "?? Création du Resource Group..." -ForegroundColor Yellow
$rgExists = az group exists --name $ResourceGroupName
if ($rgExists -eq "false") {
    az group create --name $ResourceGroupName --location $Location | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Resource Group '$ResourceGroupName' créé" -ForegroundColor Green
    } else {
        Write-Host "? Échec de la création du Resource Group" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "? Resource Group '$ResourceGroupName' existe déjà" -ForegroundColor Green
}
Write-Host ""

# Créer la Static Web App
Write-Host "?? Création d'Azure Static Web App..." -ForegroundColor Yellow
$swaExists = az staticwebapp show --name $StaticWebAppName --resource-group $ResourceGroupName 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "?? Connexion à GitHub requise..." -ForegroundColor Yellow
    Write-Host "?? Veuillez autoriser Azure à accéder à votre GitHub" -ForegroundColor Cyan
    
    az staticwebapp create `
        --name $StaticWebAppName `
        --resource-group $ResourceGroupName `
        --source "https://github.com/Koj-Kyo/front-smart-office" `
        --branch $Branch `
        --app-location "/MySmartApp.Ui" `
        --output-location "wwwroot" `
        --login-with-github
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Static Web App '$StaticWebAppName' créée" -ForegroundColor Green
    } else {
        Write-Host "? Échec de la création de la Static Web App" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "? Static Web App '$StaticWebAppName' existe déjà" -ForegroundColor Green
}
Write-Host ""

# Récupérer le token de déploiement
Write-Host "?? Récupération du token de déploiement..." -ForegroundColor Yellow
$deploymentToken = az staticwebapp secrets list `
    --name $StaticWebAppName `
    --resource-group $ResourceGroupName `
    --query "properties.apiKey" -o tsv

if ($LASTEXITCODE -eq 0 -and $deploymentToken) {
    Write-Host "? Token récupéré avec succès" -ForegroundColor Green
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "  ?? IMPORTANT : Configurez GitHub Secret" -ForegroundColor Yellow
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "1. Allez sur : https://github.com/Koj-Kyo/front-smart-office/settings/secrets/actions" -ForegroundColor White
    Write-Host "2. Cliquez sur 'New repository secret'" -ForegroundColor White
    Write-Host "3. Name : AZURE_STATIC_WEB_APPS_API_TOKEN" -ForegroundColor Green
    Write-Host "4. Value : Le token ci-dessous" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? COPIEZ CE TOKEN (cliquez pour sélectionner) :" -ForegroundColor Yellow
    Write-Host $deploymentToken -ForegroundColor Cyan
    Write-Host ""
    
    # Copier dans le presse-papier
    $deploymentToken | Set-Clipboard
    Write-Host "? Token copié dans le presse-papier !" -ForegroundColor Green
} else {
    Write-Host "?? Impossible de récupérer le token automatiquement" -ForegroundColor Yellow
    Write-Host "?? Récupérez-le manuellement :" -ForegroundColor Cyan
    Write-Host "   1. Allez sur https://portal.azure.com" -ForegroundColor White
    Write-Host "   2. Ouvrez votre Static Web App '$StaticWebAppName'" -ForegroundColor White
    Write-Host "   3. Cliquez sur 'Manage deployment token'" -ForegroundColor White
}
Write-Host ""

# Récupérer l'URL de l'application
Write-Host "?? Récupération de l'URL de l'application..." -ForegroundColor Yellow
$appUrl = az staticwebapp show `
    --name $StaticWebAppName `
    --resource-group $ResourceGroupName `
    --query "defaultHostname" -o tsv

if ($LASTEXITCODE -eq 0 -and $appUrl) {
    Write-Host "? URL de l'application : https://$appUrl" -ForegroundColor Green
} else {
    Write-Host "?? URL non disponible pour le moment" -ForegroundColor Yellow
}
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  ? Configuration Azure terminée !" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? Prochaines étapes :" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. ? Configurez le secret GitHub (voir ci-dessus)" -ForegroundColor White
Write-Host "2. ?? Commitez et pushez les workflows :" -ForegroundColor White
Write-Host "     git add .github/workflows/" -ForegroundColor Gray
Write-Host "     git commit -m '?? Add CI/CD pipeline'" -ForegroundColor Gray
Write-Host "     git push origin $Branch" -ForegroundColor Gray
Write-Host "3. ?? Suivez le déploiement :" -ForegroundColor White
Write-Host "     https://github.com/Koj-Kyo/front-smart-office/actions" -ForegroundColor Gray
Write-Host "4. ?? Testez votre application :" -ForegroundColor White
if ($appUrl) {
    Write-Host "     https://$appUrl" -ForegroundColor Gray
}
Write-Host ""
Write-Host "?? Bravo ! Votre infrastructure est prête !" -ForegroundColor Green
Write-Host ""
