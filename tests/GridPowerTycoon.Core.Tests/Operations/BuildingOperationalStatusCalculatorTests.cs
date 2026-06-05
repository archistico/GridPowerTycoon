using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Heat;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Operations;
using GridPowerTycoon.Core.Research;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Tests.Operations;

public sealed class BuildingOperationalStatusCalculatorTests
{
    [Fact]
    public void Calculate_ForActiveProducer_ShouldReturnActive()
    {
        var world = CreateWorld(startingEnergy: 10);
        var build = new BuildSystem(world);
        var result = build.Build("wind_turbine", new GridPosition(1, 1));
        Assert.True(result.Success);

        var status = BuildingOperationalStatusCalculator.Calculate(world, world.BuildingInstances[result.BuildingId!.Value]);

        Assert.Equal(BuildingOperationalState.Active, status.State);
        Assert.Equal(1, status.EnergyOutputPerSecond);
    }

    [Fact]
    public void Calculate_ForConsumerWithoutEnergy_ShouldReturnNoEnergyAndZeroOutputs()
    {
        var world = CreateWorld(startingEnergy: 0);
        var build = new BuildSystem(world);
        var result = build.Build("research_small", new GridPosition(1, 1));
        Assert.True(result.Success);

        var status = BuildingOperationalStatusCalculator.Calculate(world, world.BuildingInstances[result.BuildingId!.Value]);

        Assert.Equal(BuildingOperationalState.NoEnergy, status.State);
        Assert.Equal(0, status.ResearchOutputPerSecond);
        Assert.Equal(0.5, status.EnergyInputPerSecond);
    }

    [Fact]
    public void Calculate_ForHeatProducerWithoutGenerator_ShouldReturnNoHeatConversion()
    {
        var world = CreateWorld(startingEnergy: 10);
        var build = new BuildSystem(world);
        var result = build.Build("solar_panel", new GridPosition(1, 1));
        Assert.True(result.Success);

        var status = BuildingOperationalStatusCalculator.Calculate(world, world.BuildingInstances[result.BuildingId!.Value]);

        Assert.Equal(BuildingOperationalState.NoHeatConversion, status.State);
        Assert.False(status.HasHeatConverterInRange);
    }

    [Fact]
    public void Calculate_ForHeatProducerWithHighHeat_ShouldReturnHeatWarningWhenConverterExists()
    {
        var world = CreateWorld(startingEnergy: 10);
        var build = new BuildSystem(world);
        var solarResult = build.Build("solar_panel", new GridPosition(1, 1));
        Assert.True(solarResult.Success);
        Assert.True(build.Build("generator_small", new GridPosition(2, 1)).Success);
        var solar = world.BuildingInstances[solarResult.BuildingId!.Value];
        solar.AddHeat(20);

        var status = BuildingOperationalStatusCalculator.Calculate(world, solar);

        Assert.Equal(BuildingOperationalState.HeatWarning, status.State);
        Assert.True(status.HasHeatConverterInRange);
    }

    private static GameWorld CreateWorld(double startingEnergy)
    {
        var map = new GridMap(5, 5, TileType.Land);
        var catalog = BuildingCatalog.FromDefinitions(new[]
        {
            new BuildingDefinition
            {
                Id = "wind_turbine",
                Name = "Pala eolica",
                Category = BuildingCategory.PowerProducer,
                Cost = 1,
                EnergyPerSecond = 1,
                LifetimeSeconds = 100
            },
            new BuildingDefinition
            {
                Id = "research_small",
                Name = "Centro ricerca piccolo",
                Category = BuildingCategory.Research,
                Cost = 1,
                ResearchPerSecond = 1.25,
                EnergyConsumptionPerSecond = 0.5
            },
            new BuildingDefinition
            {
                Id = "solar_panel",
                Name = "Pannello solare",
                Category = BuildingCategory.HeatProducer,
                Cost = 1,
                HeatPerSecond = 10,
                LifetimeSeconds = 100
            },
            new BuildingDefinition
            {
                Id = "generator_small",
                Name = "Generatore piccolo",
                Category = BuildingCategory.HeatConverter,
                Cost = 1,
                HeatConversionPerSecond = 20,
                HeatRange = 1
            }
        });

        var economy = new EconomySettings
        {
            StartingMoney = 100,
            StartingEnergy = startingEnergy,
            StartingMaxEnergy = 100,
            EnergySellValue = 1,
            ManualSellMultiplier = 1,
            AutoSellMultiplier = 1
        };

        var heat = new HeatSettings
        {
            HeatWarningThreshold = 15,
            HeatExplosionThreshold = 100,
            HeatEnergyConversionRate = 1
        };

        return new GameWorld(map, new GameData(catalog, economy, ResearchCatalog.Empty, heat));
    }
}
