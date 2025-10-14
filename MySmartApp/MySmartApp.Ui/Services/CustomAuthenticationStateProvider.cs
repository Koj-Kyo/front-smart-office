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

            if (keyValuePairs.TryGetValue(ClaimTypes.Name, out var name))
            {
                claims.Add(new Claim(ClaimTypes.Name, name.ToString()!));
            }

            if (keyValuePairs.TryGetValue(ClaimTypes.Email, out var email))
            {
                claims.Add(new Claim(ClaimTypes.Email, email.ToString()!));
            }

            // Gestion des rôles (peut être un seul rôle ou un tableau)
            if (keyValuePairs.TryGetValue(ClaimTypes.Role, out var rolesClaim))
            {
                if (rolesClaim is JsonElement rolesElement)
                {
                    if (rolesElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var role in rolesElement.EnumerateArray())
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role.GetString()!));
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(ClaimTypes.Role, rolesElement.GetString()!));
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, rolesClaim.ToString()!));
                }
            }

            // Ajouter d'autres claims si nécessaire
            foreach (var kvp in keyValuePairs)
            {
                if (kvp.Key != ClaimTypes.NameIdentifier && 
                    kvp.Key != ClaimTypes.Name && 
                    kvp.Key != ClaimTypes.Email && 
                    kvp.Key != ClaimTypes.Role &&
                    kvp.Key != "iat" && 
                    kvp.Key != "exp" && 
                    kvp.Key != "aud" && 
                    kvp.Key != "iss")
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value.ToString()!));
                }
            }
        }

        return claims;
    }
}