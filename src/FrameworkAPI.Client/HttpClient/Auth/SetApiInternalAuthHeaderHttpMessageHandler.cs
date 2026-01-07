using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace WuH.Ruby.FrameworkAPI.Client;

internal class SetApiInternalAuthHeaderHttpMessageHandler(
    IApiInternalClientSecretProvider apiInternalClientSecretProvider,
    IClientSecretAuthTokenClient authTokenClient)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var apiClientSecret = await apiInternalClientSecretProvider.GetApiInternalClientSecret(cancellationToken);

        var bearerToken = await authTokenClient.GetToken(apiClientSecret, cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        return await base.SendAsync(request, cancellationToken);
    }
}