using System;
using System.Linq;

namespace FrameworkAPI.Test.TestHelpers;

public static class EnumExtensions
{
    public static string ToScreamingSnakeCase(this Enum value)
    {
        var name = value.ToString();

        // Insert underscores in front of capital letters except the first one
        var result = string.Concat(name.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? "_" + c : c.ToString()
        ));

        return result.ToUpperInvariant();
    }
}