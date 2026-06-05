namespace GridPowerTycoon.Core.Research;

public sealed class ResearchCatalog
{
    private readonly Dictionary<string, ResearchDefinition> _definitions;

    private ResearchCatalog(IEnumerable<ResearchDefinition> definitions)
    {
        _definitions = definitions.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<ResearchDefinition> All => _definitions.Values;

    public static ResearchCatalog Empty { get; } = new(Array.Empty<ResearchDefinition>());

    public static ResearchCatalog FromDefinitions(IEnumerable<ResearchDefinition> definitions)
    {
        var list = definitions.ToList();
        ValidateDefinitions(list);
        return new ResearchCatalog(list);
    }

    public bool TryGet(string id, out ResearchDefinition definition)
    {
        return _definitions.TryGetValue(id, out definition!);
    }

    public ResearchDefinition GetRequired(string id)
    {
        if (!_definitions.TryGetValue(id, out var definition))
            throw new InvalidOperationException($"Unknown research definition '{id}'.");

        return definition;
    }

    private static void ValidateDefinitions(IReadOnlyCollection<ResearchDefinition> definitions)
    {
        var duplicateIds = definitions
            .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateIds.Count > 0)
            throw new InvalidOperationException("Duplicate research ids: " + string.Join(", ", duplicateIds));

        foreach (var definition in definitions)
        {
            if (string.IsNullOrWhiteSpace(definition.Id))
                throw new InvalidOperationException("Research id cannot be empty.");

            if (string.IsNullOrWhiteSpace(definition.Name))
                throw new InvalidOperationException($"Research '{definition.Id}' has empty name.");

            if (definition.Cost < 0)
                throw new InvalidOperationException($"Research '{definition.Id}' has negative cost.");

            if (definition.RequiredResearchIds.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException($"Research '{definition.Id}' has an empty required research id.");

            if (definition.UnlockBuildingIds.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException($"Research '{definition.Id}' has an empty unlock building id.");

            if (definition.ManagedBuildingIds.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException($"Research '{definition.Id}' has an empty managed building id.");
        }
    }
}
