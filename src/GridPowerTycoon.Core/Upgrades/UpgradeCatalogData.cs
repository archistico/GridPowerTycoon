namespace GridPowerTycoon.Core.Upgrades;

public sealed class UpgradeCatalogData
{
    public int Version { get; init; } = 1;
    public List<UpgradeDefinition> Upgrades { get; init; } = new();
}
