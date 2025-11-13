# ? Fix MudSelect - Devices.razor

## Problème
`InvalidCastException: Specified cast is not valid` ? Écran noir sur `/devices`

## Cause
MudBlazor exige que `MudSelect<T>` et `MudSelectItem<T>` aient le **même type générique**.

## Solution

### 1. Pour `int?` (nullable)
```razor
<MudSelect T="int?" @bind-Value="selectedZoneFilter">
    <MudSelectItem T="int?" Value="@((int?)zone.Id)">@zone.Name</MudSelectItem>
    <!--           ^^^^^^^         ^^^^^^^^^^^^^^^^ Cast obligatoire -->
</MudSelect>
```

### 2. Pour `int` (non-nullable)
```razor
<MudSelect T="int" @bind-Value="editingDevice.ZoneId">
    <MudSelectItem T="int" Value="@zone.Id">@zone.Name</MudSelectItem>
    <!--           ^^^^^^         ^^^^^^^^^ Pas de cast nécessaire -->
</MudSelect>
```

## Règle d'or MudBlazor
**Toujours spécifier `T` explicitement sur `MudSelect` ET `MudSelectItem`**

## Résultat
? Page `/devices` fonctionne  
? Filtres opérationnels  
? Dialogs opérationnels  
? Pas d'erreurs console
