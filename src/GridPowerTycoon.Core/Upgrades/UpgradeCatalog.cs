namespace GridPowerTycoon.Core.Upgrades;

public sealed class UpgradeCatalog
{
    private readonly Dictionary<string, UpgradeDefinition> _definitions;

    public static UpgradeCatalog Empty { get; } = new(Array.Empty<UpgradeDefinition>());

    private UpgradeCatalog(IEnumerable<UpgradeDefinition> definitions)
    {
        _definitions = definitions.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<UpgradeDefinition> All => _definitions.Values;

    public static UpgradeCatalog FromDefinitions(IEnumerable<UpgradeDefinition> definitions)
    {
        var list = definitions.ToList();
        ValidateDefinitions(list);
        return new UpgradeCatalog(list);
    }

    public bool TryGet(string id, out UpgradeDefinition definition)
    {
        return _definitions.TryGetValue(id, out definition!);
    }

    public UpgradeDefinition GetRequired(string id)
    {
        if (!_definitions.TryGetValue(id, out var definition))
            throw new InvalidOperationException($"Unknown upgrade definition '{id}'.");

        return definition;
    }

    private static void ValidateDefinitions(IReadOnlyCollection<UpgradeDefinition> definitions)
    {
        var duplicateIds = definitions
            .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateIds.Count > 0)
            throw new InvalidOperationException("Duplicate upgrade ids: " + string.Join(", ", duplicateIds));

        foreach (var definition in definitions)
        {
            if (string.IsNullOrWhiteSpace(definition.Id))
                throw new InvalidOperationException("Upgrade id cannot be empty.");

            if (string.IsNullOrWhiteSpace(definition.Name))
                throw new InvalidOperationException($"Upgrade '{definition.Id}' has empty name.");

            if (definition.CostMoney < 0)
                throw new InvalidOperationException($"Upgrade '{definition.Id}' has negative money cost.");

            if (definition.CostResearch < 0)
                throw new InvalidOperationException($"Upgrade '{definition.Id}' has negative research cost.");

            if (definition.CostGrowthMultiplier < 1)
                throw new InvalidOperationException($"Upgrade '{definition.Id}' must have costGrowthMultiplier greater than or equal to 1.");

            if (definition.Multiplier <= 0)
                throw new InvalidOperationException($"Upgrade '{definition.Id}' must have a positive multiplier.");

            if (definition.MaxLevel <= 0)
                throw new InvalidOperationException($"Upgrade '{definition.Id}' must have maxLevel greater than zero.");
        }
    }
}
