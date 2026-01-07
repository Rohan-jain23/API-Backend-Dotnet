using System;
using FrameworkAPI.Models.Enums;
using WuH.Ruby.KpiDataHandler.Client;

namespace FrameworkAPI.Extensions;

internal static class StandardKpisExtensions
{
    public static bool? GetBoolValue(this StandardKpis standardKpis, KpiAttribute kpiAttribute)
    {
        return kpiAttribute switch
        {
            KpiAttribute.IsApparentlyWrongGoodProductionCount => standardKpis.ProductionData.IsApparentlyWrongGoodProductionCount,
            _ => throw new ArgumentException($"{kpiAttribute} can not be mapped to bool value.")
        };
    }

    public static double? GetDoubleValue(this StandardKpis standardKpis, KpiAttribute kpiAttribute)
    {
        return kpiAttribute switch
        {
            KpiAttribute.TargetSpeed => standardKpis.TargetSpeed,
            KpiAttribute.TotalTimeInMin => standardKpis.TotalTimeInMin,
            KpiAttribute.TotalPlannedProductionTimeInMin => standardKpis.TotalPlannedProductionTimeInMin,
            KpiAttribute.NotQueryRelatedTimeInMin => standardKpis.NotQueryRelatedTimeInMin,
            KpiAttribute.TotalProductionCount => standardKpis.TotalProductionCount,
            KpiAttribute.OriginalGoodProductionCount => standardKpis.OriginalGoodProductionCount,
            KpiAttribute.ThroughputRate => standardKpis.ThroughputRate,
            KpiAttribute.ScrapRatioTotal => standardKpis.ScrapRatioTotal,
            KpiAttribute.ScrapRatioDuringProduction => standardKpis.ScrapRatioDuringProduction,
            KpiAttribute.SetupRatio => standardKpis.SetupRatio,
            KpiAttribute.DownTimeRatio => standardKpis.DownTimeRatio,
            KpiAttribute.Availability => standardKpis.Availability,
            KpiAttribute.Effectiveness => standardKpis.Effectiveness,
            KpiAttribute.QualityRatio => standardKpis.QualityRatio,
            KpiAttribute.OEE => standardKpis.OEE,
            KpiAttribute.AverageProductionSpeed => standardKpis.ProductionData.AverageProductionSpeed,
            KpiAttribute.PlannedNoProductionTimeInMin => standardKpis.ProductionData.PlannedNoProductionTimeInMin,
            KpiAttribute.ScrapTimeInMin => standardKpis.ProductionData.ScrapTimeInMin,
            KpiAttribute.SetupTimeInMin => standardKpis.ProductionData.SetupTimeInMin,
            KpiAttribute.JobRelatedDownTimeInMin => standardKpis.ProductionData.JobRelatedDownTimeInMin,
            KpiAttribute.DownTimeInMin => standardKpis.ProductionData.DownTimeInMin,
            KpiAttribute.ProductionTimeInMin => standardKpis.ProductionData.ProductionTimeInMin,
            KpiAttribute.SetupScrapCount => standardKpis.ProductionData.SetupScrapCount,
            KpiAttribute.SetupScrapCountInSecondUnit => standardKpis.ProductionData.SetupScrapCountInSecondUnit,
            KpiAttribute.ScrapProductionCount => standardKpis.ProductionData.ScrapProductionCount,
            KpiAttribute.ScrapProductionCountInSecondUnit => standardKpis.ProductionData.ScrapProductionCountInSecondUnit,
            KpiAttribute.GoodProductionCount => standardKpis.ProductionData.GoodProductionCount,
            KpiAttribute.GoodProductionCountInSecondUnit => standardKpis.ProductionData.GoodProductionCountInSecondUnit,
            _ => throw new ArgumentException($"{kpiAttribute} can not be mapped to double value.")
        };
    }

    public static double? GetDoubleValue(this StandardProductGroupKpis standardProductGroupKpis, KpiAttribute kpiAttribute)
    {
        try
        {
            return GetDoubleValue((StandardKpis)standardProductGroupKpis, kpiAttribute);
        }
        catch
        {
            return kpiAttribute switch
            {
                KpiAttribute.TargetSetupTimeInMin => standardProductGroupKpis.TargetSetupTimeInMin,
                KpiAttribute.TargetDowntimeInMin => standardProductGroupKpis.TargetDowntimeInMin,
                KpiAttribute.TargetTotalScrapCount => standardProductGroupKpis.TargetTotalScrapCount,
                KpiAttribute.TargetThroughputRate => standardProductGroupKpis.TargetThroughputRate,
                KpiAttribute.LostTimeDueToSpeedInMin => standardProductGroupKpis.PerformanceComparedToTargets.LostTimeDueToSpeedInMin,
                KpiAttribute.LostTimeDueToSetupInMin => standardProductGroupKpis.PerformanceComparedToTargets.LostTimeDueToSetupInMin,
                KpiAttribute.LostTimeDueToDowntimeInMin => standardProductGroupKpis.PerformanceComparedToTargets.LostTimeDueToDowntimeInMin,
                KpiAttribute.LostTimeDueToScrapInMin => standardProductGroupKpis.PerformanceComparedToTargets.LostTimeDueToScrapInMin,
                KpiAttribute.LostTimeTotalInMin => standardProductGroupKpis.PerformanceComparedToTargets.LostTimeTotalInMin,
                KpiAttribute.WonProductivityDueToSpeedInPercent => standardProductGroupKpis.PerformanceComparedToTargets.WonProductivityDueToSpeedInPercent,
                KpiAttribute.WonProductivityDueToSetupInPercent => standardProductGroupKpis.PerformanceComparedToTargets.WonProductivityDueToSetupInPercent,
                KpiAttribute.WonProductivityDueToDowntimeInPercent => standardProductGroupKpis.PerformanceComparedToTargets.WonProductivityDueToDowntimeInPercent,
                KpiAttribute.WonProductivityDueToScrapInPercent => standardProductGroupKpis.PerformanceComparedToTargets.WonProductivityDueToScrapInPercent,
                KpiAttribute.WonProductivityTotalInPercent => standardProductGroupKpis.PerformanceComparedToTargets.WonProductivityTotalInPercent,
                _ => throw new ArgumentException($"{kpiAttribute} can not be mapped to double value.")
            };
        }
    }

    public static double? GetDoubleValue(this StandardJobKpis standardJobKpis, KpiAttribute kpiAttribute)
    {
        try
        {
            return GetDoubleValue((StandardProductGroupKpis)standardJobKpis, kpiAttribute);
        }
        catch
        {
            return kpiAttribute switch
            {
                KpiAttribute.TargetJobTimeInMin => standardJobKpis.TargetJobTimeInMin,
                KpiAttribute.JobSize => standardJobKpis.JobSize,
                _ => throw new ArgumentException($"{kpiAttribute} can not be mapped to double value.")
            };
        }
    }
}