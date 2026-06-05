using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Heat;
using GridPowerTycoon.Core.Lifetime;
using GridPowerTycoon.Core.Tools;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Simulation;

public sealed class OfflineProgressSystem
{
    private const double SimulationStepSeconds = 1.0;

    private readonly GameWorld _world;
    private readonly LifetimeSystem _lifetimeSystem;
    private readonly ProductionSystem _productionSystem;
    private readonly HeatSystem _heatSystem;
    private readonly AutoSellSystem _autoSellSystem;
    private readonly ToolGenerationSystem _toolGenerationSystem;

    public OfflineProgressSystem(GameWorld world, SellSystem sellSystem)
    {
        _world = world;
        _lifetimeSystem = new LifetimeSystem(world);
        _productionSystem = new ProductionSystem(world);
        _heatSystem = new HeatSystem(world);
        _autoSellSystem = new AutoSellSystem(world, sellSystem);
        _toolGenerationSystem = new ToolGenerationSystem(world);
    }

    public OfflineProgressResult Apply(DateTimeOffset savedAtUtc, DateTimeOffset nowUtc)
    {
        var realSecondsAway = Math.Max(0, (nowUtc - savedAtUtc).TotalSeconds);
        var appliedSeconds = Math.Min(realSecondsAway, Math.Max(0, _world.EconomySettings.MaxOfflineSeconds));

        if (appliedSeconds <= 0)
            return OfflineProgressResult.None;

        var startEnergy = _world.Resources.Energy;
        var startResearch = _world.Resources.Research;
        var startMoney = _world.Resources.Money;
        var startAxes = _world.Resources.Axes;
        var startMines = _world.Resources.Mines;
        var initialStates = _world.BuildingInstances.Values.ToDictionary(x => x.Id, x => x.State);

        var remaining = appliedSeconds;
        while (remaining > 0)
        {
            var step = Math.Min(SimulationStepSeconds, remaining);
            UpdateStep(step);
            remaining -= step;
        }

        var expired = CountStateTransitions(initialStates, BuildingState.Active, BuildingState.Expired);
        var exploded = CountStateTransitions(initialStates, BuildingState.Active, BuildingState.Exploded);

        return new OfflineProgressResult(
            realSecondsAway,
            appliedSeconds,
            _world.Resources.Energy - startEnergy,
            _world.Resources.Research - startResearch,
            _world.Resources.Money - startMoney,
            _world.Resources.Axes - startAxes,
            _world.Resources.Mines - startMines,
            expired,
            exploded);
    }

    private void UpdateStep(double deltaSeconds)
    {
        _lifetimeSystem.Update(deltaSeconds);
        _productionSystem.Update(deltaSeconds);
        _heatSystem.Update(deltaSeconds, allowExplosions: false);
        _autoSellSystem.Update(deltaSeconds);
        _toolGenerationSystem.Update(deltaSeconds);
    }

    private int CountStateTransitions(
        IReadOnlyDictionary<Guid, BuildingState> initialStates,
        BuildingState from,
        BuildingState to)
    {
        var count = 0;

        foreach (var instance in _world.BuildingInstances.Values)
        {
            if (!initialStates.TryGetValue(instance.Id, out var initialState))
                continue;

            if (initialState == from && instance.State == to)
                count++;
        }

        return count;
    }
}
