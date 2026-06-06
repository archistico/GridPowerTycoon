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

        var currentLevel = _world.Upgrades.GetLevel(upgradeId);
        if (currentLevel >= definition.MaxLevel)
            return UpgradeFailureReason.MaxLevelReached;

        if (!string.IsNullOrWhiteSpace(definition.RequiredResearchId) &&
            !_world.Research.IsCompleted(definition.RequiredResearchId))
        {
            return UpgradeFailureReason.MissingResearch;
        }

        if (_world.Resources.Money < GetMoneyCost(definition, currentLevel))
            return UpgradeFailureReason.NotEnoughMoney;

        if (_world.Resources.Research < GetResearchCost(definition, currentLevel))
            return UpgradeFailureReason.NotEnoughResearch;

        return UpgradeFailureReason.None;
    }

    public UpgradeResult Purchase(string upgradeId)
    {
        var validation = CanPurchase(upgradeId);
        if (validation != UpgradeFailureReason.None)
            return UpgradeResult.Fail(validation, upgradeId);

        var definition = _world.UpgradeCatalog.GetRequired(upgradeId);
        var currentLevel = _world.Upgrades.GetLevel(upgradeId);
        var moneyCost = GetMoneyCost(definition, currentLevel);
        var researchCost = GetResearchCost(definition, currentLevel);

        if (!_world.Resources.TrySpendMoney(moneyCost))
            return UpgradeResult.Fail(UpgradeFailureReason.NotEnoughMoney, upgradeId);

        if (!_world.Resources.TrySpendResearch(researchCost))
            return UpgradeResult.Fail(UpgradeFailureReason.NotEnoughResearch, upgradeId);

        _world.Upgrades.SetLevel(upgradeId, currentLevel + 1);

        if (definition.EffectType == UpgradeEffectType.MultiplyBatteryCapacity)
            RecalculateMaxEnergy();

        return UpgradeResult.Ok(upgradeId, currentLevel + 1);
    }

    public static decimal GetMoneyCost(UpgradeDefinition definition, int currentLevel)
    {
        if (currentLevel <= 0)
            return definition.CostMoney;

        return RoundCost(definition.CostMoney * (decimal)Math.Pow(definition.CostGrowthMultiplier, currentLevel));
    }

    public static double GetResearchCost(UpgradeDefinition definition, int currentLevel)
    {
        if (currentLevel <= 0)
            return definition.CostResearch;

        return Math.Round(definition.CostResearch * Math.Pow(definition.CostGrowthMultiplier, currentLevel), 2);
    }

    private static decimal RoundCost(decimal value)
    {
        if (value < 1000m)
            return Math.Ceiling(value);

        if (value < 1000000m)
            return Math.Ceiling(value / 10m) * 10m;

        return Math.Ceiling(value / 1000m) * 1000m;
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
