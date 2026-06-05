namespace GridPowerTycoon.Core.Upgrades;

public sealed class UpgradeResult
{
    public bool Success { get; }
    public UpgradeFailureReason FailureReason { get; }
    public string? UpgradeId { get; }

    private UpgradeResult(bool success, UpgradeFailureReason failureReason, string? upgradeId)
    {
        Success = success;
        FailureReason = failureReason;
        UpgradeId = upgradeId;
    }

    public static UpgradeResult Ok(string upgradeId)
    {
        return new UpgradeResult(true, UpgradeFailureReason.None, upgradeId);
    }

    public static UpgradeResult Fail(UpgradeFailureReason reason)
    {
        return new UpgradeResult(false, reason, null);
    }
}
