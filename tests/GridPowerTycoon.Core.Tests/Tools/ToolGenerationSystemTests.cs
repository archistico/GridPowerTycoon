using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Tools;
using GridPowerTycoon.Core.Upgrades;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Tests.Tools;

public sealed class ToolGenerationSystemTests
{
    [Fact]
    public void Update_ShouldGenerateAxesAndMines()
    {
        var world = CreateWorld();
        var system = new ToolGenerationSystem(world);

        system.Update(10);

        Assert.Equal(1, world.Resources.Axes);
        Assert.Equal(0.5, world.Resources.Mines);
    }

    [Fact]
    public void Update_ShouldRespectToolCaps()
    {
        var world = CreateWorld();
        var system = new ToolGenerationSystem(world);

        system.Update(1000);

        Assert.Equal(5, world.Resources.Axes);
        Assert.Equal(3, world.Resources.Mines);
    }


    [Fact]
    public void Update_WithToolWarehouse_ShouldIncreaseToolCaps()
    {
        var world = CreateWorld();
        var warehouse = new BuildingInstance(
            Guid.NewGuid(),
            "tool_warehouse_small",
            new GridPosition(1, 1),
            lifetimeSeconds: 0);
        world.AddBuilding(warehouse);
        var system = new ToolGenerationSystem(world);

        system.Update(1000);

        Assert.Equal(30, world.Resources.Axes);
        Assert.Equal(28, world.Resources.Mines);
        Assert.Equal(30, UpgradeCalculator.GetMaxAxes(world));
        Assert.Equal(28, UpgradeCalculator.GetMaxMines(world));
    }

    private static GameWorld CreateWorld()
    {
        var map = new GridMap(4, 4, TileType.Land);
        var catalog = BuildingCatalog.FromDefinitions(new[]
        {
            new BuildingDefinition
            {
                Id = "dummy",
                Name = "Dummy",
                Category = BuildingCategory.Special,
                Cost = 0
            },
            new BuildingDefinition
            {
                Id = "tool_warehouse_small",
                Name = "Magazzino strumenti",
                Category = BuildingCategory.ToolStorage,
                Cost = 1,
                ToolCapacityBonus = 25
            }
        });

        var economy = new EconomySettings
        {
            StartingMoney = 0,
            StartingMaxEnergy = 100
        };

        var tools = new ToolSettings
        {
            AxesPerSecond = 0.1,
            MinesPerSecond = 0.05,
            MaxAxes = 5,
            MaxMines = 3,
            ForestClearAxesCost = 4,
            MountainClearMinesCost = 4
        };

        return new GameWorld(map, new GameData(catalog, economy, GridPowerTycoon.Core.Research.ResearchCatalog.Empty, new GridPowerTycoon.Core.Heat.HeatSettings(), tools));
    }
}
