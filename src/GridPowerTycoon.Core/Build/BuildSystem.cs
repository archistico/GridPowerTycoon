using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Build;

public sealed class BuildSystem
{
    private readonly GameWorld _world;

    public BuildSystem(GameWorld world)
    {
        _world = world;
    }

    public BuildResult Build(string buildingDefinitionId, GridPosition position)
    {
        if (!_world.BuildingCatalog.TryGet(buildingDefinitionId, out var definition))
            return BuildResult.Fail(BuildFailureReason.UnknownBuilding);

        var validation = CanBuild(definition, position);
        if (validation != BuildFailureReason.None)
            return BuildResult.Fail(validation);

        if (!_world.Resources.TrySpendMoney(definition.Cost))
            return BuildResult.Fail(BuildFailureReason.NotEnoughMoney);

        var instance = new BuildingInstance(
            Guid.NewGuid(),
            definition.Id,
            position,
            definition.LifetimeSeconds);

        _world.AddBuilding(instance);
        OccupyTiles(instance, definition);
        ApplyImmediateBuildingEffects(definition);

        return BuildResult.Ok(instance.Id);
    }

    public BuildFailureReason CanBuild(string buildingDefinitionId, GridPosition position)
    {
        if (!_world.BuildingCatalog.TryGet(buildingDefinitionId, out var definition))
            return BuildFailureReason.UnknownBuilding;

        return CanBuild(definition, position);
    }


    public BuildResult ReplaceExpired(Guid buildingId)
    {
        if (!_world.TryGetBuilding(buildingId, out var instance))
            return BuildResult.Fail(BuildFailureReason.BuildingNotFound);

        if (!_world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
            return BuildResult.Fail(BuildFailureReason.UnknownBuilding);

        if (instance.State != BuildingState.Expired)
            return BuildResult.Fail(BuildFailureReason.BuildingNotExpired);

        if (!_world.Resources.TrySpendMoney(definition.Cost))
            return BuildResult.Fail(BuildFailureReason.NotEnoughMoney);

        instance.Replace(definition.LifetimeSeconds);
        return BuildResult.Ok(instance.Id);
    }

    public BuildResult ReplaceExpiredAt(GridPosition position)
    {
        if (!_world.Map.Contains(position))
            return BuildResult.Fail(BuildFailureReason.OutOfMap);

        var tile = _world.Map.GetTile(position);
        if (!tile.BuildingId.HasValue)
            return BuildResult.Fail(BuildFailureReason.BuildingNotFound);

        return ReplaceExpired(tile.BuildingId.Value);
    }

    private BuildFailureReason CanBuild(BuildingDefinition definition, GridPosition position)
    {
        if (definition.Width <= 0 || definition.Height <= 0)
            return BuildFailureReason.InvalidBuildingSize;

        for (var y = 0; y < definition.Height; y++)
        {
            for (var x = 0; x < definition.Width; x++)
            {
                var tilePosition = new GridPosition(position.X + x, position.Y + y);

                if (!_world.Map.Contains(tilePosition))
                    return BuildFailureReason.OutOfMap;

                var tile = _world.Map.GetTile(tilePosition);

                if (tile.HasBuilding)
                    return BuildFailureReason.TileAlreadyOccupied;

                if (tile.Type != TileType.Land)
                    return BuildFailureReason.TileNotBuildable;
            }
        }

        if (_world.Resources.Money < definition.Cost)
            return BuildFailureReason.NotEnoughMoney;

        if (!string.IsNullOrWhiteSpace(definition.RequiredResearchId) &&
            !_world.Research.IsCompleted(definition.RequiredResearchId))
        {
            return BuildFailureReason.ResearchRequired;
        }

        return BuildFailureReason.None;
    }

    private void OccupyTiles(BuildingInstance instance, BuildingDefinition definition)
    {
        for (var y = 0; y < definition.Height; y++)
        {
            for (var x = 0; x < definition.Width; x++)
            {
                var tilePosition = new GridPosition(instance.Position.X + x, instance.Position.Y + y);
                _world.Map.GetTile(tilePosition).SetBuilding(instance.Id);
            }
        }
    }

    private void ApplyImmediateBuildingEffects(BuildingDefinition definition)
    {
        if (definition.BatteryCapacity > 0)
            _world.Resources.IncreaseMaxEnergy(definition.BatteryCapacity);
    }
}
