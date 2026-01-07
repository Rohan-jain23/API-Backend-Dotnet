using System;
using System.Threading.Tasks;
using PhysicalAssetDataHandler.Client.Models;

namespace FrameworkAPI.Extensions;

public static class ValueWithUnitExtensions
{
    public static Schema.Misc.ValueWithUnit<T> ToSchema<T>(this ValueWithUnit<T> valueWithUnit)
        where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
    {
        return new Schema.Misc.ValueWithUnit<T>(
            valueFunc: _ => Task.FromResult((T?)valueWithUnit.Value),
            unitFunc: _ => Task.FromResult((string?)valueWithUnit.Unit));
    }
}