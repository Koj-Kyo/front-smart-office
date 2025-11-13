# ?? MODIFICATIONS BACKEND CRITIQUES - INCIDENTS

**Date:** 2025-01-30  
**Status:** ?? **MODIFICATIONS REQUISES**

---

## ?? CE QUI DOIT ÊTRE MODIFIÉ AU BACKEND

### 1. ? Ajouter Propriété `Priorite`

**Fichier:** `SmartOfficeAPP/DTOs/Request/IncidentTicketCreateDto.cs`

**Actuel:**
```csharp
public class IncidentTicketCreateDto
{
    [Required]
    public string Sujet { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public string Statut { get; set; }

    public int? ZoneId { get; set; }
    public int? PeripheriqueLogiqueId { get; set; }
}
```

**À Modifier:**
```csharp
public class IncidentTicketCreateDto
{
    [Required]
    public string Sujet { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public string Statut { get; set; }

    // ? AJOUTER
    public string Priorite { get; set; } = "Moyenne";

    public int? ZoneId { get; set; }
    public int? PeripheriqueLogiqueId { get; set; }
}
```

---

### 2. ? Modifier UpdateTicketStatus

**Fichier:** `SmartOfficeAPP/Controllers/IncidentTicketsController.cs`

**Problème:** Frontend envoie un objet JSON, backend attend string direct

**Frontend envoie:**
```json
{
  "status": "Résolu"
}
```

**Actuel (Backend):**
```csharp
[HttpPut("{id}/status")]
public async Task<IActionResult> UpdateTicketStatus(int id, [FromBody] string newStatus)
{
    var result = await _incidentTicketService.UpdateTicketStatusAsync(id, newStatus);
    if (!result)
    {
        return NotFound();
    }
    return NoContent();
}
```

**Solution A - Modifier Backend (RECOMMANDÉ):**
```csharp
// Créer DTO
public record UpdateStatusRequest(string Status);

[HttpPut("{id}/status")]
public async Task<IActionResult> UpdateTicketStatus(int id, [FromBody] UpdateStatusRequest request)
{
    var result = await _incidentTicketService.UpdateTicketStatusAsync(id, request.Status);
    if (!result)
    {
        return NotFound();
    }
    return NoContent();
}
```

**Solution B - Modifier Frontend:**
```csharp
// Dans IncidentTicketService.cs
var response = await _apiService.PutAsync<object>($"/api/IncidentTickets/{id}/status", newStatus);
// Au lieu de:
// var response = await _apiService.PutAsync<object>($"/api/IncidentTickets/{id}/status", new { status = newStatus });
```

---

### 3. ? Modifier AssignTicket

**Fichier:** `SmartOfficeAPP/Controllers/IncidentTicketsController.cs`

**Problème:** Même problème que UpdateStatus

**Frontend envoie:**
```json
{
  "assignedToId": "user-guid-123"
}
```

**Actuel (Backend):**
```csharp
[HttpPut("{id}/assign")]
[Authorize(Roles = "Manager,Admin")]
public async Task<IActionResult> AssignTicket(int id, [FromBody] string assignedToId)
{
    var result = await _incidentTicketService.AssignTicketAsync(id, assignedToId);
    if (!result)
    {
        return NotFound();
    }
    return NoContent();
}
```

**Solution A - Modifier Backend (RECOMMANDÉ):**
```csharp
// Créer DTO
public record AssignTicketRequest(string AssignedToId);

[HttpPut("{id}/assign")]
[Authorize(Roles = "Manager,Admin")]
public async Task<IActionResult> AssignTicket(int id, [FromBody] AssignTicketRequest request)
{
    var result = await _incidentTicketService.AssignTicketAsync(id, request.AssignedToId);
    if (!result)
    {
        return NotFound();
    }
    return NoContent();
}
```

**Solution B - Modifier Frontend:**
```csharp
// Dans IncidentTicketService.cs
var response = await _apiService.PutAsync<object>($"/api/IncidentTickets/{id}/assign", assignedToId);
// Au lieu de:
// var response = await _apiService.PutAsync<object>($"/api/IncidentTickets/{id}/assign", new { assignedToId });
```

---

## ?? MODIFICATIONS FRONTEND NÉCESSAIRES

### Route API

**Actuel Frontend:**
```csharp
var response = await _apiService.GetAsync<List<IncidentTicketDto>>("/api/incidents");
var response = await _apiService.PostAsync<IncidentTicketDto>("/api/incidents", ticketDto);
// etc.
```

**Backend utilise:**
```csharp
[Route("api/[controller]")]  // ? /api/IncidentTickets
```

**Correction Frontend:**
```csharp
// Dans MySmartApp.Ui/Services/IncidentTicketService.cs
// Remplacer TOUTES les occurrences de "/api/incidents" par "/api/IncidentTickets"

CreateTicketAsync: "/api/IncidentTickets"
GetTicketByIdAsync: "/api/IncidentTickets/{id}"
GetTicketsByStatusAsync: "/api/IncidentTickets/status/{status}"
GetTicketsByUserIdAsync: "/api/IncidentTickets/user/{userId}"
UpdateTicketStatusAsync: "/api/IncidentTickets/{id}/status"
AssignTicketAsync: "/api/IncidentTickets/{id}/assign"
DeleteTicketAsync: "/api/IncidentTickets/{id}"
GetAllTicketsAsync: "/api/IncidentTickets"
```

---

## ?? RÉCAPITULATIF

| Modification | Fichier Backend | Ligne | Priorité |
|--------------|----------------|-------|----------|
| **Ajouter Priorite** | `IncidentTicketCreateDto.cs` | ~15 | ?? HAUTE |
| **UpdateStatus DTO** | `IncidentTicketsController.cs` | ~75 | ?? HAUTE |
| **AssignTicket DTO** | `IncidentTicketsController.cs` | ~92 | ?? HAUTE |

---

## ? APRÈS MODIFICATIONS

### Test 1: Créer Ticket avec Priorité

**Requête:**
```http
POST http://localhost:5210/api/IncidentTickets
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN

{
  "sujet": "Test Priorité",
  "description": "Ticket de test",
  "statut": "Ouvert",
  "priorite": "Haute",
  "zoneId": null,
  "peripheriqueLogiqueId": null
}
```

**Réponse Attendue:** `201 Created` avec ticket créé

---

### Test 2: Update Status avec Objet

**Requête:**
```http
PUT http://localhost:5210/api/IncidentTickets/1/status
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN

{
  "status": "Résolu"
}
```

**Réponse Attendue:** `204 No Content`

---

### Test 3: Assign avec Objet

**Requête:**
```http
PUT http://localhost:5210/api/IncidentTickets/1/assign
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN

{
  "assignedToId": "user-guid-123"
}
```

**Réponse Attendue:** `204 No Content`

---

## ?? PLAN D'ACTION

### Backend (Toi)
1. ? Ajouter `public string Priorite { get; set; } = "Moyenne";` dans `IncidentTicketCreateDto`
2. ? Créer `UpdateStatusRequest` record
3. ? Créer `AssignTicketRequest` record
4. ? Modifier signature `UpdateTicketStatus` méthode
5. ? Modifier signature `AssignTicket` méthode
6. ? Rebuild & Tester

### Frontend (Moi)
1. ? Corriger routes `/api/incidents` ? `/api/IncidentTickets`
2. ? (Déjà fait) Ajouter Priorite dans DTO frontend
3. ? (Déjà fait) Envoyer objets JSON pour Update/Assign

---

## ?? FICHIERS À MODIFIER

### Backend
- `SmartOfficeAPP/DTOs/Request/IncidentTicketCreateDto.cs`
- `SmartOfficeAPP/Controllers/IncidentTicketsController.cs`

### Frontend
- `MySmartApp.Ui/Services/IncidentTicketService.cs` (remplacer routes)

---

**Status:** ? **En attente modifications backend**

**Après modifications backend, je corrigerai les routes frontend !** ??
