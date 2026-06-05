using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Tools;
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
