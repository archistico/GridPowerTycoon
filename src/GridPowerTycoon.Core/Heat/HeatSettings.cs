namespace GridPowerTycoon.Core.Heat;

public sealed class HeatSettings
{
    public int Version { get; init; } = 1;

    public double HeatWarningThreshold { get; init; } = 60;
    public double HeatExplosionThreshold { get; init; } = 100;
    public double HeatEnergyConversionRate { get; init; } = 1;
}
