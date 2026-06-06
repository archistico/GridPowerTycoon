using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Data;

namespace GridPowerTycoon.Core.Tests.Data;

public sealed class NuclearReactorRuntimeDataTests
{
    [Fact]
    public void NuclearReactor_ShouldBeHighTierRiskyHeatProducer()
    {
        var (buildings, _, _) = LoadRuntimeData();

        var nuclear = buildings.GetRequired("nuclear_reactor");
        var gas = buildings.GetRequired("gas_power_plant");
        var dataCenter = buildings.GetRequired("data_center");

        Assert.Equal(BuildingCategory.HeatProducer, nuclear.Category);
        Assert.Equal("nuclear_power", nuclear.RequiredResearchId);
        Assert.Equal(0, nuclear.EnergyPerSecond);
        Assert.True(nuclear.HeatPerSecond > gas.HeatPerSecond);
        Assert.True(nuclear.Cost > gas.Cost);
        Assert.True(nuclear.EnergyConsumptionPerSecond > dataCenter.EnergyConsumptionPerSecond);
        Assert.Equal(3, nuclear.Width);
        Assert.Equal(3, nuclear.Height);
    }

    [Fact]
    public void NuclearResearch_ShouldRequireAdvancedInfrastructureAndUnlockReactor()
    {
        var (_, researches, _) = LoadRuntimeData();

        var nuclear = researches.GetRequired("nuclear_power");

        Assert.Contains("geothermal_power", nuclear.RequiredResearchIds);
        Assert.Contains("maintenance_center", nuclear.RequiredResearchIds);
        Assert.Contains("data_center", nuclear.RequiredResearchIds);
        Assert.Contains("nuclear_reactor", nuclear.UnlockBuildingIds);
    }

    [Fact]
    public void NuclearUpgrades_ShouldTargetNuclearReactorAndRequireNuclearResearch()
    {
        var (_, _, upgrades) = LoadRuntimeData();

        var heatUpgrade = upgrades.GetRequired("nuclear_heat_1");
        var lifetimeUpgrade = upgrades.GetRequired("nuclear_lifetime_1");

        Assert.Equal("nuclear_reactor", heatUpgrade.TargetBuildingId);
        Assert.Equal("nuclear_power", heatUpgrade.RequiredResearchId);
        Assert.Equal("nuclear_reactor", lifetimeUpgrade.TargetBuildingId);
        Assert.Equal("nuclear_power", lifetimeUpgrade.RequiredResearchId);
    }

    private static (GridPowerTycoon.Core.Buildings.BuildingCatalog Buildings, GridPowerTycoon.Core.Research.ResearchCatalog Researches, GridPowerTycoon.Core.Upgrades.UpgradeCatalog Upgrades) LoadRuntimeData()
    {
        var root = FindRepositoryRoot();
        var dataPath = Path.Combine(root, "src", "GridPowerTycoon.MonoGame", "Data");
        var loader = new GameDataLoader();

        return (
            loader.LoadBuildingCatalog(Path.Combine(dataPath, "buildings.json")),
            loader.LoadResearchCatalog(Path.Combine(dataPath, "research.json")),
            loader.LoadUpgradeCatalog(Path.Combine(dataPath, "upgrades.json")));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "GridPowerTycoon.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Unable to locate GridPowerTycoon repository root.");
    }
}
