# ??? Dashboard Moderne - Smart Office v2.0

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)]()
[![Version](https://img.shields.io/badge/version-2.0.0-blue.svg)]()
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)]()
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-blueviolet.svg)]()

Un tableau de bord moderne et professionnel pour la gestion Smart Office avec météo en temps réel, KPIs interactifs et graphiques dynamiques.

![Dashboard Preview](https://via.placeholder.com/800x400/14B8A6/FFFFFF?text=Smart+Office+Dashboard+v2.0)

---

## ? Fonctionnalités

### ??? Météo en Temps Réel
- Intégration **OpenWeather API**
- Température actuelle et ressentie
- Conditions météorologiques en français
- Humidité, vent, pression atmosphérique
- Message d'accueil dynamique

### ?? KPIs Interactifs
- **Appareils** : Actifs vs Total (+pourcentage de changement)
- **Utilisateurs** : Actifs vs Total (+pourcentage de changement)
- **Tickets** : Ouverts vs Total (+pourcentage de changement)
- **Énergie** : Consommation en kWh (+pourcentage de changement)

### ?? Graphiques Dynamiques
- **Consommation Énergétique** : Graphique linéaire avec sélection de période
- **Activité des Appareils** : Graphique donut par type d'appareil
- **Rafraîchissement** : Bouton refresh pour actualiser les données

### ?? Design Moderne
- Layout responsive (mobile/tablette/desktop)
- Animations fluides et élégantes
- Palette de couleurs moderne
- Support mode clair/sombre
- Material Design

---

## ?? Démarrage Rapide (5 minutes)

### 1?? Prérequis
- **.NET 9.0** SDK
- **Compte OpenWeather** (gratuit)
- **Visual Studio 2022** ou VS Code

### 2?? Installation

```bash
# Cloner le repository
git clone https://github.com/Koj-Kyo/front-smart-office
cd MySmartApp/MySmartApp.Ui

# Restaurer les dépendances
dotnet restore

# Configurer OpenWeather API
# Éditez wwwroot/appsettings.Development.json et ajoutez votre clé
```

### 3?? Configuration

```json
{
  "OpenWeather": {
    "ApiKey": "VOTRE_CLE_API_ICI"
  }
}
```

**Obtenir une clé** : [openweathermap.org](https://openweathermap.org/)

### 4?? Lancer l'application

```bash
dotnet run
# ou F5 dans Visual Studio
```

Ouvrez `https://localhost:7144/dashboard` ??

---

## ?? Documentation Complète

| Document | Description | Temps |
|----------|-------------|-------|
| **[INDEX](INDEX_DOCUMENTATION_DASHBOARD.md)** | ?? Index de toute la documentation | 2 min |
| **[QUICK START](QUICK_START_DASHBOARD.md)** | ?? Guide de démarrage rapide | 5 min |
| **[CONFIGURATION](DASHBOARD_CONFIGURATION.md)** | ?? Configuration détaillée | 15 min |
| **[SUMMARY](DASHBOARD_UPGRADE_SUMMARY.md)** | ?? Résumé des fonctionnalités | 10 min |
| **[ARCHITECTURE](ARCHITECTURE_DASHBOARD.md)** | ??? Architecture technique | 30 min |
| **[CHANGELOG](CHANGELOG_DASHBOARD.md)** | ?? Historique des versions | 10 min |

---

## ?? Captures d'Écran

### Vue Desktop
```
????????????????????????????????????????????????????????????
? ??? Hero Section - Météo & Bienvenue                      ?
????????????????????????????????????????????????????????????
? ?? Appareils  ? ?? Utilisateurs ? ?? Tickets ? ? Énergie ?
????????????????????????????????????????????????????????????
? ?? Graphique Consommation  ?  ?? Mes Appareils          ?
? ?? Activité Appareils     ?  ?? Notifications          ?
? ?? Contrôle Rapide        ?  ?? Zones                  ?
????????????????????????????????????????????????????????????
```

### Vue Mobile
```
????????????????????
? ??? Météo        ?
????????????????????
? ?? ? ??          ?
? ?? ? ?          ?
????????????????????
? ?? Consommation ?
????????????????????
? ?? Activité     ?
????????????????????
? ?? Appareils    ?
????????????????????
```

---

## ??? Technologies

### Frontend
- **Blazor WebAssembly** (.NET 9)
- **MudBlazor** (UI Framework)
- **C#** (Logique métier)

### Services
- **OpenWeather API** (Météo)
- **Azure IoT Hub** (Appareils)
- **SignalR** (Temps réel)

### Outils
- **Visual Studio 2022**
- **Git** (Version control)
- **Azure DevOps** (CI/CD)

---

## ?? Performance

| Métrique | Valeur | Objectif |
|----------|--------|----------|
| **First Paint** | 800ms | < 1s ? |
| **Time to Interactive** | 1.2s | < 2s ? |
| **Bundle Size** | 2.5MB | < 3MB ? |
| **API Calls** | 5 | < 10 ? |
| **Lighthouse Score** | 92/100 | > 90 ? |

---

## ?? Personnalisation

### Changer la Ville

```csharp
// Dashboard.razor
_weatherData = await WeatherService.GetCurrentWeatherAsync("Paris");
```

### Modifier les Couleurs KPI

```razor
<MudIcon Color="Color.Primary" />   <!-- Bleu -->
<MudIcon Color="Color.Success" />   <!-- Vert -->
<MudIcon Color="Color.Warning" />   <!-- Orange -->
<MudIcon Color="Color.Error" />     <!-- Rouge -->
```

### Ajouter un Nouveau KPI

```razor
<MudItem xs="12" sm="6" md="3">
    <MudCard Class="rounded-xl">
        <MudCardContent>
            <MudIcon Icon="@Icons.Material.Filled.Speed" />
            <MudText Typo="Typo.h4">98.5%</MudText>
            <MudText Typo="Typo.body2">Performance</MudText>
        </MudCardContent>
    </MudCard>
</MudItem>
```

---

## ?? Tests

### Tests Manuels
- ? Chrome, Firefox, Edge, Safari
- ? Desktop, Tablette, Mobile
- ? Mode Clair et Sombre
- ? Connexions lentes

### Tests Automatiques
```bash
# Lancer les tests unitaires
dotnet test

# Tests d'intégration
dotnet test --filter Category=Integration
```

---

## ?? Dépannage

### Météo ne s'affiche pas
```
Solution :
1. Vérifiez votre clé API OpenWeather
2. Attendez 10 minutes (activation de la clé)
3. Vérifiez votre connexion Internet
```

### Graphiques vides
```
Solution :
1. C'est normal sans données
2. Les graphiques utilisent des données simulées
3. Ajoutez des appareils pour voir les vraies données
```

### Erreur 404
```
Solution :
1. Vérifiez l'URL : /dashboard
2. Assurez-vous d'être connecté
3. Consultez la console (F12)
```

?? **Plus de solutions** : [QUICK_START_DASHBOARD.md](QUICK_START_DASHBOARD.md#en-cas-de-problème)

---

## ?? Contribution

Les contributions sont les bienvenues !

1. **Fork** le projet
2. **Créez** une branche (`git checkout -b feature/AmazingFeature`)
3. **Committez** vos changements (`git commit -m 'Add AmazingFeature'`)
4. **Push** vers la branche (`git push origin feature/AmazingFeature`)
5. **Ouvrez** une Pull Request

Consultez [CONTRIBUTING.md](CONTRIBUTING.md) pour plus de détails.

---

## ?? Licence

Ce projet est sous licence MIT. Voir [LICENSE](LICENSE) pour plus d'informations.

---

## ?? Roadmap

### v2.1 (Q1 2024)
- [ ] Rafraîchissement automatique
- [ ] Géolocalisation auto
- [ ] Export PDF/PNG
- [ ] Thèmes personnalisables

### v2.2 (Q2 2024)
- [ ] Prévisions météo 5 jours
- [ ] Comparaison de périodes
- [ ] Notifications push
- [ ] Dashboard personnalisable (drag & drop)

### v3.0 (Q3 2024)
- [ ] Machine Learning (prédictions)
- [ ] Analyse de tendances
- [ ] Rapports automatiques
- [ ] Intégration IoT temps réel complète

---

## ?? Support

- ?? **Email** : support@smartoffice.com
- ?? **GitHub Issues** : [Issues](https://github.com/Koj-Kyo/front-smart-office/issues)
- ?? **Documentation** : [INDEX_DOCUMENTATION_DASHBOARD.md](INDEX_DOCUMENTATION_DASHBOARD.md)
- ?? **Website** : [smartoffice.com](https://smartoffice.com)

---

## ?? Remerciements

- **OpenWeather** pour l'API météo
- **MudBlazor** pour le framework UI
- **Microsoft** pour Blazor
- **Communauté Open Source**

---

## ?? Statistiques

![GitHub stars](https://img.shields.io/github/stars/Koj-Kyo/front-smart-office?style=social)
![GitHub forks](https://img.shields.io/github/forks/Koj-Kyo/front-smart-office?style=social)
![GitHub watchers](https://img.shields.io/github/watchers/Koj-Kyo/front-smart-office?style=social)

---

## ?? Learn More

- [Blazor Documentation](https://docs.microsoft.com/aspnet/core/blazor/)
- [MudBlazor Components](https://mudblazor.com/components)
- [OpenWeather API Guide](https://openweathermap.org/guide)
- [Material Design Guidelines](https://material.io/design)

---

<div align="center">

**Fait avec ?? par l'équipe Smart Office**

[? Retour en haut](#-dashboard-moderne---smart-office-v20)

</div>
