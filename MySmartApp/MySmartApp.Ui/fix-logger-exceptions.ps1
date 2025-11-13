# Script de correction des logs avec exceptions pour Blazor WebAssembly
# Ce script remplace les _logger.LogError(ex, ...) par _logger.LogError("...: {Message}", ..., ex.Message)

param(
    [string]$Path = "Services",
    [switch]$WhatIf = $false
)

Write-Host "?? Recherche des fichiers avec _logger.LogError(ex, ..." -ForegroundColor Cyan

$files = Get-ChildItem -Path $Path -Filter "*.cs" -Recurse
$totalReplacements = 0

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    $replacementsInFile = 0
    
    # Pattern: _logger.LogError(ex, "Message...", params)
    # On capture le message et les paramètres pour les réorganiser
    
    # Méthode simple: remplacer ex, par juste le message et ajouter ex.Message à la fin
    # Pattern pour LogError
    $pattern = '(_logger\.LogError)\(ex,\s*("[^"]+")([^)]*)\)'
    $matches = [regex]::Matches($content, $pattern)
    
    if ($matches.Count -gt 0) {
        foreach ($match in $matches) {
            $logMethod = $match.Groups[1].Value
            $message = $match.Groups[2].Value
            $parameters = $match.Groups[3].Value.Trim()
            
            # Ajouter {Message} à la fin du message s'il n'est pas déjà présent
            $newMessage = $message
            if (-not $message.Contains("{Message}")) {
                $newMessage = $message.TrimEnd('"') + ': {Message}"'
            }
            
            # Construire le nouvel appel
            if ($parameters) {
                $newCall = "$logMethod($newMessage$parameters, ex.Message)"
            } else {
                $newCall = "$logMethod($newMessage, ex.Message)"
            }
            
            $content = $content.Replace($match.Value, $newCall)
            $replacementsInFile++
        }
    }
    
    # Pattern pour LogWarning
    $pattern = '(_logger\.LogWarning)\(ex,\s*("[^"]+")([^)]*)\)'
    $matches = [regex]::Matches($content, $pattern)
    
    if ($matches.Count -gt 0) {
        foreach ($match in $matches) {
            $logMethod = $match.Groups[1].Value
            $message = $match.Groups[2].Value
            $parameters = $match.Groups[3].Value.Trim()
            
            $newMessage = $message
            if (-not $message.Contains("{Message}")) {
                $newMessage = $message.TrimEnd('"') + ': {Message}"'
            }
            
            if ($parameters) {
                $newCall = "$logMethod($newMessage$parameters, ex.Message)"
            } else {
                $newCall = "$logMethod($newMessage, ex.Message)"
            }
            
            $content = $content.Replace($match.Value, $newCall)
            $replacementsInFile++
        }
    }
    
    if ($content -ne $originalContent) {
        if ($WhatIf) {
            Write-Host "  ?? $($file.Name): $replacementsInFile remplacement(s) [SIMULATION]" -ForegroundColor Yellow
        } else {
            Set-Content -Path $file.FullName -Value $content -NoNewline
            Write-Host "  ? $($file.Name): $replacementsInFile remplacement(s)" -ForegroundColor Green
        }
        $totalReplacements += $replacementsInFile
    }
}

if ($totalReplacements -eq 0) {
    Write-Host "`n? Aucun remplacement nécessaire!" -ForegroundColor Green
} else {
    if ($WhatIf) {
        Write-Host "`n?? Total: $totalReplacements remplacement(s) détecté(s) [SIMULATION]" -ForegroundColor Yellow
        Write-Host "   Exécutez sans -WhatIf pour appliquer les changements" -ForegroundColor Cyan
    } else {
        Write-Host "`n? Total: $totalReplacements remplacement(s) effectué(s)" -ForegroundColor Green
    }
}

# Afficher un résumé des patterns restants
Write-Host "`n?? Vérification des patterns restants..." -ForegroundColor Cyan
$remaining = Get-ChildItem -Path $Path -Filter "*.cs" -Recurse | 
    Select-String -Pattern "_logger\.Log(?:Error|Warning|Critical)\(ex," | 
    Measure-Object

if ($remaining.Count -gt 0) {
    Write-Host "??  Il reste $($remaining.Count) occurrence(s) à vérifier manuellement" -ForegroundColor Yellow
    Get-ChildItem -Path $Path -Filter "*.cs" -Recurse | 
        Select-String -Pattern "_logger\.Log(?:Error|Warning|Critical)\(ex," | 
        Select-Object -First 5 Path, LineNumber, Line | 
        Format-Table -AutoSize
} else {
    Write-Host "? Aucun pattern problématique détecté!" -ForegroundColor Green
}

