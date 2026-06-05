using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Upgrades;

public static class UpgradeCalculator
{
    public static double GetEnergyPerSecond(GameWorld world, BuildingDefinition definition)
    {
        return definition.EnergyPerSecond * GetBuildingMultiplier(world, definition.Id, UpgradeEffectType.MultiplyEnergyProduction);
    }

    public static double GetResearchPerSecond(GameWorld world, BuildingDefinition definition)
    {
        return definition.ResearchPerSecond * GetBuildingMultiplier(world, definition.Id, UpgradeEffectType.MultiplyResearchProduction);
    }

    public static double GetHeatPerSecond(GameWorld world, BuildingDefinition definition)
    {
        return definition.HeatPerSecond * GetBuildingMultiplier(world, definition.Id, UpgradeEffectType.MultiplyHeatProduction);
    }

    public static double GetEnergyConsumptionPerSecond(GameWorld world, BuildingDefinition definition)
    {
        return definition.EnergyConsumptionPerSecond;
    }

    public static double GetBatteryCapacity(GameWorld world, BuildingDefinition definition)
    {
        return definition.BatteryCapacity * GetBuildingMultiplier(world, definition.Id, UpgradeEffectType.MultiplyBatteryCapacity);
    }

    public static double GetAutoSellPerSecond(GameWorld world, BuildingDefinition definition)
    {
        return definition.AutoSellPerSecond * GetBuildingMultiplier(world, definition.Id, UpgradeEffectType.MultiplyAutoSell);
    }

    public static double GetHeatConversionPerSecond(GameWorld world, BuildingDefinition definition)
    {
        return definition.HeatConversionPerSecond * GetBuildingMultiplier(world, definition.Id, UpgradeEffectType.MultiplyHeatConversion);
    }

    public static double GetLifetimeSeconds(GameWorld world, BuildingDefinition definition)
    {
        return definition.LifetimeSeconds * GetBuildingMultiplier(world, definition.Id, UpgradeEffectType.MultiplyLifetime);
    }

    public static double GetAxesPerSecond(GameWorld world)
    {
        return world.ToolSettings.AxesPerSecond * GetGlobalMultiplier(world, UpgradeEffectType.MultiplyToolAxesGeneration);
    }

    public static double GetMinesPerSecond(GameWorld world)
    {
        return world.ToolSettings.MinesPerSecond * GetGlobalMultiplier(world, UpgradeEffectType.MultiplyToolMinesGeneration);
    }

    private static double GetBuildingMultiplier(GameWorld world, string buildingId, UpgradeEffectType effectType)
    {
        var multiplier = 1d;

        foreach (var upgrade in world.UpgradeCatalog.All)
        {
            if (upgrade.EffectType != effectType)
                continue;

            if (!string.Equals(upgrade.TargetBuildingId, buildingId, StringComparison.OrdinalIgnoreCase))
                continue;

            var level = world.Upgrades.GetLevel(upgrade.Id);
            if (level <= 0)
                continue;

            multiplier *= Math.Pow(upgrade.Multiplier, level);
        }

        return multiplier;
    }

    private static double GetGlobalMultiplier(GameWorld world, UpgradeEffectType effectType)
    {
        var multiplier = 1d;

        foreach (var upgrade in world.UpgradeCatalog.All)
        {
            if (upgrade.EffectType != effectType)
                continue;

            var level = world.Upgrades.GetLevel(upgrade.Id);
            if (level <= 0)
                continue;

            multiplier *= Math.Pow(upgrade.Multiplier, level);
        }

        return multiplier;
    }
}
