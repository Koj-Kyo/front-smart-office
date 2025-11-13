# Configuration du Dashboard avec OpenWeather API

## ?? Nouveau Dashboard

Le Dashboard a été entièrement repensé pour offrir une expérience moderne et informative :

### ? Fonctionnalités principales

1. **Hero Section avec Météo en Temps Réel**
   - Intégration de l'API OpenWeather
   - Affichage de la température actuelle et ressentie
   - Description des conditions météorologiques
   - Vitesse du vent et humidité
   - Message d'accueil personnalisé en fonction de la météo

2. **KPIs (Indicateurs Clés de Performance)**
   - Appareils actifs/total
   - Utilisateurs actifs/total
   - Tickets ouverts/total
   - Consommation énergétique
   - Pourcentages de changement pour chaque métrique

3. **Graphiques Interactifs**
   - **Consommation Énergétique** : Graphique linéaire avec sélection de période (mensuel/hebdomadaire/quotidien)
   - **Activité des Appareils** : Graphique en donut montrant la répartition par type d'appareil

4. **Widgets Latéraux**
   - Mes Appareils (contrôle rapide)
   - Télémétrie en temps réel
   - Mes tickets d'incidents
   - Notifications récentes
   - Gestion des zones

## ?? Configuration OpenWeather API

### Étape 1 : Obtenir une clé API

1. Créez un compte gratuit sur [OpenWeatherMap](https://openweathermap.org/)
2. Allez dans [API Keys](https://home.openweathermap.org/api_keys)
3. Copiez votre clé API

### Étape 2 : Configuration dans l'application

Ouvrez le fichier `wwwroot/appsettings.Development.json` et remplacez `YOUR_OPENWEATHER_API_KEY_HERE` par votre clé :

```json
{
  "OpenWeather": {
    "ApiKey": "votre_cle_api_ici"
  }
}
```

### Étape 3 : Configuration pour la production

Pour la production, créez un fichier `wwwroot/appsettings.json` avec la même structure :

```json
{
  "OpenWeather": {
    "ApiKey": "votre_cle_api_production_ici"
  }
}
```

**?? Important :** Ne commitez jamais vos clés API dans Git ! Ajoutez les fichiers de configuration à `.gitignore` ou utilisez des variables d'environnement.

## ?? Personnalisation

### Changer la ville par défaut

Dans `WeatherService.cs`, modifiez la ville par défaut :

```csharp
public async Task<WeatherData?> GetCurrentWeatherAsync(string city = "VotreVille")
```

### Modifier le message d'accueil

Dans `Dashboard.razor`, section `LoadWeatherDataAsync()` :

```csharp
_weatherGreeting = _weatherData.Temperature switch
{
    > 25 => "Votre message pour temps chaud",
    > 15 => "Votre message pour temps agréable",
    > 5 => "Votre message pour temps frais",
    _ => "Votre message pour temps froid"
};
```

## ?? Layout Responsive

Le nouveau Dashboard s'adapte automatiquement à toutes les tailles d'écran :

- **Mobile (xs)** : Une seule colonne, widgets empilés
- **Tablette (md)** : Deux colonnes (70%/30%)
- **Desktop (lg+)** : Layout optimisé avec espacement maximal

## ?? Données en Temps Réel

Les services suivants fournissent des données dynamiques :

- `IWeatherService` : Météo en temps réel depuis OpenWeather
- `IDashboardStatsService` : Statistiques agrégées depuis vos services métier
- Actualisation automatique au chargement de la page
- Possibilité d'ajouter un rafraîchissement automatique périodique

## ?? Prochaines Améliorations Suggérées

1. **Géolocalisation** : Détecter automatiquement la position de l'utilisateur
2. **Actualisation automatique** : Rafraîchir les données toutes les 5-10 minutes
3. **Personnalisation** : Permettre à l'utilisateur de choisir les widgets à afficher
4. **Thèmes** : Support des graphiques en mode sombre
5. **Export** : Exporter les graphiques en PDF/Image
6. **Prévisions** : Ajouter les prévisions météo sur 5 jours

## ?? Services Créés

### WeatherService
- `GetCurrentWeatherAsync(city)` : Météo par ville
- `GetWeatherByCoordinatesAsync(lat, lon)` : Météo par coordonnées

### DashboardStatsService
- `GetDashboardStatsAsync()` : Statistiques globales
- `GetEnergyConsumptionDataAsync(period)` : Données de consommation
- `GetDeviceActivityDataAsync()` : Activité des appareils

## ?? Dépannage

### La météo ne s'affiche pas
1. Vérifiez que votre clé API est correcte
2. Vérifiez que vous avez une connexion Internet
3. Consultez les logs du navigateur (F12)
4. L'API gratuite a une limite de 60 appels/minute

### Les graphiques ne s'affichent pas
1. Vérifiez que MudBlazor est correctement installé
2. Assurez-vous que les services sont enregistrés dans `Program.cs`
3. Vérifiez la console pour les erreurs JavaScript

## ?? Support

Pour toute question ou problème :
- Consultez la [documentation OpenWeather](https://openweathermap.org/api)
- Consultez la [documentation MudBlazor](https://mudblazor.com/)
- Vérifiez les logs de l'application

---

**Version** : 2.0  
**Date** : 2024  
**Auteur** : Smart Office Team
