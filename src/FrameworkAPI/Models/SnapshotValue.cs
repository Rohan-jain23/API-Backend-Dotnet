namespace FrameworkAPI.Models;

public class SnapshotValue(string columnId, object? columnValue, bool? isCreatedByVirtualTime = false)
{
    public string ColumnId { get; } = columnId;
    public object? ColumnValue { get; } = columnValue;
    public bool? IsCreatedByVirtualTime { get; } = isCreatedByVirtualTime;
}