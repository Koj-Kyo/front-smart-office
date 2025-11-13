# ?? Incidents Page - Quick Test Guide

## ?? 5-Minute Test Checklist

### Prerequisites
- [ ] Backend API running on https://localhost:7144
- [ ] User authenticated (login page)
- [ ] At least 1 user account in database

---

## ?? Test Scenarios

### Test 1: View All Tickets (30 seconds)
```
1. Go to /incidents page
2. Wait for loading
3. ? Verify: Tickets table loads
4. ? Verify: Statistics cards show numbers
5. ? Verify: No error messages
```

### Test 2: Create a Ticket (1 minute)
```
1. Click "Créer un ticket" button
2. Fill form:
   - Sujet: "Test ticket"
   - Description: "Test description"
   - Statut: "Ouvert"
   - Type de ressource: "Général"
3. Click "Créer le ticket"
4. ? Verify: Success notification
5. ? Verify: Ticket appears in table
6. ? Verify: Statistics updated (Total +1, Ouvert +1)
```

### Test 3: Mark as Resolved (30 seconds)
```
1. Find an "Ouvert" ticket in table
2. Click the ? button
3. ? Verify: Success notification
4. ? Verify: Status chip changes to "Résolu" (green)
5. ? Verify: Statistics updated
```

### Test 4: Filter by Status (20 seconds)
```
1. Select "Résolu" in status dropdown
2. ? Verify: Only resolved tickets show
3. Select "Ouvert"
4. ? Verify: Only open tickets show
5. Clear filter
6. ? Verify: All tickets show again
```

### Test 5: Search Tickets (20 seconds)
```
1. Type "test" in search box
2. ? Verify: Results filter in real-time
3. ? Verify: Matches appear in subject/description
4. Clear search
5. ? Verify: All tickets show again
```

### Test 6: My Tickets Filter (30 seconds)
```
1. Toggle "Mes tickets" switch ON
2. ? Verify: Only tickets created by or assigned to you
3. ? Verify: Count matches filtered results
4. Toggle OFF
5. ? Verify: All tickets show again
```

### Test 7: View Ticket Details (20 seconds)
```
1. Click ??? (eye icon) on any ticket
2. ? Verify: Details dialog opens
3. ? Verify: Full description shown
4. ? Verify: All information fields populated
5. Click "Fermer"
6. ? Verify: Dialog closes
```

### Test 8: Assign Ticket (Manager/Admin - 30 seconds)
```
1. Login as Manager or Admin
2. Find a ticket not assigned to you
3. Click ?? (assignment) button
4. Confirm dialog
5. ? Verify: Success notification
6. ? Verify: "Assigné à" column shows your name
```

### Test 9: Delete Ticket (Admin - 20 seconds)
```
1. Login as Admin
2. Click ??? (delete) button on a test ticket
3. Confirm deletion
4. ? Verify: Success notification
5. ? Verify: Ticket removed from list
6. ? Verify: Statistics updated (Total -1)
```

### Test 10: Update Status (30 seconds)
```
1. Click ?? (edit) button on any ticket
2. Confirm in dialog
3. ? Verify: Status changes to next in sequence
4. ? Verify: Chip color updates
5. ? Verify: Statistics update
```

---

## ?? Expected Results

### Statistics Cards Should Show:
```
???????????????????????????
? Total: XX               ?
???????????????????????????

???????????????????????????
? Ouvert: XX              ?
???????????????????????????

???????????????????????????
? En cours: XX            ?
???????????????????????????

???????????????????????????
? Résolu: XX              ?
???????????????????????????
```

### Table Should Show:
- **Columns:** ID, Sujet, Statut, Créé par, Assigné à, Ressource, Créé le, Actions
- **Status chips:** Color-coded by status
- **Action buttons:** View (all), Resolve (all), Edit (all), Assign (Manager/Admin), Delete (Admin)

### Status Colors:
- ?? **Ouvert** - Orange
- ?? **En cours** - Blue
- ? **En attente** - Gray
- ?? **Résolu** - Green
- ? **Fermé** - Gray
- ?? **Annulé** - Red

---

## ?? Common Issues & Solutions

### Issue: "Aucun ticket trouvé"
**Solution:**
- Create a test ticket first
- Check backend is running
- Check database has tickets

### Issue: "Impossible d'obtenir l'ID utilisateur"
**Solution:**
- Re-login to refresh token
- Check authentication is working
- Check JWT token is valid

### Issue: "Erreur lors de l'assignation"
**Solution:**
- Check user has Manager or Admin role
- Verify backend authorization is working
- Check user ID is correct

### Issue: "Erreur lors de la suppression"
**Solution:**
- Check user has Admin role
- Only Admin can delete tickets
- Try logging in as Admin

### Issue: Empty statistics
**Solution:**
- Wait for tickets to load
- Check network in browser DevTools
- Verify backend endpoints responding

---

## ? Success Criteria

All tests pass if:
- [ ] ? Tickets load from backend
- [ ] ? Create ticket works
- [ ] ? Statistics show correct numbers
- [ ] ? Status update works
- [ ] ? Filters work (status, search, my tickets)
- [ ] ? Details dialog opens
- [ ] ? Assignment works (Manager/Admin)
- [ ] ? Deletion works (Admin)
- [ ] ? No console errors
- [ ] ? Notifications appear correctly

---

## ?? Quick Debug Commands

### Check API is responding:
```bash
# Get tickets by status
curl https://localhost:7144/api/IncidentTickets/status/Ouvert \
  -H "Authorization: Bearer YOUR_TOKEN"

# Create a ticket
curl -X POST https://localhost:7144/api/IncidentTickets \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"sujet":"Test","description":"Test desc","statut":"Ouvert"}'
```

### Check browser console:
```javascript
// Open DevTools (F12)
// Check Console tab for errors
// Check Network tab for API calls
// Filter by "IncidentTickets" to see ticket API calls
```

### Check service registration:
```csharp
// Program.cs should have:
builder.Services.AddScoped<IIncidentTicketService, IncidentTicketService>();
```

---

## ?? Test Data Setup

### Create Test Tickets via Backend:

```http
POST https://localhost:7144/api/IncidentTickets
Authorization: Bearer {token}
Content-Type: application/json

{
  "sujet": "Capteur défectueux",
  "description": "Le capteur de température ne répond plus",
  "statut": "Ouvert",
  "zoneId": 1
}
```

### Create Tickets with Different Statuses:

```json
// Ouvert
{"sujet": "Ticket 1", "description": "Test", "statut": "Ouvert"}

// En cours
{"sujet": "Ticket 2", "description": "Test", "statut": "En cours"}

// Résolu
{"sujet": "Ticket 3", "description": "Test", "statut": "Résolu"}
```

---

## ?? Debugging Tips

### If tickets don't load:
1. Check browser console for errors
2. Check Network tab - look for 401/403/500 errors
3. Verify backend is running
4. Check authentication token is valid

### If statistics are wrong:
1. Refresh the page
2. Check tickets are loading completely
3. Verify status values match exactly ("Ouvert" not "ouvert")
4. Check calculation logic in GetTicketStatisticsAsync

### If "My tickets" shows nothing:
1. Check currentUserId is not null
2. Create a ticket first
3. Check ticket.CreatedById matches your user ID
4. Check IsMyTicket logic

### If assignment fails:
1. Check you're logged in as Manager or Admin
2. Verify backend authorization
3. Check userId is being sent correctly
4. Look at backend logs for errors

---

## ?? Need Help?

### Documentation:
- Implementation details: `INCIDENTS_PAGE_IMPLEMENTATION.md`
- Backend tests: `FRONTEND-INCIDENTS-COMPLETE-GUIDE.md`
- API reference: Backend `IncidentTicketsController.cs`

### Common Errors:
1. **Build fails** ? Check service registration in Program.cs
2. **404 errors** ? Check backend API endpoints
3. **Empty data** ? Create test tickets first
4. **Auth errors** ? Re-login and try again
5. **Permission errors** ? Check user role (User/Manager/Admin)

---

## ?? Video Walkthrough Script

```
1. [00:00] Open /incidents page
2. [00:05] Show loading spinner
3. [00:10] Tickets table loads
4. [00:15] Point to statistics cards
5. [00:20] Click "Créer un ticket"
6. [00:25] Fill form
7. [00:35] Submit - show success notification
8. [00:40] New ticket appears in table
9. [00:45] Click ? to mark resolved
10. [00:50] Status changes to green
11. [00:55] Filter by status
12. [01:00] Toggle "Mes tickets"
13. [01:05] Search for ticket
14. [01:10] Click ??? to view details
15. [01:15] Close details dialog
16. [01:20] Refresh page - data persists
```

---

**Test Duration:** ~5 minutes for basic tests  
**Full Test Duration:** ~15 minutes including all scenarios  
**Last Updated:** 2025-01-06  
**Status:** Ready for testing ?
