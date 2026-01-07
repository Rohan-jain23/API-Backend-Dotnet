using HotChocolate;
using HotChocolate.Types;

namespace FrameworkAPI.Test.Interceptors.Helper;

/// <summary>
/// GraphQL helper query to validate the setting of the userId by HttpRequestInterceptor.
/// </summary>
[ExtendObjectType("Query")]
public class HttpRequestInterceptorTestQuery
{
    /// <summary>
    /// Query to get userId from global settings.
    /// </summary>
    public string GetUserId([GlobalState] string userId) => userId;
}