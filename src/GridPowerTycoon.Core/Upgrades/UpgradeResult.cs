namespace GridPowerTycoon.Core.Upgrades;

public sealed class UpgradeResult
{
    public bool Success { get; }
    public UpgradeFailureReason FailureReason { get; }
    public string? UpgradeId { get; }
    public int NewLevel { get; }

    private UpgradeResult(bool success, UpgradeFailureReason failureReason, string? upgradeId, int newLevel)
    {
        Success = success;
        FailureReason = failureReason;
        UpgradeId = upgradeId;
        NewLevel = newLevel;
    }

    public static UpgradeResult Ok(string upgradeId, int newLevel)
    {
        return new UpgradeResult(true, UpgradeFailureReason.None, upgradeId, newLevel);
    }

    public static UpgradeResult Fail(UpgradeFailureReason reason, string? upgradeId = null)
    {
        return new UpgradeResult(false, reason, upgradeId, 0);
    }
}
