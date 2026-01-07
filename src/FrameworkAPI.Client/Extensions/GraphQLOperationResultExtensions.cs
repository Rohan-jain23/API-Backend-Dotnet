using System.Linq;
using System.Text;
using StrawberryShake;
using WuH.Ruby.Common.Core;

namespace WuH.Ruby.FrameworkAPI.Client;

public static class GraphQLOperationResultExtensions
{
    private static string GenerateErrorMessage(IOperationResult operationResult)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{operationResult.Errors.Count} error(s) occurred when executing the query:");

        foreach (var error in operationResult.Errors)
        {
            stringBuilder.AppendLine($"- Message: {error.Message}");

            if (error.Path is not null)
            {
                stringBuilder.AppendLine($"  Path: {string.Join('.', error.Path)}");
            }

            if (error.Extensions is null)
            {
                continue;
            }

            if (error.Extensions.TryGetValue("message", out var message) && message is not null)
            {
                stringBuilder.AppendLine($"  Extensions.Message: {message}");
            }

            if (error.Extensions.TryGetValue("stackTrace", out var stackTrace) && stackTrace is not null)
            {
                stringBuilder.AppendLine($"  Extensions.StackTrace: {stackTrace}");
            }
        }

        return stringBuilder.ToString();
    }

    public static InternalItemResponse<T> ToInternalItemResponse<T>(this IOperationResult<T> operationResult) where T : class
    {
        if (operationResult.Errors.Any())
        {
            return new InternalItemResponse<T>(500, GenerateErrorMessage(operationResult));
        }

        var data = operationResult.Data;

        return data is null
            ? new InternalItemResponse<T>(500, "Unexpectedly received null for Data in from GraphQl in the operation result.")
            : new InternalItemResponse<T>(data);
    }

    public static InternalResponse ToInternalResponse<T>(this IOperationResult<T> operationResult) where T : class
    {
        if (operationResult.Errors.Any())
        {
            return new InternalResponse(500, GenerateErrorMessage(operationResult));
        }

        return new InternalResponse();
    }
}
