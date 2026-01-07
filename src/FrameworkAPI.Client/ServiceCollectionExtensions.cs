using System;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace WuH.Ruby.FrameworkAPI.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthenticatedFrameworkAPIClients(
        this IServiceCollection services,
        Func<IServiceProvider, string> getAuthServiceUrl,
        Func<IServiceProvider, string> getFrameworkApiUrl,
        Action<IHttpClientBuilder> configureClientBuilder)
    {
        configureClientBuilder(services
            .AddHttpClient(
                nameof(ApiInternalClientSecretAuthTokenClient),
                (serviceProvider, client) =>
                {
                    client.BaseAddress = new Uri(getAuthServiceUrl(serviceProvider));
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                })
            .AddHeaderPropagation());
        services.AddScoped<IClientSecretAuthTokenClient, ApiInternalClientSecretAuthTokenClient>();
        services.AddTransient<SetApiInternalAuthHeaderHttpMessageHandler>();
        services
            .AddFrameworkAPIGraphQLClient()
            .ConfigureHttpClient(
                (serviceProvider, client) =>
                {
                    client.BaseAddress = new Uri(getFrameworkApiUrl(serviceProvider));
                },
                builder => configureClientBuilder(
                    builder
                        .AddHeaderPropagation()
                        .AddHttpMessageHandler<SetApiInternalAuthHeaderHttpMessageHandler>()));

        services.AddTransient<IFrameworkAPIClientForProducedJob, FrameworkAPIClientForProducedJob>();
        services.AddTransient<IFrameworkAPIClientForMachineTimeSpan, FrameworkAPIClientForMachineTimeSpan>();
        services.AddTransient<IFrameworkAPIClientForMutations, FrameworkAPIClientForMutations>();

        return services;
    }
}