using System.Text.Json;
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

public sealed class SaveCompatibilityTests
{
    [Fact]
    public void CreateSave_WithMilestone26Buildings_ShouldKeepSchemaVersionStableAndAvoidDefinitionDataDuplication()
    {
        var data = CreateCompatibilityData();
        var world = CreateCompatibilityWorld(data);

        BuildRequiredCompatibilityBuildings(world);

        var save = new SaveGameService().CreateSave(world);
        var json = JsonSerializer.Serialize(save);

        Assert.Equal(SaveGame.CurrentVersion, save.Version);
        Assert.DoesNotContain(nameof(BuildingDefinition.EnergyEfficiencyBonus), json);
        Assert.DoesNotContain(nameof(BuildingDefinition.HeatDissipationPerSecond), json);
        Assert.DoesNotContain(nameof(BuildingDefinition.MaintenanceEfficiencyBonus), json);
        Assert.DoesNotContain(nameof(BuildingDefinition.ToolCapacityBonus), json);
    }

    [Fact]
    public void RestoreWorld_WithMilestone26Buildings_ShouldKeepDefinitionDrivenRuntimeBonusesAvailable()
    {
        var data = CreateCompatibilityData();
        var world = CreateCompatibilityWorld(data);

        BuildRequiredCompatibilityBuildings(world);

        var service = new SaveGameService();
        var restored = service.RestoreWorld(service.CreateSave(world), data);
        var rates = ResourceRateSnapshot.Calculate(restored);

        Assert.Equal(12, rates.EnergyPerSecond, 5);
        Assert.Equal(1.2, rates.EnergyEfficiencyMultiplier, 5);
        Assert.Equal(0.75, UpgradeCalculator.GetLifetimeDecayMultiplier(restored), 5);
        Assert.Equal(45, UpgradeCalculator.GetMaxAxes(restored), 5);
        Assert.Equal(45, UpgradeCalculator.GetMaxMines(restored), 5);
        Assert.Equal(50, restored.BuildingCatalog.GetRequired("heat_sink").HeatDissipationPerSecond);
    }

    [Fact]
    public void RestoreWorld_ShouldPreserveLargeBuildingFootprint()
    {
        var data = CreateCompatibilityData();
        var world = CreateCompatibilityWorld(data);
        world.Research.Complete("nuclear_power");

        var build = new BuildSystem(world);
        var result = build.Build("nuclear_reactor", new GridPosition(1, 1));
        Assert.True(result.Success);

        var service = new SaveGameService();
        var restored = service.RestoreWorld(service.CreateSave(world), data);
        var restoredBuildingId = restored.BuildingInstances.Values.Single(x => x.DefinitionId == "nuclear_reactor").Id;

        var occupiedTiles = restored.Map.Tiles
            .Where(x => x.BuildingId == restoredBuildingId)
            .Select(x => x.Position)
            .ToHashSet();

        Assert.Equal(9, occupiedTiles.Count);
        for (var y = 1; y <= 3; y++)
        {
            for (var x = 1; x <= 3; x++)
                Assert.Contains(new GridPosition(x, y), occupiedTiles);
        }
    }

    [Fact]
    public void RestoreWorld_WhenSaveReferencesUnknownBuildingDefinition_ShouldFailExplicitly()
    {
        var data = CreateCompatibilityData();
        var buildingId = Guid.NewGuid();
        var save = new SaveGame
        {
            Version = SaveGame.CurrentVersion,
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

        var exception = Assert.Throws<InvalidOperationException>(() => new SaveGameService().RestoreWorld(save, data));

        Assert.True(exception.Message.Contains("unknown building definition", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void RestoreWorld_WhenSaveReferencesUnknownResearchOrUpgrade_ShouldFailExplicitly()
    {
        var data = CreateCompatibilityData();
        var saveWithResearch = new SaveGame
        {
            Version = SaveGame.CurrentVersion,
            Map = CreateSaveMap(2, 2, null),
            CompletedResearchIds = new List<string> { "removed_research" }
        };
        var saveWithUpgrade = new SaveGame
        {
            Version = SaveGame.CurrentVersion,
            Map = CreateSaveMap(2, 2, null),
            UpgradeLevels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["removed_upgrade"] = 1
            }
        };

        var researchException = Assert.Throws<InvalidOperationException>(() => new SaveGameService().RestoreWorld(saveWithResearch, data));
        var upgradeException = Assert.Throws<InvalidOperationException>(() => new SaveGameService().RestoreWorld(saveWithUpgrade, data));

        Assert.True(researchException.Message.Contains("unknown research", StringComparison.OrdinalIgnoreCase));
        Assert.True(upgradeException.Message.Contains("unknown upgrade", StringComparison.OrdinalIgnoreCase));
    }

    private static void BuildRequiredCompatibilityBuildings(GameWorld world)
    {
        var build = new BuildSystem(world);

        Assert.True(build.Build("wind_turbine", new GridPosition(0, 0)).Success);
        Assert.True(build.Build("substation", new GridPosition(1, 0)).Success);
        Assert.True(build.Build("heat_sink", new GridPosition(2, 0)).Success);
        Assert.True(build.Build("maintenance_center", new GridPosition(3, 0)).Success);
        Assert.True(build.Build("tool_warehouse", new GridPosition(4, 0)).Success);
    }

    private static GameWorld CreateCompatibilityWorld(GameData data)
    {
        return new GameWorld(new GridMap(6, 6, TileType.Land), data);
    }

    private static GameData CreateCompatibilityData()
    {
        var buildings = BuildingCatalog.FromDefinitions(new[]
        {
            new BuildingDefinition
            {
                Id = "wind_turbine",
                Name = "Pala eolica",
                Category = BuildingCategory.PowerProducer,
                Cost = 1,
                EnergyPerSecond = 10,
                LifetimeSeconds = 60
            },
            new BuildingDefinition
            {
                Id = "substation",
                Name = "Trasformatore",
                Category = BuildingCategory.Special,
                Cost = 1,
                EnergyEfficiencyBonus = 0.2
            },
            new BuildingDefinition
            {
                Id = "heat_sink",
                Name = "Raffreddatore",
                Category = BuildingCategory.Special,
                Cost = 1,
                HeatDissipationPerSecond = 50,
                HeatRange = 2
            },
            new BuildingDefinition
            {
                Id = "maintenance_center",
                Name = "Centro manutenzione",
                Category = BuildingCategory.Special,
                Cost = 1,
                MaintenanceEfficiencyBonus = 0.25
            },
            new BuildingDefinition
            {
                Id = "tool_warehouse",
                Name = "Magazzino strumenti",
                Category = BuildingCategory.Special,
                Cost = 1,
                ToolCapacityBonus = 25
            },
            new BuildingDefinition
            {
                Id = "nuclear_reactor",
                Name = "Reattore nucleare",
                Category = BuildingCategory.HeatProducer,
                Cost = 1,
                Width = 3,
                Height = 3,
                HeatPerSecond = 9000,
                EnergyConsumptionPerSecond = 120,
                LifetimeSeconds = 900,
                RequiredResearchId = "nuclear_power"
            }
        });

        var economy = new EconomySettings
        {
            StartingMoney = 1000,
            StartingMaxEnergy = 1000,
            EnergySellValue = 1,
            ManualSellMultiplier = 1,
            AutoSellMultiplier = 1
        };

        var research = ResearchCatalog.FromDefinitions(new[]
        {
            new ResearchDefinition
            {
                Id = "nuclear_power",
                Name = "Energia nucleare",
                Cost = 1,
                UnlockBuildingIds = new List<string> { "nuclear_reactor" }
            }
        });

        var upgrades = UpgradeCatalog.FromDefinitions(new[]
        {
            new UpgradeDefinition
            {
                Id = "wind_energy_1",
                Name = "Pale migliorate",
                TargetBuildingId = "wind_turbine",
                EffectType = UpgradeEffectType.MultiplyEnergyProduction,
                Multiplier = 1.1,
                MaxLevel = 1
            }
        });

        return new GameData(
            buildings,
            economy,
            research,
            new HeatSettings(),
            new ToolSettings
            {
                MaxAxes = 20,
                MaxMines = 20
            },
            upgrades,
            new AreaUnlockSettings());
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
}
