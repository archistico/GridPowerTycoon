using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Upgrades;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Operations;

public static class BuildingOperationalStatusCalculator
{
    public static BuildingOperationalStatus Calculate(GameWorld world, BuildingInstance instance)
    {
        if (!world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
            throw new InvalidOperationException($"Unknown building definition '{instance.DefinitionId}'.");

        var energyInput = UpgradeCalculator.GetEnergyConsumptionPerSecond(world, definition);
        var energyOutput = UpgradeCalculator.GetEnergyPerSecond(world, definition);
        var heatOutput = UpgradeCalculator.GetHeatPerSecond(world, definition);
        var heatConversionInput = UpgradeCalculator.GetHeatConversionPerSecond(world, definition);
        var heatConversionEnergyOutput = heatConversionInput * world.HeatSettings.HeatEnergyConversionRate;
        var researchOutput = UpgradeCalculator.GetResearchPerSecond(world, definition);
        var autoSellInput = UpgradeCalculator.GetAutoSellPerSecond(world, definition);
        var batteryCapacity = UpgradeCalculator.GetBatteryCapacity(world, definition);
        var hasHeatConverter = heatOutput <= 0 || HasActiveHeatConverterInRange(world, instance);

        var state = GetState(world, instance, definition, energyInput, hasHeatConverter);

        return new BuildingOperationalStatus(
            state,
            GetLabel(state),
            energyInput,
            IsOutputEnabled(state) ? energyOutput : 0,
            IsOutputEnabled(state) ? heatOutput : 0,
            instance.AccumulatedHeat,
            world.HeatSettings.HeatWarningThreshold,
            world.HeatSettings.HeatExplosionThreshold,
            IsOutputEnabled(state) ? heatConversionInput : 0,
            IsOutputEnabled(state) ? heatConversionEnergyOutput : 0,
            IsOutputEnabled(state) ? researchOutput : 0,
            IsOutputEnabled(state) ? autoSellInput : 0,
            batteryCapacity,
            hasHeatConverter);
    }

    private static BuildingOperationalState GetState(
        GameWorld world,
        BuildingInstance instance,
        BuildingDefinition definition,
        double energyInput,
        bool hasHeatConverter)
    {
        if (instance.State == BuildingState.Expired)
            return BuildingOperationalState.Expired;

        if (instance.State == BuildingState.Exploded)
            return BuildingOperationalState.Exploded;

        if (energyInput > 0 && world.Resources.Energy <= 0.0001)
            return BuildingOperationalState.NoEnergy;

        if (UpgradeCalculator.GetHeatPerSecond(world, definition) > 0 && !hasHeatConverter)
            return BuildingOperationalState.NoHeatConversion;

        if (instance.AccumulatedHeat >= world.HeatSettings.HeatWarningThreshold && world.HeatSettings.HeatWarningThreshold > 0)
            return BuildingOperationalState.HeatWarning;

        return BuildingOperationalState.Active;
    }

    private static bool IsOutputEnabled(BuildingOperationalState state)
    {
        return state is BuildingOperationalState.Active or BuildingOperationalState.HeatWarning or BuildingOperationalState.NoHeatConversion;
    }

    private static string GetLabel(BuildingOperationalState state)
    {
        return state switch
        {
            BuildingOperationalState.Active => "ACTIVE",
            BuildingOperationalState.NoEnergy => "NO ENERGY",
            BuildingOperationalState.Expired => "EXPIRED",
            BuildingOperationalState.Exploded => "EXPLODED",
            BuildingOperationalState.HeatWarning => "HEAT WARNING",
            BuildingOperationalState.NoHeatConversion => "NO HEAT CONVERSION",
            _ => state.ToString().ToUpperInvariant()
        };
    }

    private static bool HasActiveHeatConverterInRange(GameWorld world, BuildingInstance producer)
    {
        foreach (var instance in world.BuildingInstances.Values)
        {
            if (!instance.IsActive || instance.Id == producer.Id)
                continue;

            if (!world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            if (definition.HeatRange <= 0)
                continue;

            if (UpgradeCalculator.GetHeatConversionPerSecond(world, definition) <= 0)
                continue;

            if (GetChebyshevDistance(instance, producer) <= definition.HeatRange)
                return true;
        }

        return false;
    }

    private static int GetChebyshevDistance(BuildingInstance a, BuildingInstance b)
    {
        var dx = Math.Abs(a.Position.X - b.Position.X);
        var dy = Math.Abs(a.Position.Y - b.Position.Y);
        return Math.Max(dx, dy);
    }
}
