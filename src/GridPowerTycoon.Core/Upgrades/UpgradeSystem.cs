using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Upgrades;

public sealed class UpgradeSystem
{
    private readonly GameWorld _world;

    public UpgradeSystem(GameWorld world)
    {
        _world = world;
    }

    public UpgradeFailureReason CanPurchase(string upgradeId)
    {
        if (!_world.UpgradeCatalog.TryGet(upgradeId, out var definition))
            return UpgradeFailureReason.UnknownUpgrade;

        if (_world.Upgrades.GetLevel(upgradeId) >= definition.MaxLevel)
            return UpgradeFailureReason.MaxLevelReached;

        if (!string.IsNullOrWhiteSpace(definition.RequiredResearchId) &&
            !_world.Research.IsCompleted(definition.RequiredResearchId))
        {
            return UpgradeFailureReason.MissingResearch;
        }

        if (_world.Resources.Money < definition.CostMoney)
            return UpgradeFailureReason.NotEnoughMoney;

        if (_world.Resources.Research < definition.CostResearch)
            return UpgradeFailureReason.NotEnoughResearch;

        return UpgradeFailureReason.None;
    }

    public UpgradeResult Purchase(string upgradeId)
    {
        var validation = CanPurchase(upgradeId);
        if (validation != UpgradeFailureReason.None)
            return UpgradeResult.Fail(validation);

        var definition = _world.UpgradeCatalog.GetRequired(upgradeId);

        if (!_world.Resources.TrySpendMoney(definition.CostMoney))
            return UpgradeResult.Fail(UpgradeFailureReason.NotEnoughMoney);

        if (!_world.Resources.TrySpendResearch(definition.CostResearch))
            return UpgradeResult.Fail(UpgradeFailureReason.NotEnoughResearch);

        _world.Upgrades.IncreaseLevel(upgradeId);

        if (definition.EffectType == UpgradeEffectType.MultiplyBatteryCapacity)
            RecalculateMaxEnergy();

        return UpgradeResult.Ok(upgradeId);
    }

    private void RecalculateMaxEnergy()
    {
        var newMaxEnergy = _world.EconomySettings.StartingMaxEnergy;

        foreach (var instance in _world.BuildingInstances.Values)
        {
            if (!_world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            if (definition.BatteryCapacity <= 0)
                continue;

            newMaxEnergy += UpgradeCalculator.GetBatteryCapacity(_world, definition);
        }

        _world.Resources.SetMaxEnergy(newMaxEnergy);
    }
}
