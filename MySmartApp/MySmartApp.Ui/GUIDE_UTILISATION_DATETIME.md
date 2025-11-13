# ?? Guide d'Utilisation - DateTime & Extensions

## ?? Introduction

Ce guide explique comment utiliser les nouvelles fonctionnalités de gestion des dates et heures dans l'application MySmartApp.Ui après les corrections apportées le 16 janvier 2025.

---

## ? Nouvelles Fonctionnalités

### 1. Désérialisation JSON Flexible

Les dates sont maintenant désérialisées automatiquement quel que soit leur format :

```json
// ? Tous ces formats fonctionnent maintenant
"2025-01-16T06:30:00.1234567Z"
"2025-01-16T06:30:00Z"
"2025-01-16 06:30:00"
"2025-01-16T06:30:00+01:00"
```

**Aucune action requise** - Cela fonctionne automatiquement !

---

### 2. Extensions DateTime

15+ méthodes d'extension pour faciliter la manipulation des dates.

---

## ?? Guide d'Utilisation des Extensions

### Validation de Date

#### `IsValid()`
Vérifie si une date est valide (pas `MinValue` ou `MaxValue`).

```razor
@if (device.LastActivityTime.IsValid())
{
    <span>Dernière connexion : @device.LastActivityTime</span>
}
else
{
    <span class="text-muted">Jamais connecté</span>
}
```

**Quand l'utiliser** :
- ? Avant d'afficher une date
- ? Avant de formater une date
- ? Dans les conditions d'affichage

---

### Affichage "Friendly"

#### `ToFriendlyString()`
Convertit une date en format relatif lisible.

```razor
<span>@device.LastActivityTime.ToFriendlyString()</span>
```

**Exemples de sortie** :
- `"À l'instant"` (< 1 minute)
- `"Il y a 5 min"` (< 1 heure)
- `"Il y a 2 h"` (< 24 heures)
- `"Hier"` (1 jour)
- `"Il y a 3 jours"` (< 1 semaine)
- `"Il y a 2 semaines"` (< 1 mois)
- `"Il y a 3 mois"` (< 1 an)
- `"Il y a 2 ans"` (> 1 an)

**Quand l'utiliser** :
- ? Affichage de dates d'activité
- ? Dates de dernière connexion
- ? Historique d'événements

---

### Formats Français

#### `ToShortFrenchString()`
Format court : `dd/MM/yyyy`

```razor
<span>Date : @ticket.CreatedAt.ToShortFrenchString()</span>
```

**Sortie** : `16/01/2025`

---

#### `ToLongFrenchString()`
Format long : `dd/MM/yyyy HH:mm`

```razor
<span>Créé le : @ticket.CreatedAt.ToLongFrenchString()</span>
```

**Sortie** : `16/01/2025 06:30`

---

#### `ToFullFrenchString()`
Format complet : `dd mois yyyy à HH:mm`

```razor
<span>@ticket.CreatedAt.ToFullFrenchString()</span>
```

**Sortie** : `16 janvier 2025 à 06:30`

---

### Icônes & Couleurs

#### `GetAgeIcon()`
Retourne une icône FontAwesome selon l'âge de la date.

```razor
<MudIcon Icon="@device.LastActivityTime.GetAgeIcon()" />
```

**Icônes retournées** :
- `fa-clock` : < 5 minutes (très récent)
- `fa-history` : < 1 heure (récent)
- `fa-calendar-day` : < 1 jour (aujourd'hui)
- `fa-calendar-week` : < 1 semaine (cette semaine)
- `fa-calendar` : > 1 semaine (ancien)

---

#### `GetAgeColor()`
Retourne une couleur MudBlazor selon l'âge.

```razor
<MudChip Color="@device.LastActivityTime.GetAgeColor()">
    @device.LastActivityTime.ToFriendlyString()
</MudChip>
```

**Couleurs retournées** :
- `Color.Success` (vert) : < 5 minutes
- `Color.Info` (bleu) : < 1 heure
- `Color.Warning` (orange) : < 1 jour
- `Color.Error` (rouge) : > 1 jour

---

### Exemple Complet : Badge d'Activité

```razor
<MudChip Icon="@device.LastActivityTime.GetAgeIcon()" 
         Color="@device.LastActivityTime.GetAgeColor()"
         Size="Size.Small">
    @device.LastActivityTime.ToFriendlyString("Jamais vu")
</MudChip>
```

**Résultat visuel** :
- ?? Horloge verte : "À l'instant"
- ?? Historique bleu : "Il y a 30 min"
- ?? Calendrier orange : "Il y a 8 h"
- ?? Calendrier rouge : "Il y a 3 jours"

---

### Calculs de Durée

#### `GetDurationString()`
Calcule et formate la durée entre deux dates.

```razor
<span>
    Durée : @ticket.CreatedAt.GetDurationString(ticket.UpdatedAt ?? DateTime.UtcNow)
</span>
```

**Exemples de sortie** :
- `"30 sec"`
- `"15 min"`
- `"2 h 30 min"`
- `"3 j 5 h"`
- `"14 jours"`

---

### Calculs de Période

#### Début/Fin de Journée

```razor
@code {
    private DateTime startDate = DateTime.Today.StartOfDay();  // 00:00:00
    private DateTime endDate = DateTime.Today.EndOfDay();      // 23:59:59.999
}
```

---

#### Début/Fin de Semaine

```razor
@code {
    private DateTime startOfWeek = DateTime.Today.StartOfWeek();  // Lundi
    private DateTime endOfWeek = DateTime.Today.AddDays(6);       // Dimanche
}
```

---

#### Début/Fin de Mois

```razor
@code {
    private DateTime startOfMonth = DateTime.Today.StartOfMonth();  // 1er du mois
    private DateTime endOfMonth = DateTime.Today.EndOfMonth();      // Dernier jour
}
```

---

### Tests de Date

#### `IsToday()`
```razor
@if (notification.Horodatage.IsToday())
{
    <MudBadge Color="Color.Success">Nouveau</MudBadge>
}
```

---

#### `IsPast()` / `IsFuture()`
```razor
@if (event.Date.IsFuture())
{
    <span>À venir</span>
}
else if (event.Date.IsPast())
{
    <span>Passé</span>
}
```

---

## ?? Exemples par Composant

### Page Devices

```razor
<MudTable Items="@devices">
    <HeaderContent>
        <MudTh>Device</MudTh>
        <MudTh>Dernière activité</MudTh>
        <MudTh>État</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd DataLabel="Device">@context.DeviceId</MudTd>
        <MudTd DataLabel="Dernière activité">
            @if (context.LastActivityTime.IsValid())
            {
                <MudChip Icon="@context.LastActivityTime.GetAgeIcon()" 
                         Color="@context.LastActivityTime.GetAgeColor()"
                         Size="Size.Small">
                    @context.LastActivityTime.ToFriendlyString()
                </MudChip>
            }
            else
            {
                <MudText Typo="Typo.body2" Color="Color.Default">Jamais connecté</MudText>
            }
        </MudTd>
        <MudTd DataLabel="État">
            <MudChip Color="@(context.IsConnected ? Color.Success : Color.Error)">
                @(context.IsConnected ? "Connecté" : "Déconnecté")
            </MudChip>
        </MudTd>
    </RowTemplate>
</MudTable>
```

---

### Page Incidents

```razor
<MudCard>
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">@ticket.Sujet</MudText>
            <MudText Typo="Typo.body2" Color="Color.Default">
                Créé @ticket.CreatedAt.ToFriendlyString()
            </MudText>
        </CardHeaderContent>
    </MudCardHeader>
    <MudCardContent>
        <MudText>@ticket.Description</MudText>
        
        @if (ticket.UpdatedAt.IsValid())
        {
            <MudDivider Class="my-2" />
            <MudText Typo="Typo.caption" Color="Color.Default">
                Dernière mise à jour : @ticket.UpdatedAt.ToLongFrenchString()
            </MudText>
            <MudText Typo="Typo.caption" Color="Color.Default">
                Durée : @ticket.CreatedAt.GetDurationString(ticket.UpdatedAt.Value)
            </MudText>
        }
    </MudCardContent>
    <MudCardActions>
        <MudButton Color="Color.Primary">Voir détails</MudButton>
    </MudCardActions>
</MudCard>
```

---

### Page Télémétrie

```razor
<MudTimeline>
    @foreach (var event in telemetryEvents)
    {
        <MudTimelineItem Color="@event.Horodatage.GetAgeColor()"
                         Icon="@event.Horodatage.GetAgeIcon()">
            <MudText Typo="Typo.body1">@event.TypeEvenement</MudText>
            <MudText Typo="Typo.body2" Color="Color.Default">
                @event.Horodatage.ToFriendlyString()
            </MudText>
            <MudText Typo="Typo.caption">
                Valeur : @event.Valeur.ToString("F2") @event.Unit
            </MudText>
        </MudTimelineItem>
    }
</MudTimeline>
```

---

### Filtre de Période

```razor
<MudGrid>
    <MudItem xs="12" md="6">
        <MudDatePicker Label="Date de début" 
                       @bind-Date="startDate" 
                       MaxDate="DateTime.Today.EndOfDay()" />
    </MudItem>
    <MudItem xs="12" md="6">
        <MudDatePicker Label="Date de fin" 
                       @bind-Date="endDate" 
                       MaxDate="DateTime.Today.EndOfDay()" />
    </MudItem>
    <MudItem xs="12">
        <MudButtonGroup>
            <MudButton OnClick="@(() => SetPeriod(PeriodType.Today))">
                Aujourd'hui
            </MudButton>
            <MudButton OnClick="@(() => SetPeriod(PeriodType.Week))">
                Cette semaine
            </MudButton>
            <MudButton OnClick="@(() => SetPeriod(PeriodType.Month))">
                Ce mois
            </MudButton>
        </MudButtonGroup>
    </MudItem>
</MudGrid>

@code {
    private DateTime? startDate;
    private DateTime? endDate;

    private enum PeriodType { Today, Week, Month }

    private void SetPeriod(PeriodType type)
    {
        switch (type)
        {
            case PeriodType.Today:
                startDate = DateTime.Today.StartOfDay();
                endDate = DateTime.Today.EndOfDay();
                break;
            case PeriodType.Week:
                startDate = DateTime.Today.StartOfWeek();
                endDate = DateTime.Today;
                break;
            case PeriodType.Month:
                startDate = DateTime.Today.StartOfMonth();
                endDate = DateTime.Today.EndOfMonth();
                break;
        }
    }
}
```

---

## ?? Pièges à Éviter

### ? Ne pas vérifier la validité

```razor
<!-- MAUVAIS -->
<span>@device.LastActivityTime.ToString("dd/MM/yyyy")</span>
<!-- Peut planter si date invalide -->
```

### ? Toujours valider

```razor
<!-- BON -->
@if (device.LastActivityTime.IsValid())
{
    <span>@device.LastActivityTime.ToShortFrenchString()</span>
}
else
{
    <span>-</span>
}
```

---

### ? Utiliser ToString() directement

```razor
<!-- MAUVAIS -->
<span>@device.LastActivityTime.ToString()</span>
<!-- Format non contrôlé, peut être illisible -->
```

### ? Utiliser les extensions

```razor
<!-- BON -->
<span>@device.LastActivityTime.ToLongFrenchString("Jamais vu")</span>
```

---

## ?? Configuration Avancée

### Personnaliser le Fallback

Toutes les méthodes d'affichage acceptent un paramètre `fallback` pour personnaliser le texte affiché si la date est invalide :

```razor
@device.LastActivityTime.ToFriendlyString("Non disponible")
@device.LastActivityTime.ToShortFrenchString("--/--/----")
@device.LastActivityTime.ToLongFrenchString("Aucune donnée")
```

---

### Combiner avec MudBlazor

```razor
<MudTooltip Text="@device.LastActivityTime.ToFullFrenchString()">
    <MudChip Icon="@device.LastActivityTime.GetAgeIcon()" 
             Color="@device.LastActivityTime.GetAgeColor()">
        @device.LastActivityTime.ToFriendlyString()
    </MudChip>
</MudTooltip>
```

---

## ?? Tableau Récapitulatif

| Méthode | Format | Exemple de sortie |
|---------|--------|-------------------|
| `ToFriendlyString()` | Relatif | "Il y a 5 min" |
| `ToShortFrenchString()` | Court | "16/01/2025" |
| `ToLongFrenchString()` | Long | "16/01/2025 06:30" |
| `ToFullFrenchString()` | Complet | "16 janvier 2025 à 06:30" |
| `GetAgeIcon()` | Icône | "fa-clock" |
| `GetAgeColor()` | Couleur | `Color.Success` |
| `GetDurationString()` | Durée | "2 j 5 h" |
| `IsValid()` | Booléen | `true`/`false` |
| `IsToday()` | Booléen | `true`/`false` |
| `StartOfDay()` | DateTime | 00:00:00 |
| `EndOfMonth()` | DateTime | Dernier jour 23:59:59 |

---

## ?? Bonnes Pratiques

1. **Toujours valider** : Utilisez `IsValid()` avant de formater
2. **Utiliser les extensions** : Préférez `ToFriendlyString()` à `ToString()`
3. **Fournir un fallback** : Définissez toujours un texte par défaut
4. **Combiner icône + couleur** : Pour une meilleure UX
5. **Tester les cas limites** : Dates null, MinValue, MaxValue

---

## ?? Aller Plus Loin

### Créer des Extensions Personnalisées

Vous pouvez étendre `DateTimeExtensions.cs` avec vos propres méthodes :

```csharp
public static class CustomDateTimeExtensions
{
    public static string ToCustomFormat(this DateTime date)
    {
        // Votre logique personnalisée
        return date.ToString("votre format");
    }
}
```

---

### Localisation

Pour supporter d'autres langues, créez des fichiers d'extension séparés :

```csharp
// DateTimeExtensions.en.cs
public static string ToFriendlyStringEn(this DateTime date)
{
    // English version
}
```

---

## ?? Ressources

- **Guide technique** : `CORRECTION_DATETIME_DESERIALISATION.md`
- **Code source** : `Extensions/DateTimeExtensions.cs`
- **Exemples** : Ce fichier

---

## ?? Support

En cas de problème :
1. Vérifiez que la date est valide avec `IsValid()`
2. Consultez la documentation technique
3. Vérifiez les logs pour les avertissements de désérialisation

---

*Guide créé le 16 janvier 2025*  
*Version : 1.0*  
*Auteur : Équipe MySmartApp*
