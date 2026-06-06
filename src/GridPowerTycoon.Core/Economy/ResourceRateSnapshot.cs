using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Upgrades;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Economy;

public sealed class ResourceRateSnapshot
{
    public double EnergyPerSecond { get; init; }
    public double ResearchPerSecond { get; init; }
    public decimal MoneyPerSecond { get; init; }

    public double RawEnergyProductionPerSecond { get; init; }
    public double RawResearchProductionPerSecond { get; init; }
    public double AutoSellEnergyPerSecond { get; init; }
    public double EnergyConsumptionPerSecond { get; init; }
    public double HeatProducedPerSecond { get; init; }
    public double HeatConvertedEnergyPerSecond { get; init; }
    public double HeatDissipatedPerSecond { get; init; }
    public double EnergyEfficiencyMultiplier { get; init; } = 1;

    public static ResourceRateSnapshot Calculate(GameWorld world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var availableEnergy = world.Resources.Energy;
        var rawEnergyProduction = 0d;
        var rawResearchProduction = 0d;
        var autoSell = 0d;
        var energyConsumption = 0d;
        var heatProduced = 0d;
        var moneyPerSecond = 0m;
        var energyEfficiencyMultiplier = GetEnergyEfficiencyMultiplier(world);

        foreach (var instance in world.BuildingInstances.Values)
        {
            if (!instance.IsActive)
                continue;

            if (!world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            var energyPerSecond = UpgradeCalculator.GetEnergyPerSecond(world, definition);
            if (energyPerSecond <= 0)
                continue;

            var consumption = UpgradeCalculator.GetEnergyConsumptionPerSecond(world, definition);
            if (consumption > 0)
            {
                if (availableEnergy < consumption)
                    continue;

                availableEnergy -= consumption;
                energyConsumption += consumption;
            }

            var effectiveEnergyPerSecond = energyPerSecond * energyEfficiencyMultiplier;
            rawEnergyProduction += effectiveEnergyPerSecond;
            availableEnergy = Math.Min(world.Resources.MaxEnergy, availableEnergy + effectiveEnergyPerSecond);
        }

        var heatConvertedEnergy = EstimateHeatConvertedEnergyPerSecond(world) * energyEfficiencyMultiplier;
        var heatDissipated = EstimateHeatDissipatedPerSecond(world);
        if (heatConvertedEnergy > 0)
            availableEnergy = Math.Min(world.Resources.MaxEnergy, availableEnergy + heatConvertedEnergy);

        foreach (var instance in world.BuildingInstances.Values)
        {
            if (!instance.IsActive)
                continue;

            if (!world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            var heatPerSecond = UpgradeCalculator.GetHeatPerSecond(world, definition);
            if (heatPerSecond <= 0)
                continue;

            var consumption = UpgradeCalculator.GetEnergyConsumptionPerSecond(world, definition);
            if (consumption > 0)
            {
                if (availableEnergy < consumption)
                    continue;

                availableEnergy -= consumption;
                energyConsumption += consumption;
            }

            heatProduced += heatPerSecond;
        }

        foreach (var instance in world.BuildingInstances.Values)
        {
            if (!instance.IsActive)
                continue;

            if (!world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            var researchPerSecond = UpgradeCalculator.GetResearchPerSecond(world, definition);
            if (researchPerSecond <= 0)
                continue;

            var consumption = UpgradeCalculator.GetEnergyConsumptionPerSecond(world, definition);
            if (consumption > 0)
            {
                if (availableEnergy < consumption)
                    continue;

                availableEnergy -= consumption;
                energyConsumption += consumption;
            }

            rawResearchProduction += researchPerSecond;
        }

        foreach (var instance in world.BuildingInstances.Values)
        {
            if (!instance.IsActive)
                continue;

            if (!world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            var autoSellPerSecond = UpgradeCalculator.GetAutoSellPerSecond(world, definition);
            if (autoSellPerSecond <= 0)
                continue;

            var consumption = UpgradeCalculator.GetEnergyConsumptionPerSecond(world, definition);
            if (consumption > 0)
            {
                if (availableEnergy < consumption)
                    continue;

                availableEnergy -= consumption;
                energyConsumption += consumption;
            }

            var effectiveAutoSell = Math.Min(availableEnergy, autoSellPerSecond);
            if (effectiveAutoSell <= 0)
                continue;

            availableEnergy -= effectiveAutoSell;
            autoSell += effectiveAutoSell;

            var autoSellMultiplier = Math.Max(0, world.EconomySettings.AutoSellMultiplier);
            moneyPerSecond += (decimal)effectiveAutoSell *
                              world.EconomySettings.EnergySellValue *
                              (decimal)autoSellMultiplier;
        }

        var netEnergyPerSecond = availableEnergy - world.Resources.Energy;

        return new ResourceRateSnapshot
        {
            EnergyPerSecond = netEnergyPerSecond,
            ResearchPerSecond = rawResearchProduction,
            MoneyPerSecond = moneyPerSecond,
            RawEnergyProductionPerSecond = rawEnergyProduction,
            RawResearchProductionPerSecond = rawResearchProduction,
            AutoSellEnergyPerSecond = autoSell,
            EnergyConsumptionPerSecond = energyConsumption,
            HeatProducedPerSecond = heatProduced,
            HeatConvertedEnergyPerSecond = heatConvertedEnergy,
            HeatDissipatedPerSecond = heatDissipated,
            EnergyEfficiencyMultiplier = energyEfficiencyMultiplier
        };
    }

    private static double EstimateHeatDissipatedPerSecond(GameWorld world)
    {
        var availableHeatByProducer = new Dictionary<Guid, double>();

        foreach (var instance in world.BuildingInstances.Values)
        {
            if (!instance.IsActive)
                continue;

            if (!world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            if (UpgradeCalculator.GetHeatPerSecond(world, definition) <= 0 && instance.AccumulatedHeat <= 0)
                continue;

            availableHeatByProducer[instance.Id] = UpgradeCalculator.GetHeatPerSecond(world, definition) + instance.AccumulatedHeat;
        }

        var dissipatedHeat = 0d;

        foreach (var sink in world.BuildingInstances.Values)
        {
            if (!sink.IsActive)
                continue;

            if (!world.BuildingCatalog.TryGet(sink.DefinitionId, out var sinkDefinition))
                continue;

            if (UpgradeCalculator.GetHeatDissipationPerSecond(world, sinkDefinition) <= 0 || sinkDefinition.HeatRange <= 0)
                continue;

            var remainingCapacity = UpgradeCalculator.GetHeatDissipationPerSecond(world, sinkDefinition);

            foreach (var producer in world.BuildingInstances.Values
                         .Where(x => x.IsActive)
                         .Where(x => x.Id != sink.Id)
                         .Where(x => availableHeatByProducer.ContainsKey(x.Id))
                         .Where(x => IsWithinRange(sink, x, sinkDefinition.HeatRange))
                         .OrderBy(x => GetChebyshevDistance(sink, x))
                         .ThenBy(x => x.Id))
            {
                if (remainingCapacity <= 0)
                    break;

                var availableHeat = availableHeatByProducer[producer.Id];
                var heatToDissipate = Math.Min(availableHeat, remainingCapacity);
                if (heatToDissipate <= 0)
                    continue;

                availableHeatByProducer[producer.Id] = availableHeat - heatToDissipate;
                remainingCapacity -= heatToDissipate;
                dissipatedHeat += heatToDissipate;
            }
        }

        return dissipatedHeat;
    }

    private static double GetEnergyEfficiencyMultiplier(GameWorld world)
    {
        var bonus = 0d;

        foreach (var instance in world.BuildingInstances.Values)
        {
            if (!instance.IsActive)
                continue;

            if (!world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            bonus += definition.EnergyEfficiencyBonus;
        }

        return 1d + Math.Max(0d, bonus);
    }

    private static double EstimateHeatConvertedEnergyPerSecond(GameWorld world)
    {
        var availableHeatByProducer = new Dictionary<Guid, double>();

        foreach (var instance in world.BuildingInstances.Values)
        {
            if (!instance.IsActive)
                continue;

            if (!world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            if (UpgradeCalculator.GetHeatPerSecond(world, definition) <= 0 && instance.AccumulatedHeat <= 0)
                continue;

            availableHeatByProducer[instance.Id] = UpgradeCalculator.GetHeatPerSecond(world, definition) + instance.AccumulatedHeat;
        }

        var convertedHeat = 0d;

        foreach (var converter in world.BuildingInstances.Values)
        {
            if (!converter.IsActive)
                continue;

            if (!world.BuildingCatalog.TryGet(converter.DefinitionId, out var converterDefinition))
                continue;

            if (UpgradeCalculator.GetHeatConversionPerSecond(world, converterDefinition) <= 0 || converterDefinition.HeatRange <= 0)
                continue;

            var remainingCapacity = UpgradeCalculator.GetHeatConversionPerSecond(world, converterDefinition);

            foreach (var producer in world.BuildingInstances.Values
                         .Where(x => x.IsActive)
                         .Where(x => x.Id != converter.Id)
                         .Where(x => availableHeatByProducer.ContainsKey(x.Id))
                         .Where(x => IsWithinRange(converter, x, converterDefinition.HeatRange))
                         .OrderBy(x => GetChebyshevDistance(converter, x))
                         .ThenBy(x => x.Id))
            {
                if (remainingCapacity <= 0)
                    break;

                var availableHeat = availableHeatByProducer[producer.Id];
                var heatToConvert = Math.Min(availableHeat, remainingCapacity);
                if (heatToConvert <= 0)
                    continue;

                availableHeatByProducer[producer.Id] = availableHeat - heatToConvert;
                remainingCapacity -= heatToConvert;
                convertedHeat += heatToConvert;
            }
        }

        return convertedHeat * world.HeatSettings.HeatEnergyConversionRate;
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
