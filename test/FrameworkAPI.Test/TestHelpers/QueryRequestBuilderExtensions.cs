using System.Linq;
using System.Security.Claims;
using HotChocolate.Execution;

namespace FrameworkAPI.Test.TestHelpers;

public static class QueryRequestBuilderExtensions
{
    public static IQueryRequestBuilder AddRoleClaims(this IQueryRequestBuilder builder, params string[] roles) =>
        builder.AddGlobalState(
            nameof(ClaimsPrincipal),
            new ClaimsPrincipal(
                new ClaimsIdentity(
                    roles
                        .Select(role => new Claim(ClaimTypes.Role, role))
                        .ToArray())));
}