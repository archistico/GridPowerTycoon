namespace GridPowerTycoon.Core.Map;

public sealed class Tile
{
    public GridPosition Position { get; }
    public TileType Type { get; private set; }
    public Guid? BuildingId { get; private set; }

    public bool HasBuilding => BuildingId.HasValue;

    public bool IsBuildable => Type == TileType.Land && !HasBuilding;

    public Tile(GridPosition position, TileType type)
    {
        Position = position;
        Type = type;
    }

    public void SetType(TileType type)
    {
        Type = type;
    }

    public void SetBuilding(Guid buildingId)
    {
        if (HasBuilding)
            throw new InvalidOperationException("Tile already has a building.");

        BuildingId = buildingId;
    }

    public void ClearBuilding()
    {
        BuildingId = null;
    }
}
