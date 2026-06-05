using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Tools;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Tests.Tools;

public sealed class TerrainClearSystemTests
{
    [Fact]
    public void ClearForest_WhenEnoughAxes_ShouldTurnTileToLandAndSpendAxes()
    {
        var world = CreateWorld(startingAxes: 4, startingMines: 0);
        var position = new GridPosition(1, 1);
        world.Map.GetTile(position).SetType(TileType.Forest);
        var system = new TerrainClearSystem(world);

        var result = system.Clear(position);

        Assert.True(result.Success);
        Assert.Equal(TileType.Land, world.Map.GetTile(position).Type);
        Assert.Equal(0, world.Resources.Axes);
    }

    [Fact]
    public void ClearForest_WhenNotEnoughAxes_ShouldFail()
    {
        var world = CreateWorld(startingAxes: 3, startingMines: 0);
        var position = new GridPosition(1, 1);
        world.Map.GetTile(position).SetType(TileType.Forest);
        var system = new TerrainClearSystem(world);

        var result = system.Clear(position);

        Assert.False(result.Success);
        Assert.Equal(TerrainClearFailureReason.NotEnoughAxes, result.FailureReason);
        Assert.Equal(TileType.Forest, world.Map.GetTile(position).Type);
        Assert.Equal(3, world.Resources.Axes);
    }

    [Fact]
    public void ClearMountain_WhenEnoughMines_ShouldTurnTileToLandAndSpendMines()
    {
        var world = CreateWorld(startingAxes: 0, startingMines: 4);
        var position = new GridPosition(1, 1);
        world.Map.GetTile(position).SetType(TileType.Mountain);
        var system = new TerrainClearSystem(world);

        var result = system.Clear(position);

        Assert.True(result.Success);
        Assert.Equal(TileType.Land, world.Map.GetTile(position).Type);
        Assert.Equal(0, world.Resources.Mines);
    }

    [Fact]
    public void ClearLand_ShouldFailAsNotClearable()
    {
        var world = CreateWorld(startingAxes: 10, startingMines: 10);
        var position = new GridPosition(1, 1);
        var system = new TerrainClearSystem(world);

        var result = system.Clear(position);

        Assert.False(result.Success);
        Assert.Equal(TerrainClearFailureReason.NotClearableTerrain, result.FailureReason);
        Assert.Equal(TileType.Land, world.Map.GetTile(position).Type);
    }

    private static GameWorld CreateWorld(int startingAxes, int startingMines)
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
            StartingMaxEnergy = 100,
            StartingAxes = startingAxes,
            StartingMines = startingMines
        };

        var tools = new ToolSettings
        {
            ForestClearAxesCost = 4,
            MountainClearMinesCost = 4,
            MaxAxes = 20,
            MaxMines = 20
        };

        return new GameWorld(map, new GameData(catalog, economy, GridPowerTycoon.Core.Research.ResearchCatalog.Empty, new GridPowerTycoon.Core.Heat.HeatSettings(), tools));
    }
}
