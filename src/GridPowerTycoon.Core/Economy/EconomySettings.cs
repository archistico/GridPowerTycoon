namespace GridPowerTycoon.Core.Economy;

public sealed class EconomySettings
{
    public int Version { get; init; } = 1;

    public decimal StartingMoney { get; init; } = 1m;

    public double StartingEnergy { get; init; }
    public double StartingMaxEnergy { get; init; } = 100;
    public double StartingResearch { get; init; }

    public decimal EnergySellValue { get; init; } = 1m;

    public double ManualSellMultiplier { get; init; } = 1;
    public double AutoSellMultiplier { get; init; } = 1;

    public double MaxOfflineSeconds { get; init; } = 7200;

    public int StartingAxes { get; init; }
    public int StartingMines { get; init; }
}
