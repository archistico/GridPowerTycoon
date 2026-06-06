using System.Text.Json;
using System.Text.Json.Serialization;
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

public sealed class PersistenceRegressionTests
{
    [Fact]
    public void SaveToFile_AfterMultipleWrites_ShouldKeepCurrentSaveAndLatestPreviousBackup()
    {
        var data = CreateGameData();
        var service = new SaveGameService();
        var directory = Directory.CreateTempSubdirectory("gridpower-persistence-rotation-test-");
        var path = Path.Combine(directory.FullName, "savegame.json");
        var backupPath = SaveGameService.GetBackupPath(path);

        try
        {
            service.SaveToFile(CreateWorldWithEnergy(data, 10), path, new DateTimeOffset(2026, 6, 6, 10, 0, 0, TimeSpan.Zero));
            service.SaveToFile(CreateWorldWithEnergy(data, 25), path, new DateTimeOffset(2026, 6, 6, 11, 0, 0, TimeSpan.Zero));
            service.SaveToFile(CreateWorldWithEnergy(data, 40), path, new DateTimeOffset(2026, 6, 6, 12, 0, 0, TimeSpan.Zero));

            var current = service.LoadSaveFromFile(path);
            var backup = service.LoadSaveFromFile(backupPath);

            Assert.Equal(40, current.Resources.Energy);
            Assert.Equal(new DateTimeOffset(2026, 6, 6, 12, 0, 0, TimeSpan.Zero), current.SavedAt);
            Assert.Equal(25, backup.Resources.Energy);
            Assert.Equal(new DateTimeOffset(2026, 6, 6, 11, 0, 0, TimeSpan.Zero), backup.SavedAt);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void LoadFromFileWithBackup_WhenBackupUsesOldIds_ShouldApplyMigrationDuringFallback()
    {
        var data = CreateGameData();
        var buildingId = Guid.NewGuid();
        var oldSave = new SaveGame
        {
            Version = SaveGame.CurrentVersion,
            DataVersion = GameData.CurrentVersion,
            SavedAt = new DateTimeOffset(2026, 6, 6, 13, 0, 0, TimeSpan.Zero),
            Resources = CreateSaveResources(),
            Map = CreateSaveMap(2, 2, buildingId),
            Buildings = new List<SaveBuildingInstance>
            {
                new()
                {
                    Id = buildingId,
                    DefinitionId = "old_wind_turbine",
                    X = 0,
                    Y = 0,
                    RemainingLifetimeSeconds = 55,
                    State = BuildingState.Active
                }
            },
            CompletedResearchIds = new List<string> { "old_battery" },
            UpgradeLevels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["old_wind_energy"] = 1
            }
        };

        var migration = new SaveIdMigrationMap(
            buildingDefinitionIds: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["old_wind_turbine"] = "wind_turbine"
            },
            researchIds: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["old_battery"] = "battery"
            },
            upgradeIds: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["old_wind_energy"] = "wind_turbine_energy_1"
            });

        var directory = Directory.CreateTempSubdirectory("gridpower-persistence-migrated-backup-test-");
        var path = Path.Combine(directory.FullName, "savegame.json");
        var backupPath = SaveGameService.GetBackupPath(path);
        var service = new SaveGameService(migration);

        try
        {
            File.WriteAllText(path, "{ invalid primary save");
            File.WriteAllText(backupPath, SerializeSave(oldSave));

            var result = service.LoadFromFileWithBackup(path, data);

            Assert.True(result.LoadedFromBackup);
            Assert.Equal(oldSave.SavedAt, result.Summary.SavedAt);

            var restoredBuilding = result.World.BuildingInstances.Values.Single();
            Assert.Equal("wind_turbine", restoredBuilding.DefinitionId);
            Assert.True(result.World.Research.IsCompleted("battery"));
            Assert.Equal(1, result.World.Upgrades.GetLevel("wind_turbine_energy_1"));
            Assert.True(result.World.Map.GetTile(new GridPosition(0, 0)).HasBuilding);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void AutoSaveState_AfterTrigger_ShouldRequireFreshDirtyTimeBeforeTriggeringAgain()
    {
        var state = new AutoSaveState(TimeSpan.FromSeconds(60));

        Assert.True(state.Tick(TimeSpan.FromSeconds(60), isDirty: true));
        Assert.False(state.Tick(TimeSpan.FromSeconds(59), isDirty: true));
        Assert.Equal(TimeSpan.FromSeconds(59), state.ElapsedDirtyTime);
        Assert.True(state.Tick(TimeSpan.FromSeconds(1), isDirty: true));
    }

    private static GameWorld CreateWorldWithEnergy(GameData data, double energy)
    {
        var world = new GameWorld(new GridMap(2, 2, TileType.Land), data);
        world.Resources.AddEnergy(energy);
        return world;
    }

    private static SaveResources CreateSaveResources()
    {
        return new SaveResources
        {
            Energy = 0,
            MaxEnergy = 100,
            Research = 0,
            Money = 100,
            Axes = 0,
            Mines = 0
        };
    }

    private static SaveMap CreateSaveMap(int width, int height, Guid? buildingId)
    {
        var tiles = new List<SaveTile>();
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                tiles.Add(new SaveTile
                {
                    X = x,
                    Y = y,
                    Type = TileType.Land,
                    BuildingId = x == 0 && y == 0 ? buildingId : null
                });
            }
        }

        return new SaveMap
        {
            Width = width,
            Height = height,
            Tiles = tiles
        };
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
                Cost = 1
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
