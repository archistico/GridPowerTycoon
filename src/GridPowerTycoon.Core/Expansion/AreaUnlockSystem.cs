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

        var tile = _world.Map.GetTile(position);

        if (!_world.Resources.TrySpendMoney(_world.AreaUnlockSettings.CloudUnlockMoneyCost))
            return AreaUnlockResult.Fail(position, AreaUnlockFailureReason.NotEnoughMoney);

        if (!_world.Resources.TrySpendResearch(_world.AreaUnlockSettings.CloudUnlockResearchCost))
            return AreaUnlockResult.Fail(position, AreaUnlockFailureReason.NotEnoughResearch);

        var revealed = tile.RevealCoveredType();
        return AreaUnlockResult.Ok(position, revealed);
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
}
