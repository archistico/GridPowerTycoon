namespace GridPowerTycoon.Core.Buildings;

public sealed class BuildingCatalog
{
    private readonly Dictionary<string, BuildingDefinition> _definitions;

    private BuildingCatalog(IEnumerable<BuildingDefinition> definitions)
    {
        _definitions = definitions.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<BuildingDefinition> All => _definitions.Values;

    public static BuildingCatalog FromDefinitions(IEnumerable<BuildingDefinition> definitions)
    {
        var list = definitions.ToList();
        ValidateDefinitions(list);
        return new BuildingCatalog(list);
    }

    public bool TryGet(string id, out BuildingDefinition definition)
    {
        return _definitions.TryGetValue(id, out definition!);
    }

    public BuildingDefinition GetRequired(string id)
    {
        if (!_definitions.TryGetValue(id, out var definition))
            throw new InvalidOperationException($"Unknown building definition '{id}'.");

        return definition;
    }

    private static void ValidateDefinitions(IReadOnlyCollection<BuildingDefinition> definitions)
    {
        if (definitions.Count == 0)
            throw new InvalidOperationException("Building catalog cannot be empty.");

        var duplicateIds = definitions
            .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateIds.Count > 0)
            throw new InvalidOperationException("Duplicate building ids: " + string.Join(", ", duplicateIds));

        foreach (var definition in definitions)
        {
            if (string.IsNullOrWhiteSpace(definition.Id))
                throw new InvalidOperationException("Building id cannot be empty.");

            if (string.IsNullOrWhiteSpace(definition.Name))
                throw new InvalidOperationException($"Building '{definition.Id}' has empty name.");

            if (definition.Cost < 0)
                throw new InvalidOperationException($"Building '{definition.Id}' has negative cost.");

            if (definition.Width <= 0 || definition.Height <= 0)
                throw new InvalidOperationException($"Building '{definition.Id}' has invalid size.");

            if (definition.EnergyPerSecond < 0 || definition.HeatPerSecond < 0 || definition.ResearchPerSecond < 0)
                throw new InvalidOperationException($"Building '{definition.Id}' has negative production values.");

            if (definition.EnergyConsumptionPerSecond < 0)
                throw new InvalidOperationException($"Building '{definition.Id}' has negative energy consumption.");

            if (definition.BatteryCapacity < 0 || definition.AutoSellPerSecond < 0)
                throw new InvalidOperationException($"Building '{definition.Id}' has negative capacity or autosell values.");

            if (definition.HeatConversionPerSecond < 0 || definition.HeatRange < 0)
                throw new InvalidOperationException($"Building '{definition.Id}' has invalid heat conversion values.");

            if (definition.HeatDissipationPerSecond < 0)
                throw new InvalidOperationException($"Building '{definition.Id}' has negative heat dissipation.");

            if (definition.EnergyEfficiencyBonus < 0)
                throw new InvalidOperationException($"Building '{definition.Id}' has negative energy efficiency bonus.");

            if (definition.MaintenanceEfficiencyBonus < 0)
                throw new InvalidOperationException($"Building '{definition.Id}' has negative maintenance efficiency bonus.");

            if (definition.ToolCapacityBonus < 0)
                throw new InvalidOperationException($"Building '{definition.Id}' has negative tool capacity bonus.");

            if (definition.LifetimeSeconds < 0)
                throw new InvalidOperationException($"Building '{definition.Id}' has negative lifetime.");
        }
    }
}
