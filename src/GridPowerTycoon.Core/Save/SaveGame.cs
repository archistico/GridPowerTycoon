using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Map;

namespace GridPowerTycoon.Core.Save;

public sealed class SaveGame
{
    public int Version { get; init; } = 1;
    public DateTimeOffset SavedAt { get; init; } = DateTimeOffset.UtcNow;
    public SaveResources Resources { get; init; } = new();
    public SaveMap Map { get; init; } = new();
    public List<SaveBuildingInstance> Buildings { get; init; } = new();
    public List<string> CompletedResearchIds { get; init; } = new();
    public Dictionary<string, int> UpgradeLevels { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class SaveResources
{
    public double Energy { get; init; }
    public double MaxEnergy { get; init; }
    public double Research { get; init; }
    public decimal Money { get; init; }
    public double Axes { get; init; }
    public double Mines { get; init; }
}

public sealed class SaveMap
{
    public int Width { get; init; }
    public int Height { get; init; }
    public List<SaveTile> Tiles { get; init; } = new();
}

public sealed class SaveTile
{
    public int X { get; init; }
    public int Y { get; init; }
    public TileType Type { get; init; }
    public TileType? CoveredType { get; init; }
    public Guid? BuildingId { get; init; }
}

public sealed class SaveBuildingInstance
{
    public Guid Id { get; init; }
    public string DefinitionId { get; init; } = string.Empty;
    public int X { get; init; }
    public int Y { get; init; }
    public double RemainingLifetimeSeconds { get; init; }
    public double AccumulatedHeat { get; init; }
    public BuildingState State { get; init; }
}
