namespace GridPowerTycoon.Core.Tools;

public sealed class TerrainClearResult
{
    public bool Success { get; }
    public TerrainClearFailureReason FailureReason { get; }

    private TerrainClearResult(bool success, TerrainClearFailureReason failureReason)
    {
        Success = success;
        FailureReason = failureReason;
    }

    public static TerrainClearResult Ok()
    {
        return new TerrainClearResult(true, TerrainClearFailureReason.None);
    }

    public static TerrainClearResult Fail(TerrainClearFailureReason reason)
    {
        return new TerrainClearResult(false, reason);
    }
}
