using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace WuH.Ruby.FrameworkAPI.Client;

public class ApiInternalClientSecretAuthTokenClient(IHttpClientFactory httpClientFactory) : IClientSecretAuthTokenClient
{
    private class AccessTokenResponse
    {
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; init; }
    }

    private readonly HttpClient _httpClient =
        httpClientFactory.CreateClient(nameof(ApiInternalClientSecretAuthTokenClient));

    public async Task<string> GetToken(string apiClientSecret, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsync(
            "/auth/realms/master/protocol/openid-connect/token",
            new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials"),
                new("client_id", "api-internal"),
                new("scope", "api-internal-general"),
                new("client_secret", apiClientSecret)
            }),
            cancellationToken);

        var content = await response.Content.ReadAsStreamAsync(cancellationToken);

        var result = await JsonSerializer.DeserializeAsync<AccessTokenResponse>(
            content,
            cancellationToken: cancellationToken);

        return result?.AccessToken
               ?? throw new Exception("Cannot deserialize token response");
    }
}