using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Map;

namespace GridPowerTycoon.Core.World;

public sealed class GameWorld
{
    private readonly Dictionary<Guid, BuildingInstance> _buildingInstances = new();

    public GridMap Map { get; }
    public ResourceState Resources { get; }
    public BuildingCatalog BuildingCatalog { get; }
    public EconomySettings EconomySettings { get; }

    public IReadOnlyDictionary<Guid, BuildingInstance> BuildingInstances => _buildingInstances;

    public GameWorld(GridMap map, GameData data)
    {
        Map = map;
        BuildingCatalog = data.Buildings;
        EconomySettings = data.Economy;
        Resources = new ResourceState(data.Economy);
    }

    public void AddBuilding(BuildingInstance instance)
    {
        if (_buildingInstances.ContainsKey(instance.Id))
            throw new InvalidOperationException($"Building instance '{instance.Id}' already exists.");

        _buildingInstances.Add(instance.Id, instance);
    }

    public bool TryGetBuilding(Guid id, out BuildingInstance instance)
    {
        return _buildingInstances.TryGetValue(id, out instance!);
    }
}
