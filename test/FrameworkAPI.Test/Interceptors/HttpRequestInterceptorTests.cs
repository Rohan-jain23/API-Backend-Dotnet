using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Interceptors;
using FrameworkAPI.Test.Interceptors.Helper;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FrameworkAPI.Test.Interceptors;

public class HttpRequestInterceptorTests
{
    private readonly Mock<ILogger<DefaultAuthorizationService>> _defaultAuthorizationServiceMock = new();
    private readonly HttpRequestInterceptor _subject = new();

    [Fact]
    public async Task OnCreateAsync_Makes_NameIdentifier_Available_In_GlobalState()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "TestUserId")
        };

        var context = new DefaultHttpContext();
        context.User.AddIdentity(new ClaimsIdentity(claims));

        var queryRequest = QueryRequestBuilder.New().SetQuery("{ userId }");
        var executor = await InitializeExecutor();

        // Act
        await _subject.OnCreateAsync(context, executor, queryRequest, CancellationToken.None);

        // Assert
        await using var result = await executor.ExecuteAsync(queryRequest.Create());
        result.ToJson().Should().Contain("TestUserId");
    }

    private async Task<IRequestExecutor> InitializeExecutor()
    {
        var services = new ServiceCollection();
        return await services
            .AddSingleton(_defaultAuthorizationServiceMock.Object)
            .AddHttpContextAccessor()
            .AddAuthorization()
            .AddGraphQLServer()
            .AddAuthorization()
            .AddQueryType(q => q.Name("Query"))
            .AddType<HttpRequestInterceptorTestQuery>()
            .BuildRequestExecutorAsync();
    }
}