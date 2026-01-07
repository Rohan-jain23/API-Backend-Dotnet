using FrameworkAPI.Schema.Misc;
using WuH.Ruby.KpiDataHandler.Client;

namespace FrameworkAPI.Extensions;

public static class TargetValueExtensions
{
    public static TargetValueSource? ToFrameworkApiEnum(
        this TargetSource? kpiDataHandlerEnum)
    {
        return kpiDataHandlerEnum switch
        {
            TargetSource.JobCorrection => TargetValueSource.JobCorrection,
            TargetSource.ProductGroup => TargetValueSource.ProductGroup,
            TargetSource.ProductionRequest => TargetValueSource.ProductionRequest,
            TargetSource.Machine => TargetValueSource.Machine,
            TargetSource.AdminSetting => TargetValueSource.AdminSetting,
            _ => null
        };
    }
}