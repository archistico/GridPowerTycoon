namespace GridPowerTycoon.Core.Upgrades;

public sealed class UpgradeDefinition
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string? TargetBuildingId { get; init; }
    public decimal CostMoney { get; init; }
    public double CostResearch { get; init; }
    public double CostGrowthMultiplier { get; init; } = 1.75;
    public UpgradeEffectType EffectType { get; init; }
    public double Multiplier { get; init; } = 1;
    public string? RequiredResearchId { get; init; }
    public int MaxLevel { get; init; } = 1;
}
