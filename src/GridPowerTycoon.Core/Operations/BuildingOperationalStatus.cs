namespace GridPowerTycoon.Core.Operations;

public sealed record BuildingOperationalStatus(
    BuildingOperationalState State,
    string Label,
    double EnergyInputPerSecond,
    double EnergyOutputPerSecond,
    double HeatOutputPerSecond,
    double HeatStored,
    double HeatWarningThreshold,
    double HeatExplosionThreshold,
    double HeatConversionInputPerSecond,
    double HeatConversionEnergyOutputPerSecond,
    double ResearchOutputPerSecond,
    double AutoSellInputPerSecond,
    double BatteryCapacity,
    bool HasHeatConverterInRange);
