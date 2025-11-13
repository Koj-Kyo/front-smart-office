# ?? Analytics Page - Quick Test Guide

## ?? 5-Minute Test Checklist

### Prerequisites
- [ ] Backend API running on https://localhost:7144
- [ ] User authenticated (login page)
- [ ] At least 1 device with telemetry data in database

---

## ?? Test Scenarios

### Test 1: Load Statistics (30 seconds)
```
1. Go to /analytics page
2. See welcome message: "?? Sélectionnez un périphérique..."
3. Select "Climatiseur Salon" (ID: 1)
4. Keep default dates (last 7 days)
5. Click "Analyser" button
6. Wait for loading spinner
7. ? Verify: 4 stats cards appear with numbers > 0
8. ? Verify: Hourly chart loads with data
```

### Test 2: Top Consumers (20 seconds)
```
1. Click "Top Consommateurs" button
2. Wait for loading
3. ? Verify: List of devices appears
4. ? Verify: Progress bars show percentages
5. ? Verify: Total ? 100%
```

### Test 3: Anomaly Detection (30 seconds)
```
1. Select a device
2. Select period (last 30 days)
3. Click "Détecter Anomalies"
4. ? If anomalies: table appears with columns
5. ? If no anomalies: "Aucune anomalie détectée"
6. ? Verify: Severity colors (red/orange/blue)
```

### Test 4: Generate Report (20 seconds)
```
1. Select dates
2. Click "Générer Rapport"
3. ? Verify: 3 main metrics display
4. ? Verify: Recommendations section (if any)
5. ? Verify: Period comparison (if available)
```

### Test 5: Validation (20 seconds)
```
1. Clear device selection
2. Click "Analyser"
3. ? See warning: "Veuillez sélectionner un périphérique"

4. Select device
5. Set start date > end date
6. Click "Analyser"
7. ? See error: "date de début doit être antérieure"

8. Set period > 365 days
9. Click "Analyser"
10. ? See error: "ne peut pas dépasser 365 jours"
```

---

## ?? Expected Results

### Stats Cards Should Show:
```
???????????????????????????
? ? XXX.X mWh            ?
? Consommation totale     ?
???????????????????????????

???????????????????????????
? ?? XX.X mW              ?
? Puissance moyenne       ?
???????????????????????????

???????????????????????????
? ?? XXX.X h              ?
? Temps fonctionnement    ?
???????????????????????????

???????????????????????????
? ?? XX.X%                ?
? Efficacité              ?
???????????????????????????
```

### Hourly Chart Should Show:
- Line chart with 2 series
- 24 data points (0:00 to 23:00)
- Smooth curves
- Legend: "Consommation (mWh)" and "Puissance moyenne (mW)"

### Top Consumers Should Show:
```
?? Top Consommateurs

Climatiseur Salon           45.2 mWh
Salon                       31.0%
[????????????????????] 

Éclairage Bureau            28.7 mWh
Bureau                      19.7%
[???????????????????]

...
```

---

## ?? Common Issues & Solutions

### Issue: "Aucune donnée disponible"
**Solution:**
- Check backend is running
- Verify device ID exists in database
- Check telemetry data exists for selected period

### Issue: Empty chart
**Solution:**
- Verify `consommationHoraire` has data
- Check browser console for errors
- Try selecting today's date

### Issue: 401 Unauthorized
**Solution:**
- Re-login to get fresh token
- Check token expiration
- Verify authentication headers

### Issue: Graphs not rendering
**Solution:**
- Clear browser cache
- Check MudBlazor is loaded
- Verify chart data is valid (no NaN, no null)

---

## ?? Test Data Requirements

### Minimum Database Data:
```sql
-- 1. At least 1 device
INSERT INTO PeripheriquesLogiques (Name, Type, IotDeviceId, ZoneId)
VALUES ('Climatiseur Test', 'HVAC', 'test-device-001', 1);

-- 2. At least 24h of telemetry
INSERT INTO Telemetries (PeripheriqueLogiqueId, TypeEvenement, Valeur, Horodatage)
VALUES 
  (1, 'Consommation', 5.2, NOW() - INTERVAL '1 hour'),
  (1, 'Consommation', 6.1, NOW() - INTERVAL '2 hours'),
  -- ... repeat for 24 hours
```

---

## ? Success Criteria

### All tests pass if:
- [ ] ? Stats cards display real numbers
- [ ] ? Chart renders with data
- [ ] ? Top consumers list appears
- [ ] ? Anomaly detection works
- [ ] ? Report generates successfully
- [ ] ? Validation messages show correctly
- [ ] ? No console errors
- [ ] ? Loading spinners appear/disappear correctly
- [ ] ? Snackbar notifications work
- [ ] ? Page responsive on mobile

---

## ?? Quick Debug Commands

### Check API is responding:
```bash
curl https://localhost:7144/api/ConsommationStats/peripherique/1?startDate=2025-01-01&endDate=2025-01-07 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Check browser console:
```javascript
// Open DevTools (F12)
// Check Console tab for errors
// Check Network tab for API calls
```

### Check service registration:
```csharp
// Program.cs should have:
builder.Services.AddScoped<IConsommationStatsService, ConsommationStatsService>();
```

---

## ?? Need Help?

### Documentation:
- Full guide: `FRONTEND-ANALYTICS-COMPLETE-GUIDE.md`
- API reference: Backend ConsommationStatsController
- DTOs: `ApiModels.cs`

### Common Errors:
1. **Build fails** ? Check Program.cs service registration
2. **404 errors** ? Check backend API is running
3. **Empty data** ? Check database has telemetry
4. **Auth errors** ? Re-login and try again

---

**Test Duration:** ~5 minutes  
**Last Updated:** 2025-01-06  
**Status:** Ready for testing ?
