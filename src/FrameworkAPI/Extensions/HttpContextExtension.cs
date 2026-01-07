using Microsoft.AspNetCore.Http;

namespace FrameworkAPI.Extensions;

public static class HttpContextExtension
{
    public static bool IsSubscriptionOrNull(this HttpContext? context)
    {
        return context is null || context.Request.Method.Equals("GET") && context.WebSockets.IsWebSocketRequest;
    }
}