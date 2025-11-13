using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace MySmartApp.Ui.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ITokenProvider _tokenProvider;
    private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(ITokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _tokenProvider.GetTokenAsync();
            
            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(_anonymous);
            }

            try 
            {
                // Décoder le JWT pour extraire les claims (sans validation serveur pour éviter les blocages)
                var claims = ParseClaimsFromJwt(token);
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

                return new AuthenticationState(user);
            }
            catch (Exception)
            {
                // Token invalide, le supprimer silencieusement
                try 
                {
                    await _tokenProvider.ClearTokenAsync();
                }
                catch 
                {
                    // Ignorer les erreurs de suppression du token
                }
                return new AuthenticationState(_anonymous);
            }
        }
        catch (Exception)
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task MarkUserAsAuthenticated(string token)
    {
        await _tokenProvider.SetTokenAsync(token);
        var claims = ParseClaimsFromJwt(token);
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _tokenProvider.ClearTokenAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }

    private List<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];
        
        // Ajouter le padding si nécessaire
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }
        
        var jsonBytes = Convert.FromBase64String(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs != null)
        {
            // Ajouter les claims standards
            if (keyValuePairs.TryGetValue(ClaimTypes.NameIdentifier, out var userId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()!));
            }
            
            // ? CORRECTION: Essayer plusieurs variantes pour NameIdentifier
            if (keyValuePairs.TryGetValue("sub", out var sub))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, sub.ToString()!));
            }
            
            if (keyValuePairs.TryGetValue("nameid", out var nameid))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, nameid.ToString()!));
            }

            if (keyValuePairs.TryGetValue(ClaimTypes.Name, out var name))
            {
                claims.Add(new Claim(ClaimTypes.Name, name.ToString()!));
            }
            
            // ? CORRECTION: Essayer "unique_name" pour Name
            if (keyValuePairs.TryGetValue("unique_name", out var uniqueName))
            {
                claims.Add(new Claim(ClaimTypes.Name, uniqueName.ToString()!));
            }

            if (keyValuePairs.TryGetValue(ClaimTypes.Email, out var email))
            {
                claims.Add(new Claim(ClaimTypes.Email, email.ToString()!));
            }
            
            // ? CORRECTION: Essayer "email" standard
            if (keyValuePairs.TryGetValue("email", out var emailStandard))
            {
                claims.Add(new Claim(ClaimTypes.Email, emailStandard.ToString()!));
            }

            // ? CORRECTION CRITIQUE: Gestion des rôles avec TOUTES les variantes
            bool roleFound = false;
            
            // Variante 1: ClaimTypes.Role standard
            if (keyValuePairs.TryGetValue(ClaimTypes.Role, out var rolesClaim))
            {
                roleFound = true;
                AddRoleClaims(claims, rolesClaim);
            }
            
            // Variante 2: "role" (minuscule)
            if (!roleFound && keyValuePairs.TryGetValue("role", out var roleStandard))
            {
                roleFound = true;
                AddRoleClaims(claims, roleStandard);
            }
            
            // Variante 3: "roles" (pluriel)
            if (!roleFound && keyValuePairs.TryGetValue("roles", out var rolesStandard))
            {
                roleFound = true;
                AddRoleClaims(claims, rolesStandard);
            }
            
            // Variante 4: "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            var msRoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
            if (!roleFound && keyValuePairs.TryGetValue(msRoleClaimType, out var msRoles))
            {
                roleFound = true;
                AddRoleClaims(claims, msRoles);
            }

            // Ajouter d'autres claims si nécessaire
            var excludedKeys = new[] { 
                ClaimTypes.NameIdentifier, ClaimTypes.Name, ClaimTypes.Email, ClaimTypes.Role,
                "sub", "nameid", "unique_name", "email", "role", "roles", msRoleClaimType,
                "iat", "exp", "aud", "iss", "nbf", "jti"
            };
            
            foreach (var kvp in keyValuePairs)
            {
                if (!excludedKeys.Contains(kvp.Key))
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value.ToString()!));
                }
            }
        }

        return claims;
    }

    // ? NOUVELLE MÉTHODE: Ajouter claims rôles de manière robuste
    private void AddRoleClaims(List<Claim> claims, object rolesClaim)
    {
        if (rolesClaim is JsonElement rolesElement)
        {
            if (rolesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var role in rolesElement.EnumerateArray())
                {
                    var roleValue = role.GetString();
                    if (!string.IsNullOrEmpty(roleValue))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, roleValue));
                    }
                }
            }
            else if (rolesElement.ValueKind == JsonValueKind.String)
            {
                var roleValue = rolesElement.GetString();
                if (!string.IsNullOrEmpty(roleValue))
                {
                    claims.Add(new Claim(ClaimTypes.Role, roleValue));
                }
            }
        }
        else if (rolesClaim is string roleString)
        {
            if (!string.IsNullOrEmpty(roleString))
            {
                claims.Add(new Claim(ClaimTypes.Role, roleString));
            }
        }
        else
        {
            var roleValue = rolesClaim.ToString();
            if (!string.IsNullOrEmpty(roleValue))
            {
                claims.Add(new Claim(ClaimTypes.Role, roleValue));
            }
        }
    }
}