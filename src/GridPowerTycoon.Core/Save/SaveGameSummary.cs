namespace GridPowerTycoon.Core.Save;

public sealed record SaveGameSummary(int Version, int DataVersion, DateTimeOffset SavedAt)
{
    public string FormatCompact()
    {
        return $"SAVE V{Version} DATA V{DataVersion} LAST {SavedAt.ToUniversalTime():yyyy-MM-dd HH:mm} UTC";
    }
}
