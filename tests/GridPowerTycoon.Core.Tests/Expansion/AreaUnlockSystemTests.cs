using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Expansion;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Tests.Expansion;

public sealed class AreaUnlockSystemTests
{
    [Fact]
    public void UnlockCloud_WhenEnoughResources_ShouldRevealCoveredTileAndSpendResources()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 50);
        var position = new GridPosition(1, 1);
        var tile = world.Map.GetTile(position);
        tile.SetType(TileType.Cloud);
        tile.SetCoveredType(TileType.Forest);
        var system = new AreaUnlockSystem(world);

        var result = system.UnlockCloud(position);

        Assert.True(result.Success);
        Assert.Equal(TileType.Forest, result.RevealedTileType);
        Assert.Equal(TileType.Forest, tile.Type);
        Assert.Null(tile.CoveredType);
        Assert.Equal(500, world.Resources.Money);
        Assert.Equal(40, world.Resources.Research);
    }

    [Fact]
    public void UnlockCloud_WhenNotCloud_ShouldFail()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 50);
        var position = new GridPosition(1, 1);
        var system = new AreaUnlockSystem(world);

        var result = system.UnlockCloud(position);

        Assert.False(result.Success);
        Assert.Equal(AreaUnlockFailureReason.TileNotCloud, result.FailureReason);
        Assert.Equal(TileType.Land, world.Map.GetTile(position).Type);
    }

    [Fact]
    public void UnlockCloud_WhenNotEnoughMoney_ShouldFailWithoutRevealing()
    {
        var world = CreateWorld(startingMoney: 499, startingResearch: 50);
        var position = new GridPosition(1, 1);
        var tile = world.Map.GetTile(position);
        tile.SetType(TileType.Cloud);
        tile.SetCoveredType(TileType.Land);
        var system = new AreaUnlockSystem(world);

        var result = system.UnlockCloud(position);

        Assert.False(result.Success);
        Assert.Equal(AreaUnlockFailureReason.NotEnoughMoney, result.FailureReason);
        Assert.Equal(TileType.Cloud, tile.Type);
        Assert.Equal(499, world.Resources.Money);
    }

    [Fact]
    public void UnlockCloud_WhenNotEnoughResearch_ShouldFailWithoutSpendingMoney()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 9);
        var position = new GridPosition(1, 1);
        var tile = world.Map.GetTile(position);
        tile.SetType(TileType.Cloud);
        tile.SetCoveredType(TileType.Mountain);
        var system = new AreaUnlockSystem(world);

        var result = system.UnlockCloud(position);

        Assert.False(result.Success);
        Assert.Equal(AreaUnlockFailureReason.NotEnoughResearch, result.FailureReason);
        Assert.Equal(TileType.Cloud, tile.Type);
        Assert.Equal(1000, world.Resources.Money);
        Assert.Equal(9, world.Resources.Research);
    }


    [Fact]
    public void UnlockCloud_WithRadius_ShouldRevealConnectedCloudGroupUpToConfiguredMaximum()
    {
        var world = CreateWorld(startingMoney: 1000, startingResearch: 50, radius: 2, maxTiles: 4);
        var positions = new[]
        {
            new GridPosition(1, 1),
            new GridPosition(2, 1),
            new GridPosition(1, 2),
            new GridPosition(2, 2),
            new GridPosition(3, 3)
        };

        foreach (var position in positions)
        {
            var tile = world.Map.GetTile(position);
            tile.SetType(TileType.Cloud);
            tile.SetCoveredType(TileType.Land);
        }

        var system = new AreaUnlockSystem(world);

        var result = system.UnlockCloud(new GridPosition(1, 1));

        Assert.True(result.Success);
        Assert.Equal(4, result.TilesUnlocked);
        Assert.Equal(4, positions.Count(x => world.Map.GetTile(x).Type == TileType.Land));
        Assert.Equal(TileType.Cloud, world.Map.GetTile(new GridPosition(3, 3)).Type);
        Assert.Equal(500, world.Resources.Money);
        Assert.Equal(40, world.Resources.Research);
    }

    [Fact]
    public void GetUnlockableCloudTiles_WhenResourcesMissing_ShouldStillReturnPreviewTiles()
    {
        var world = CreateWorld(startingMoney: 0, startingResearch: 0, radius: 1, maxTiles: 3);
        var positions = new[]
        {
            new GridPosition(1, 1),
            new GridPosition(2, 1),
            new GridPosition(1, 2)
        };

        foreach (var position in positions)
        {
            var tile = world.Map.GetTile(position);
            tile.SetType(TileType.Cloud);
            tile.SetCoveredType(TileType.Land);
        }

        var system = new AreaUnlockSystem(world);

        var tiles = system.GetUnlockableCloudTiles(new GridPosition(1, 1));

        Assert.Equal(3, tiles.Count);
        Assert.Contains(new GridPosition(1, 1), tiles);
        Assert.Contains(new GridPosition(2, 1), tiles);
        Assert.Contains(new GridPosition(1, 2), tiles);
    }

    private static GameWorld CreateWorld(decimal startingMoney, double startingResearch, int radius = 0, int maxTiles = 1)
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
            StartingMoney = startingMoney,
            StartingResearch = startingResearch,
            StartingMaxEnergy = 100
        };

        var areaUnlock = new AreaUnlockSettings
        {
            CloudUnlockMoneyCost = 500,
            CloudUnlockResearchCost = 10,
            CloudUnlockRadius = radius,
            MaxCloudTilesPerUnlock = maxTiles
        };

        return new GameWorld(map, new GameData(catalog, economy, GridPowerTycoon.Core.Research.ResearchCatalog.Empty, new GridPowerTycoon.Core.Heat.HeatSettings(), new GridPowerTycoon.Core.Tools.ToolSettings(), GridPowerTycoon.Core.Upgrades.UpgradeCatalog.Empty, areaUnlock));
    }
}
