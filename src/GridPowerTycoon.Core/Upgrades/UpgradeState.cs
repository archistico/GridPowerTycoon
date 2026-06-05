namespace GridPowerTycoon.Core.Upgrades;

public sealed class UpgradeState
{
    private readonly Dictionary<string, int> _levels = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, int> Levels => _levels;

    public int GetLevel(string upgradeId)
    {
        return _levels.TryGetValue(upgradeId, out var level) ? level : 0;
    }

    public bool IsPurchased(string upgradeId)
    {
        return GetLevel(upgradeId) > 0;
    }

    public void IncreaseLevel(string upgradeId)
    {
        if (string.IsNullOrWhiteSpace(upgradeId))
            throw new ArgumentException("Upgrade id cannot be empty.", nameof(upgradeId));

        _levels[upgradeId] = GetLevel(upgradeId) + 1;
    }
}
