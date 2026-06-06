using System.Text.Json;
using System.Text.Json.Serialization;
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
    public void CreateSave_ShouldExposeSaveAndDataVersions()
    {
        var data = CreateGameData();
        var world = new GameWorld(new GridMap(2, 2, TileType.Land), data);
        var savedAt = new DateTimeOffset(2026, 6, 6, 14, 30, 0, TimeSpan.Zero);

        var service = new SaveGameService();
        var save = service.CreateSave(world, savedAt);
        var summary = service.CreateSummary(save);

        Assert.Equal(SaveGame.CurrentVersion, save.Version);
        Assert.Equal(GameData.CurrentVersion, save.DataVersion);
        Assert.Equal(SaveGame.CurrentVersion, summary.Version);
        Assert.Equal(GameData.CurrentVersion, summary.DataVersion);
        Assert.Equal(savedAt, summary.SavedAt);
        Assert.Contains("SAVE V", summary.FormatCompact());
        Assert.Contains("DATA V", summary.FormatCompact());
    }

    [Fact]
    public void RestoreWorld_WhenDataVersionIsUnsupported_ShouldFailExplicitly()
    {
        var data = CreateGameData();
        var save = new SaveGame
        {
            Version = SaveGame.CurrentVersion,
            DataVersion = GameData.CurrentVersion + 1,
            Map = new SaveMap
            {
                Width = 2,
                Height = 2,
                Tiles = new List<SaveTile>
                {
                    new() { X = 0, Y = 0, Type = TileType.Land },
                    new() { X = 1, Y = 0, Type = TileType.Land },
                    new() { X = 0, Y = 1, Type = TileType.Land },
                    new() { X = 1, Y = 1, Type = TileType.Land }
                }
            }
        };

        var exception = Assert.Throws<InvalidOperationException>(() => new SaveGameService().RestoreWorld(save, data));

        Assert.Contains("Unsupported data version", exception.Message);
    }

    [Fact]
    public void SaveToFile_WhenSaveAlreadyExists_ShouldCreateBackupFromPreviousFile()
    {
        var data = CreateGameData();
        var firstWorld = new GameWorld(new GridMap(2, 2, TileType.Land), data);
        firstWorld.Resources.AddEnergy(10);

        var secondWorld = new GameWorld(new GridMap(2, 2, TileType.Land), data);
        secondWorld.Resources.AddEnergy(25);

        var directory = Directory.CreateTempSubdirectory("gridpower-save-backup-test-");
        var path = Path.Combine(directory.FullName, "savegame.json");
        var backupPath = SaveGameService.GetBackupPath(path);
        var service = new SaveGameService();

        try
        {
            service.SaveToFile(firstWorld, path, new DateTimeOffset(2026, 6, 6, 10, 0, 0, TimeSpan.Zero));
            Assert.False(File.Exists(backupPath));

            service.SaveToFile(secondWorld, path, new DateTimeOffset(2026, 6, 6, 11, 0, 0, TimeSpan.Zero));

            Assert.True(File.Exists(path));
            Assert.True(File.Exists(backupPath));

            var current = service.LoadSaveFromFile(path);
            var backup = service.LoadSaveFromFile(backupPath);

            Assert.Equal(25, current.Resources.Energy);
            Assert.Equal(new DateTimeOffset(2026, 6, 6, 11, 0, 0, TimeSpan.Zero), current.SavedAt);
            Assert.Equal(10, backup.Resources.Energy);
            Assert.Equal(new DateTimeOffset(2026, 6, 6, 10, 0, 0, TimeSpan.Zero), backup.SavedAt);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void GetBackupPath_ShouldUseBackupSuffixBeforeJsonExtension()
    {
        var path = Path.Combine("Saves", "savegame.json");

        var backupPath = SaveGameService.GetBackupPath(path);

        Assert.Equal(Path.Combine("Saves", "savegame.backup.json"), backupPath);
    }


    [Fact]
    public void LoadFromFileWithBackup_WhenPrimaryJsonIsCorrupted_ShouldRestoreBackup()
    {
        var data = CreateGameData();
        var backupWorld = new GameWorld(new GridMap(2, 2, TileType.Land), data);
        backupWorld.Resources.AddEnergy(10);

        var currentWorld = new GameWorld(new GridMap(2, 2, TileType.Land), data);
        currentWorld.Resources.AddEnergy(25);

        var directory = Directory.CreateTempSubdirectory("gridpower-save-fallback-corrupt-test-");
        var path = Path.Combine(directory.FullName, "savegame.json");
        var service = new SaveGameService();

        try
        {
            service.SaveToFile(backupWorld, path, new DateTimeOffset(2026, 6, 6, 10, 0, 0, TimeSpan.Zero));
            service.SaveToFile(currentWorld, path, new DateTimeOffset(2026, 6, 6, 11, 0, 0, TimeSpan.Zero));
            File.WriteAllText(path, "{ this is not valid json");

            var result = service.LoadFromFileWithBackup(path, data);

            Assert.True(result.LoadedFromBackup);
            Assert.Equal(10, result.World.Resources.Energy);
            Assert.Equal(new DateTimeOffset(2026, 6, 6, 10, 0, 0, TimeSpan.Zero), result.Save.SavedAt);
            Assert.Equal(result.Save.SavedAt, result.Summary.SavedAt);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void LoadFromFileWithBackup_WhenPrimaryRestoreIsInvalid_ShouldRestoreBackup()
    {
        var data = CreateGameData();
        var backupWorld = new GameWorld(new GridMap(2, 2, TileType.Land), data);
        backupWorld.Resources.AddEnergy(15);

        var directory = Directory.CreateTempSubdirectory("gridpower-save-fallback-invalid-test-");
        var path = Path.Combine(directory.FullName, "savegame.json");
        var service = new SaveGameService();

        try
        {
            service.SaveToFile(backupWorld, path, new DateTimeOffset(2026, 6, 6, 10, 0, 0, TimeSpan.Zero));
            File.Copy(path, SaveGameService.GetBackupPath(path), overwrite: true);

            var invalidSave = service.CreateSave(new GameWorld(new GridMap(2, 2, TileType.Land), data));
            var invalidSaveJson = SerializeSave(invalidSave)
                .Replace($"\"DataVersion\": {GameData.CurrentVersion}", $"\"DataVersion\": {GameData.CurrentVersion + 1}");
            File.WriteAllText(path, invalidSaveJson);

            var result = service.LoadFromFileWithBackup(path, data);

            Assert.True(result.LoadedFromBackup);
            Assert.Equal(15, result.World.Resources.Energy);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void LoadFromFileWithBackup_WhenOnlyBackupExists_ShouldRestoreBackup()
    {
        var data = CreateGameData();
        var backupWorld = new GameWorld(new GridMap(2, 2, TileType.Land), data);
        backupWorld.Resources.AddEnergy(30);

        var directory = Directory.CreateTempSubdirectory("gridpower-save-fallback-only-backup-test-");
        var path = Path.Combine(directory.FullName, "savegame.json");
        var backupPath = SaveGameService.GetBackupPath(path);
        var service = new SaveGameService();

        try
        {
            service.SaveToFile(backupWorld, backupPath, new DateTimeOffset(2026, 6, 6, 10, 0, 0, TimeSpan.Zero));

            var result = service.LoadFromFileWithBackup(path, data);

            Assert.True(result.LoadedFromBackup);
            Assert.Equal(30, result.World.Resources.Energy);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void LoadFromFileWithBackup_WhenBothPrimaryAndBackupFail_ShouldFailExplicitly()
    {
        var data = CreateGameData();
        var directory = Directory.CreateTempSubdirectory("gridpower-save-fallback-both-fail-test-");
        var path = Path.Combine(directory.FullName, "savegame.json");
        var service = new SaveGameService();

        try
        {
            File.WriteAllText(path, "{ bad primary");
            File.WriteAllText(SaveGameService.GetBackupPath(path), "{ bad backup");

            var exception = Assert.Throws<InvalidOperationException>(() => service.LoadFromFileWithBackup(path, data));

            Assert.Contains("backup", exception.Message.ToLowerInvariant());
        }
        finally
        {
            directory.Delete(recursive: true);
        }
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

    private static string SerializeSave(SaveGame save)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        return JsonSerializer.Serialize(save, options);
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
