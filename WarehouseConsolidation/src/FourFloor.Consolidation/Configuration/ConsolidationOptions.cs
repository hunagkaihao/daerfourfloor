namespace FourFloor.Consolidation.Configuration;

public sealed class ConsolidationOptions
{
    public const string SectionName = "Consolidation";

    public bool Enabled { get; set; } = true;
    public bool ExecutionEnabled { get; set; }
    public bool RequireManualConfirmation { get; set; } = true;
    public string OperatorKey { get; set; } = string.Empty;
    public List<int> Rows { get; set; } = [12, 11, 10, 9, 8, 7, 6, 5, 4];
    public List<int> LayerOrder { get; set; } = [2, 1];
    public bool AllowCrossRow { get; set; } = true;
    public List<CellRangeOptions> ExcludedRanges { get; set; } = [];
    public List<string> BufferCells { get; set; } = [];
    public int MinimumEmptyBufferCells { get; set; } = 1;
    public int PollIntervalSeconds { get; set; } = 5;
    public int TaskTimeoutMinutes { get; set; } = 60;
    public bool StrictSerialExecution { get; set; } = true;
    public bool StopOnDataChange { get; set; } = true;
    public bool StopOnTaskFailure { get; set; } = true;
}

public sealed class CellRangeOptions
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}
