using GridPowerTycoon.Core.Upgrades;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Lifetime;

public sealed class LifetimeSystem
{
    private readonly GameWorld _world;

    public LifetimeSystem(GameWorld world)
    {
        _world = world;
    }

    public void Update(double deltaSeconds)
    {
        if (deltaSeconds <= 0)
            return;

        var effectiveDeltaSeconds = deltaSeconds * UpgradeCalculator.GetLifetimeDecayMultiplier(_world);

        foreach (var instance in _world.BuildingInstances.Values)
            instance.ReduceLifetime(effectiveDeltaSeconds);
    }
}
