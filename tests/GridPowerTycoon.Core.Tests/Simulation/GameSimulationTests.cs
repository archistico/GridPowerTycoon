using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Simulation;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Tests.Simulation;

public sealed class GameSimulationTests
{
    [Fact]
    public void WindTurbine_After10Seconds_ShouldProduceEnergy()
    {
        var world = CreateWorld(startingMoney: 100);
        var build = new BuildSystem(world);
        Assert.True(build.Build("wind_turbine", new GridPosition(1, 1)).Success);
        var sell = new SellSystem(world);
        var simulation = new GameSimulation(world, sell);

        simulation.Update(10);

        Assert.Equal(10, world.Resources.Energy);
    }

    [Fact]
    public void ResearchCenter_After10Seconds_ShouldProduceResearch()
    {
        var world = CreateWorld(startingMoney: 1000);
        var build = new BuildSystem(world);
        Assert.True(build.Build("research_small", new GridPosition(1, 1)).Success);
        var sell = new SellSystem(world);
        var simulation = new GameSimulation(world, sell);

        simulation.Update(10);

        Assert.Equal(12.5, world.Resources.Research);
    }

    [Fact]
    public void SellAll_ShouldConvertEnergyToMoneyAndClearEnergy()
    {
        var world = CreateWorld(startingMoney: 100, energySellValue: 2);
        world.Resources.AddEnergy(25);
        var sell = new SellSystem(world);

        var earned = sell.SellAll();

        Assert.Equal(50m, earned);
        Assert.Equal(150m, world.Resources.Money);
        Assert.Equal(0, world.Resources.Energy);
    }

    [Fact]
    public void OfficeSmall_ShouldAutoSellEnergy()
    {
        var world = CreateWorld(startingMoney: 100);
        var build = new BuildSystem(world);
        Assert.True(build.Build("office_small", new GridPosition(1, 1)).Success);
        world.Resources.AddEnergy(50);
        var sell = new SellSystem(world);
        var simulation = new GameSimulation(world, sell);

        simulation.Update(2);

        Assert.Equal(30, world.Resources.Energy);
        Assert.Equal(70m, world.Resources.Money);
    }

    [Fact]
    public void ExpiredWindTurbine_ShouldStopProducing()
    {
        var world = CreateWorld(startingMoney: 100);
        var build = new BuildSystem(world);
        var result = build.Build("short_wind_turbine", new GridPosition(1, 1));
        Assert.True(result.Success);
        var sell = new SellSystem(world);
        var simulation = new GameSimulation(world, sell);

        simulation.Update(1);
        simulation.Update(10);

        Assert.Equal(0, world.Resources.Energy);
        var instance = world.BuildingInstances[result.BuildingId!.Value];
        Assert.Equal(BuildingState.Expired, instance.State);
    }

    [Fact]
    public void Production_ShouldRespectMaxEnergy()
    {
        var world = CreateWorld(startingMoney: 100);
        var build = new BuildSystem(world);
        Assert.True(build.Build("long_wind_turbine", new GridPosition(1, 1)).Success);
        var sell = new SellSystem(world);
        var simulation = new GameSimulation(world, sell);

        simulation.Update(500);

        Assert.Equal(100, world.Resources.Energy);
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
                Id = "long_wind_turbine",
                Name = "Pala eolica lunga",
                Category = BuildingCategory.PowerProducer,
                Cost = 1,
                EnergyPerSecond = 1,
                LifetimeSeconds = 1000
            },
            new BuildingDefinition
            {
                Id = "short_wind_turbine",
                Name = "Pala eolica breve",
                Category = BuildingCategory.PowerProducer,
                Cost = 1,
                EnergyPerSecond = 1,
                LifetimeSeconds = 1
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
