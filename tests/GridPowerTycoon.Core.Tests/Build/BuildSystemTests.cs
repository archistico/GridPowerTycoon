using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Tests.Build;

public sealed class BuildSystemTests
{
    [Fact]
    public void BuildWindTurbine_OnLand_ShouldSucceed()
    {
        var world = CreateWorld(startingMoney: 100);
        var system = new BuildSystem(world);

        var result = system.Build("wind_turbine", new GridPosition(1, 1));

        Assert.True(result.Success);
        Assert.NotNull(result.BuildingId);
        Assert.True(world.Map.GetTile(new GridPosition(1, 1)).HasBuilding);
        Assert.Single(world.BuildingInstances);
    }

    [Fact]
    public void BuildWindTurbine_ShouldDecreaseMoney()
    {
        var world = CreateWorld(startingMoney: 100);
        var system = new BuildSystem(world);

        var result = system.Build("wind_turbine", new GridPosition(1, 1));

        Assert.True(result.Success);
        Assert.Equal(99, world.Resources.Money);
    }

    [Fact]
    public void Build_OnForest_ShouldFail()
    {
        var world = CreateWorld(startingMoney: 100);
        world.Map.GetTile(new GridPosition(1, 1)).SetType(TileType.Forest);
        var system = new BuildSystem(world);

        var result = system.Build("wind_turbine", new GridPosition(1, 1));

        Assert.False(result.Success);
        Assert.Equal(BuildFailureReason.TileNotBuildable, result.FailureReason);
        Assert.False(world.Map.GetTile(new GridPosition(1, 1)).HasBuilding);
        Assert.Empty(world.BuildingInstances);
    }

    [Fact]
    public void Build_OnOccupiedTile_ShouldFail()
    {
        var world = CreateWorld(startingMoney: 100);
        var system = new BuildSystem(world);
        var first = system.Build("wind_turbine", new GridPosition(1, 1));

        var second = system.Build("wind_turbine", new GridPosition(1, 1));

        Assert.True(first.Success);
        Assert.False(second.Success);
        Assert.Equal(BuildFailureReason.TileAlreadyOccupied, second.FailureReason);
        Assert.Single(world.BuildingInstances);
    }

    [Fact]
    public void Build_WhenNotEnoughMoney_ShouldFail()
    {
        var world = CreateWorld(startingMoney: 0);
        var system = new BuildSystem(world);

        var result = system.Build("wind_turbine", new GridPosition(1, 1));

        Assert.False(result.Success);
        Assert.Equal(BuildFailureReason.NotEnoughMoney, result.FailureReason);
        Assert.Equal(0, world.Resources.Money);
        Assert.Empty(world.BuildingInstances);
    }

    [Fact]
    public void BuildBattery_ShouldIncreaseMaxEnergy()
    {
        var world = CreateWorld(startingMoney: 100);
        var system = new BuildSystem(world);

        var result = system.Build("battery_small", new GridPosition(1, 1));

        Assert.True(result.Success);
        Assert.Equal(600, world.Resources.MaxEnergy);
    }

    [Fact]
    public void Build_UnknownBuilding_ShouldFail()
    {
        var world = CreateWorld(startingMoney: 100);
        var system = new BuildSystem(world);

        var result = system.Build("missing", new GridPosition(1, 1));

        Assert.False(result.Success);
        Assert.Equal(BuildFailureReason.UnknownBuilding, result.FailureReason);
    }

    [Fact]
    public void Build_OutOfMap_ShouldFail()
    {
        var world = CreateWorld(startingMoney: 100);
        var system = new BuildSystem(world);

        var result = system.Build("wind_turbine", new GridPosition(-1, 0));

        Assert.False(result.Success);
        Assert.Equal(BuildFailureReason.OutOfMap, result.FailureReason);
    }



    [Fact]
    public void ReplaceExpired_ShouldSpendMoneyAndReactivateBuilding()
    {
        var world = CreateWorld(startingMoney: 100);
        var system = new BuildSystem(world);
        var result = system.Build("wind_turbine", new GridPosition(1, 1));
        Assert.True(result.Success);
        var instance = world.BuildingInstances[result.BuildingId!.Value];
        instance.ReduceLifetime(60);

        var replace = system.ReplaceExpired(instance.Id);

        Assert.True(replace.Success);
        Assert.Equal(BuildingState.Active, instance.State);
        Assert.Equal(60, instance.RemainingLifetimeSeconds);
        Assert.Equal(98, world.Resources.Money);
    }

    [Fact]
    public void ReplaceExpired_WhenBuildingIsActive_ShouldFail()
    {
        var world = CreateWorld(startingMoney: 100);
        var system = new BuildSystem(world);
        var result = system.Build("wind_turbine", new GridPosition(1, 1));
        Assert.True(result.Success);

        var replace = system.ReplaceExpired(result.BuildingId!.Value);

        Assert.False(replace.Success);
        Assert.Equal(BuildFailureReason.BuildingNotExpired, replace.FailureReason);
        Assert.Equal(99, world.Resources.Money);
    }

    [Fact]
    public void ReplaceExpired_WhenNotEnoughMoney_ShouldFail()
    {
        var world = CreateWorld(startingMoney: 1);
        var system = new BuildSystem(world);
        var result = system.Build("wind_turbine", new GridPosition(1, 1));
        Assert.True(result.Success);
        var instance = world.BuildingInstances[result.BuildingId!.Value];
        instance.ReduceLifetime(60);

        var replace = system.ReplaceExpired(instance.Id);

        Assert.False(replace.Success);
        Assert.Equal(BuildFailureReason.NotEnoughMoney, replace.FailureReason);
        Assert.Equal(BuildingState.Expired, instance.State);
    }


    [Fact]
    public void Build_WhenResearchIsRequiredButNotCompleted_ShouldFail()
    {
        var world = CreateWorldWithResearchRequirement(startingMoney: 100);
        var system = new BuildSystem(world);

        var result = system.Build("battery_small", new GridPosition(1, 1));

        Assert.False(result.Success);
        Assert.Equal(BuildFailureReason.ResearchRequired, result.FailureReason);
        Assert.False(world.Map.GetTile(new GridPosition(1, 1)).HasBuilding);
    }

    [Fact]
    public void Build_WhenRequiredResearchIsCompleted_ShouldSucceed()
    {
        var world = CreateWorldWithResearchRequirement(startingMoney: 100);
        world.Research.Complete("battery");
        var system = new BuildSystem(world);

        var result = system.Build("battery_small", new GridPosition(1, 1));

        Assert.True(result.Success);
        Assert.Equal(550, world.Resources.MaxEnergy);
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
                Id = "battery_small",
                Name = "Batteria",
                Category = BuildingCategory.Storage,
                Cost = 50,
                BatteryCapacity = 500
            }
        });

        var economy = new EconomySettings
        {
            StartingMoney = startingMoney,
            StartingMaxEnergy = 100
        };

        return new GameWorld(map, new GameData(catalog, economy));
    }

    private static GameWorld CreateWorldWithResearchRequirement(decimal startingMoney)
    {
        var map = new GridMap(4, 4, TileType.Land);
        var catalog = BuildingCatalog.FromDefinitions(new[]
        {
            new BuildingDefinition
            {
                Id = "battery_small",
                Name = "Batteria",
                Category = BuildingCategory.Storage,
                Cost = 50,
                BatteryCapacity = 450,
                RequiredResearchId = "battery"
            }
        });

        var economy = new EconomySettings
        {
            StartingMoney = startingMoney,
            StartingMaxEnergy = 100
        };

        var research = GridPowerTycoon.Core.Research.ResearchCatalog.FromDefinitions(new[]
        {
            new GridPowerTycoon.Core.Research.ResearchDefinition
            {
                Id = "battery",
                Name = "Batterie",
                Cost = 10
            }
        });

        return new GameWorld(map, new GameData(catalog, economy, research));
    }

}
