using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Expansion;

public sealed class AreaUnlockSystem
{
    private readonly GameWorld _world;

    public AreaUnlockSystem(GameWorld world)
    {
        _world = world;
    }

    public AreaUnlockResult UnlockCloud(GridPosition position)
    {
        var validation = CanUnlockCloud(position);
        if (validation != AreaUnlockFailureReason.None)
            return AreaUnlockResult.Fail(position, validation);

        if (!_world.Resources.TrySpendMoney(_world.AreaUnlockSettings.CloudUnlockMoneyCost))
            return AreaUnlockResult.Fail(position, AreaUnlockFailureReason.NotEnoughMoney);

        if (!_world.Resources.TrySpendResearch(_world.AreaUnlockSettings.CloudUnlockResearchCost))
            return AreaUnlockResult.Fail(position, AreaUnlockFailureReason.NotEnoughResearch);

        var tilesToReveal = GetUnlockableCloudTiles(position);
        var revealedTiles = new List<AreaUnlockRevealedTile>();

        foreach (var tilePosition in tilesToReveal)
        {
            var tile = _world.Map.GetTile(tilePosition);
            var revealed = tile.RevealCoveredType();
            revealedTiles.Add(new AreaUnlockRevealedTile(tilePosition, revealed));
        }

        return AreaUnlockResult.Ok(position, revealedTiles);
    }

    public AreaUnlockFailureReason CanUnlockCloud(GridPosition position)
    {
        if (!_world.Map.Contains(position))
            return AreaUnlockFailureReason.OutOfMap;

        var tile = _world.Map.GetTile(position);

        if (tile.Type != TileType.Cloud)
            return AreaUnlockFailureReason.TileNotCloud;

        if (tile.HasBuilding)
            return AreaUnlockFailureReason.TileAlreadyOccupied;

        if (!tile.CoveredType.HasValue)
            return AreaUnlockFailureReason.MissingHiddenTile;

        if (_world.Resources.Money < _world.AreaUnlockSettings.CloudUnlockMoneyCost)
            return AreaUnlockFailureReason.NotEnoughMoney;

        if (_world.Resources.Research < _world.AreaUnlockSettings.CloudUnlockResearchCost)
            return AreaUnlockFailureReason.NotEnoughResearch;

        return AreaUnlockFailureReason.None;
    }

    public IReadOnlyList<GridPosition> GetUnlockableCloudTiles(GridPosition start)
    {
        if (CanUnlockCloud(start) is AreaUnlockFailureReason.OutOfMap or AreaUnlockFailureReason.TileNotCloud or AreaUnlockFailureReason.MissingHiddenTile)
            return Array.Empty<GridPosition>();

        var radius = Math.Max(0, _world.AreaUnlockSettings.CloudUnlockRadius);
        var maxTiles = Math.Max(1, _world.AreaUnlockSettings.MaxCloudTilesPerUnlock);
        var result = new List<GridPosition>();
        var visited = new HashSet<GridPosition>();
        var queue = new Queue<GridPosition>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0 && result.Count < maxTiles)
        {
            var current = queue.Dequeue();
            var distance = Math.Abs(current.X - start.X) + Math.Abs(current.Y - start.Y);
            if (distance > radius)
                continue;

            if (_world.Map.Contains(current))
            {
                var tile = _world.Map.GetTile(current);
                if (tile.Type == TileType.Cloud && tile.CoveredType.HasValue && !tile.HasBuilding)
                    result.Add(current);
            }

            foreach (var neighbor in GetCardinalNeighbors(current))
            {
                if (visited.Contains(neighbor))
                    continue;

                if (!_world.Map.Contains(neighbor))
                    continue;

                var neighborDistance = Math.Abs(neighbor.X - start.X) + Math.Abs(neighbor.Y - start.Y);
                if (neighborDistance > radius)
                    continue;

                var neighborTile = _world.Map.GetTile(neighbor);
                if (neighborTile.Type != TileType.Cloud)
                    continue;

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        return result;
    }

    private static IEnumerable<GridPosition> GetCardinalNeighbors(GridPosition position)
    {
        yield return new GridPosition(position.X + 1, position.Y);
        yield return new GridPosition(position.X - 1, position.Y);
        yield return new GridPosition(position.X, position.Y + 1);
        yield return new GridPosition(position.X, position.Y - 1);
    }
}
