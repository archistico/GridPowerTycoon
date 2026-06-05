namespace GridPowerTycoon.Core.Buildings;

public sealed class BuildingCatalogData
{
    public int Version { get; init; } = 1;
    public List<BuildingDefinition> Buildings { get; init; } = new();
}
