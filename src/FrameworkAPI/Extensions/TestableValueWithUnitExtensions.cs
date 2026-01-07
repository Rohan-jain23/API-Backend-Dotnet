using System;
using PhysicalAssetDataHandler.Client.Models;

namespace FrameworkAPI.Extensions;

public static class TestableValueWithUnitExtensions
{
    public static Schema.PhysicalAsset.TestableValueWithUnit<T> ToSchema<T>(
        this TestableValueWithUnit<T> testableValueWithUnit)
        where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
    {
        return new Schema.PhysicalAsset.TestableValueWithUnit<T>(
            testableValueWithUnit.SetValue,
            testableValueWithUnit.MeasuredValue,
            testableValueWithUnit.MeasuredAt,
            testableValueWithUnit.Unit);
    }
}