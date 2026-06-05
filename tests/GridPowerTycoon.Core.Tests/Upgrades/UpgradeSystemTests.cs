using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Research;
using GridPowerTycoon.Core.Upgrades;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Tests.Upgrades;

public sealed class UpgradeSystemTests
{
    [Fact]
    public void PurchaseEnergyUpgrade_ShouldIncreaseProductionRate()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 0);
        var build = new BuildSystem(world);
        Assert.True(build.Build("wind_turbine", new GridPosition(1, 1)).Success);
        var system = new UpgradeSystem(world);

        var result = system.Purchase("wind_energy_1");
        var rates = ResourceRateSnapshot.Calculate(world);

        Assert.True(result.Success);
        Assert.Equal(1.5, rates.RawEnergyProductionPerSecond);
    }

    [Fact]
    public void PurchaseLifetimeUpgrade_ShouldAffectNewBuildings()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 0);
        var system = new UpgradeSystem(world);
        Assert.True(system.Purchase("wind_lifetime_1").Success);
        var build = new BuildSystem(world);

        var result = build.Build("wind_turbine", new GridPosition(1, 1));

        Assert.True(result.Success);
        var instance = world.BuildingInstances[result.BuildingId!.Value];
        Assert.Equal(120, instance.RemainingLifetimeSeconds);
    }

    [Fact]
    public void PurchaseBatteryUpgrade_ShouldRecalculateExistingBatteryCapacity()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 1000);
        world.Research.Complete("battery");
        var build = new BuildSystem(world);
        Assert.True(build.Build("battery_small", new GridPosition(1, 1)).Success);
        Assert.Equal(600, world.Resources.MaxEnergy);
        var system = new UpgradeSystem(world);

        var result = system.Purchase("battery_capacity_1");

        Assert.True(result.Success);
        Assert.Equal(850, world.Resources.MaxEnergy);
    }

    [Fact]
    public void Purchase_WhenMissingResearch_ShouldFail()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 1000);
        var system = new UpgradeSystem(world);

        var result = system.Purchase("battery_capacity_1");

        Assert.False(result.Success);
        Assert.Equal(UpgradeFailureReason.MissingResearch, result.FailureReason);
    }

    [Fact]
    public void Purchase_WhenAlreadyAtMaxLevel_ShouldFail()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 0);
        var system = new UpgradeSystem(world);
        Assert.True(system.Purchase("wind_energy_1").Success);

        var second = system.Purchase("wind_energy_1");

        Assert.False(second.Success);
        Assert.Equal(UpgradeFailureReason.MaxLevelReached, second.FailureReason);
    }


    [Fact]
    public void PurchaseMultiLevelUpgrade_ShouldIncreaseLevelAndUseGrowingCost()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 0);
        var system = new UpgradeSystem(world);

        var first = system.Purchase("wind_multi_energy");
        var second = system.Purchase("wind_multi_energy");

        Assert.True(first.Success);
        Assert.Equal(1, first.NewLevel);
        Assert.True(second.Success);
        Assert.Equal(2, second.NewLevel);
        Assert.Equal(2, world.Upgrades.GetLevel("wind_multi_energy"));
        Assert.Equal(1 * Math.Pow(1.5, 2), UpgradeCalculator.GetEnergyPerSecond(world, world.BuildingCatalog.GetRequired("wind_turbine")), 6);
        Assert.Equal(1000m - 100m - 200m, world.Resources.Money);
    }

    [Fact]
    public void PurchaseMultiLevelUpgrade_WhenAtMaxLevel_ShouldFail()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 0);
        var system = new UpgradeSystem(world);

        Assert.True(system.Purchase("wind_multi_energy").Success);
        Assert.True(system.Purchase("wind_multi_energy").Success);
        var third = system.Purchase("wind_multi_energy");

        Assert.False(third.Success);
        Assert.Equal(UpgradeFailureReason.MaxLevelReached, third.FailureReason);
    }

    [Fact]
    public void ToolGenerationUpgrade_ShouldIncreaseAxesRate()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 1000);
        var system = new UpgradeSystem(world);
        Assert.True(system.Purchase("axes_generation_1").Success);

        var axesPerSecond = UpgradeCalculator.GetAxesPerSecond(world);

        Assert.Equal(0.125, axesPerSecond);
    }

    private static GameWorld CreateWorld(decimal startingMoney, double startingResearch)
    {
        var map = new GridMap(4, 4, TileType.Land);
        var buildings = BuildingCatalog.FromDefinitions(new[]
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
                Id = "battery_small",
                Name = "Batteria",
                Category = BuildingCategory.Storage,
                Cost = 50,
                BatteryCapacity = 500,
                RequiredResearchId = "battery"
            }
        });

        var research = ResearchCatalog.FromDefinitions(new[]
        {
            new ResearchDefinition
            {
                Id = "battery",
                Name = "Batterie",
                Cost = 10,
                UnlockBuildingIds = new List<string> { "battery_small" }
            }
        });

        var upgrades = UpgradeCatalog.FromDefinitions(new[]
        {
            new UpgradeDefinition
            {
                Id = "wind_energy_1",
                Name = "Eolico I",
                TargetBuildingId = "wind_turbine",
                CostMoney = 100,
                EffectType = UpgradeEffectType.MultiplyEnergyProduction,
                Multiplier = 1.5,
                MaxLevel = 1
            },

            new UpgradeDefinition
            {
                Id = "wind_multi_energy",
                Name = "Eolico multi",
                TargetBuildingId = "wind_turbine",
                CostMoney = 100,
                CostGrowthMultiplier = 2,
                EffectType = UpgradeEffectType.MultiplyEnergyProduction,
                Multiplier = 1.5,
                MaxLevel = 2
            },
            new UpgradeDefinition
            {
                Id = "wind_lifetime_1",                Name = "Vita Eolico I",
                TargetBuildingId = "wind_turbine",
                CostMoney = 100,
                EffectType = UpgradeEffectType.MultiplyLifetime,
                Multiplier = 2,
                MaxLevel = 1
            },
            new UpgradeDefinition
            {
                Id = "battery_capacity_1",
                Name = "Batterie I",
                TargetBuildingId = "battery_small",
                CostMoney = 100,
                EffectType = UpgradeEffectType.MultiplyBatteryCapacity,
                Multiplier = 1.5,
                RequiredResearchId = "battery",
                MaxLevel = 1
            },
            new UpgradeDefinition
            {
                Id = "axes_generation_1",
                Name = "Asce I",
                CostMoney = 100,
                EffectType = UpgradeEffectType.MultiplyToolAxesGeneration,
                Multiplier = 1.25,
                MaxLevel = 1
            }
        });

        var economy = new EconomySettings
        {
            StartingMoney = startingMoney,
            StartingResearch = startingResearch,
            StartingMaxEnergy = 100
        };

        var tools = new GridPowerTycoon.Core.Tools.ToolSettings
        {
            AxesPerSecond = 0.1,
            MinesPerSecond = 0.05,
            MaxAxes = 10,
            MaxMines = 10,
            ForestClearAxesCost = 4,
            MountainClearMinesCost = 4
        };

        return new GameWorld(
            map,
            new GameData(
                buildings,
                economy,
                research,
                new GridPowerTycoon.Core.Heat.HeatSettings(),
                tools,
                upgrades));
    }
}
