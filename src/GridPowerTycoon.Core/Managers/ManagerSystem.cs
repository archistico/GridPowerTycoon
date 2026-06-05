using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Upgrades;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Managers;

public sealed class ManagerSystem
{
    private readonly GameWorld _world;

    public ManagerSystem(GameWorld world)
    {
        _world = world;
    }

    public ManagerRenewalResult Update()
    {
        var renewed = 0;
        var notEnoughMoney = 0;
        var moneySpent = 0m;

        foreach (var instance in _world.BuildingInstances.Values)
        {
            if (instance.State != BuildingState.Expired)
                continue;

            if (!IsManaged(_world, instance.DefinitionId))
                continue;

            if (!_world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            if (!_world.Resources.TrySpendMoney(definition.Cost))
            {
                notEnoughMoney++;
                continue;
            }

            instance.Replace(UpgradeCalculator.GetLifetimeSeconds(_world, definition));
            renewed++;
            moneySpent += definition.Cost;
        }

        if (renewed == 0 && notEnoughMoney == 0)
            return ManagerRenewalResult.None;

        return new ManagerRenewalResult(renewed, notEnoughMoney, moneySpent);
    }

    public static bool IsManaged(GameWorld world, string buildingDefinitionId)
    {
        if (string.IsNullOrWhiteSpace(buildingDefinitionId))
            return false;

        foreach (var researchId in world.Research.CompletedResearchIds)
        {
            if (!world.ResearchCatalog.TryGet(researchId, out var research))
                continue;

            if (research.ManagedBuildingIds.Any(id => string.Equals(id, buildingDefinitionId, StringComparison.OrdinalIgnoreCase)))
                return true;
        }

        return false;
    }
}
