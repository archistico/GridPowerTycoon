using GridPowerTycoon.Core.Upgrades;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Tools;

public sealed class ToolGenerationSystem
{
    private readonly GameWorld _world;

    public ToolGenerationSystem(GameWorld world)
    {
        _world = world;
    }

    public void Update(double deltaSeconds)
    {
        if (deltaSeconds <= 0)
            return;

        _world.Resources.AddAxes(UpgradeCalculator.GetAxesPerSecond(_world) * deltaSeconds, UpgradeCalculator.GetMaxAxes(_world));
        _world.Resources.AddMines(UpgradeCalculator.GetMinesPerSecond(_world) * deltaSeconds, UpgradeCalculator.GetMaxMines(_world));
    }
}
