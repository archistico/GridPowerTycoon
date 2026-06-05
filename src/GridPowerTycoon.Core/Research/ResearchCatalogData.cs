namespace GridPowerTycoon.Core.Research;

public sealed class ResearchCatalogData
{
    public int Version { get; init; } = 1;
    public List<ResearchDefinition> Researches { get; init; } = new();
}
