using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Managers;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Research;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Tests.Managers;

public sealed class ManagerSystemTests
{
    [Fact]
    public void Update_WhenManagerResearchCompleted_ShouldRenewExpiredBuildingAndSpendMoney()
    {
        var world = CreateWorld(startingMoney: 100);
        world.Research.Complete("wind_turbine_manager");
        var build = new BuildSystem(world);
        var buildResult = build.Build("wind_turbine", new GridPosition(1, 1));
        Assert.True(buildResult.Success);
        var instance = world.BuildingInstances[buildResult.BuildingId!.Value];
        instance.ReduceLifetime(60);

        var result = new ManagerSystem(world).Update();

        Assert.Equal(1, result.RenewedCount);
        Assert.Equal(1m, result.MoneySpent);
        Assert.Equal(BuildingState.Active, instance.State);
        Assert.Equal(60, instance.RemainingLifetimeSeconds);
        Assert.Equal(98, world.Resources.Money);
    }

    [Fact]
    public void Update_WhenManagerResearchIsMissing_ShouldLeaveExpiredBuildingUntouched()
    {
        var world = CreateWorld(startingMoney: 100);
        var build = new BuildSystem(world);
        var buildResult = build.Build("wind_turbine", new GridPosition(1, 1));
        Assert.True(buildResult.Success);
        var instance = world.BuildingInstances[buildResult.BuildingId!.Value];
        instance.ReduceLifetime(60);

        var result = new ManagerSystem(world).Update();

        Assert.Equal(0, result.RenewedCount);
        Assert.Equal(BuildingState.Expired, instance.State);
        Assert.Equal(99, world.Resources.Money);
    }

    [Fact]
    public void Update_WhenNotEnoughMoney_ShouldLeaveExpiredBuildingAndReportFailure()
    {
        var world = CreateWorld(startingMoney: 1);
        world.Research.Complete("wind_turbine_manager");
        var build = new BuildSystem(world);
        var buildResult = build.Build("wind_turbine", new GridPosition(1, 1));
        Assert.True(buildResult.Success);
        var instance = world.BuildingInstances[buildResult.BuildingId!.Value];
        instance.ReduceLifetime(60);

        var result = new ManagerSystem(world).Update();

        Assert.Equal(0, result.RenewedCount);
        Assert.Equal(1, result.NotEnoughMoneyCount);
        Assert.Equal(BuildingState.Expired, instance.State);
        Assert.Equal(0, world.Resources.Money);
    }

    [Fact]
    public void Update_ShouldNotRenewExplodedBuildings()
    {
        var world = CreateWorld(startingMoney: 100);
        world.Research.Complete("wind_turbine_manager");
        var build = new BuildSystem(world);
        var buildResult = build.Build("wind_turbine", new GridPosition(1, 1));
        Assert.True(buildResult.Success);
        var instance = world.BuildingInstances[buildResult.BuildingId!.Value];
        instance.MarkExploded();

        var result = new ManagerSystem(world).Update();

        Assert.Equal(0, result.RenewedCount);
        Assert.Equal(BuildingState.Exploded, instance.State);
        Assert.Equal(99, world.Resources.Money);
    }

    [Fact]
    public void IsManaged_ShouldReturnTrueOnlyForCompletedManagerResearch()
    {
        var world = CreateWorld(startingMoney: 100);

        Assert.False(ManagerSystem.IsManaged(world, "wind_turbine"));

        world.Research.Complete("wind_turbine_manager");

        Assert.True(ManagerSystem.IsManaged(world, "wind_turbine"));
        Assert.False(ManagerSystem.IsManaged(world, "solar_panel"));
    }

    private static GameWorld CreateWorld(decimal startingMoney)
    {
        var map = new GridMap(4, 4, TileType.Land);
        var catalog = BuildingCatalog.FromDefinitions(new[]
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
                Id = "solar_panel",
                Name = "Pannello solare",
                Category = BuildingCategory.HeatProducer,
                Cost = 200,
                HeatPerSecond = 10,
                LifetimeSeconds = 180
            }
        });

        var economy = new EconomySettings
        {
            StartingMoney = startingMoney,
            StartingMaxEnergy = 100
        };

        var research = ResearchCatalog.FromDefinitions(new[]
        {
            new ResearchDefinition
            {
                Id = "wind_turbine_manager",
                Name = "Gestore pale eoliche",
                Cost = 300,
                ManagedBuildingIds = new List<string> { "wind_turbine" }
            },
            new ResearchDefinition
            {
                Id = "solar_panel_manager",
                Name = "Gestore pannelli solari",
                Cost = 1000,
                ManagedBuildingIds = new List<string> { "solar_panel" }
            }
        });

        return new GameWorld(map, new GameData(catalog, economy, research));
    }
}
