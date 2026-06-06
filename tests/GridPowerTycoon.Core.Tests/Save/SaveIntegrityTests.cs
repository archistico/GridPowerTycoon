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

public sealed class SaveIntegrityTests
{
    [Fact]
    public void RestoreWorld_WhenMapHasDuplicateTileCoordinates_ShouldFailExplicitly()
    {
        var save = new SaveGame
        {
            Version = SaveGame.CurrentVersion,
            Map = new SaveMap
            {
                Width = 2,
                Height = 2,
                Tiles = new List<SaveTile>
                {
                    CreateTile(0, 0),
                    CreateTile(1, 0),
                    CreateTile(0, 1),
                    CreateTile(0, 1)
                }
            }
        };

        var exception = Assert.Throws<InvalidOperationException>(() => Restore(save));

        Assert.Contains("duplicate tile coordinates", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RestoreWorld_WhenBuildingFootprintIsNotFullyLinked_ShouldFailExplicitly()
    {
        var buildingId = Guid.NewGuid();
        var save = new SaveGame
        {
            Version = SaveGame.CurrentVersion,
            Map = CreateSaveMap(3, 3, tile => tile.X == 0 && tile.Y == 0 ? buildingId : null),
            Buildings = new List<SaveBuildingInstance>
            {
                CreateBuilding(buildingId, "large_plant", 0, 0)
            }
        };

        var exception = Assert.Throws<InvalidOperationException>(() => Restore(save));

        Assert.Contains("footprint is not fully linked", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RestoreWorld_WhenTileReferencesBuildingOutsideItsFootprint_ShouldFailExplicitly()
    {
        var buildingId = Guid.NewGuid();
        var save = new SaveGame
        {
            Version = SaveGame.CurrentVersion,
            Map = CreateSaveMap(3, 3, tile =>
                (tile.X == 0 && tile.Y == 0) || (tile.X == 2 && tile.Y == 2) ? buildingId : null),
            Buildings = new List<SaveBuildingInstance>
            {
                CreateBuilding(buildingId, "wind_turbine", 0, 0)
            }
        };

        var exception = Assert.Throws<InvalidOperationException>(() => Restore(save));

        Assert.Contains("outside its footprint", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RestoreWorld_WhenUpgradeLevelExceedsMaxLevel_ShouldFailExplicitly()
    {
        var save = new SaveGame
        {
            Version = SaveGame.CurrentVersion,
            Map = CreateSaveMap(1, 1, _ => null),
            UpgradeLevels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["wind_turbine_energy_1"] = 3
            }
        };

        var exception = Assert.Throws<InvalidOperationException>(() => Restore(save));

        Assert.Contains("exceeds max level", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LoadSaveFromFile_WhenJsonIsCorrupted_ShouldFailWithReadableMessage()
    {
        var path = Path.Combine(Path.GetTempPath(), "gridpower-corrupted-save-" + Guid.NewGuid() + ".json");
        File.WriteAllText(path, "{ not valid json");

        try
        {
            var exception = Assert.Throws<InvalidOperationException>(() => new SaveGameService().LoadSaveFromFile(path));

            Assert.Contains("save JSON is invalid or corrupted", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    private static GameWorld Restore(SaveGame save)
    {
        return new SaveGameService().RestoreWorld(save, CreateGameData());
    }

    private static SaveMap CreateSaveMap(int width, int height, Func<SaveTile, Guid?> getBuildingId)
    {
        var map = new SaveMap
        {
            Width = width,
            Height = height
        };

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tile = CreateTile(x, y);
                map.Tiles.Add(new SaveTile
                {
                    X = x,
                    Y = y,
                    Type = tile.Type,
                    CoveredType = tile.CoveredType,
                    BuildingId = getBuildingId(tile)
                });
            }
        }

        return map;
    }

    private static SaveTile CreateTile(int x, int y)
    {
        return new SaveTile
        {
            X = x,
            Y = y,
            Type = TileType.Land
        };
    }

    private static SaveBuildingInstance CreateBuilding(Guid id, string definitionId, int x, int y)
    {
        return new SaveBuildingInstance
        {
            Id = id,
            DefinitionId = definitionId,
            X = x,
            Y = y,
            RemainingLifetimeSeconds = 60,
            State = BuildingState.Active
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
            },
            new BuildingDefinition
            {
                Id = "large_plant",
                Name = "Centrale grande",
                Category = BuildingCategory.PowerProducer,
                Cost = 1,
                Width = 2,
                Height = 2,
                EnergyPerSecond = 4,
                LifetimeSeconds = 60
            }
        });

        var economy = new EconomySettings
        {
            StartingMoney = 100,
            StartingMaxEnergy = 100
        };

        var research = ResearchCatalog.Empty;
        var upgrades = UpgradeCatalog.FromDefinitions(new[]
        {
            new UpgradeDefinition
            {
                Id = "wind_turbine_energy_1",
                Name = "Pale eoliche migliorate I",
                TargetBuildingId = "wind_turbine",
                EffectType = UpgradeEffectType.MultiplyEnergyProduction,
                Multiplier = 1.5,
                MaxLevel = 2
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
