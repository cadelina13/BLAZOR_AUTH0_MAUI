using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


public class AppAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly JwtSecurityTokenHandler _jwtHandler = new();
    
    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var savedToken = await SecureStorage.Default.GetAsync("bearerToken");
            if (string.IsNullOrEmpty(savedToken))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            JwtSecurityToken jwtSecurityToken = _jwtHandler.ReadJwtToken(savedToken);
            var expiry = jwtSecurityToken.ValidTo;
            if(expiry < DateTime.UtcNow)
            {
                SecureStorage.Default.Remove("bearerToken");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            // Get claims from token and build authenticated user object
            var claims = ParseClaims(jwtSecurityToken);
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            return new AuthenticationState(user);

        }
        catch (Exception)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    private IList<Claim> ParseClaims(JwtSecurityToken jwtSecurityToken)
    {
        var claims = jwtSecurityToken.Claims.ToList();
        // the value of tokenContent.Subject is the user's email
        claims.Add(new Claim(ClaimTypes.Name, jwtSecurityToken.Subject));
        return claims;
    }

    internal async Task SignIn()
    {
        var savedToken = await SecureStorage.Default.GetAsync("bearerToken");
        JwtSecurityToken jwtSecurityToken = _jwtHandler.ReadJwtToken(savedToken);
        var claims = ParseClaims(jwtSecurityToken);
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        var authenticate = Task.FromResult(new AuthenticationState(user));
        NotifyAuthenticationStateChanged(authenticate);
    }

    internal void SignOut()
    {
        ClaimsPrincipal nobody = new ClaimsPrincipal(new ClaimsIdentity());
        var authenticate = Task.FromResult(new AuthenticationState(nobody));
        NotifyAuthenticationStateChanged(authenticate);
    }
}
