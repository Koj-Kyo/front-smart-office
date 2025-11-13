# ? Checklist MudSelect - Bonnes pratiques

## ?? Checklist à suivre lors de l'ajout d'un MudSelect

Utilisez cette checklist chaque fois que vous ajoutez un composant `MudSelect` dans votre application Blazor.

---

## ?? Avant d'écrire le code

- [ ] **Identifier le type de données**
  - Type primitif ? (`int`, `string`, `bool`)
  - Type personnalisé ? (`MyDto`, `MyEnum`)
  - Type nullable ? (`int?`, `string?`)

- [ ] **Identifier si le champ est obligatoire**
  - Obligatoire ? Utiliser un type **non-nullable** (`T="int"`)
  - Optionnel ? Utiliser un type **nullable** (`T="int?"`)

---

## ?? Template de code correct

### Pour un int nullable (filtres optionnels)

```razor
<MudSelect T="int?" @bind-Value="selectedValue" Label="Sélectionnez" Clearable>
    @foreach (var item in items)
    {
        <MudSelectItem T="int?" Value="@((int?)item.Id)">@item.Name</MudSelectItem>
    }
</MudSelect>
```

**Checklist :**
- [x] `MudSelect` a `T="int?"`
- [x] `MudSelectItem` a `T="int?"`
- [x] `Value` a un cast explicite `(int?)`
- [x] `Clearable` est présent pour permettre la désélection

---

### Pour un int non-nullable (champs obligatoires)

```razor
<MudSelect T="int" @bind-Value="selectedValue" Label="Sélectionnez" Required="true">
    @foreach (var item in items)
    {
        <MudSelectItem T="int" Value="@item.Id">@item.Name</MudSelectItem>
    }
</MudSelect>
```

**Checklist :**
- [x] `MudSelect` a `T="int"`
- [x] `MudSelectItem` a `T="int"`
- [x] `Value` est directement `item.Id` (pas de cast)
- [x] `Required="true"` est présent
- [x] `RequiredError` est défini avec un message clair

---

### Pour un string nullable (filtres optionnels)

```razor
<MudSelect T="string?" @bind-Value="selectedValue" Label="Sélectionnez" Clearable>
    @foreach (var item in items)
    {
        <MudSelectItem T="string?" Value="@item.Name">@item.Name</MudSelectItem>
    }
</MudSelect>
```

**Checklist :**
- [x] `MudSelect` a `T="string?"`
- [x] `MudSelectItem` a `T="string?"`
- [x] `Clearable` est présent

---

### Pour un string non-nullable (champs obligatoires)

```razor
<MudSelect T="string" @bind-Value="selectedValue" Label="Sélectionnez" Required="true">
    @foreach (var item in items)
    {
        <MudSelectItem T="string" Value="@item.Name">@item.Name</MudSelectItem>
    }
</MudSelect>
```

**Checklist :**
- [x] `MudSelect` a `T="string"`
- [x] `MudSelectItem` a `T="string"`
- [x] `Required="true"` est présent

---

### Pour un enum

```razor
<MudSelect T="MyEnum" @bind-Value="selectedEnum" Label="Type">
    <MudSelectItem T="MyEnum" Value="@MyEnum.Option1">Option 1</MudSelectItem>
    <MudSelectItem T="MyEnum" Value="@MyEnum.Option2">Option 2</MudSelectItem>
</MudSelect>
```

**Checklist :**
- [x] `MudSelect` a `T="MyEnum"`
- [x] `MudSelectItem` a `T="MyEnum"`
- [x] Les valeurs sont des constantes d'enum

---

### Pour un objet personnalisé

```razor
<MudSelect T="MyDto" @bind-Value="selectedObject" Label="Objet" ToStringFunc="@(x => x.Name)">
    @foreach (var item in items)
    {
        <MudSelectItem T="MyDto" Value="@item">@item.Name</MudSelectItem>
    }
</MudSelect>
```

**Checklist :**
- [x] `MudSelect` a `T="MyDto"`
- [x] `MudSelectItem` a `T="MyDto"`
- [x] `ToStringFunc` est défini pour l'affichage
- [x] `Value` contient l'objet complet

---

## ?? Erreurs courantes à éviter

### ? Erreur #1 : Oublier le type générique sur MudSelectItem

```razor
<!-- MAUVAIS -->
<MudSelect T="int?" @bind-Value="selectedId">
    <MudSelectItem Value="@item.Id">Item</MudSelectItem>  <!-- ? Pas de T -->
</MudSelect>

<!-- BON -->
<MudSelect T="int?" @bind-Value="selectedId">
    <MudSelectItem T="int?" Value="@((int?)item.Id)">Item</MudSelectItem>  <!-- ? T spécifié -->
</MudSelect>
```

---

### ? Erreur #2 : Types incompatibles entre parent et enfant

```razor
<!-- MAUVAIS -->
<MudSelect T="int?" @bind-Value="selectedId">
    <MudSelectItem T="int" Value="@item.Id">Item</MudSelectItem>  <!-- ? int au lieu de int? -->
</MudSelect>

<!-- BON -->
<MudSelect T="int?" @bind-Value="selectedId">
    <MudSelectItem T="int?" Value="@((int?)item.Id)">Item</MudSelectItem>  <!-- ? Types correspondent -->
</MudSelect>
```

---

### ? Erreur #3 : Oublier le cast explicite pour nullable

```razor
<!-- MAUVAIS -->
<MudSelect T="int?" @bind-Value="selectedId">
    <MudSelectItem T="int?" Value="@item.Id">Item</MudSelectItem>  <!-- ? item.Id est int, pas int? -->
</MudSelect>

<!-- BON -->
<MudSelect T="int?" @bind-Value="selectedId">
    <MudSelectItem T="int?" Value="@((int?)item.Id)">Item</MudSelectItem>  <!-- ? Cast explicite -->
</MudSelect>
```

---

### ? Erreur #4 : Utiliser nullable pour un champ obligatoire

```razor
<!-- MAUVAIS -->
<MudSelect T="int?" @bind-Value="requiredId" Required="true">  <!-- ? Nullable + Required -->
    <MudSelectItem T="int?" Value="@((int?)item.Id)">Item</MudSelectItem>
</MudSelect>

<!-- BON -->
<MudSelect T="int" @bind-Value="requiredId" Required="true">  <!-- ? Non-nullable pour required -->
    <MudSelectItem T="int" Value="@item.Id">Item</MudSelectItem>
</MudSelect>
```

---

## ?? Tests à effectuer après l'ajout

- [ ] **Compilation**
  - Le code compile sans erreurs
  - Aucun warning de type

- [ ] **Runtime - Sélection**
  - Je peux sélectionner une valeur
  - La valeur sélectionnée s'affiche correctement
  - Le `@bind-Value` fonctionne dans les deux sens

- [ ] **Runtime - Désélection** (si Clearable)
  - Je peux cliquer sur le bouton "X"
  - La valeur devient `null`
  - L'interface réagit correctement

- [ ] **Console navigateur**
  - Aucune erreur JavaScript
  - Aucune erreur `InvalidCastException`
  - Aucune erreur MudBlazor

- [ ] **Validation** (si Required)
  - Le champ est marqué invalide si vide
  - Le message d'erreur s'affiche
  - La validation se déclenche au bon moment

---

## ?? Code review checklist

Lors de la revue de code, vérifiez :

- [ ] Tous les `MudSelect` ont un `T` explicite
- [ ] Tous les `MudSelectItem` ont un `T` correspondant au parent
- [ ] Les casts sont corrects (nullable ? non-nullable)
- [ ] `Clearable` est présent pour les champs optionnels
- [ ] `Required` est présent pour les champs obligatoires
- [ ] `RequiredError` a un message clair et en français
- [ ] Les libellés (`Label`) sont clairs et en français
- [ ] Le `@bind-Value` cible la bonne propriété

---

## ?? Outils de diagnostic

### Si une erreur survient à l'exécution

1. **Vérifier la console du navigateur (F12)**
   ```
   Chercher : "InvalidCastException", "Unable to set property", "IMudShadowSelect"
   ```

2. **Vérifier les types dans le @code**
   ```csharp
   // La propriété liée doit correspondre au type T du MudSelect
   private int? selectedId;  // ? Correspond à T="int?"
   private int requiredId;   // ? Correspond à T="int"
   ```

3. **Vérifier le type de la source de données**
   ```csharp
   // Si items a un Id de type int, il faut un cast pour int?
   <MudSelectItem T="int?" Value="@((int?)item.Id)">  // ?
   ```

---

## ?? Tips & Astuces

### Tip #1 : Utiliser des snippets
Créez des snippets dans Visual Studio pour les cas courants :

**Snippet "mudselect-int-nullable"**
```razor
<MudSelect T="int?" @bind-Value="$selected$" Label="$label$" Clearable>
    @foreach (var item in $items$)
    {
        <MudSelectItem T="int?" Value="@((int?)item.Id)">@item.Name</MudSelectItem>
    }
</MudSelect>
```

---

### Tip #2 : Commenter les types complexes
Pour les types personnalisés, ajoutez un commentaire :

```razor
@* MudSelect<MyDto> - Utilise ToStringFunc pour l'affichage *@
<MudSelect T="MyDto" @bind-Value="selectedObject" ToStringFunc="@(x => x.Name)">
    ...
</MudSelect>
```

---

### Tip #3 : Tester avec des valeurs null
Toujours tester votre MudSelect avec :
- Une valeur sélectionnée
- Aucune valeur (`null`)
- La désélection (si `Clearable`)

---

## ?? Ressources

- [Documentation MudBlazor Select](https://mudblazor.com/components/select)
- [Guide des composants génériques MudBlazor](https://mudblazor.com/getting-started/usage#generic-components)
- [Corrections appliquées dans ce projet](./CORRECTIONS_MUDSELECT_TOUTES_PAGES.md)

---

## ?? Résumé en une ligne

> **Règle d'or :** Le type `T` du `MudSelect` DOIT correspondre EXACTEMENT au type `T` de TOUS ses `MudSelectItem`, avec un cast explicite si nécessaire.

---

**Date de création :** ${new Date().toLocaleDateString('fr-FR')}  
**Version :** 1.0  
**Auteur :** GitHub Copilot - Équipe MySmartApp
