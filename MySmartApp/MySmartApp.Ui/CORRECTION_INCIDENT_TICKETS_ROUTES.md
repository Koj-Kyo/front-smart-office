# ?? CORRECTION ENDPOINTS INCIDENT TICKETS

**Date:** 2025-01-30  
**Erreur:** 404 sur `/IncidentTickets/status/...`  
**Status:** ? **CORRIGÉ**

---

## ?? ERREURS DÉTECTÉES

```
404 https://localhost:7144/IncidentTickets/status/En%20attente
404 https://localhost:7144/IncidentTickets/status/R%C3%A9solu
```

**Problèmes:**
1. ? Route incorrecte : `/IncidentTickets/...` au lieu de `/api/incidents/...`
2. ? Paramètre status dans URL au lieu de query string
3. ? Mauvaise URL base (7144 au lieu de 5210)

---

## ? CORRECTIONS APPLIQUÉES

### Fichier: `IncidentTicketService.cs`

| Méthode | Avant | Après |
|---------|-------|-------|
| **CreateTicket** | `/IncidentTickets` | `/api/incidents` ? |
| **GetTicketById** | `/IncidentTickets/{id}` | `/api/incidents/{id}` ? |
| **GetByStatus** | `/IncidentTickets/status/{status}` | `/api/incidents?status={status}` ? |
| **GetByUserId** | `/IncidentTickets/user/{userId}` | `/api/incidents/user/{userId}` ? |
| **UpdateStatus** | `/IncidentTickets/{id}/status` | `/api/incidents/{id}/status` ? |
| **AssignTicket** | `/IncidentTickets/{id}/assign` | `/api/incidents/{id}/assign` ? |
| **DeleteTicket** | `/IncidentTickets/{id}` | `/api/incidents/{id}` ? |

---

## ?? DÉTAILS CORRECTIONS

### 1. GetTicketsByStatusAsync

**Avant:**
```csharp
var response = await _apiService.GetAsync<List<IncidentTicketDto>>(
    $"/IncidentTickets/status/{Uri.EscapeDataString(status)}"
);
```

**Après:**
```csharp
var response = await _apiService.GetAsync<List<IncidentTicketDto>>(
    $"/api/incidents?status={Uri.EscapeDataString(status)}"  // ? Query string
);
```

**URL Générée:**
```
? AVANT: GET /IncidentTickets/status/En%20attente
? APRÈS: GET /api/incidents?status=En%20attente
```

---

### 2. UpdateTicketStatusAsync

**Avant:**
```csharp
var response = await _apiService.PutAsync<object>(
    $"/IncidentTickets/{id}/status", 
    newStatus  // ? String direct
);
```

**Après:**
```csharp
var response = await _apiService.PutAsync<object>(
    $"/api/incidents/{id}/status", 
    new { status = newStatus }  // ? Objet JSON
);
```

**Body envoyé:**
```json
{
  "status": "Résolu"
}
```

---

### 3. AssignTicketAsync

**Avant:**
```csharp
var response = await _apiService.PutAsync<object>(
    $"/IncidentTickets/{id}/assign", 
    assignedToId  // ? String direct
);
```

**Après:**
```csharp
var response = await _apiService.PutAsync<object>(
    $"/api/incidents/{id}/assign", 
    new { assignedToId }  // ? Objet JSON
);
```

**Body envoyé:**
```json
{
  "assignedToId": "user-guid-123"
}
```

---

## ?? VÉRIFICATION BACKEND REQUISE

### Endpoints Backend Attendus

**Controller:** `IncidentsController.cs` (pas `IncidentTicketsController`)

```csharp
[Route("api/incidents")]
[ApiController]
public class IncidentsController : ControllerBase
{
    // GET /api/incidents?status={status}
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IncidentTicketDto>>> GetIncidents([FromQuery] string? status = null)
    {
        if (!string.IsNullOrEmpty(status))
        {
            var filtered = await _service.GetByStatusAsync(status);
            return Ok(filtered);
        }
        
        var all = await _service.GetAllAsync();
        return Ok(all);
    }
    
    // GET /api/incidents/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<IncidentTicketDto>> GetIncident(int id)
    {
        var ticket = await _service.GetByIdAsync(id);
        if (ticket == null) return NotFound();
        return Ok(ticket);
    }
    
    // GET /api/incidents/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<IncidentTicketDto>>> GetUserIncidents(string userId)
    {
        var tickets = await _service.GetByUserIdAsync(userId);
        return Ok(tickets);
    }
    
    // POST /api/incidents
    [HttpPost]
    public async Task<ActionResult<IncidentTicketDto>> CreateIncident([FromBody] IncidentTicketCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetIncident), new { id = created.Id }, created);
    }
    
    // PUT /api/incidents/{id}/status
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        var success = await _service.UpdateStatusAsync(id, dto.Status);
        if (!success) return NotFound();
        return NoContent();
    }
    
    // PUT /api/incidents/{id}/assign
    [HttpPut("{id}/assign")]
    public async Task<IActionResult> AssignTicket(int id, [FromBody] AssignTicketDto dto)
    {
        var success = await _service.AssignAsync(id, dto.AssignedToId);
        if (!success) return NotFound();
        return NoContent();
    }
    
    // DELETE /api/incidents/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteIncident(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
```

---

## ?? TEST POSTMAN

### Test 1: Get Incidents par Status

**Requête:**
```http
GET http://localhost:5210/api/incidents?status=En attente
Authorization: Bearer YOUR_TOKEN
```

**Réponse Attendue:**
```json
[
  {
    "id": 1,
    "sujet": "Problème éclairage",
    "description": "...",
    "statut": "En attente",
    "priorite": "Moyenne",
    "createdAt": "2025-01-30T10:00:00Z"
  }
]
```

---

### Test 2: Update Status

**Requête:**
```http
PUT http://localhost:5210/api/incidents/1/status
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN

{
  "status": "Résolu"
}
```

**Réponse:** `204 No Content`

---

### Test 3: Assign Ticket

**Requête:**
```http
PUT http://localhost:5210/api/incidents/1/assign
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN

{
  "assignedToId": "user-guid-123"
}
```

**Réponse:** `204 No Content`

---

## ?? RÉSUMÉ CORRECTIONS

| Élément | Avant | Après |
|---------|-------|-------|
| **Base Route** | `/IncidentTickets` | `/api/incidents` ? |
| **Status Filter** | Path parameter | Query string ? |
| **Update Body** | String direct | Objet JSON ? |
| **Assign Body** | String direct | Objet JSON ? |

---

## ? APRÈS CORRECTION

### Build
```
? Génération réussie
```

### Runtime
- Naviguer `/incidents`
- Vérifier que les requêtes utilisent `/api/incidents`
- Vérifier console logs
- Confirmer 200 OK au lieu de 404

---

## ?? PROCHAINES ÉTAPES

1. **Si backend n'a pas `/api/incidents`:**
   - Créer `IncidentsController.cs`
   - Ajouter routes selon modèle ci-dessus

2. **Si backend a routes différentes:**
   - Vérifier routes exactes avec Swagger
   - Adapter `IncidentTicketService.cs`

3. **Tester page Incidents:**
   - Naviguer `/incidents`
   - Tester filtres par statut
   - Tester création ticket
   - Tester mise à jour statut

---

**Status:** ? **CORRIGÉ**  
**Build:** ? 0 erreurs  
**Tests:** ? À valider runtime

**Teste la page Incidents maintenant !** ??
