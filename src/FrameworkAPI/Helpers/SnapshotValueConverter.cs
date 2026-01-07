using System;
using FrameworkAPI.Schema.ProducedJob.MachineSettings.PaperSack;

namespace FrameworkAPI.Helpers;

/// <summary>
/// This helper can be used to pass a convert function to the dynamic ValueDuringProduction to specific enums.
/// </summary>
internal static class SnapshotValueConverter
{
    /// <summary>
    /// Convert bool to paper sack cut type.
    /// </summary>
    /// <param name="isFlushCutBooleanAsObject"></param>
    /// <returns></returns>
    public static PaperSackCutType? ConvertToPaperSackCutType(object? isFlushCutBooleanAsObject)
    {
        if (isFlushCutBooleanAsObject is null)
        {
            return null;
        }

        var isFlushCut = Convert.ToBoolean(isFlushCutBooleanAsObject);

        return isFlushCut ? PaperSackCutType.FlushCut : PaperSackCutType.SteppedEnd;
    }
}