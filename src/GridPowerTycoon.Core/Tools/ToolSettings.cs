namespace GridPowerTycoon.Core.Tools;

public sealed class ToolSettings
{
    public int Version { get; init; } = 1;

    public double AxesPerSecond { get; init; } = 0.02;
    public double MinesPerSecond { get; init; } = 0.01;

    public int MaxAxes { get; init; } = 20;
    public int MaxMines { get; init; } = 20;

    public int ForestClearAxesCost { get; init; } = 4;
    public int MountainClearMinesCost { get; init; } = 4;
}
