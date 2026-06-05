using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Heat;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Simulation;
using GridPowerTycoon.Core.Tools;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Tests.Simulation;

public sealed class OfflineProgressSystemTests
{
    [Fact]
    public void Apply_ShouldProduceEnergyResearchToolsAndAutoSell()
    {
        var world = CreateWorld(startingMoney: 2000, maxOfflineSeconds: 120);
        var build = new BuildSystem(world);
        Assert.True(build.Build("wind_turbine", new GridPosition(0, 0)).Success);
        Assert.True(build.Build("research_small", new GridPosition(1, 0)).Success);
        Assert.True(build.Build("office_small", new GridPosition(2, 0)).Success);
        world.Resources.AddEnergy(50);

        var system = new OfflineProgressSystem(world, new SellSystem(world));
        var result = system.Apply(
            new DateTimeOffset(2026, 6, 5, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 6, 5, 10, 0, 10, TimeSpan.Zero));

        Assert.Equal(10, result.AppliedSeconds);
        Assert.True(world.Resources.Money > 0);
        Assert.True(world.Resources.Research > 0);
        Assert.True(world.Resources.Axes > 0);
        Assert.True(world.Resources.Mines > 0);
    }

    [Fact]
    public void Apply_ShouldRespectMaxOfflineSeconds()
    {
        var world = CreateWorld(startingMoney: 100, maxOfflineSeconds: 5);
        var build = new BuildSystem(world);
        Assert.True(build.Build("long_wind_turbine", new GridPosition(0, 0)).Success);

        var system = new OfflineProgressSystem(world, new SellSystem(world));
        var result = system.Apply(
            new DateTimeOffset(2026, 6, 5, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 6, 5, 11, 0, 0, TimeSpan.Zero));

        Assert.Equal(3600, result.RealSecondsAway);
        Assert.Equal(5, result.AppliedSeconds);
        Assert.Equal(5, world.Resources.Energy);
    }

    [Fact]
    public void Apply_ShouldExpireBuildingsButNotExplodeForOfflineHeat()
    {
        var world = CreateWorld(startingMoney: 100, maxOfflineSeconds: 20);
        var build = new BuildSystem(world);
        var heatProducer = build.Build("short_heat_producer", new GridPosition(0, 0));
        Assert.True(heatProducer.Success);

        var system = new OfflineProgressSystem(world, new SellSystem(world));
        var result = system.Apply(
            new DateTimeOffset(2026, 6, 5, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 6, 5, 10, 0, 10, TimeSpan.Zero));

        var instance = world.BuildingInstances[heatProducer.BuildingId!.Value];
        Assert.Equal(BuildingState.Expired, instance.State);
        Assert.Equal(1, result.BuildingsExpired);
        Assert.Equal(0, result.BuildingsExploded);
    }

    private static GameWorld CreateWorld(decimal startingMoney, double maxOfflineSeconds)
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
                Id = "research_small",
                Name = "Centro ricerca piccolo",
                Category = BuildingCategory.Research,
                Cost = 1,
                ResearchPerSecond = 1.25,
                EnergyConsumptionPerSecond = 0.5
            },
            new BuildingDefinition
            {
                Id = "office_small",
                Name = "Ufficio piccolo",
                Category = BuildingCategory.Automation,
                Cost = 1,
                AutoSellPerSecond = 5,
                EnergyConsumptionPerSecond = 0.2
            },
            new BuildingDefinition
            {
                Id = "short_heat_producer",
                Name = "Produttore calore breve",
                Category = BuildingCategory.HeatProducer,
                Cost = 1,
                HeatPerSecond = 200,
                LifetimeSeconds = 5
            }
        });

        var economy = new EconomySettings
        {
            StartingMoney = startingMoney,
            StartingMaxEnergy = 100,
            EnergySellValue = 1,
            ManualSellMultiplier = 1,
            AutoSellMultiplier = 1,
            MaxOfflineSeconds = maxOfflineSeconds
        };

        var tools = new ToolSettings
        {
            AxesPerSecond = 1,
            MinesPerSecond = 1,
            MaxAxes = 100,
            MaxMines = 100
        };

        var heat = new HeatSettings
        {
            HeatExplosionThreshold = 100,
            HeatEnergyConversionRate = 1
        };

        return new GameWorld(map, new GameData(catalog, economy, GridPowerTycoon.Core.Research.ResearchCatalog.Empty, heat, tools));
    }
}
