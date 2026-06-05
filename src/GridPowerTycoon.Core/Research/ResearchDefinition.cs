namespace GridPowerTycoon.Core.Research;

public sealed class ResearchDefinition
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public double Cost { get; init; }
    public List<string> UnlockBuildingIds { get; init; } = new();
    public List<string> ManagedBuildingIds { get; init; } = new();
    public List<string> RequiredResearchIds { get; init; } = new();
}
