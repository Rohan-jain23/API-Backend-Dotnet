using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate.AspNetCore;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Http;

namespace FrameworkAPI.Interceptors;

/// <summary>
/// Middleware to wrap data from a request.
/// </summary>
public class HttpRequestInterceptor : DefaultHttpRequestInterceptor
{
    /// <summary>
    /// Initialize the Global State before the request is being executed.
    /// </summary>
    public override ValueTask OnCreateAsync(
        HttpContext context,
        IRequestExecutor requestExecutor,
        IQueryRequestBuilder requestBuilder,
        CancellationToken cancellationToken)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        requestBuilder.SetGlobalState("userId", userId);

        return base.OnCreateAsync(
            context,
            requestExecutor,
            requestBuilder,
            cancellationToken);
    }
}