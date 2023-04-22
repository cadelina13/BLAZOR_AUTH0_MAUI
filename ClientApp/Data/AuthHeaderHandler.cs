using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace ClientApp.Data
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var savedToken = await SecureStorage.Default.GetAsync("bearerToken");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", savedToken);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
