using System;
using FrameworkAPI.Models.Enums;
using WuH.Ruby.Common.Core;

namespace FrameworkAPI.Extensions;

public static class KpiAttributeExtensions
{
    public static string ToVariableIdentifier(this KpiAttribute kpiAttribute)
    {
        var variableIdentifier = kpiAttribute switch
        {
            KpiAttribute.GoodProductionCount
                or KpiAttribute.OriginalGoodProductionCount
                or KpiAttribute.ScrapProductionCount
                or KpiAttribute.SetupScrapCount
                or KpiAttribute.TargetTotalScrapCount
                => VariableIdentifier.JobQuantityActual,

            KpiAttribute.GoodProductionCountInSecondUnit
                or KpiAttribute.ScrapProductionCountInSecondUnit
                or KpiAttribute.SetupScrapCountInSecondUnit
                => VariableIdentifier.JobQuantityActualInSecondUnit,

            KpiAttribute.TargetSpeed
                or KpiAttribute.ThroughputRate
                or KpiAttribute.AverageProductionSpeed
                or KpiAttribute.TargetThroughputRate
                => VariableIdentifier.MachineSpeed,

            _ => throw new ArgumentException($"{kpiAttribute} can not be mapped to machine metadata unit.")
        };

        return variableIdentifier;
    }
}