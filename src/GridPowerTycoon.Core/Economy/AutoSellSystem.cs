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

        var autoSellPerSecond = 0d;

        foreach (var instance in _world.BuildingInstances.Values)
        {
            if (!instance.IsActive)
                continue;

            if (!_world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            autoSellPerSecond += definition.AutoSellPerSecond;
        }

        if (autoSellPerSecond <= 0)
            return 0m;

        return _sellSystem.SellAmount(autoSellPerSecond * deltaSeconds);
    }
}
