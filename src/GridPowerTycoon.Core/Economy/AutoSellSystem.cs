using GridPowerTycoon.Core.Upgrades;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Economy;

public sealed class AutoSellSystem
{
    private readonly GameWorld _world;
    private readonly SellSystem _sellSystem;

    public AutoSellSystem(GameWorld world, SellSystem sellSystem)
    {
        _world = world;
        _sellSystem = sellSystem;
    }

    public decimal Update(double deltaSeconds)
    {
        if (deltaSeconds <= 0)
            return 0m;

        var earned = 0m;

        foreach (var instance in _world.BuildingInstances.Values)
        {
            if (!instance.IsActive)
                continue;

            if (!_world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            var autoSellPerSecond = UpgradeCalculator.GetAutoSellPerSecond(_world, definition);
            if (autoSellPerSecond <= 0)
                continue;

            var energyConsumption = UpgradeCalculator.GetEnergyConsumptionPerSecond(_world, definition) * deltaSeconds;
            if (!_world.Resources.TrySpendEnergy(energyConsumption))
                continue;

            earned += _sellSystem.SellAmount(autoSellPerSecond * deltaSeconds);
        }

        return earned;
    }
}
