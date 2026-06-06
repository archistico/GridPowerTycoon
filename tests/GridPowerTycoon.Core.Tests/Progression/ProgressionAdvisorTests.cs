using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Expansion;
using GridPowerTycoon.Core.Heat;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Progression;
using GridPowerTycoon.Core.Research;
using GridPowerTycoon.Core.Tools;
using GridPowerTycoon.Core.Upgrades;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Tests.Progression;

public sealed class ProgressionAdvisorTests
{
    [Fact]
    public void GetCurrentObjectiveHint_WhenNoWindTurbine_ShouldSuggestWindTurbine()
    {
        var world = CreateWorld(startingMoney: 0);
        var advisor = new ProgressionAdvisor(world);

        var hint = advisor.GetCurrentObjectiveHint();

        Assert.Equal("OBJECTIVE: BUILD WIND TURBINE - START ENERGY PRODUCTION", hint);
    }

    [Fact]
    public void GetCurrentObjectiveDetailHint_WhenOfficeMoneyMissing_ShouldShowMoneyGap()
    {
        var world = CreateWorld(startingMoney: 20);
        var build = new BuildSystem(world);
        Assert.True(build.Build("wind_turbine", new GridPosition(0, 0)).Success);
        var advisor = new ProgressionAdvisor(world);

        var detail = advisor.GetCurrentObjectiveDetailHint();

        Assert.Equal("NEED $40 MORE MONEY", detail);
    }

    [Fact]
    public void GetCurrentObjectiveHint_WhenEarlyGameCompleteAndUpgradeAffordable_ShouldSuggestFirstUpgrade()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 100);
        BuildEarlyGame(world);
        var advisor = new ProgressionAdvisor(world);

        var hint = advisor.GetCurrentObjectiveHint();

        Assert.Equal("OBJECTIVE: BUY FIRST UPGRADE - WIND POWER", hint);
    }

    [Fact]
    public void GetCurrentBottleneckHint_WhenHeatProducerHasNoConverter_ShouldReportHeatCoverage()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 100);
        var build = new BuildSystem(world);
        Assert.True(build.Build("solar_panel", new GridPosition(0, 0)).Success);
        var advisor = new ProgressionAdvisor(world);

        var bottleneck = advisor.GetCurrentBottleneckHint();

        Assert.Equal("HEAT: PRODUCER WITHOUT GENERATOR COVERAGE", bottleneck);
    }

    [Fact]
    public void GetCurrentObjectiveDetailHint_WhenCloudResourcesMissing_ShouldShowCloudResourceGap()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 100);
        BuildEarlyGame(world);
        world.Upgrades.SetLevel("wind_power", 1);
        world.Research.Complete("manager_wind");
        world.Research.Complete("coal_power");
        world.Resources.Restore(10, 100, 5, 100m, 10, 10);

        var cloud = world.Map.GetTile(new GridPosition(4, 4));
        cloud.SetType(TileType.Cloud);
        cloud.SetCoveredType(TileType.Land);

        var advisor = new ProgressionAdvisor(world);

        var detail = advisor.GetCurrentObjectiveDetailHint();

        Assert.Equal("NEED $400 AND R35 FOR CLOUD", detail);
    }

    private static void BuildEarlyGame(GameWorld world)
    {
        var build = new BuildSystem(world);
        Assert.True(build.Build("wind_turbine", new GridPosition(0, 0)).Success);
        Assert.True(build.Build("office_small", new GridPosition(1, 0)).Success);
        Assert.True(build.Build("research_small", new GridPosition(2, 0)).Success);
        Assert.True(build.Build("battery_small", new GridPosition(3, 0)).Success);
        Assert.True(build.Build("solar_panel", new GridPosition(0, 1)).Success);
        Assert.True(build.Build("generator_small", new GridPosition(1, 1)).Success);
    }

    private static GameWorld CreateWorld(decimal startingMoney, double startingResearch = 0)
    {
        var map = new GridMap(6, 6, TileType.Land);
        var buildings = BuildingCatalog.FromDefinitions(new[]
        {
            new BuildingDefinition
            {
                Id = "wind_turbine",
                Name = "Wind Turbine",
                Category = BuildingCategory.PowerProducer,
                Cost = 10,
                EnergyPerSecond = 1,
                LifetimeSeconds = 60
            },
            new BuildingDefinition
            {
                Id = "office_small",
                Name = "Small Office",
                Category = BuildingCategory.Automation,
                Cost = 50,
                AutoSellPerSecond = 5
            },
            new BuildingDefinition
            {
                Id = "research_small",
                Name = "Research Center",
                Category = BuildingCategory.Research,
                Cost = 60,
                ResearchPerSecond = 1
            },
            new BuildingDefinition
            {
                Id = "battery_small",
                Name = "Small Battery",
                Category = BuildingCategory.Storage,
                Cost = 70,
                BatteryCapacity = 100
            },
            new BuildingDefinition
            {
                Id = "solar_panel",
                Name = "Solar Panel",
                Category = BuildingCategory.HeatProducer,
                Cost = 80,
                HeatPerSecond = 10,
                LifetimeSeconds = 120
            },
            new BuildingDefinition
            {
                Id = "generator_small",
                Name = "Small Generator",
                Category = BuildingCategory.HeatConverter,
                Cost = 90,
                HeatConversionPerSecond = 20,
                HeatRange = 1
            },
            new BuildingDefinition
            {
                Id = "coal_power_plant",
                Name = "Coal Plant",
                Category = BuildingCategory.HeatProducer,
                Cost = 500,
                HeatPerSecond = 100,
                RequiredResearchId = "coal_power"
            }
        });

        var research = ResearchCatalog.FromDefinitions(new[]
        {
            new ResearchDefinition
            {
                Id = "manager_wind",
                Name = "Wind Manager",
                Cost = 20,
                ManagedBuildingIds = new List<string> { "wind_turbine" }
            },
            new ResearchDefinition
            {
                Id = "coal_power",
                Name = "Coal Power",
                Cost = 80,
                UnlockBuildingIds = new List<string> { "coal_power_plant" }
            }
        });

        var upgrades = UpgradeCatalog.FromDefinitions(new[]
        {
            new UpgradeDefinition
            {
                Id = "wind_power",
                Name = "Wind Power",
                TargetBuildingId = "wind_turbine",
                CostMoney = 100,
                CostResearch = 0,
                EffectType = UpgradeEffectType.MultiplyEnergyProduction,
                Multiplier = 1.25,
                MaxLevel = 10
            }
        });

        var economy = new EconomySettings
        {
            StartingMoney = startingMoney,
            StartingResearch = startingResearch,
            StartingMaxEnergy = 100,
            EnergySellValue = 1,
            ManualSellMultiplier = 1,
            AutoSellMultiplier = 1
        };

        var tools = new ToolSettings
        {
            ForestClearAxesCost = 4,
            MountainClearMinesCost = 4,
            MaxAxes = 20,
            MaxMines = 20
        };

        var area = new AreaUnlockSettings
        {
            CloudUnlockMoneyCost = 500m,
            CloudUnlockResearchCost = 40
        };

        return new GameWorld(map, new GameData(buildings, economy, research, new HeatSettings(), tools, upgrades, area));
    }
}
