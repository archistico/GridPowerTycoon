using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Expansion;
using GridPowerTycoon.Core.Heat;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Research;
using GridPowerTycoon.Core.Save;
using GridPowerTycoon.Core.Tools;
using GridPowerTycoon.Core.Upgrades;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Tests.Save;

public sealed class SaveGameServiceTests
{
    [Fact]
    public void RestoreWorld_ShouldPreserveResourcesBuildingsResearchAndUpgrades()
    {
        var data = CreateGameData();
        var world = new GameWorld(new GridMap(4, 4, TileType.Land), data);
        var buildSystem = new BuildSystem(world);
        var build = buildSystem.Build("wind_turbine", new GridPosition(1, 1));
        Assert.True(build.Success);

        world.Resources.AddEnergy(25);
        world.Resources.AddResearch(75);
        world.Resources.AddAxes(3, 20);
        world.Resources.AddMines(2, 20);
        world.Research.Complete("battery");
        world.Upgrades.SetLevel("wind_turbine_energy_1", 1);

        var instance = world.BuildingInstances[build.BuildingId!.Value];
        instance.ReduceLifetime(10);
        instance.AddHeat(12);

        var service = new SaveGameService();
        var save = service.CreateSave(world, new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));

        var restored = service.RestoreWorld(save, data);

        Assert.Equal(25, restored.Resources.Energy);
        Assert.Equal(75, restored.Resources.Research);
        Assert.Equal(3, restored.Resources.Axes);
        Assert.Equal(2, restored.Resources.Mines);
        Assert.True(restored.Research.IsCompleted("battery"));
        Assert.Equal(1, restored.Upgrades.GetLevel("wind_turbine_energy_1"));
        Assert.Single(restored.BuildingInstances);

        var restoredInstance = restored.BuildingInstances.Values.Single();
        Assert.Equal("wind_turbine", restoredInstance.DefinitionId);
        Assert.Equal(new GridPosition(1, 1), restoredInstance.Position);
        Assert.Equal(50, restoredInstance.RemainingLifetimeSeconds);
        Assert.Equal(12, restoredInstance.AccumulatedHeat);
        Assert.True(restored.Map.GetTile(new GridPosition(1, 1)).HasBuilding);
    }

    [Fact]
    public void RestoreWorld_ShouldPreserveModifiedMapTilesAndCoveredTypes()
    {
        var data = CreateGameData();
        var map = new GridMap(3, 3, TileType.Land);
        map.GetTile(new GridPosition(0, 0)).SetType(TileType.Water);
        map.GetTile(new GridPosition(1, 0)).SetType(TileType.Cloud);
        map.GetTile(new GridPosition(1, 0)).SetCoveredType(TileType.Forest);
        map.GetTile(new GridPosition(2, 0)).SetType(TileType.Mountain);

        var world = new GameWorld(map, data);
        var service = new SaveGameService();
        var restored = service.RestoreWorld(service.CreateSave(world), data);

        Assert.Equal(TileType.Water, restored.Map.GetTile(new GridPosition(0, 0)).Type);
        Assert.Equal(TileType.Cloud, restored.Map.GetTile(new GridPosition(1, 0)).Type);
        Assert.Equal(TileType.Forest, restored.Map.GetTile(new GridPosition(1, 0)).CoveredType);
        Assert.Equal(TileType.Mountain, restored.Map.GetTile(new GridPosition(2, 0)).Type);
    }

    [Fact]
    public void SaveToFileAndLoadFromFile_ShouldRoundTripWorld()
    {
        var data = CreateGameData();
        var world = new GameWorld(new GridMap(2, 2, TileType.Land), data);
        world.Resources.AddEnergy(10);

        var path = Path.Combine(Path.GetTempPath(), "gridpower-save-test-" + Guid.NewGuid() + ".json");
        var service = new SaveGameService();

        try
        {
            service.SaveToFile(world, path);
            var restored = service.LoadFromFile(path, data);

            Assert.Equal(10, restored.Resources.Energy);
            Assert.Equal(2, restored.Map.Width);
            Assert.Equal(2, restored.Map.Height);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    private static GameData CreateGameData()
    {
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
            }
        });

        var economy = new EconomySettings
        {
            StartingMoney = 100,
            StartingMaxEnergy = 100
        };

        var research = ResearchCatalog.FromDefinitions(new[]
        {
            new ResearchDefinition
            {
                Id = "battery",
                Name = "Batterie",
                Cost = 10
            }
        });

        var upgrades = UpgradeCatalog.FromDefinitions(new[]
        {
            new UpgradeDefinition
            {
                Id = "wind_turbine_energy_1",
                Name = "Pale eoliche migliorate I",
                TargetBuildingId = "wind_turbine",
                EffectType = UpgradeEffectType.MultiplyEnergyProduction,
                Multiplier = 1.5,
                MaxLevel = 1
            }
        });

        return new GameData(
            buildings,
            economy,
            research,
            new HeatSettings(),
            new ToolSettings(),
            upgrades,
            new AreaUnlockSettings());
    }
}
