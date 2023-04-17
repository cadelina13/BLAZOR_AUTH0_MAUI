using ClientApp.Data;
using Microsoft.AspNetCore.Components.Authorization;
using SharedLibrary.Models;
using System.Security.Claims;


public class Auth0AuthenticationStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal currentUser = new ClaimsPrincipal(new ClaimsIdentity());
    private readonly Auth0Client auth0Client;
    private readonly IDataAccess db;
    public Auth0AuthenticationStateProvider(Auth0Client client, IDataAccess _db)
    {
        auth0Client = client;
        this.db = _db;
    }
    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(new AuthenticationState(currentUser));

    public Task LogInAsync()
    {
        var loginTask = LogInAsyncCore();
        NotifyAuthenticationStateChanged(loginTask);

        return loginTask;

        async Task<AuthenticationState> LogInAsyncCore()
        {
            var user = await LoginWithAuth0Async();
            currentUser = user;

            return new AuthenticationState(currentUser);
        }
    }

    private async Task<ClaimsPrincipal> LoginWithAuth0Async()
    {
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity());
        var loginResult = await auth0Client.LoginAsync();

        if (!loginResult.IsError)
        {
            authenticatedUser = loginResult.User;
            await SaveUpdateUser(authenticatedUser);
        }
        return authenticatedUser;
    }

    public async void LogOut()
    {
        await auth0Client.LogoutAsync();
        currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(currentUser)));
    }

    private async Task SaveUpdateUser(ClaimsPrincipal claim)
    {
        var userId = claim.Claims.Where(c => c.Type.Equals("sub"))
                    .Select(c => c.Value)
                    .FirstOrDefault() ?? string.Empty;
        AccountModel user = new AccountModel();
        try
        {
            user = await db.GetUser(userId);
        }catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        if(string.IsNullOrEmpty(user.Id))
        {
            
            var firstName = claim.Claims.Where(c => c.Type.Equals("given_name"))
                        .Select(c => c.Value)
                        .FirstOrDefault() ?? string.Empty;
            var lastName = claim.Claims.Where(c => c.Type.Equals("family_name"))
                        .Select(c => c.Value)
                        .FirstOrDefault() ?? string.Empty;
            var fullName = claim.Claims.Where(c => c.Type.Equals("name"))
                        .Select(c => c.Value)
                        .FirstOrDefault() ?? string.Empty;
            var picture = claim.Claims.Where(c => c.Type.Equals("picture"))
                        .Select(c => c.Value)
                        .FirstOrDefault() ?? string.Empty;
            user.Id = userId;
            user.Fullname = fullName;
            user.FirstName = firstName;
            user.LastName = lastName;
            user.Picture = picture;
            await db.SaveUser(user);
        }
        
    }
}
