# ?? Guide de Démarrage Rapide - Nouveau Dashboard

## En 5 Minutes ??

### 1. Configuration OpenWeather API (2 min)

1. **Créer un compte gratuit** :
   - Allez sur [openweathermap.org](https://openweathermap.org/)
   - Cliquez sur "Sign Up"
   - Confirmez votre email

2. **Obtenir votre clé API** :
   - Connectez-vous
   - Allez dans "API keys"
   - Copiez votre clé (ex: `a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6`)

3. **Configurer l'application** :
   ```json
   // Dans wwwroot/appsettings.Development.json
   {
     "OpenWeather": {
       "ApiKey": "COLLEZ_VOTRE_CLE_ICI"
     }
   }
   ```

### 2. Lancer l'Application (1 min)

```bash
cd MySmartApp.Ui
dotnet run
```

Ou dans Visual Studio : `F5`

### 3. Accéder au Dashboard (30 sec)

1. Ouvrez votre navigateur
2. Allez sur `https://localhost:7144/dashboard`
3. Connectez-vous si nécessaire

### 4. Explorer les Fonctionnalités (1 min 30)

? **Météo en Temps Réel**
- Consultez la température actuelle
- Vérifiez les conditions météorologiques

? **KPIs**
- Voyez vos appareils actifs
- Consultez les utilisateurs connectés
- Vérifiez les tickets ouverts
- Suivez la consommation énergétique

? **Graphiques**
- Changez la période (jour/semaine/mois)
- Rafraîchissez les données avec le bouton refresh
- Analysez la répartition des appareils

? **Contrôles**
- Testez les commandes des appareils
- Naviguez vers les pages détaillées

## ?? Premier Test

### Vérifier que tout fonctionne :

1. **La météo s'affiche** ?
   - Vous devriez voir la température
   - Une description météo
   - L'humidité et le vent

2. **Les KPIs sont visibles** ?
   - 4 cartes avec des chiffres
   - Des pourcentages de changement
   - Des couleurs distinctives

3. **Les graphiques se chargent** ?
   - Un graphique linéaire (consommation)
   - Un graphique donut (répartition)

### En cas de problème :

#### ? Météo ne s'affiche pas
```
Erreur probable : Clé API invalide ou manquante
Solution :
1. Vérifiez que vous avez copié la BONNE clé
2. Attendez 10 minutes (activation de la clé)
3. Vérifiez votre connexion Internet
```

#### ? Graphiques vides
```
Erreur probable : Pas de données disponibles
Solution :
1. C'est normal si c'est votre première utilisation
2. Les graphiques utilisent des données simulées au départ
3. Ajoutez des appareils pour voir les vraies données
```

#### ? Erreur 404 ou page blanche
```
Erreur probable : Problème de navigation
Solution :
1. Assurez-vous d'être connecté
2. Vérifiez l'URL : /dashboard
3. Consultez la console du navigateur (F12)
```

## ?? Personnaliser Votre Dashboard

### Changer la Ville

1. Ouvrez `Services/WeatherService.cs`
2. Ligne 24, remplacez :
   ```csharp
   _apiKey = configuration["OpenWeather:ApiKey"] ?? "YOUR_API_KEY_HERE";
   ```

3. Dans `Dashboard.razor`, ligne avec `GetCurrentWeatherAsync` :
   ```csharp
   _weatherData = await WeatherService.GetCurrentWeatherAsync("Paris");
   // ou votre ville
   ```

### Modifier les Couleurs des KPIs

Dans `Dashboard.razor`, section des KPIs :
```razor
<MudIcon Icon="@Icons.Material.Filled.Devices" 
         Color="Color.Primary" />  <!-- Changez cette couleur -->
```

Couleurs disponibles :
- `Color.Primary` (Bleu)
- `Color.Success` (Vert)
- `Color.Warning` (Orange)
- `Color.Error` (Rouge)
- `Color.Info` (Cyan)
- `Color.Dark` (Noir)

### Ajouter un Nouveau KPI

1. Copiez une carte KPI existante
2. Modifiez l'icône, le titre et les données
3. Ajoutez la logique dans `DashboardStatsService`

Exemple :
```razor
<MudItem xs="12" sm="6" md="3">
    <MudCard Class="rounded-xl">
        <MudCardContent>
            <div class="d-flex justify-space-between align-center mb-2">
                <MudIcon Icon="@Icons.Material.Filled.Speed" 
                         Color="Color.Secondary" 
                         Size="Size.Large" />
                <MudChip T="string" Size="Size.Small" Color="Color.Success">
                    +5.2%
                </MudChip>
            </div>
            <MudText Typo="Typo.h4" Class="font-weight-bold">
                98.5%
            </MudText>
            <MudText Typo="Typo.body2" Color="Color.Secondary">
                Performance Système
            </MudText>
        </MudCardContent>
    </MudCard>
</MudItem>
```

## ?? Configuration Avancée

### Activer le Rafraîchissement Automatique

Dans `Dashboard.razor`, ajoutez :

```csharp
private System.Threading.Timer? _refreshTimer;

protected override async Task OnInitializedAsync()
{
    await base.OnInitializedAsync();
    
    // Rafraîchir toutes les 5 minutes
    _refreshTimer = new System.Threading.Timer(
        async _ => await RefreshDataAsync(),
        null,
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(5)
    );
}

private async Task RefreshDataAsync()
{
    await LoadWeatherDataAsync();
    await LoadDashboardStatsAsync();
    await LoadEnergyDataAsync();
    await LoadDeviceActivityAsync();
    StateHasChanged();
}

public void Dispose()
{
    _refreshTimer?.Dispose();
}
```

### Utiliser la Géolocalisation

```javascript
// Dans wwwroot/js/dashboard.js
window.getLocation = () => {
    return new Promise((resolve, reject) => {
        if (!navigator.geolocation) {
            reject('Geolocation not supported');
        }
        
        navigator.geolocation.getCurrentPosition(
            position => resolve({
                latitude: position.coords.latitude,
                longitude: position.coords.longitude
            }),
            error => reject(error.message)
        );
    });
};
```

Puis dans Dashboard.razor :
```csharp
@inject IJSRuntime JS

private async Task LoadWeatherByLocationAsync()
{
    try
    {
        var location = await JS.InvokeAsync<Location>("getLocation");
        _weatherData = await WeatherService.GetWeatherByCoordinatesAsync(
            location.Latitude, 
            location.Longitude
        );
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Erreur de géolocalisation");
        // Fallback sur la ville par défaut
        _weatherData = await WeatherService.GetCurrentWeatherAsync();
    }
}

private class Location
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
```

## ?? Responsive Design

Le Dashboard s'adapte automatiquement :

### Mobile (< 768px)
- 1 colonne
- KPIs empilés (2 par ligne)
- Graphiques adaptés
- Navigation tactile optimisée

### Tablette (768px - 1280px)
- 2 colonnes (50/50)
- KPIs 2 par ligne
- Graphiques full-width

### Desktop (> 1280px)
- 2 colonnes (70/30)
- KPIs 4 par ligne
- Layout optimisé pour grand écran

## ?? Styles Personnalisés

Le fichier `wwwroot/css/dashboard.css` contient tous les styles.

### Ajouter une Animation
```css
.mon-element {
    animation: monAnimation 1s ease-in-out;
}

@keyframes monAnimation {
    from { opacity: 0; }
    to { opacity: 1; }
}
```

### Modifier le Gradient du Hero
```razor
<MudPaper Style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);">
    <!-- Votre contenu -->
</MudPaper>
```

## ?? Dépannage Rapide

### Problème : "Icons ne contient pas de définition"
**Solution** : Ajoutez `@using MudBlazor` dans `_Imports.razor`

### Problème : "WeatherService introuvable"
**Solution** : Vérifiez que le service est enregistré dans `Program.cs`

### Problème : Graphiques ne s'affichent pas
**Solution** : 
1. Vérifiez que MudBlazor est à jour
2. Vérifiez la console (F12)
3. Assurez-vous que les données ne sont pas nulles

### Problème : Météo retourne toujours les mêmes données
**Solution** :
1. Videz le cache du navigateur
2. Vérifiez votre clé API
3. Attendez quelques minutes (limitation API)

## ?? Ressources

- [Documentation MudBlazor](https://mudblazor.com/)
- [OpenWeather API Docs](https://openweathermap.org/api)
- [Blazor WebAssembly Docs](https://docs.microsoft.com/aspnet/core/blazor/)

## ? Checklist de Démarrage

- [ ] Compte OpenWeather créé
- [ ] Clé API obtenue et configurée
- [ ] Application compilée sans erreur
- [ ] Dashboard accessible
- [ ] Météo affichée correctement
- [ ] KPIs visibles
- [ ] Graphiques fonctionnels
- [ ] Contrôles d'appareils testés

---

**Temps total** : ~5 minutes  
**Difficulté** : ?? (Facile)  
**Support** : GitHub Issues ou documentation

?? **Félicitations ! Votre Dashboard moderne est opérationnel !**
