using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Lifetime;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Simulation;

public sealed class GameSimulation
{
    private readonly LifetimeSystem _lifetimeSystem;
    private readonly ProductionSystem _productionSystem;
    private readonly AutoSellSystem _autoSellSystem;

    public GameSimulation(GameWorld world, SellSystem sellSystem)
    {
        _lifetimeSystem = new LifetimeSystem(world);
        _productionSystem = new ProductionSystem(world);
        _autoSellSystem = new AutoSellSystem(world, sellSystem);
    }

    public void Update(double deltaSeconds)
    {
        if (deltaSeconds <= 0)
            return;

        _lifetimeSystem.Update(deltaSeconds);
        _productionSystem.Update(deltaSeconds);
        _autoSellSystem.Update(deltaSeconds);
    }
}
