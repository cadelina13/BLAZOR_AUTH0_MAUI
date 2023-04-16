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
            SaveUpdateUser(authenticatedUser);
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

    private async void SaveUpdateUser(ClaimsPrincipal claim)
    {
        var userId = claim.GetUserId();
        var user = new AccountModel();
        user.Id = userId;
        user.Fullname = claim.Identity.Name;
        await db.UpdateUser(user);
    }
}
