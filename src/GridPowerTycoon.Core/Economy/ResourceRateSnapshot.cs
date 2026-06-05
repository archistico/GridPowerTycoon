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

    public static ResourceRateSnapshot Calculate(GameWorld world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var rawEnergyProduction = 0d;
        var rawResearchProduction = 0d;
        var autoSell = 0d;

        foreach (var instance in world.BuildingInstances.Values)
        {
            if (!instance.IsActive)
                continue;

            if (!world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            rawEnergyProduction += definition.EnergyPerSecond;
            rawResearchProduction += definition.ResearchPerSecond;
            autoSell += definition.AutoSellPerSecond;
        }

        var energyBeforeAutoSell = Math.Min(
            world.Resources.MaxEnergy,
            world.Resources.Energy + rawEnergyProduction);

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
            AutoSellEnergyPerSecond = autoSell
        };
    }
}
