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

        foreach (var instance in _world.BuildingInstances.Values)
            instance.ReduceLifetime(deltaSeconds);
    }
}
