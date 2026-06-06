using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Tests.Economy;

public sealed class ResourceRateSnapshotTests
{
    [Fact]
    public void Calculate_WithActiveProducer_ShouldReturnEnergyRate()
    {
        var world = CreateWorld(startingMoney: 100);
        var build = new BuildSystem(world);
        Assert.True(build.Build("wind_turbine", new GridPosition(1, 1)).Success);

        var rates = ResourceRateSnapshot.Calculate(world);

        Assert.Equal(1, rates.EnergyPerSecond);
        Assert.Equal(0m, rates.MoneyPerSecond);
    }

    [Fact]
    public void Calculate_WithResearchBuilding_ShouldReturnResearchRate()
    {
        var world = CreateWorld(startingMoney: 1000);
        var build = new BuildSystem(world);
        Assert.True(build.Build("research_small", new GridPosition(1, 1)).Success);

        var rates = ResourceRateSnapshot.Calculate(world);

        Assert.Equal(1.25, rates.ResearchPerSecond);
    }

    [Fact]
    public void Calculate_WithOfficeAndStoredEnergy_ShouldReturnMoneyRateAndNegativeEnergyRate()
    {
        var world = CreateWorld(startingMoney: 100, energySellValue: 2);
        var build = new BuildSystem(world);
        Assert.True(build.Build("office_small", new GridPosition(1, 1)).Success);
        world.Resources.AddEnergy(50);

        var rates = ResourceRateSnapshot.Calculate(world);

        Assert.Equal(-10, rates.EnergyPerSecond);
        Assert.Equal(20m, rates.MoneyPerSecond);
    }

    [Fact]
    public void Calculate_WhenEnergyIsFull_ShouldReturnZeroEnergyGrowth()
    {
        var world = CreateWorld(startingMoney: 100);
        var build = new BuildSystem(world);
        Assert.True(build.Build("wind_turbine", new GridPosition(1, 1)).Success);
        world.Resources.AddEnergy(100);

        var rates = ResourceRateSnapshot.Calculate(world);

        Assert.Equal(0, rates.EnergyPerSecond);
    }


    [Fact]
    public void Calculate_WithConsumingResearchBuildingAndStoredEnergy_ShouldIncludeEnergyConsumption()
    {
        var world = CreateWorld(startingMoney: 1000);
        var build = new BuildSystem(world);
        Assert.True(build.Build("consuming_research", new GridPosition(1, 1)).Success);
        world.Resources.AddEnergy(10);

        var rates = ResourceRateSnapshot.Calculate(world);

        Assert.Equal(-0.5, rates.EnergyPerSecond);
        Assert.Equal(1.25, rates.ResearchPerSecond);
        Assert.Equal(0.5, rates.EnergyConsumptionPerSecond);
    }


    [Fact]
    public void Calculate_WithActiveSubstation_ShouldApplyEnergyEfficiencyBonus()
    {
        var world = CreateWorld(startingMoney: 100);
        var build = new BuildSystem(world);
        Assert.True(build.Build("wind_turbine", new GridPosition(1, 1)).Success);
        Assert.True(build.Build("substation_small", new GridPosition(2, 1)).Success);

        var rates = ResourceRateSnapshot.Calculate(world);

        Assert.Equal(1.1, rates.EnergyPerSecond, 5);
        Assert.Equal(1.1, rates.RawEnergyProductionPerSecond, 5);
        Assert.Equal(1.1, rates.EnergyEfficiencyMultiplier, 5);
    }

    private static GameWorld CreateWorld(decimal startingMoney, decimal energySellValue = 1)
    {
        var map = new GridMap(4, 4, TileType.Land);
        var catalog = BuildingCatalog.FromDefinitions(new[]
        {
            new BuildingDefinition
            {
                Id = "wind_turbine",
                Name = "Pala eolica",
                Category = BuildingCategory.PowerProducer,
                Cost = 1,
                EnergyPerSecond = 1,
                LifetimeSeconds = 60
            },
            new BuildingDefinition
            {
                Id = "office_small",
                Name = "Ufficio piccolo",
                Category = BuildingCategory.Automation,
                Cost = 50,
                AutoSellPerSecond = 10
            },
            new BuildingDefinition
            {
                Id = "research_small",
                Name = "Centro ricerca piccolo",
                Category = BuildingCategory.Research,
                Cost = 1000,
                ResearchPerSecond = 1.25
            },
            new BuildingDefinition
            {
                Id = "consuming_research",
                Name = "Centro ricerca alimentato",
                Category = BuildingCategory.Research,
                Cost = 1,
                ResearchPerSecond = 1.25,
                EnergyConsumptionPerSecond = 0.5
            },
            new BuildingDefinition
            {
                Id = "substation_small",
                Name = "Trasformatore",
                Category = BuildingCategory.Special,
                Cost = 1,
                EnergyEfficiencyBonus = 0.1
            }
        });

        var economy = new EconomySettings
        {
            StartingMoney = startingMoney,
            StartingMaxEnergy = 100,
            EnergySellValue = energySellValue,
            ManualSellMultiplier = 1,
            AutoSellMultiplier = 1
        };

        return new GameWorld(map, new GameData(catalog, economy));
    }
}
