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

public sealed class SaveIdMigrationTests
{
    [Fact]
    public void RestoreWorld_WithRenamedBuildingResearchAndUpgradeIds_ShouldResolveToCurrentIds()
    {
        var buildingId = Guid.NewGuid();
        var save = new SaveGame
        {
            Version = SaveGame.CurrentVersion,
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

        var restored = new SaveGameService(migration).RestoreWorld(save, CreateGameData());

        var restoredBuilding = restored.BuildingInstances.Values.Single();
        Assert.Equal("wind_turbine", restoredBuilding.DefinitionId);
        Assert.True(restored.Research.IsCompleted("battery"));
        Assert.Equal(1, restored.Upgrades.GetLevel("wind_turbine_energy_1"));
        Assert.True(restored.Map.GetTile(new GridPosition(0, 0)).HasBuilding);
    }

    [Fact]
    public void RestoreWorld_WhenResearchIdsCollideAfterMigration_ShouldFailExplicitly()
    {
        var save = new SaveGame
        {
            Version = SaveGame.CurrentVersion,
            Resources = CreateSaveResources(),
            Map = CreateSaveMap(2, 2, null),
            CompletedResearchIds = new List<string> { "old_battery", "battery" }
        };
        var migration = new SaveIdMigrationMap(
            researchIds: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["old_battery"] = "battery"
            });

        var exception = Assert.Throws<InvalidOperationException>(() => new SaveGameService(migration).RestoreWorld(save, CreateGameData()));

        Assert.Contains("after id migration", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RestoreWorld_WhenUpgradeIdsCollideAfterMigration_ShouldFailExplicitly()
    {
        var save = new SaveGame
        {
            Version = SaveGame.CurrentVersion,
            Resources = CreateSaveResources(),
            Map = CreateSaveMap(2, 2, null),
            UpgradeLevels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["old_wind_energy"] = 1,
                ["wind_turbine_energy_1"] = 1
            }
        };
        var migration = new SaveIdMigrationMap(
            upgradeIds: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["old_wind_energy"] = "wind_turbine_energy_1"
            });

        var exception = Assert.Throws<InvalidOperationException>(() => new SaveGameService(migration).RestoreWorld(save, CreateGameData()));

        Assert.Contains("after id migration", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RestoreWorld_WhenUnknownIdHasNoMigration_ShouldKeepExistingExplicitFailure()
    {
        var buildingId = Guid.NewGuid();
        var save = new SaveGame
        {
            Version = SaveGame.CurrentVersion,
            Resources = CreateSaveResources(),
            Map = CreateSaveMap(2, 2, buildingId),
            Buildings = new List<SaveBuildingInstance>
            {
                new()
                {
                    Id = buildingId,
                    DefinitionId = "removed_building",
                    X = 0,
                    Y = 0,
                    RemainingLifetimeSeconds = 60,
                    State = BuildingState.Active
                }
            }
        };

        var exception = Assert.Throws<InvalidOperationException>(() => new SaveGameService().RestoreWorld(save, CreateGameData()));

        Assert.Contains("unknown building definition", exception.Message, StringComparison.OrdinalIgnoreCase);
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
