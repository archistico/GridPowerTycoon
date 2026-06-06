using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Heat;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Simulation;
using GridPowerTycoon.Core.Research;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Tests.Heat;

public sealed class HeatSystemTests
{
    [Fact]
    public void HeatProducer_WithoutGenerator_ShouldAccumulateHeat()
    {
        var world = CreateWorld();
        var build = new BuildSystem(world);
        var result = build.Build("solar_panel", new GridPosition(1, 1));
        Assert.True(result.Success);

        var system = new HeatSystem(world);

        system.Update(3);

        var solar = world.BuildingInstances[result.BuildingId!.Value];
        Assert.Equal(30, solar.AccumulatedHeat);
        Assert.Equal(0, world.Resources.Energy);
    }

    [Fact]
    public void GeneratorInRange_ShouldConvertHeatToEnergy()
    {
        var world = CreateWorld();
        var build = new BuildSystem(world);
        var solarResult = build.Build("solar_panel", new GridPosition(1, 1));
        Assert.True(solarResult.Success);
        Assert.True(build.Build("generator_small", new GridPosition(2, 1)).Success);

        var system = new HeatSystem(world);

        system.Update(1);

        var solar = world.BuildingInstances[solarResult.BuildingId!.Value];
        Assert.Equal(0, solar.AccumulatedHeat);
        Assert.Equal(10, world.Resources.Energy);
    }

    [Fact]
    public void GeneratorDiagonalWithinChebyshevRange_ShouldConvertHeatToEnergy()
    {
        var world = CreateWorld();
        var build = new BuildSystem(world);
        var solarResult = build.Build("solar_panel", new GridPosition(1, 1));
        Assert.True(solarResult.Success);
        Assert.True(build.Build("generator_small", new GridPosition(2, 2)).Success);

        var system = new HeatSystem(world);

        system.Update(1);

        var solar = world.BuildingInstances[solarResult.BuildingId!.Value];
        Assert.Equal(0, solar.AccumulatedHeat);
        Assert.Equal(10, world.Resources.Energy);
    }

    [Fact]
    public void GeneratorOutOfRange_ShouldNotConvertHeat()
    {
        var world = CreateWorld();
        var build = new BuildSystem(world);
        var solarResult = build.Build("solar_panel", new GridPosition(1, 1));
        Assert.True(solarResult.Success);
        Assert.True(build.Build("generator_small", new GridPosition(4, 4)).Success);

        var system = new HeatSystem(world);

        system.Update(1);

        var solar = world.BuildingInstances[solarResult.BuildingId!.Value];
        Assert.Equal(10, solar.AccumulatedHeat);
        Assert.Equal(0, world.Resources.Energy);
    }

    [Fact]
    public void HeatProducer_OverThreshold_ShouldExplode()
    {
        var world = CreateWorld(explosionThreshold: 25);
        var build = new BuildSystem(world);
        var result = build.Build("solar_panel", new GridPosition(1, 1));
        Assert.True(result.Success);

        var system = new HeatSystem(world);

        system.Update(3);

        var solar = world.BuildingInstances[result.BuildingId!.Value];
        Assert.Equal(BuildingState.Exploded, solar.State);
    }

    [Fact]
    public void Simulation_ShouldConvertHeatBeforeAutoSell()
    {
        var world = CreateWorld();
        var build = new BuildSystem(world);
        Assert.True(build.Build("solar_panel", new GridPosition(1, 1)).Success);
        Assert.True(build.Build("generator_small", new GridPosition(2, 1)).Success);
        Assert.True(build.Build("office_small", new GridPosition(3, 1)).Success);
        var sell = new SellSystem(world);
        var simulation = new GameSimulation(world, sell);

        simulation.Update(1);

        Assert.Equal(0, world.Resources.Energy);
        Assert.Equal(760m, world.Resources.Money);
    }

    private static GameWorld CreateWorld(double explosionThreshold = 100)
    {
        var map = new GridMap(6, 6, TileType.Land);
        var catalog = BuildingCatalog.FromDefinitions(new[]
        {
            new BuildingDefinition
            {
                Id = "solar_panel",
                Name = "Pannello solare",
                Category = BuildingCategory.HeatProducer,
                Cost = 200,
                HeatPerSecond = 10,
                LifetimeSeconds = 100
            },
            new BuildingDefinition
            {
                Id = "generator_small",
                Name = "Generatore piccolo",
                Category = BuildingCategory.HeatConverter,
                Cost = 1000,
                HeatConversionPerSecond = 20,
                HeatRange = 1
            },
            new BuildingDefinition
            {
                Id = "office_small",
                Name = "Ufficio piccolo",
                Category = BuildingCategory.Automation,
                Cost = 50,
                AutoSellPerSecond = 10
            }
        });

        var economy = new EconomySettings
        {
            StartingMoney = 2000,
            StartingMaxEnergy = 100,
            EnergySellValue = 1,
            ManualSellMultiplier = 1,
            AutoSellMultiplier = 1
        };

        var heat = new HeatSettings
        {
            HeatWarningThreshold = 15,
            HeatExplosionThreshold = explosionThreshold,
            HeatEnergyConversionRate = 1
        };

        return new GameWorld(map, new GameData(catalog, economy, ResearchCatalog.Empty, heat));
    }
}
