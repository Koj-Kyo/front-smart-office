# ?? ANALYSE COMPLÈTE - PAGE INCIDENTS

**Route:** `/incidents`  
**Autorisation:** ? `[Authorize]` - Connexion requise  
**Objectif:** Gestion des tickets d'incidents techniques IoT

---

## ?? ENDPOINTS BACKEND REQUIS

### 1. GET - Liste Tous les Tickets
```http
GET /api/incidents
Authorization: Bearer {token}
```

**Réponse attendue:**
```json
[
  {
    "id": 1,
    "sujet": "Problème éclairage",
    "description": "L'éclairage ne s'allume plus",
    "statut": "Ouvert",
    "priorite": "Moyenne",
    "createdById": "user-guid",
    "createdByName": "John Doe",
    "assignedToId": null,
    "assignedToName": null,
    "zoneId": 5,
    "zoneName": "Bureau Principal",
    "peripheriqueLogiqueId": null,
    "peripheriqueName": null,
    "createdAt": "2025-01-30T10:00:00Z",
    "updatedAt": null
  }
]
```

---

### 2. GET - Filtrer par Statut
```http
GET /api/incidents?status=En attente
Authorization: Bearer {token}
```

**Réponse:** Liste filtrée

---

### 3. GET - Tickets Utilisateur
```http
GET /api/incidents/user/{userId}
Authorization: Bearer {token}
```

**Réponse:** Tickets créés/assignés à l'utilisateur

---

### 4. GET - Ticket par ID
```http
GET /api/incidents/{id}
Authorization: Bearer {token}
```

**Réponse:** Détails complets du ticket

---

### 5. POST - Créer Ticket
```http
POST /api/incidents
Content-Type: application/json
Authorization: Bearer {token}

{
  "sujet": "Problème capteur",
  "description": "Le capteur ne répond plus",
  "statut": "Ouvert",
  "zoneId": 5,
  "peripheriqueLogiqueId": null
}
```

**Réponse:**
```json
{
  "id": 10,
  "sujet": "Problème capteur",
  ...
}
```

---

### 6. PUT - Mettre à Jour Statut
```http
PUT /api/incidents/{id}/status
Content-Type: application/json
Authorization: Bearer {token}

{
  "status": "Résolu"
}
```

**Réponse:** `204 No Content`

---

### 7. PUT - Assigner Ticket
```http
PUT /api/incidents/{id}/assign
Content-Type: application/json
Authorization: Bearer {token}

{
  "assignedToId": "user-guid-123"
}
```

**Réponse:** `204 No Content`

---

### 8. DELETE - Supprimer Ticket
```http
DELETE /api/incidents/{id}
Authorization: Bearer {token}
```

**Réponse:** `204 No Content`

---

## ?? DTOs NÉCESSAIRES

### Frontend (MySmartApp.Ui/Models/ApiModels.cs)

#### ? IncidentTicketDto (Response)
```csharp
public class IncidentTicketDto
{
    public int Id { get; set; }
    
    // Contenu du ticket
    public string Sujet { get; set; }
    public string Description { get; set; }
    public string Statut { get; set; }  // "Ouvert", "En cours", "Résolu", etc.
    public string? Priorite { get; set; }  // "Basse", "Moyenne", "Haute", "Critique"
    
    // Créateur
    public string CreatedById { get; set; }
    public string? CreatedByName { get; set; }
    
    // Assignation
    public string? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    
    // Ressource liée (optionnel)
    public int? ZoneId { get; set; }
    public string? ZoneName { get; set; }
    public int? PeripheriqueLogiqueId { get; set; }
    public string? PeripheriqueName { get; set; }
    
    // Métadonnées
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

#### ? IncidentTicketCreateDto (Request)
```csharp
public class IncidentTicketCreateDto
{
    public string Sujet { get; set; } = "";
    public string Description { get; set; } = "";
    public string Statut { get; set; } = "Ouvert";  // Statut initial
    public int? ZoneId { get; set; }
    public int? PeripheriqueLogiqueId { get; set; }
}
```

**Validation:**
- `Sujet`: **obligatoire**, max 100 caractères
- `Description`: **obligatoire**, max 500 caractères
- `Statut`: **obligatoire**, valeurs autorisées
- `ZoneId` et `PeripheriqueLogiqueId`: **optionnels**, nullable

---

## ?? SERVICES FRONTEND

### IIncidentTicketService

**Fichier:** `MySmartApp.Ui/Services/IncidentTicketService.cs`

**Interface attendue:**
```csharp
public interface IIncidentTicketService
{
    // CRUD
    Task<ApiResponse<IncidentTicketDto>> CreateTicketAsync(IncidentTicketCreateDto ticketDto);
    Task<ApiResponse<IncidentTicketDto>> GetTicketByIdAsync(int id);
    Task<ApiResponse<List<IncidentTicketDto>>> GetTicketsByStatusAsync(string status);
    Task<ApiResponse<List<IncidentTicketDto>>> GetTicketsByUserIdAsync(string userId);
    Task<ApiResponse> UpdateTicketStatusAsync(int id, string newStatus);
    Task<ApiResponse> AssignTicketAsync(int id, string assignedToId);
    Task<ApiResponse> DeleteTicketAsync(int id);
    
    // Utilitaires
    Task<ApiResponse<List<IncidentTicketDto>>> GetMyTicketsAsync();
    Task<ApiResponse<List<IncidentTicketDto>>> GetAllTicketsAsync();
    Task<List<string>> GetAvailableStatusesAsync();
    Task<Dictionary<string, int>> GetTicketStatisticsAsync();
    Task<string?> GetCurrentUserIdAsync();
}
```

**? Implémenté dans:** `MySmartApp.Ui/Services/IncidentTicketService.cs`

---

## ?? PERMISSIONS & RÔLES

### Authentification
- ? **Connexion obligatoire:** `[Authorize]`
- ?? **Pas de restriction de rôle globale**

### Actions Restreintes

#### Manager/Admin Seulement
```razor
<AuthorizeView Roles="Manager,Admin" Context="authContext">
    <MudIconButton Icon="@Icons.Material.Filled.AssignmentInd" 
                   OnClick="@(() => AssignTicket(context))"
                   Title="Assigner" />
</AuthorizeView>
```

#### Admin Seulement
```razor
<AuthorizeView Roles="Admin" Context="authContext">
    <MudIconButton Icon="@Icons.Material.Filled.Delete" 
                   OnClick="@(() => DeleteTicket(context))"
                   Title="Supprimer" />
</AuthorizeView>
```

---

## ?? DÉPENDANCES & COMPOSANTS

### Services Injectés
```csharp
@inject IIncidentTicketService IncidentTicketService  // ?
@inject IDialogService DialogService                  // ? MudBlazor
@inject ILogger<Incidents> Logger                     // ?
```

### Composants Utilisés
- `RequireAuth` : Wrapper autorisation
- `MudTable` : Affichage liste tickets
- `MudOverlay` : Dialogs création/détails
- `MudForm` : Validation formulaire
- `MudChip` : Badges statut/ID
- `MudIconButton` : Actions CRUD
- `AuthorizeView` : Restrictions rôles

---

## ?? FONCTIONNALITÉS UI

### Affichage

#### Statistiques Rapides
```razor
<MudGrid>
    @foreach (var stat in statistics.Take(4))
    {
        <MudPaper>
            <MudText Typo="Typo.h4">@stat.Value</MudText>
            <MudText>@stat.Key</MudText>
        </MudPaper>
    }
</MudGrid>
```

**Stats affichées:**
- Total tickets
- Ouvert
- En cours
- Résolu

---

#### Table Tickets
- [x] Affichage liste complète
- [x] Colonnes: ID, Sujet, Statut, Créé par, Assigné à, Ressource, Date
- [x] Filtrage par recherche texte
- [x] Filtrage par statut
- [x] Filtre "Mes tickets" (créés/assignés à moi)
- [x] Actions contextuelles (Voir, Résolu, Modifier, Assigner, Supprimer)

---

### Actions CRUD

#### CREATE
```razor
<MudOverlay @bind-Visible="isCreateDialogOpen">
    <MudForm @ref="createForm" Model="@newTicket">
        <MudTextField @bind-Value="newTicket.Sujet" Required />
        <MudTextField @bind-Value="newTicket.Description" Lines="4" Required />
        <MudSelect @bind-Value="newTicket.Statut" Required />
        <!-- Type ressource: Général / Zone / Périphérique -->
    </MudForm>
</MudOverlay>
```

**Champs:**
- Sujet (obligatoire)
- Description (obligatoire)
- Statut initial (select)
- Type ressource (optionnel)
- Zone ID / Périphérique ID (selon type)

---

#### READ
```csharp
var ticketsResponse = await IncidentTicketService.GetAllTicketsAsync();
```

**Méthode:** Récupère tous tickets via appels multiples par statut

---

#### UPDATE
```razor
<MudIconButton Icon="@Icons.Material.Filled.CheckCircle"
               OnClick="@(() => SetTicketResolved(context))" />
```

**Actions:**
- ? Marquer comme résolu (bouton direct)
- ?? Changer statut (dialog confirmation)

---

#### DELETE
```razor
<AuthorizeView Roles="Admin">
    <MudIconButton Icon="@Icons.Material.Filled.Delete" 
                   OnClick="@(() => DeleteTicket(context))" />
</AuthorizeView>
```

**Sécurité:** Admin uniquement

---

### Logique Métier UI

#### GetAllTicketsAsync()
```csharp
public async Task<ApiResponse<List<IncidentTicketDto>>> GetAllTicketsAsync()
{
    var allTickets = new List<IncidentTicketDto>();
    var statuses = await GetAvailableStatusesAsync();
    
    foreach (var status in statuses)
    {
        var response = await GetTicketsByStatusAsync(status);
        if (response.IsSuccess && response.Data != null)
        {
            allTickets.AddRange(response.Data);
        }
    }
    
    // Enlever doublons
    return allTickets
        .GroupBy(t => t.Id)
        .Select(g => g.First())
        .ToList();
}
```

**?? PROBLÈME POTENTIEL:**
- Fait N requêtes (1 par statut = 6 requêtes)
- Pas optimal si backend peut retourner tous tickets d'un coup

---

#### Filtrage Tickets
```csharp
private IEnumerable<IncidentTicketDto> filteredTickets =>
    tickets.Where(t => 
        // Recherche texte
        (string.IsNullOrEmpty(searchString) || 
         t.Sujet.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
         t.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
         t.CreatedByName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true) &&
        
        // Filtre statut
        (string.IsNullOrEmpty(selectedStatusFilter) || t.Statut == selectedStatusFilter) &&
        
        // Filtre mes tickets
        (!showMyTicketsOnly || IsMyTicket(t))
    );
```

---

## ?? PROBLÈMES POTENTIELS IDENTIFIÉS

### ?? MOYENNE

#### 1. GetAllTicketsAsync Inefficace
**Problème:** Fait 6 requêtes séparées (1 par statut)

**Solution recommandée:**

**Backend:**
```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<IncidentTicketDto>>> GetIncidents(
    [FromQuery] string? status = null)
{
    if (!string.IsNullOrEmpty(status))
    {
        var filtered = await _service.GetByStatusAsync(status);
        return Ok(filtered);
    }
    
    // ? Si pas de filtre, retourner TOUS les tickets
    var all = await _service.GetAllAsync();
    return Ok(all);
}
```

**Frontend:**
```csharp
public async Task<ApiResponse<List<IncidentTicketDto>>> GetAllTicketsAsync()
{
    try
    {
        _logger.LogInformation("?? Récupération de tous les tickets");
        
        // ? CORRECTION: 1 seule requête au lieu de N
        var response = await _apiService.GetAsync<List<IncidentTicketDto>>("/api/incidents");
        
        if (response.IsSuccess && response.Data != null)
        {
            _cachedTickets = response.Data;
            _cacheExpiry = DateTime.Now.Add(_cacheTimeout);
            _logger.LogInformation("? {Count} tickets récupérés", _cachedTickets.Count);
        }
        
        return response;
    }
    catch (Exception ex)
    {
        _logger.LogError("? Erreur: {Message}", ex.Message);
        return ApiResponse<List<IncidentTicketDto>>.Error($"Erreur: {ex.Message}");
    }
}
```

---

#### 2. Sélection Zone/Périphérique Manuelle
**Problème:** Saisie numérique ID au lieu de select

**Solution:**
```razor
<!-- Remplacer MudNumericField par MudSelect -->
@if (resourceType == "Zone")
{
    <MudSelect T="int?" @bind-Value="newTicket.ZoneId" Label="Zone">
        @foreach (var zone in availableZones)
        {
            <MudSelectItem Value="@zone.Id">@zone.Nom</MudSelectItem>
        }
    </MudSelect>
}
```

**Besoin:**
```csharp
@inject IBackendZoneService ZoneService
@inject IDeviceService DeviceService

private List<BackendZoneDto> availableZones = new();
private List<DeviceDto> availableDevices = new();

protected override async Task OnInitializedAsync()
{
    availableZones = (await ZoneService.GetAllZonesAsync()).Data ?? new();
    availableDevices = (await DeviceService.GetAllDevicesLegacyAsync()).ToList();
    
    // ...
}
```

---

#### 3. Pas de Champ Priorité
**Problème:** DTO backend a `Priorite` mais formulaire ne l'utilise pas

**Solution:**
```razor
<MudItem xs="12" sm="6">
    <MudSelect T="string" @bind-Value="newTicket.Priorite"
               Label="Priorité">
        <MudSelectItem Value="@("Basse")">Basse</MudSelectItem>
        <MudSelectItem Value="@("Moyenne")">Moyenne</MudSelectItem>
        <MudSelectItem Value="@("Haute")">Haute</MudSelectItem>
        <MudSelectItem Value="@("Critique")">?? Critique</MudSelectItem>
    </MudSelect>
</MudItem>
```

**Modifier DTO:**
```csharp
public class IncidentTicketCreateDto
{
    public string Sujet { get; set; } = "";
    public string Description { get; set; } = "";
    public string Statut { get; set; } = "Ouvert";
    public string Priorite { get; set; } = "Moyenne";  // ? Ajouter
    public int? ZoneId { get; set; }
    public int? PeripheriqueLogiqueId { get; set; }
}
```

---

#### 4. Dialog Changement Statut Basique
**Problème:** Change au statut suivant sans choix précis

**Solution:**
```csharp
private async Task UpdateTicketStatus(IncidentTicketDto ticket)
{
    var parameters = new DialogParameters
    {
        ["CurrentStatus"] = ticket.Statut,
        ["AvailableStatuses"] = availableStatuses,
        ["TicketId"] = ticket.Id
    };

    var dialog = await DialogService.ShowAsync<ChangeStatusDialog>("Changer le statut", parameters);
    var result = await dialog.Result;

    if (!result.Canceled && result.Data is string newStatus)
    {
        await IncidentTicketService.UpdateTicketStatusAsync(ticket.Id, newStatus);
        await LoadData();
    }
}
```

---

### ?? FAIBLE

#### 5. Pas de Pagination
**Problème:** Tous tickets chargés en mémoire

**Solution:** `MudTable` avec `ServerData`

#### 6. Pas de Tri Colonnes
**Problème:** Tri manuel seulement

**Solution:** `MudTable` avec `SortLabel`

---

## ?? RÉCAPITULATIF STATUS

| Élément | Status | Priorité |
|---------|--------|----------|
| **Endpoints Backend** | ?? À vérifier | ?? CRITIQUE |
| **DTOs Alignement** | ? OK | - |
| **Service Frontend** | ? Implémenté | - |
| **GetAllTickets** | ?? Inefficace (N requêtes) | ?? MOYENNE |
| **Sélection Zone/Device** | ? ID manuel | ?? MOYENNE |
| **Champ Priorité** | ? Manquant | ?? MOYENNE |
| **Dialog Statut** | ?? Basique | ?? FAIBLE |
| **Pagination** | ? Manquante | ?? FAIBLE |
| **Tri Colonnes** | ? Manquant | ?? FAIBLE |

---

## ?? PLAN D'ACTION RECOMMANDÉ

### Phase 1: Validation Backend (30 min)
1. Tester `GET /api/incidents` (tous tickets)
2. Tester `GET /api/incidents?status=Ouvert`
3. Tester POST création ticket
4. Vérifier réponses conformes DTOs

### Phase 2: Corrections Moyennes (2h)
1. Optimiser `GetAllTicketsAsync()` ? 1 seule requête
2. Ajouter selects zones/devices
3. Ajouter champ Priorité
4. Améliorer dialog changement statut

### Phase 3: Améliorations UX (1h)
1. Ajouter pagination
2. Ajouter tri colonnes
3. Améliorer filtres

---

## ? CHECKLIST TESTS

### Backend
- [ ] `GET /api/incidents` retourne tous tickets
- [ ] `GET /api/incidents?status=Ouvert` filtre OK
- [ ] `GET /api/incidents/user/{userId}` tickets utilisateur
- [ ] `POST /api/incidents` crée ticket
- [ ] `PUT /api/incidents/{id}/status` modifie statut
- [ ] `PUT /api/incidents/{id}/assign` assigne ticket
- [ ] `DELETE /api/incidents/{id}` supprime (Admin)

### Frontend
- [ ] Page charge sans erreurs
- [ ] Liste tickets affichée
- [ ] Recherche fonctionne
- [ ] Filtres statut fonctionnent
- [ ] "Mes tickets" fonctionne
- [ ] Création ticket OK
- [ ] Voir détails OK
- [ ] Marquer résolu OK
- [ ] Assigner (Manager/Admin) OK
- [ ] Supprimer (Admin) OK

---

**Prochaine étape:** Veux-tu qu'on vérifie les endpoints backend ou qu'on applique les corrections frontend ?
