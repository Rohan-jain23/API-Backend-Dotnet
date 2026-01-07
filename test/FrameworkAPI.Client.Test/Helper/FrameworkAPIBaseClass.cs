using System.Collections.Generic;
using System.Text;
using StrawberryShake;

namespace FrameworkAPI.Client.Test;

public class FrameworkAPIBaseClass
{
    protected string GenerateErrorMessage(List<IClientError> errorList)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{errorList.Count} error(s) occurred when executing the query:");

        foreach (var error in errorList)
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
}
