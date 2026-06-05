using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Research;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Tests.Research;

public sealed class ResearchSystemTests
{
    [Fact]
    public void CompleteResearch_WhenEnoughResearch_ShouldSucceedAndSpendResearch()
    {
        var world = CreateWorld(startingResearch: 100);
        var system = new ResearchSystem(world);

        var result = system.Complete("battery");

        Assert.True(result.Success);
        Assert.True(world.Research.IsCompleted("battery"));
        Assert.Equal(90, world.Resources.Research);
    }

    [Fact]
    public void CompleteResearch_WhenNotEnoughResearch_ShouldFail()
    {
        var world = CreateWorld(startingResearch: 5);
        var system = new ResearchSystem(world);

        var result = system.Complete("battery");

        Assert.False(result.Success);
        Assert.Equal(ResearchFailureReason.NotEnoughResearch, result.FailureReason);
        Assert.False(world.Research.IsCompleted("battery"));
    }

    [Fact]
    public void CompleteResearch_WhenAlreadyCompleted_ShouldFail()
    {
        var world = CreateWorld(startingResearch: 100);
        var system = new ResearchSystem(world);
        Assert.True(system.Complete("battery").Success);

        var second = system.Complete("battery");

        Assert.False(second.Success);
        Assert.Equal(ResearchFailureReason.AlreadyCompleted, second.FailureReason);
    }

    [Fact]
    public void CompleteResearch_WhenMissingPrerequisite_ShouldFail()
    {
        var world = CreateWorld(startingResearch: 100);
        var system = new ResearchSystem(world);

        var result = system.Complete("generator_small");

        Assert.False(result.Success);
        Assert.Equal(ResearchFailureReason.MissingPrerequisite, result.FailureReason);
    }

    [Fact]
    public void CompleteResearch_WhenPrerequisiteCompleted_ShouldSucceed()
    {
        var world = CreateWorld(startingResearch: 100);
        var system = new ResearchSystem(world);
        Assert.True(system.Complete("solar_power").Success);

        var result = system.Complete("generator_small");

        Assert.True(result.Success);
        Assert.True(world.Research.IsCompleted("generator_small"));
    }

    private static GameWorld CreateWorld(double startingResearch)
    {
        var map = new GridMap(4, 4, TileType.Land);
        var buildings = BuildingCatalog.FromDefinitions(new[]
        {
            new BuildingDefinition
            {
                Id = "wind_turbine",
                Name = "Pala eolica",
                Category = BuildingCategory.PowerProducer,
                Cost = 1
            }
        });

        var economy = new EconomySettings
        {
            StartingMoney = 100,
            StartingMaxEnergy = 100,
            StartingResearch = startingResearch
        };

        var research = ResearchCatalog.FromDefinitions(new[]
        {
            new ResearchDefinition
            {
                Id = "battery",
                Name = "Batterie",
                Cost = 10
            },
            new ResearchDefinition
            {
                Id = "solar_power",
                Name = "Energia solare",
                Cost = 10
            },
            new ResearchDefinition
            {
                Id = "generator_small",
                Name = "Generatori base",
                Cost = 10,
                RequiredResearchIds = new List<string> { "solar_power" }
            }
        });

        return new GameWorld(map, new GameData(buildings, economy, research));
    }
}
