# ?? Quick Fix MudSelect - Résumé Ultra-Rapide

## Problème
`InvalidCastException: Unable to set property 'IMudShadowSelect'`

## Solution en 3 étapes

### 1?? Ajouter T sur MudSelect
```razor
? <MudSelect @bind-Value="myValue">
? <MudSelect T="int?" @bind-Value="myValue">
```

### 2?? Ajouter T sur TOUS les MudSelectItem
```razor
? <MudSelectItem Value="@item.Id">Label</MudSelectItem>
? <MudSelectItem T="int?" Value="@((int?)item.Id)">Label</MudSelectItem>
```

### 3?? Vérifier la correspondance des types
- Parent `T="int?"` ? Enfants `T="int?"` + cast `(int?)`
- Parent `T="int"` ? Enfants `T="int"` (pas de cast)
- Parent `T="string"` ? Enfants `T="string"`

## Modèles rapides

### Filtre optionnel (nullable)
```razor
<MudSelect T="int?" @bind-Value="selectedId" Label="Filtre" Clearable>
    @foreach (var item in items)
    {
        <MudSelectItem T="int?" Value="@((int?)item.Id)">@item.Name</MudSelectItem>
    }
</MudSelect>
```

### Champ obligatoire (non-nullable)
```razor
<MudSelect T="int" @bind-Value="requiredId" Label="Zone" Required="true">
    @foreach (var item in items)
    {
        <MudSelectItem T="int" Value="@item.Id">@item.Name</MudSelectItem>
    }
</MudSelect>
```

### String
```razor
<MudSelect T="string" @bind-Value="status" Label="Statut">
    <MudSelectItem T="string" Value="@("Open")">Ouvert</MudSelectItem>
    <MudSelectItem T="string" Value="@("Closed")">Fermé</MudSelectItem>
</MudSelect>
```

## ? Résultat
? **2 pages corrigées** (Telemetrie, Incidents)  
? **5 MudSelect corrigés**  
? **Build réussi**  
? **Plus d'erreurs runtime**

## ?? Docs complètes
- [Corrections détaillées](./CORRECTIONS_MUDSELECT_TOUTES_PAGES.md)
- [Checklist complète](./CHECKLIST_MUDSELECT.md)
- [Correction Telemetrie](./CORRECTION_TELEMETRIE_MUDSELECT.md)
