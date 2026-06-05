using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Heat;
using GridPowerTycoon.Core.Lifetime;
using GridPowerTycoon.Core.Tools;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Simulation;

public sealed class GameSimulation
{
    private readonly LifetimeSystem _lifetimeSystem;
    private readonly ProductionSystem _productionSystem;
    private readonly HeatSystem _heatSystem;
    private readonly AutoSellSystem _autoSellSystem;
    private readonly ToolGenerationSystem _toolGenerationSystem;

    public GameSimulation(GameWorld world, SellSystem sellSystem)
    {
        _lifetimeSystem = new LifetimeSystem(world);
        _productionSystem = new ProductionSystem(world);
        _heatSystem = new HeatSystem(world);
        _autoSellSystem = new AutoSellSystem(world, sellSystem);
        _toolGenerationSystem = new ToolGenerationSystem(world);
    }

    public void Update(double deltaSeconds)
    {
        if (deltaSeconds <= 0)
            return;

        _lifetimeSystem.Update(deltaSeconds);
        _productionSystem.Update(deltaSeconds);
        _heatSystem.Update(deltaSeconds);
        _autoSellSystem.Update(deltaSeconds);
        _toolGenerationSystem.Update(deltaSeconds);
    }
}
