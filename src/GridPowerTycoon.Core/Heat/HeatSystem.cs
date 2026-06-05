using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Upgrades;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Heat;

public sealed class HeatSystem
{
    private readonly GameWorld _world;

    public HeatSystem(GameWorld world)
    {
        _world = world;
    }

    public void Update(double deltaSeconds)
    {
        Update(deltaSeconds, allowExplosions: true);
    }

    public void Update(double deltaSeconds, bool allowExplosions)
    {
        if (deltaSeconds <= 0)
            return;

        ProduceHeat(deltaSeconds);
        ConvertHeat(deltaSeconds);

        if (allowExplosions)
            ExplodeOverheatedBuildings();
    }

    private void ProduceHeat(double deltaSeconds)
    {
        foreach (var instance in _world.BuildingInstances.Values)
        {
            if (!instance.IsActive)
                continue;

            if (!_world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            if (definition.HeatPerSecond <= 0)
                continue;

            var energyConsumption = UpgradeCalculator.GetEnergyConsumptionPerSecond(_world, definition) * deltaSeconds;
            if (!_world.Resources.TrySpendEnergy(energyConsumption))
                continue;

            instance.AddHeat(definition.HeatPerSecond * deltaSeconds);
        }
    }

    private void ConvertHeat(double deltaSeconds)
    {
        foreach (var converter in _world.BuildingInstances.Values)
        {
            if (!converter.IsActive)
                continue;

            if (!_world.BuildingCatalog.TryGet(converter.DefinitionId, out var converterDefinition))
                continue;

            if (UpgradeCalculator.GetHeatConversionPerSecond(_world, converterDefinition) <= 0 || converterDefinition.HeatRange <= 0)
                continue;

            var remainingCapacity = UpgradeCalculator.GetHeatConversionPerSecond(_world, converterDefinition) * deltaSeconds;
            if (remainingCapacity <= 0)
                continue;

            foreach (var producer in GetHeatProducersInRange(converter, converterDefinition.HeatRange))
            {
                if (remainingCapacity <= 0)
                    break;

                var heatToConvert = Math.Min(producer.AccumulatedHeat, remainingCapacity);
                if (heatToConvert <= 0)
                    continue;

                producer.RemoveHeat(heatToConvert);
                _world.Resources.AddEnergy(heatToConvert * _world.HeatSettings.HeatEnergyConversionRate);
                remainingCapacity -= heatToConvert;
            }
        }
    }

    private IEnumerable<BuildingInstance> GetHeatProducersInRange(BuildingInstance converter, int range)
    {
        return _world.BuildingInstances.Values
            .Where(instance => instance.IsActive)
            .Where(instance => instance.Id != converter.Id)
            .Where(instance => instance.AccumulatedHeat > 0)
            .Where(instance => IsWithinRange(converter, instance, range))
            .OrderBy(instance => GetChebyshevDistance(converter, instance))
            .ThenBy(instance => instance.Id);
    }

    private void ExplodeOverheatedBuildings()
    {
        var threshold = _world.HeatSettings.HeatExplosionThreshold;
        if (threshold <= 0)
            return;

        foreach (var instance in _world.BuildingInstances.Values)
        {
            if (!instance.IsActive)
                continue;

            if (instance.AccumulatedHeat >= threshold)
                instance.MarkExploded();
        }
    }

    private static bool IsWithinRange(BuildingInstance a, BuildingInstance b, int range)
    {
        return GetChebyshevDistance(a, b) <= range;
    }

    private static int GetChebyshevDistance(BuildingInstance a, BuildingInstance b)
    {
        var dx = Math.Abs(a.Position.X - b.Position.X);
        var dy = Math.Abs(a.Position.Y - b.Position.Y);
        return Math.Max(dx, dy);
    }
}
