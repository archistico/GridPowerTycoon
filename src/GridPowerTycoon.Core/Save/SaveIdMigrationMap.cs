namespace GridPowerTycoon.Core.Save;

public sealed class SaveIdMigrationMap
{
    public static SaveIdMigrationMap Empty { get; } = new();

    public SaveIdMigrationMap(
        IReadOnlyDictionary<string, string>? buildingDefinitionIds = null,
        IReadOnlyDictionary<string, string>? researchIds = null,
        IReadOnlyDictionary<string, string>? upgradeIds = null)
    {
        BuildingDefinitionIds = Normalize(buildingDefinitionIds);
        ResearchIds = Normalize(researchIds);
        UpgradeIds = Normalize(upgradeIds);
    }

    public IReadOnlyDictionary<string, string> BuildingDefinitionIds { get; }
    public IReadOnlyDictionary<string, string> ResearchIds { get; }
    public IReadOnlyDictionary<string, string> UpgradeIds { get; }

    public string ResolveBuildingDefinitionId(string id)
    {
        return Resolve(BuildingDefinitionIds, id);
    }

    public string ResolveResearchId(string id)
    {
        return Resolve(ResearchIds, id);
    }

    public string ResolveUpgradeId(string id)
    {
        return Resolve(UpgradeIds, id);
    }

    private static IReadOnlyDictionary<string, string> Normalize(IReadOnlyDictionary<string, string>? mappings)
    {
        if (mappings is null || mappings.Count == 0)
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var mapping in mappings)
        {
            if (string.IsNullOrWhiteSpace(mapping.Key))
                throw new InvalidOperationException("Save id migration contains an empty source id.");

            if (string.IsNullOrWhiteSpace(mapping.Value))
                throw new InvalidOperationException($"Save id migration for '{mapping.Key}' contains an empty target id.");

            normalized.Add(mapping.Key, mapping.Value);
        }

        return normalized;
    }

    private static string Resolve(IReadOnlyDictionary<string, string> mappings, string id)
    {
        return mappings.TryGetValue(id, out var replacementId)
            ? replacementId
            : id;
    }
}
