# ? ALIGNEMENT FRONTEND ? BACKEND INCIDENTS - TERMINÉ

**Date:** 2025-01-30  
**Status:** ? **100% ALIGNÉ**

---

## ?? MODIFICATIONS APPLIQUÉES

### Routes Alignées

| Méthode | Frontend Avant | Frontend Après | Backend |
|---------|----------------|----------------|---------|
| **GetAll** | `/api/incidents` | `/api/IncidentTickets` ? | `/api/IncidentTickets` |
| **Create** | `/api/incidents` | `/api/IncidentTickets` ? | `/api/IncidentTickets` |
| **GetById** | `/api/incidents/{id}` | `/api/IncidentTickets/{id}` ? | `/api/IncidentTickets/{id}` |
| **GetByStatus** | `/api/incidents?status={s}` | `/api/IncidentTickets/status/{s}` ? | `/api/IncidentTickets/status/{s}` |
| **GetByUser** | `/api/incidents/user/{userId}` | `/api/IncidentTickets/user/{userId}` ? | `/api/IncidentTickets/user/{userId}` |
| **UpdateStatus** | `/api/incidents/{id}/status` | `/api/IncidentTickets/{id}/status` ? | `/api/IncidentTickets/{id}/status` |
| **Assign** | `/api/incidents/{id}/assign` | `/api/IncidentTickets/{id}/assign` ? | `/api/IncidentTickets/{id}/assign` |
| **Delete** | `/api/incidents/{id}` | `/api/IncidentTickets/{id}` ? | `/api/IncidentTickets/{id}` |

---

### Payloads Alignés

#### UpdateTicketStatus

**Frontend Avant:**
```csharp
var response = await _apiService.PutAsync<object>($"/api/incidents/{id}/status", new { status = newStatus });
```

**Frontend Après:**
```csharp
var response = await _apiService.PutAsync<object>($"/api/IncidentTickets/{id}/status", newStatus);
// ? Envoie string direct au lieu d'objet
```

**Backend:**
```csharp
[HttpPut("{id}/status")]
public async Task<IActionResult> UpdateTicketStatus(int id, [FromBody] string newStatus)
```

**Requête HTTP:**
```http
PUT /api/IncidentTickets/1/status
Content-Type: application/json

"Résolu"
```

---

#### AssignTicket

**Frontend Avant:**
```csharp
var response = await _apiService.PutAsync<object>($"/api/incidents/{id}/assign", new { assignedToId });
```

**Frontend Après:**
```csharp
var response = await _apiService.PutAsync<object>($"/api/IncidentTickets/{id}/assign", assignedToId);
// ? Envoie string direct au lieu d'objet
```

**Backend:**
```csharp
[HttpPut("{id}/assign")]
public async Task<IActionResult> AssignTicket(int id, [FromBody] string assignedToId)
```

**Requête HTTP:**
```http
PUT /api/IncidentTickets/1/assign
Content-Type: application/json

"user-guid-abc123"
```

---

## ?? CE QUI RESTE À FAIRE AU BACKEND

### Ajouter Priorite dans DTO

**Fichier Backend:** `SmartOfficeAPP/DTOs/Request/IncidentTicketCreateDto.cs`

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

**À Ajouter:**
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

**Frontend envoie déjà:**
```json
{
  "sujet": "Problème capteur",
  "description": "Le capteur ne répond plus",
  "statut": "Ouvert",
  "priorite": "Haute",
  "zoneId": 5,
  "peripheriqueLogiqueId": null
}
```

---

## ?? TABLEAU DE BORD ALIGNEMENT

| Élément | Frontend | Backend | Status |
|---------|----------|---------|--------|
| **Routes** | `/api/IncidentTickets/*` | `/api/IncidentTickets/*` | ? ALIGNÉ |
| **UpdateStatus** | String direct | String direct | ? ALIGNÉ |
| **AssignTicket** | String direct | String direct | ? ALIGNÉ |
| **Priorite DTO** | ? Inclus | ? Manquant | ?? À AJOUTER |
| **GetAll optimisé** | ? 1 requête | ? Endpoint existe | ? ALIGNÉ |

---

## ?? TESTS À FAIRE

### Test 1: Créer Ticket avec Priorité

**Requête:**
```http
POST http://localhost:5210/api/IncidentTickets
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN

{
  "sujet": "Test Frontend Aligné",
  "description": "Vérification alignement frontend-backend",
  "statut": "Ouvert",
  "priorite": "Haute",
  "zoneId": null,
  "peripheriqueLogiqueId": null
}
```

**Résultat Attendu:**
- ? Si backend a `Priorite`: `201 Created`
- ?? Si backend n'a pas `Priorite`: `400 Bad Request` (propriété inconnue)

---

### Test 2: Update Status

**Requête:**
```http
PUT http://localhost:5210/api/IncidentTickets/1/status
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN

"Résolu"
```

**Résultat Attendu:** `204 No Content`

---

### Test 3: Assign Ticket

**Requête:**
```http
PUT http://localhost:5210/api/IncidentTickets/1/assign
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN

"user-guid-abc123"
```

**Résultat Attendu:** `204 No Content`

---

### Test 4: Get All Tickets

**Requête:**
```http
GET http://localhost:5210/api/IncidentTickets
Authorization: Bearer YOUR_TOKEN
```

**Résultat Attendu:**
```json
[
  {
    "id": 1,
    "sujet": "...",
    "description": "...",
    "statut": "Ouvert",
    "priorite": "Haute",
    "createdById": "...",
    "createdByName": "...",
    ...
  }
]
```

---

## ?? CHECKLIST FINALE

### Frontend
- [x] Routes changées en `/api/IncidentTickets`
- [x] UpdateStatus envoie string direct
- [x] AssignTicket envoie string direct
- [x] GetAllTicketsAsync optimisé (1 requête)
- [x] DTO `IncidentTicketCreateDto` inclut `Priorite`
- [x] Page Incidents avec select Priorité
- [x] Page Incidents avec select Zones
- [x] Page Incidents avec select Devices

### Backend
- [x] Routes `/api/IncidentTickets` ?
- [x] UpdateStatus accepte string ?
- [x] AssignTicket accepte string ?
- [ ] DTO `IncidentTicketCreateDto` inclut `Priorite` ??

---

## ?? PROCHAINES ÉTAPES

1. **Ajouter Priorite au backend** (1 ligne)
2. **Tester page Incidents** (création avec priorité)
3. **Valider tous les endpoints**

---

**Status Final:** ? Frontend 100% aligné sur Backend actuel  
**Reste à faire:** ?? Backend: Ajouter 1 propriété `Priorite`

**TESTE LA PAGE INCIDENTS MAINTENANT !** ??
