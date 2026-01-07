using System.Collections.Generic;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.ProducedJob;

namespace FrameworkAPI.Schema.ProductGroup;

/// <summary>
/// Statistics (produced jobs and aggregated KPIs) of one product group on one machine.
/// The produced jobs might be filtered by time and/or by product.
/// </summary>
public class PaperSackProductGroupStatisticsPerMachine(
    string machineId,
    List<PaperSackProducedJob> producedJobs,
    PaperSackProducedJob? bestJob,
    double totalProducedGoodQuantity,
    ProductionTimes productionTimes,
    ProducedPerformance performance,
    NumericValue? recommendedTargetSpeed,
    List<SpeedHistogramItem>? speedHistogram)
{
    /// <summary>
    /// Identifier of the machine for which these product group statistics were calculated.
    /// </summary>
    public string MachineId { get; } = machineId;

    /// <summary>
    /// All produced jobs of this machine that are belonging to the product group and fit to the filters.
    /// </summary>
    public List<PaperSackProducedJob> ProducedJobs { get; set; } = producedJobs;

    /// <summary>
    /// The job with the highest total productivity won percentage
    /// of all produced jobs of this machine that are belonging to the product group and fit to the filters.
    /// </summary>
    public PaperSackProducedJob? BestJob { get; set; } = bestJob;

    /// <summary>
    /// Sum of all produced items in acceptable quality within all jobs that fit to the filters.
    /// </summary>
    public double TotalProducedGoodQuantity { get; } = totalProducedGoodQuantity;

    /// <summary>
    /// Cumulated minutes the machine was in each production status during the average job that fits to the filters.
    /// Product groups always have 0 minutes as 'NotQueryRelatedTimeInMin' (-> 'TotalPlannedProductionTimeInMin' = 'TotalTimeInMin').
    /// [Source: KPIs]
    /// </summary>
    public ProductionTimes ProductionTimes { get; } = productionTimes;

    /// <summary>
    /// Values that are measuring the productivity of a product group (-> RUBYs alternative for OEE).
    /// These values are calculated by comparing actual values to target/expected values of all jobs that fit to the filters.
    /// The 'ActualValue' and 'TargetValue' are the average values per job.
    /// Therefore, some values are 'null' when the related target values are not given for a job.
    /// If the target values are set properly, this is the perfect measure to evaluate the performance of this product group.
    /// This basically says how well the production was running compared to the expectations.
    /// The greatest advantage over OEE is that the 'Total.WonProductivity' percentage allows comparison of product groups
    /// that not depends on the different setup efforts (which are highly dependant on the previous jobs).
    /// [Source: KPIs]
    /// </summary>
    public ProducedPerformance Performance { get; } = performance;

    /// <summary>
    /// RUBYs recommendation for the target speed of upcoming jobs with this product group.
    /// This value is derived from the aggregated speed histogram of all jobs that fit to the filters.
    /// This is the speed level at which the capacity utilization rate is the highest.
    /// If the machine hasn't been running faster than the determined optimum for at least one hour,
    /// the recommended target speed is increased by one speed level.
    /// Like this, the limit can be pushed and continuous optimization of the productivity is possible.
    /// [Source: KPIs]
    /// </summary>
    public NumericValue? RecommendedTargetSpeed { get; set; } = recommendedTargetSpeed;

    /// <summary>
    /// The speed histogram values of this product group aggregated from all jobs that fit to the filters.
    /// These values show how long a machine was running and how the capacity of the machine was utilized at each speed level.
    /// This list contains one item for each speed level, even if the machine was not running at that level.
    /// This list is sorted ascending by speed level.
    /// The first item is always speed level 50 and the last item is the speed level of the maximum machine speed.
    /// [Source: KPIs]
    /// </summary>
    public List<SpeedHistogramItem>? SpeedHistogram { get; set; } = speedHistogram;
}