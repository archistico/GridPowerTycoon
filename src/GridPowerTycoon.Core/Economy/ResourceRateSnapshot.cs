using GridPowerTycoon.Core.Buildings;
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
    public double HeatProducedPerSecond { get; init; }
    public double HeatConvertedEnergyPerSecond { get; init; }

    public static ResourceRateSnapshot Calculate(GameWorld world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var rawEnergyProduction = 0d;
        var rawResearchProduction = 0d;
        var autoSell = 0d;
        var heatProduced = 0d;

        foreach (var instance in world.BuildingInstances.Values)
        {
            if (!instance.IsActive)
                continue;

            if (!world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            rawEnergyProduction += definition.EnergyPerSecond;
            rawResearchProduction += definition.ResearchPerSecond;
            autoSell += definition.AutoSellPerSecond;
            heatProduced += definition.HeatPerSecond;
        }

        var heatConvertedEnergy = EstimateHeatConvertedEnergyPerSecond(world);
        var grossEnergyProduction = rawEnergyProduction + heatConvertedEnergy;

        var energyBeforeAutoSell = Math.Min(
            world.Resources.MaxEnergy,
            world.Resources.Energy + grossEnergyProduction);

        var effectiveAutoSell = Math.Min(energyBeforeAutoSell, autoSell);
        var energyAfterAutoSell = energyBeforeAutoSell - effectiveAutoSell;
        var netEnergyPerSecond = energyAfterAutoSell - world.Resources.Energy;

        var autoSellMultiplier = Math.Max(0, world.EconomySettings.AutoSellMultiplier);
        var moneyPerSecond = (decimal)effectiveAutoSell *
                             world.EconomySettings.EnergySellValue *
                             (decimal)autoSellMultiplier;

        return new ResourceRateSnapshot
        {
            EnergyPerSecond = netEnergyPerSecond,
            ResearchPerSecond = rawResearchProduction,
            MoneyPerSecond = moneyPerSecond,
            RawEnergyProductionPerSecond = rawEnergyProduction,
            RawResearchProductionPerSecond = rawResearchProduction,
            AutoSellEnergyPerSecond = autoSell,
            HeatProducedPerSecond = heatProduced,
            HeatConvertedEnergyPerSecond = heatConvertedEnergy
        };
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

            if (definition.HeatPerSecond <= 0 && instance.AccumulatedHeat <= 0)
                continue;

            availableHeatByProducer[instance.Id] = definition.HeatPerSecond + instance.AccumulatedHeat;
        }

        var convertedHeat = 0d;

        foreach (var converter in world.BuildingInstances.Values)
        {
            if (!converter.IsActive)
                continue;

            if (!world.BuildingCatalog.TryGet(converter.DefinitionId, out var converterDefinition))
                continue;

            if (converterDefinition.HeatConversionPerSecond <= 0 || converterDefinition.HeatRange <= 0)
                continue;

            var remainingCapacity = converterDefinition.HeatConversionPerSecond;

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
