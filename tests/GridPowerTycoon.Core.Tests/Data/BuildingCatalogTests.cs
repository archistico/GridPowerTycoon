using GridPowerTycoon.Core.Buildings;

namespace GridPowerTycoon.Core.Tests.Data;

public sealed class BuildingCatalogTests
{
    [Fact]
    public void FromDefinitions_ShouldRejectDuplicateIds()
    {
        var definitions = new[]
        {
            new BuildingDefinition { Id = "wind_turbine", Name = "Pala eolica", Cost = 1 },
            new BuildingDefinition { Id = "wind_turbine", Name = "Duplicato", Cost = 2 }
        };

        Assert.Throws<InvalidOperationException>(() => BuildingCatalog.FromDefinitions(definitions));
    }

    [Fact]
    public void FromDefinitions_ShouldRejectNegativeCost()
    {
        var definitions = new[]
        {
            new BuildingDefinition { Id = "wind_turbine", Name = "Pala eolica", Cost = -1 }
        };

        Assert.Throws<InvalidOperationException>(() => BuildingCatalog.FromDefinitions(definitions));
    }

    [Fact]
    public void TryGet_ShouldFindDefinitionByIdIgnoringCase()
    {
        var catalog = BuildingCatalog.FromDefinitions(new[]
        {
            new BuildingDefinition { Id = "wind_turbine", Name = "Pala eolica", Cost = 1 }
        });

        var found = catalog.TryGet("WIND_TURBINE", out var definition);

        Assert.True(found);
        Assert.Equal("wind_turbine", definition.Id);
    }
}
