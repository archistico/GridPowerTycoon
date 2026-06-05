using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Economy;

public sealed class ProductionSystem
{
    private readonly GameWorld _world;

    public ProductionSystem(GameWorld world)
    {
        _world = world;
    }

    public void Update(double deltaSeconds)
    {
        if (deltaSeconds <= 0)
            return;

        foreach (var instance in _world.BuildingInstances.Values)
        {
            if (!instance.IsActive)
                continue;

            if (!_world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            _world.Resources.AddEnergy(definition.EnergyPerSecond * deltaSeconds);
            _world.Resources.AddResearch(definition.ResearchPerSecond * deltaSeconds);
        }
    }
}
