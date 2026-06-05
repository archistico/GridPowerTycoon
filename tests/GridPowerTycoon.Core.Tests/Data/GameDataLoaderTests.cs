using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Data;

namespace GridPowerTycoon.Core.Tests.Data;

public sealed class GameDataLoaderTests
{
    [Fact]
    public void LoadBuildingCatalog_ShouldReadBuildingCategoryFromString()
    {
        var file = Path.Combine(Path.GetTempPath(), $"buildings-{Guid.NewGuid():N}.json");

        File.WriteAllText(file, """
        {
          "version": 1,
          "buildings": [
            {
              "id": "wind_turbine",
              "name": "Pala eolica",
              "description": "Produce energia.",
              "category": "PowerProducer",
              "cost": 1,
              "width": 1,
              "height": 1,
              "energyPerSecond": 1,
              "heatPerSecond": 0,
              "researchPerSecond": 0,
              "batteryCapacity": 0,
              "autoSellPerSecond": 0,
              "heatConversionPerSecond": 0,
              "heatRange": 0,
              "lifetimeSeconds": 60,
              "requiredResearchId": null,
              "unlockLevel": 1
            }
          ]
        }
        """);

        try
        {
            var loader = new GameDataLoader();
            var catalog = loader.LoadBuildingCatalog(file);

            var definition = catalog.GetRequired("wind_turbine");

            Assert.Equal(BuildingCategory.PowerProducer, definition.Category);
        }
        finally
        {
            File.Delete(file);
        }
    }
}

public sealed class GameDataLoaderResearchTests
{
    [Fact]
    public void LoadResearchCatalog_ShouldReadResearchDefinitions()
    {
        var file = Path.Combine(Path.GetTempPath(), $"research-{Guid.NewGuid():N}.json");

        File.WriteAllText(file, """
        {
          "version": 1,
          "researches": [
            {
              "id": "battery",
              "name": "Batterie",
              "description": "Sblocca le batterie.",
              "cost": 50,
              "unlockBuildingIds": [ "battery_small" ],
              "requiredResearchIds": []
            }
          ]
        }
        """);

        try
        {
            var loader = new GameDataLoader();
            var catalog = loader.LoadResearchCatalog(file);

            var definition = catalog.GetRequired("battery");

            Assert.Equal("Batterie", definition.Name);
            Assert.Equal(50, definition.Cost);
            Assert.Contains("battery_small", definition.UnlockBuildingIds);
        }
        finally
        {
            File.Delete(file);
        }
    }
}

public sealed class GameDataLoaderHeatTests
{
    [Fact]
    public void LoadHeatSettings_ShouldReadHeatSettings()
    {
        var file = Path.Combine(Path.GetTempPath(), $"heat-{Guid.NewGuid():N}.json");

        File.WriteAllText(file, """
        {
          "version": 1,
          "heatWarningThreshold": 60,
          "heatExplosionThreshold": 100,
          "heatEnergyConversionRate": 1.5
        }
        """);

        try
        {
            var loader = new GameDataLoader();
            var settings = loader.LoadHeatSettings(file);

            Assert.Equal(60, settings.HeatWarningThreshold);
            Assert.Equal(100, settings.HeatExplosionThreshold);
            Assert.Equal(1.5, settings.HeatEnergyConversionRate);
        }
        finally
        {
            File.Delete(file);
        }
    }
}

public sealed class GameDataLoaderToolTests
{
    [Fact]
    public void LoadToolSettings_ShouldReadToolSettings()
    {
        var file = Path.Combine(Path.GetTempPath(), $"tools-{Guid.NewGuid():N}.json");

        File.WriteAllText(file, """
        {
          "version": 1,
          "axesPerSecond": 0.05,
          "minesPerSecond": 0.025,
          "maxAxes": 20,
          "maxMines": 12,
          "forestClearAxesCost": 4,
          "mountainClearMinesCost": 5
        }
        """);

        try
        {
            var loader = new GameDataLoader();
            var settings = loader.LoadToolSettings(file);

            Assert.Equal(0.05, settings.AxesPerSecond);
            Assert.Equal(0.025, settings.MinesPerSecond);
            Assert.Equal(20, settings.MaxAxes);
            Assert.Equal(12, settings.MaxMines);
            Assert.Equal(4, settings.ForestClearAxesCost);
            Assert.Equal(5, settings.MountainClearMinesCost);
        }
        finally
        {
            File.Delete(file);
        }
    }
}
