using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Tools;

public sealed class TerrainClearSystem
{
    private readonly GameWorld _world;

    public TerrainClearSystem(GameWorld world)
    {
        _world = world;
    }

    public TerrainClearFailureReason CanClear(GridPosition position)
    {
        if (!_world.Map.Contains(position))
            return TerrainClearFailureReason.OutOfMap;

        var tile = _world.Map.GetTile(position);
        if (tile.HasBuilding)
            return TerrainClearFailureReason.TileHasBuilding;

        return tile.Type switch
        {
            TileType.Forest when _world.Resources.Axes < _world.ToolSettings.ForestClearAxesCost => TerrainClearFailureReason.NotEnoughAxes,
            TileType.Forest => TerrainClearFailureReason.None,
            TileType.Mountain when _world.Resources.Mines < _world.ToolSettings.MountainClearMinesCost => TerrainClearFailureReason.NotEnoughMines,
            TileType.Mountain => TerrainClearFailureReason.None,
            _ => TerrainClearFailureReason.NotClearableTerrain
        };
    }

    public TerrainClearResult Clear(GridPosition position)
    {
        var failure = CanClear(position);
        if (failure != TerrainClearFailureReason.None)
            return TerrainClearResult.Fail(failure);

        var tile = _world.Map.GetTile(position);

        if (tile.Type == TileType.Forest)
        {
            if (!_world.Resources.TrySpendAxes(_world.ToolSettings.ForestClearAxesCost))
                return TerrainClearResult.Fail(TerrainClearFailureReason.NotEnoughAxes);
        }
        else if (tile.Type == TileType.Mountain)
        {
            if (!_world.Resources.TrySpendMines(_world.ToolSettings.MountainClearMinesCost))
                return TerrainClearResult.Fail(TerrainClearFailureReason.NotEnoughMines);
        }

        tile.SetType(TileType.Land);
        return TerrainClearResult.Ok();
    }
}
