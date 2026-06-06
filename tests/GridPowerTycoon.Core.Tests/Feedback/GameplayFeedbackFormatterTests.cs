using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Feedback;
using GridPowerTycoon.Core.Heat;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Research;
using GridPowerTycoon.Core.Tools;
using GridPowerTycoon.Core.Upgrades;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Tests.Feedback;

public sealed class GameplayFeedbackFormatterTests
{
    [Fact]
    public void FormatBuildFailure_WhenNotEnoughMoney_ShouldIncludeBuildingCostHaveAndMissingAmount()
    {
        var world = CreateWorld(startingMoney: 100, startingResearch: 0);
        var formatter = new GameplayFeedbackFormatter(world);

        var message = formatter.FormatBuildFailure(BuildFailureReason.NotEnoughMoney, "battery_small", new GridPosition(2, 3));

        Assert.Contains("BATTERY", message);
        Assert.Contains("COSTS $600", message);
        Assert.Contains("HAVE $100", message);
        Assert.Contains("NEED $500", message);
    }

    [Fact]
    public void FormatBuildFailure_WhenResearchRequired_ShouldNameRequiredResearch()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 0);
        var formatter = new GameplayFeedbackFormatter(world);

        var message = formatter.FormatBuildFailure(BuildFailureReason.ResearchRequired, "battery_small");

        Assert.Equal("BUILD LOCKED: BATTERY REQUIRES BATTERIES", message);
    }

    [Fact]
    public void FormatResearchFailure_WhenMissingPrerequisite_ShouldNameMissingPrerequisite()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 100);
        var system = new ResearchSystem(world);
        var formatter = new GameplayFeedbackFormatter(world);

        var result = system.Complete("generator_small");
        var message = formatter.FormatResearchFailure(result);

        Assert.Equal("RESEARCH LOCKED: COMPLETE SOLAR POWER FIRST", message);
    }

    [Fact]
    public void FormatUpgradeFailure_WhenMissingResearch_ShouldNameRequiredResearch()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 1000);
        var system = new UpgradeSystem(world);
        var formatter = new GameplayFeedbackFormatter(world);

        var result = system.Purchase("battery_capacity_1");
        var message = formatter.FormatUpgradeFailure(result);

        Assert.Equal("UPGRADE LOCKED: BATTERY CAPACITY REQUIRES BATTERIES", message);
    }

    [Fact]
    public void FormatUpgradeFailure_WhenNotEnoughMoney_ShouldIncludeCurrentLevelCostAndAvailableMoney()
    {
        var world = CreateWorld(startingMoney: 50, startingResearch: 1000);
        var system = new UpgradeSystem(world);
        var formatter = new GameplayFeedbackFormatter(world);

        var result = system.Purchase("wind_energy_1");
        var message = formatter.FormatUpgradeFailure(result);

        Assert.Contains("WIND ENERGY LV 1", message);
        Assert.Contains("COSTS $100", message);
        Assert.Contains("HAVE $50", message);
    }



    [Fact]
    public void FormatBuildCardDetails_WhenLocked_ShouldExposeRequiredResearch()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 0);
        var formatter = new GameplayFeedbackFormatter(world);

        var lines = formatter.FormatBuildCardDetails("battery_small");

        Assert.Contains("BATTERY", lines[0]);
        Assert.Contains("LOCKED: COMPLETE BATTERIES", lines[1]);
        Assert.Contains("ADDS 500 ENERGY STORAGE", lines);
    }

    [Fact]
    public void FormatResearchCardDetails_WhenMissingPrerequisite_ShouldExposeMissingResearch()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 100);
        var formatter = new GameplayFeedbackFormatter(world);

        var lines = formatter.FormatResearchCardDetails("generator_small");

        Assert.Contains("SMALL GENERATOR", lines[0]);
        Assert.Contains("LOCKED: COMPLETE SOLAR POWER", lines[1]);
    }

    [Fact]
    public void FormatUpgradeCardDetails_WhenMissingResearch_ShouldExposeLevelCostEffectAndTarget()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 1000);
        var formatter = new GameplayFeedbackFormatter(world);

        var lines = formatter.FormatUpgradeCardDetails("battery_capacity_1");

        Assert.Contains("BATTERY CAPACITY", lines[0]);
        Assert.Contains("LOCKED: COMPLETE BATTERIES", lines[1]);
        Assert.Contains("LEVEL 0/1 -> 1/1", lines[2]);
        Assert.Contains("BATTERY CAPACITY +50%", lines[3]);
        Assert.Contains("TARGET: BATTERY", lines[4]);
    }


    [Fact]
    public void FormatBuildAvailabilityLine_WhenMissingMoney_ShouldExposeMissingAmount()
    {
        var world = CreateWorld(startingMoney: 100, startingResearch: 0);
        var formatter = new GameplayFeedbackFormatter(world);

        var line = formatter.FormatBuildAvailabilityLine("battery_small");

        Assert.Equal("LOCKED: COMPLETE BATTERIES", line);

        world.Research.Complete("battery");
        line = formatter.FormatBuildAvailabilityLine("battery_small");

        Assert.Equal("NEED MONEY: COST $600 - HAVE $100 - NEED $500", line);
    }

    [Fact]
    public void FormatResearchAvailabilityLine_WhenMissingPrerequisite_ShouldExposeConcreteRequirement()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 100);
        var formatter = new GameplayFeedbackFormatter(world);

        var line = formatter.FormatResearchAvailabilityLine("generator_small");

        Assert.Equal("LOCKED: COMPLETE SOLAR POWER", line);
    }

    [Fact]
    public void FormatUpgradeAvailabilityLine_WhenMissingMoneyAndResearch_ShouldExposeBothShortages()
    {
        var world = CreateWorld(startingMoney: 50, startingResearch: 10);
        var formatter = new GameplayFeedbackFormatter(world);

        var line = formatter.FormatUpgradeAvailabilityLine("mixed_cost_1");

        Assert.Equal("NEED MONEY/RESEARCH: COST $100 + 20 RP - HAVE $50 + 10 RP - NEED $50 + 10 RP", line);
    }


    [Fact]
    public void FormatCriticalWarning_WhenEnergyEmptyWithActiveConsumer_ShouldWarnAboutEnergy()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 0);
        AddBuilding(world, "research_lab", 0, 0, 60);
        world.Resources.Restore(0, 100, 0, 1000, 0, 0);
        var formatter = new GameplayFeedbackFormatter(world);

        var warning = formatter.FormatCriticalWarning();

        Assert.Equal("ENERGY CRITICAL: STORAGE EMPTY - 1 CONSUMER(S) MAY STOP", warning);
    }

    [Fact]
    public void FormatCriticalWarnings_WhenHeatProducerHasNoCoverage_ShouldWarnAboutHeatCoverage()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 0);
        AddBuilding(world, "coal_plant", 0, 0, 60);
        var formatter = new GameplayFeedbackFormatter(world);

        var warnings = formatter.FormatCriticalWarnings();

        Assert.Contains("HEAT WARNING: 1 PRODUCER(S) NEED CONVERTER OR COOLING", warnings);
    }

    [Fact]
    public void FormatCriticalWarnings_WhenBuildingIsNearEndOfLife_ShouldWarnAboutMaintenance()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 0);
        AddBuilding(world, "wind_turbine", 0, 0, 10);
        var formatter = new GameplayFeedbackFormatter(world);

        var warnings = formatter.FormatCriticalWarnings();

        Assert.Contains("MAINTENANCE WARNING: 1 BUILDING(S) NEAR END OF LIFE", warnings);
    }

    [Fact]
    public void FormatCriticalWarning_WhenBuildingExploded_ShouldPrioritizeExplosion()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 0);
        var instance = AddBuilding(world, "wind_turbine", 0, 0, 60);
        instance.MarkExploded();
        var formatter = new GameplayFeedbackFormatter(world);

        var warning = formatter.FormatCriticalWarning();

        Assert.Equal("CRITICAL: 1 BUILDING(S) EXPLODED - REPLACE OR DEMOLISH", warning);
    }

    [Fact]
    public void FormatProductionSummaryLines_ShouldExposeEnergyResearchMoneyHeatMaintenanceAndTools()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 0);
        AddBuilding(world, "wind_turbine", 0, 0, 60);
        AddBuilding(world, "research_lab", 1, 0, 60);
        AddBuilding(world, "coal_plant", 2, 0, 60);
        AddBuilding(world, "heat_sink", 3, 0, 60);
        AddBuilding(world, "maintenance_center", 0, 1, 60);
        world.Resources.Restore(50, 100, 0, 1000, 2, 3);
        var formatter = new GameplayFeedbackFormatter(world);

        var lines = formatter.FormatProductionSummaryLines();

        Assert.Equal("GRID SUMMARY", lines[0]);
        Assert.Contains("ENERGY PROD +1/S", lines[1]);
        Assert.Contains("USE -10/S", lines[1]);
        Assert.Contains("NET -9/S", lines[1]);
        Assert.Contains("RESEARCH +1/S", lines[2]);
        Assert.Contains("MONEY +$0/S", lines[2]);
        Assert.Contains("HEAT PROD +10/S", lines[3]);
        Assert.Contains("MANAGED -4/S", lines[3]);
        Assert.Contains("FREE +6/S", lines[3]);
        Assert.Contains("MAINTENANCE ACTIVE 5", lines[4]);
        Assert.Contains("WEAR X0.75", lines[4]);
        Assert.Contains("TOOLS AXES 2/20 +0.02/S", lines[5]);
        Assert.Contains("MINES 3/20 +0.01/S", lines[5]);
    }

    [Fact]
    public void AvailabilityLines_ShouldMatchHoverCardStatusLines()
    {
        var world = CreateWorld(startingMoney: 100, startingResearch: 10);
        var formatter = new GameplayFeedbackFormatter(world);

        Assert.Equal(
            formatter.FormatBuildAvailabilityLine("battery_small"),
            formatter.FormatBuildCardDetails("battery_small")[1]);

        Assert.Equal(
            formatter.FormatResearchAvailabilityLine("generator_small"),
            formatter.FormatResearchCardDetails("generator_small")[1]);

        Assert.Equal(
            formatter.FormatUpgradeAvailabilityLine("battery_capacity_1"),
            formatter.FormatUpgradeCardDetails("battery_capacity_1")[1]);
    }

    [Fact]
    public void AvailabilityLines_ShouldExposeCompletedAndMaxedStates()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 100);
        world.Research.Complete("solar_power");
        world.Upgrades.SetLevel("wind_energy_1", 1);
        var formatter = new GameplayFeedbackFormatter(world);

        Assert.Equal("DONE: RESEARCH COMPLETED", formatter.FormatResearchAvailabilityLine("solar_power"));
        Assert.Equal("MAX LEVEL: 1/1", formatter.FormatUpgradeAvailabilityLine("wind_energy_1"));
    }

    [Fact]
    public void FormatProductionSummaryLines_ShouldKeepStablePanelContract()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 0);
        var formatter = new GameplayFeedbackFormatter(world);

        var lines = formatter.FormatProductionSummaryLines();

        Assert.Equal(6, lines.Count);
        Assert.Equal("GRID SUMMARY", lines[0]);
        Assert.StartsWith("ENERGY PROD", lines[1]);
        Assert.StartsWith("RESEARCH", lines[2]);
        Assert.StartsWith("HEAT PROD", lines[3]);
        Assert.StartsWith("MAINTENANCE ACTIVE", lines[4]);
        Assert.StartsWith("TOOLS AXES", lines[5]);
    }

    private static BuildingInstance AddBuilding(GameWorld world, string definitionId, int x, int y, double lifetimeSeconds)
    {
        var instance = new BuildingInstance(Guid.NewGuid(), definitionId, new GridPosition(x, y), lifetimeSeconds);
        world.AddBuilding(instance);
        return instance;
    }

    private static GameWorld CreateWorld(decimal startingMoney, double startingResearch)
    {
        var buildings = BuildingCatalog.FromDefinitions(new[]
        {
            new BuildingDefinition
            {
                Id = "wind_turbine",
                Name = "Wind turbine",
                Category = BuildingCategory.PowerProducer,
                Cost = 100,
                EnergyPerSecond = 1,
                LifetimeSeconds = 60
            },
            new BuildingDefinition
            {
                Id = "battery_small",
                Name = "Battery",
                Category = BuildingCategory.Storage,
                Cost = 600,
                BatteryCapacity = 500,
                RequiredResearchId = "battery",
                LifetimeSeconds = 60
            },
            new BuildingDefinition
            {
                Id = "research_lab",
                Name = "Research lab",
                Category = BuildingCategory.Research,
                Cost = 100,
                ResearchPerSecond = 1,
                EnergyConsumptionPerSecond = 10,
                LifetimeSeconds = 60
            },
            new BuildingDefinition
            {
                Id = "coal_plant",
                Name = "Coal plant",
                Category = BuildingCategory.HeatProducer,
                Cost = 100,
                HeatPerSecond = 10,
                LifetimeSeconds = 60
            },
            new BuildingDefinition
            {
                Id = "heat_sink",
                Name = "Heat sink",
                Category = BuildingCategory.HeatSink,
                Cost = 100,
                HeatDissipationPerSecond = 4,
                HeatRange = 5,
                LifetimeSeconds = 60
            },
            new BuildingDefinition
            {
                Id = "maintenance_center",
                Name = "Maintenance center",
                Category = BuildingCategory.Maintenance,
                Cost = 100,
                MaintenanceEfficiencyBonus = 0.25,
                LifetimeSeconds = 60
            }
        });

        var research = ResearchCatalog.FromDefinitions(new[]
        {
            new ResearchDefinition
            {
                Id = "battery",
                Name = "Batteries",
                Cost = 10
            },
            new ResearchDefinition
            {
                Id = "solar_power",
                Name = "Solar power",
                Cost = 10
            },
            new ResearchDefinition
            {
                Id = "generator_small",
                Name = "Small generator",
                Cost = 10,
                RequiredResearchIds = new List<string> { "solar_power" }
            }
        });

        var upgrades = UpgradeCatalog.FromDefinitions(new[]
        {
            new UpgradeDefinition
            {
                Id = "wind_energy_1",
                Name = "Wind energy",
                TargetBuildingId = "wind_turbine",
                CostMoney = 100,
                CostResearch = 0,
                EffectType = UpgradeEffectType.MultiplyEnergyProduction,
                Multiplier = 1.5,
                MaxLevel = 1
            },
            new UpgradeDefinition
            {
                Id = "battery_capacity_1",
                Name = "Battery capacity",
                TargetBuildingId = "battery_small",
                CostMoney = 100,
                CostResearch = 0,
                RequiredResearchId = "battery",
                EffectType = UpgradeEffectType.MultiplyBatteryCapacity,
                Multiplier = 1.5,
                MaxLevel = 1
            },
            new UpgradeDefinition
            {
                Id = "mixed_cost_1",
                Name = "Mixed cost",
                TargetBuildingId = "wind_turbine",
                CostMoney = 100,
                CostResearch = 20,
                EffectType = UpgradeEffectType.MultiplyEnergyProduction,
                Multiplier = 1.2,
                MaxLevel = 1
            }
        });

        var economy = new EconomySettings
        {
            StartingMoney = startingMoney,
            StartingMaxEnergy = 100,
            StartingResearch = startingResearch
        };

        return new GameWorld(
            new GridMap(4, 4, TileType.Land),
            new GameData(buildings, economy, research, new HeatSettings(), new ToolSettings(), upgrades));
    }
}
