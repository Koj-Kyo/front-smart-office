# ?? Users Page - Quick Test Guide

## ?? 5-Minute Test Checklist

### Prerequisites
- [ ] Backend API running on https://localhost:7144
- [ ] Logged in as **Admin** (required for this page)
- [ ] At least 1 role in database (Admin, User, Manager, etc.)

---

## ?? Test Scenarios

### Test 1: View Users Page (20 seconds)
```
1. Navigate to /utilisateurs
2. ? Verify: Statistics cards display (Total, Actifs, Verrouillés, Rôles)
3. ? Verify: User list loads in expansion panels
4. ? Verify: No errors in console
```

### Test 2: Form Validation - Invalid Data (1 minute)
```
1. Click "Créer un utilisateur"
2. Try submitting empty form
   ? Verify: Required field errors appear
3. Enter "ab" in Username field
   ? Verify: Error "doit contenir au moins 3 caractères"
4. Enter "test" in Email field
   ? Verify: Error "L'email n'est pas valide"
5. Enter "12345" in Password field
   ? Verify: Multiple password errors appear:
      - "au moins 8 caractères"
      - "au moins une majuscule"
      - "au moins un caractère spécial"
6. Click "Annuler"
```

### Test 3: Form Validation - Valid Password (30 seconds)
```
1. Click "Créer un utilisateur"
2. Fill form:
   - Username: "testuser123"
   - Email: "test@example.com"
   - Nom complet: "Test User"
   - Password: "Test@1234" (with eye icon visible)
3. ? Verify: No validation errors
4. Click eye icon
   ? Verify: Password becomes visible
5. Click eye icon again
   ? Verify: Password hidden again
6. Click "Annuler"
```

### Test 4: Create User - Success (45 seconds)
```
1. Click "Créer un utilisateur"
2. Fill valid data:
   - Username: "newuser"
   - Email: "newuser@test.com"
   - Nom complet: "New Test User"
   - Password: "Password@123"
   - Select role: "User"
3. Click "Créer"
4. ? Verify: Green Snackbar "Utilisateur 'newuser' créé avec succès"
5. ? Verify: Dialog closes
6. ? Verify: User appears in list
7. ? Verify: Statistics updated (Total +1)
```

### Test 5: Create User - Duplicate Email (30 seconds)
```
1. Click "Créer un utilisateur"
2. Fill form with EXISTING email:
   - Username: "anotheruser"
   - Email: "newuser@test.com" (same as before)
   - Nom complet: "Another User"
   - Password: "Password@123"
3. Click "Créer"
4. ? Verify: Red alert in dialog "Un utilisateur avec cet email existe déjà"
5. ? Verify: Red Snackbar with same message
6. ? Verify: Dialog stays open
7. Click "Annuler"
```

### Test 6: Create User - Duplicate Username (30 seconds)
```
1. Click "Créer un utilisateur"
2. Fill form with EXISTING username:
   - Username: "newuser" (same as before)
   - Email: "different@test.com"
   - Nom complet: "Different User"
   - Password: "Password@123"
3. Click "Créer"
4. ? Verify: Red alert "Un utilisateur avec ce nom existe déjà"
5. ? Verify: Red Snackbar with same message
6. Click "Annuler"
```

### Test 7: Edit User - Success (45 seconds)
```
1. Expand a user in the list
2. Click Edit (??) button
3. ? Verify: Form populates with user data
4. Change Nom complet to "Updated Name"
5. Add a new role (if available)
6. Click "Modifier"
7. ? Verify: Green Snackbar "Utilisateur 'xxx' modifié avec succès"
8. ? Verify: Changes reflected in list
9. ? Verify: New role appears in user's chips
```

### Test 8: Edit User - Conflict (30 seconds)
```
1. Expand user A
2. Click Edit
3. Change email to user B's email
4. Click "Modifier"
5. ? Verify: Red alert "Un autre utilisateur utilise déjà cet email"
6. ? Verify: Red Snackbar with same message
7. Click "Annuler"
```

### Test 9: Toggle User Status (30 seconds)
```
1. Expand an ACTIVE user
2. Click Lock icon (??)
3. ? Verify: Dialog appears with detailed message:
   - "désactiver l'utilisateur 'xxx'"
   - "?? L'utilisateur ne pourra plus se connecter..."
4. Click "Désactiver"
5. ? Verify: Green Snackbar "Utilisateur 'xxx' désactivé"
6. ? Verify: User shows "?? Inactif" badge in title
7. ? Verify: Statistics updated (Actifs -1, Verrouillés +1)
```

### Test 10: Delete User (45 seconds)
```
1. Expand a test user
2. Click Delete (???) button
3. ? Verify: Detailed confirmation dialog appears:
   - "?? ATTENTION ??"
   - User details (Name, Email, Roles)
   - "Cette action est IRRÉVERSIBLE"
   - List of consequences
4. Click "Oui, supprimer définitivement"
5. ? Verify: Green Snackbar "Utilisateur 'xxx' supprimé définitivement"
6. ? Verify: User removed from list
7. ? Verify: Statistics updated (Total -1)
```

### Test 11: Search Users (20 seconds)
```
1. Type "test" in search box
2. ? Verify: Results filter immediately
3. ? Verify: Only matching users show
4. Clear search
5. ? Verify: All users show again
```

### Test 12: Refresh Data (15 seconds)
```
1. Click "Actualiser" button
2. ? Verify: Loading spinner appears
3. ? Verify: Green Snackbar "Données actualisées"
4. ? Verify: Data reloaded
```

---

## ?? Validation Tests

### Username Validation
| Input | Expected Result |
|-------|----------------|
| "" | ? "Le nom d'utilisateur est obligatoire" |
| "ab" | ? "au moins 3 caractères" |
| "a".repeat(51) | ? "ne peut pas dépasser 50 caractères" |
| "test@user" | ? "ne peut contenir que des lettres..." |
| "test_user-123" | ? Valid |

### Email Validation
| Input | Expected Result |
|-------|----------------|
| "" | ? "L'email est obligatoire" |
| "test" | ? "L'email n'est pas valide" |
| "test@" | ? "L'email n'est pas valide" |
| "test@example.com" | ? Valid |

### Password Validation
| Input | Expected Result |
|-------|----------------|
| "" | ? "Le mot de passe est obligatoire" |
| "short" | ? "au moins 8 caractères" |
| "password123" | ? "au moins une majuscule" |
| "Password" | ? "au moins un chiffre" |
| "Password123" | ? "au moins un caractère spécial" |
| "Password@123" | ? Valid |

### Phone Validation
| Input | Expected Result |
|-------|----------------|
| "" | ? Valid (optional) |
| "123" | ? "doit contenir entre 10 et 15 chiffres" |
| "+33 6 12 34 56 78" | ? Valid |
| "06 12 34 56 78" | ? Valid |
| "abc123" | ? "contient des caractères invalides" |

---

## ?? Error Scenarios to Test

### 1. Conflict Errors (409)
```
Create/Update with:
- Existing email
- Existing username

Expected:
? Red alert in dialog
? Red Snackbar notification
? Dialog stays open
? Can correct and retry
```

### 2. Bad Request (400)
```
Submit invalid data that passes client validation

Expected:
? Error message with details
? Red Snackbar
```

### 3. Not Found (404)
```
Try to edit a user that was deleted

Expected:
? "Utilisateur introuvable"
? Red Snackbar
```

### 4. Forbidden (403)
```
Try to delete your own account

Expected:
? "Vous ne pouvez pas supprimer votre propre compte"
? Red Snackbar
```

---

## ? Success Criteria

### Visual Checks
- [ ] ? Statistics cards display correctly
- [ ] ? User list loads without errors
- [ ] ? Expansion panels work smoothly
- [ ] ? Status badges show correctly (Inactif, Email non confirmé)
- [ ] ? Role chips have correct colors
- [ ] ? Icons display properly

### Functionality Checks
- [ ] ? Create user with valid data works
- [ ] ? Duplicate detection works (client-side)
- [ ] ? 409 Conflict handling works (server-side)
- [ ] ? Form validation prevents invalid submission
- [ ] ? Password visibility toggle works
- [ ] ? Edit user updates data correctly
- [ ] ? Role assignment works
- [ ] ? Toggle status works with confirmation
- [ ] ? Delete works with detailed warning
- [ ] ? Search filters results immediately
- [ ] ? Refresh reloads data with notification

### Notification Checks
- [ ] ? Success Snackbars appear (green)
- [ ] ? Error Snackbars appear (red)
- [ ] ? Warning Snackbars appear (orange)
- [ ] ? Inline errors show in dialog
- [ ] ? Confirmations show detailed messages

---

## ?? Common Issues & Solutions

### Issue: "Impossible de charger les utilisateurs"
**Solution:**
- Check backend is running on port 7144
- Verify you're logged in as Admin
- Check network tab for 401/403 errors
- Re-login if token expired

### Issue: "Snackbar ne s'affiche pas"
**Solution:**
- Check MudBlazor is configured in Program.cs
- Verify ISnackbar is injected
- Check browser console for errors

### Issue: "Validation n'empêche pas la soumission"
**Solution:**
- Verify MudForm is bound with @ref="form"
- Check isFormValid is set correctly
- Ensure button is disabled when !isFormValid

### Issue: "Password eye icon ne fonctionne pas"
**Solution:**
- Check showPassword boolean is toggled
- Verify InputType changes correctly
- Check AdornmentIcon is set properly

### Issue: "Duplicate pas détecté"
**Solution:**
- Refresh data (users list)
- Check comparison is case-insensitive
- Verify backend returns 409 for server-side check

---

## ?? Test Results Template

```
Date: _____________
Tester: _____________

Basic Operations:
[ ] View page
[ ] Create user
[ ] Edit user
[ ] Delete user
[ ] Toggle status

Validation:
[ ] Username validation
[ ] Email validation
[ ] Password validation
[ ] Phone validation

Error Handling:
[ ] 409 Conflict
[ ] 400 Bad Request
[ ] 403 Forbidden
[ ] 404 Not Found

UI Features:
[ ] Password toggle
[ ] Search filter
[ ] Refresh data
[ ] Snackbar notifications
[ ] Confirmation dialogs

Notes:
_______________________________
_______________________________
_______________________________
```

---

## ?? Video Walkthrough Script

```
1. [00:00] Open /utilisateurs page
2. [00:05] Show statistics cards
3. [00:10] Expand a user panel
4. [00:15] Click "Créer un utilisateur"
5. [00:20] Show form validation with errors
6. [00:30] Fill valid data
7. [00:35] Toggle password visibility
8. [00:40] Submit - show success
9. [00:45] Try duplicate email
10. [00:50] Show conflict error
11. [00:55] Edit existing user
12. [01:00] Change role
13. [01:05] Show success
14. [01:10] Toggle user status
15. [01:15] Show confirmation dialog
16. [01:20] Confirm and show result
17. [01:25] Delete user
18. [01:30] Show detailed warning
19. [01:35] Confirm deletion
20. [01:40] Search users
21. [01:45] Refresh data
22. [01:50] Show final state
```

---

**Test Duration:** ~10 minutes for complete test  
**Quick Test Duration:** ~5 minutes for basic scenarios  
**Last Updated:** 2025-01-06  
**Status:** Ready for testing ?
