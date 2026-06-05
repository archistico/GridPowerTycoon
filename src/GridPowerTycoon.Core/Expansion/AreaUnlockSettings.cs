namespace GridPowerTycoon.Core.Expansion;

public sealed class AreaUnlockSettings
{
    public int Version { get; init; } = 1;
    public decimal CloudUnlockMoneyCost { get; init; } = 500m;
    public double CloudUnlockResearchCost { get; init; } = 0;
}
