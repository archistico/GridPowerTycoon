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
