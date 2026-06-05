using GridPowerTycoon.Core.Upgrades;
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

        ProduceEnergy(deltaSeconds);
        ProduceResearch(deltaSeconds);
    }

    private void ProduceEnergy(double deltaSeconds)
    {
        foreach (var instance in _world.BuildingInstances.Values)
        {
            if (!instance.IsActive)
                continue;

            if (!_world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            var energyPerSecond = UpgradeCalculator.GetEnergyPerSecond(_world, definition);
            if (energyPerSecond <= 0)
                continue;

            var energyConsumption = UpgradeCalculator.GetEnergyConsumptionPerSecond(_world, definition) * deltaSeconds;
            if (!_world.Resources.TrySpendEnergy(energyConsumption))
                continue;

            _world.Resources.AddEnergy(energyPerSecond * deltaSeconds);
        }
    }

    private void ProduceResearch(double deltaSeconds)
    {
        foreach (var instance in _world.BuildingInstances.Values)
        {
            if (!instance.IsActive)
                continue;

            if (!_world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            var researchPerSecond = UpgradeCalculator.GetResearchPerSecond(_world, definition);
            if (researchPerSecond <= 0)
                continue;

            var energyConsumption = UpgradeCalculator.GetEnergyConsumptionPerSecond(_world, definition) * deltaSeconds;
            if (!_world.Resources.TrySpendEnergy(energyConsumption))
                continue;

            _world.Resources.AddResearch(researchPerSecond * deltaSeconds);
        }
    }
}
