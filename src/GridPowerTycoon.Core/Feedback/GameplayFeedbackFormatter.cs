using System.Globalization;
using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Operations;
using GridPowerTycoon.Core.Research;
using GridPowerTycoon.Core.Upgrades;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Feedback;

public sealed class GameplayFeedbackFormatter
{
    private readonly GameWorld _world;

    public GameplayFeedbackFormatter(GameWorld world)
    {
        _world = world;
    }

    public string FormatBuildFailure(BuildFailureReason reason, string? buildingDefinitionId, GridPosition? position = null)
    {
        var definition = TryGetBuilding(buildingDefinitionId);
        var name = GetBuildingName(definition, buildingDefinitionId);

        return reason switch
        {
            BuildFailureReason.NotEnoughMoney => definition is null
                ? "BUILD FAILED: NEED MORE MONEY"
                : $"BUILD FAILED: {name} COSTS {FormatMoney(definition.Cost)} - HAVE {FormatMoney(_world.Resources.Money)} - NEED {FormatMoney(GetMissingMoney(definition.Cost))}",

            BuildFailureReason.ResearchRequired => definition is null
                ? "BUILD LOCKED: RESEARCH REQUIRED"
                : $"BUILD LOCKED: {name} REQUIRES {GetResearchName(definition.RequiredResearchId)}",

            BuildFailureReason.TileAlreadyOccupied => position.HasValue
                ? $"BUILD FAILED: CELL {FormatPosition(position.Value)} OCCUPIED - CHOOSE FREE LAND"
                : "BUILD FAILED: CELL OCCUPIED - CHOOSE FREE LAND",

            BuildFailureReason.TileNotBuildable => position.HasValue
                ? $"BUILD FAILED: CELL {FormatPosition(position.Value)} IS NOT LAND"
                : "BUILD FAILED: NEED LAND TILE",

            BuildFailureReason.OutOfMap => position.HasValue
                ? $"BUILD FAILED: CELL {FormatPosition(position.Value)} OUTSIDE MAP"
                : "BUILD FAILED: OUTSIDE MAP",

            BuildFailureReason.InvalidBuildingSize => definition is null
                ? "BUILD FAILED: INVALID BUILDING SIZE"
                : $"BUILD FAILED: {name} NEEDS {definition.Width}X{definition.Height} FREE CELLS",

            BuildFailureReason.UnknownBuilding => string.IsNullOrWhiteSpace(buildingDefinitionId)
                ? "BUILD FAILED: UNKNOWN BUILDING"
                : $"BUILD FAILED: UNKNOWN BUILDING {buildingDefinitionId}",

            BuildFailureReason.BuildingNotFound => "ACTION FAILED: BUILDING NOT FOUND",
            BuildFailureReason.BuildingNotExpired => "REPLACE FAILED: BUILDING IS STILL ACTIVE",
            _ => $"BUILD FAILED: {reason}"
        };
    }

    public string FormatResearchFailure(ResearchResult result)
    {
        var researchId = result.ResearchId;
        var definition = TryGetResearch(researchId);
        var name = GetResearchName(researchId);

        return result.FailureReason switch
        {
            ResearchFailureReason.NotEnoughResearch => definition is null
                ? "RESEARCH FAILED: NEED MORE RESEARCH POINTS"
                : $"RESEARCH FAILED: {name} COSTS {FormatNumber(definition.Cost)} RP - HAVE {FormatNumber(_world.Resources.Research)} - NEED {FormatNumber(GetMissingResearch(definition.Cost))}",

            ResearchFailureReason.MissingPrerequisite => definition is null
                ? "RESEARCH LOCKED: MISSING PREREQUISITE"
                : $"RESEARCH LOCKED: COMPLETE {FormatMissingResearch(definition.RequiredResearchIds)} FIRST",

            ResearchFailureReason.AlreadyCompleted => $"RESEARCH ALREADY COMPLETED: {name}",
            ResearchFailureReason.UnknownResearch => string.IsNullOrWhiteSpace(researchId)
                ? "RESEARCH FAILED: UNKNOWN RESEARCH"
                : $"RESEARCH FAILED: UNKNOWN RESEARCH {researchId}",
            _ => $"RESEARCH FAILED: {result.FailureReason}"
        };
    }

    public string FormatUpgradeFailure(UpgradeResult result)
    {
        var upgradeId = result.UpgradeId;
        var definition = TryGetUpgrade(upgradeId);
        var name = GetUpgradeName(definition, upgradeId);
        var currentLevel = string.IsNullOrWhiteSpace(upgradeId) ? 0 : _world.Upgrades.GetLevel(upgradeId);

        return result.FailureReason switch
        {
            UpgradeFailureReason.NotEnoughMoney => definition is null
                ? "UPGRADE FAILED: NEED MORE MONEY"
                : $"UPGRADE FAILED: {name} LV {currentLevel + 1} COSTS {FormatMoney(UpgradeSystem.GetMoneyCost(definition, currentLevel))} - HAVE {FormatMoney(_world.Resources.Money)}",

            UpgradeFailureReason.NotEnoughResearch => definition is null
                ? "UPGRADE FAILED: NEED MORE RESEARCH POINTS"
                : $"UPGRADE FAILED: {name} LV {currentLevel + 1} COSTS {FormatNumber(UpgradeSystem.GetResearchCost(definition, currentLevel))} RP - HAVE {FormatNumber(_world.Resources.Research)}",

            UpgradeFailureReason.MissingResearch => definition is null
                ? "UPGRADE LOCKED: RESEARCH REQUIRED"
                : $"UPGRADE LOCKED: {name} REQUIRES {GetResearchName(definition.RequiredResearchId)}",

            UpgradeFailureReason.MaxLevelReached => definition is null
                ? "UPGRADE FAILED: MAX LEVEL REACHED"
                : $"UPGRADE MAXED: {name} IS LV {currentLevel}/{definition.MaxLevel}",

            UpgradeFailureReason.UnknownUpgrade => string.IsNullOrWhiteSpace(upgradeId)
                ? "UPGRADE FAILED: UNKNOWN UPGRADE"
                : $"UPGRADE FAILED: UNKNOWN UPGRADE {upgradeId}",
            _ => $"UPGRADE FAILED: {result.FailureReason}"
        };
    }

    public string FormatBuildAvailabilityLine(string? buildingDefinitionId)
    {
        var definition = TryGetBuilding(buildingDefinitionId);
        if (definition is null)
            return string.IsNullOrWhiteSpace(buildingDefinitionId)
                ? "UNKNOWN BUILDING"
                : $"UNKNOWN BUILDING: {buildingDefinitionId.ToUpperInvariant()}";

        if (!IsResearchUnlocked(definition.RequiredResearchId))
            return $"LOCKED: COMPLETE {GetResearchName(definition.RequiredResearchId)}";

        if (_world.Resources.Money < definition.Cost)
            return $"NEED MONEY: COST {FormatMoney(definition.Cost)} - HAVE {FormatMoney(_world.Resources.Money)} - NEED {FormatMoney(GetMissingMoney(definition.Cost))}";

        return $"READY: COST {FormatMoney(definition.Cost)}";
    }

    public string FormatResearchAvailabilityLine(string? researchId)
    {
        var definition = TryGetResearch(researchId);
        if (definition is null)
            return string.IsNullOrWhiteSpace(researchId)
                ? "UNKNOWN RESEARCH"
                : $"UNKNOWN RESEARCH: {researchId.ToUpperInvariant()}";

        if (_world.Research.IsCompleted(definition.Id))
            return "DONE: RESEARCH COMPLETED";

        var missingPrerequisites = GetMissingResearchNames(definition.RequiredResearchIds);
        if (missingPrerequisites.Count > 0)
            return "LOCKED: COMPLETE " + FormatNameList(missingPrerequisites);

        if (_world.Resources.Research < definition.Cost)
            return $"NEED RESEARCH: COST {FormatNumber(definition.Cost)} RP - HAVE {FormatNumber(_world.Resources.Research)} - NEED {FormatNumber(GetMissingResearch(definition.Cost))}";

        return $"READY: COST {FormatNumber(definition.Cost)} RP";
    }

    public string FormatUpgradeAvailabilityLine(string? upgradeId)
    {
        var definition = TryGetUpgrade(upgradeId);
        if (definition is null)
            return string.IsNullOrWhiteSpace(upgradeId)
                ? "UNKNOWN UPGRADE"
                : $"UNKNOWN UPGRADE: {upgradeId.ToUpperInvariant()}";

        var level = _world.Upgrades.GetLevel(definition.Id);
        if (level >= definition.MaxLevel)
            return $"MAX LEVEL: {level}/{definition.MaxLevel}";

        if (!string.IsNullOrWhiteSpace(definition.RequiredResearchId) && !_world.Research.IsCompleted(definition.RequiredResearchId))
            return $"LOCKED: COMPLETE {GetResearchName(definition.RequiredResearchId)}";

        var moneyCost = UpgradeSystem.GetMoneyCost(definition, level);
        var researchCost = UpgradeSystem.GetResearchCost(definition, level);
        var missingMoney = Math.Max(0m, moneyCost - _world.Resources.Money);
        var missingResearch = Math.Max(0d, researchCost - _world.Resources.Research);

        if (missingMoney <= 0m && missingResearch <= 0d)
            return $"READY: {FormatUpgradeCost(moneyCost, researchCost)}";

        return FormatUpgradeShortage(moneyCost, researchCost, missingMoney, missingResearch);
    }




    public string? FormatCriticalWarning()
    {
        return FormatCriticalWarnings().FirstOrDefault();
    }

    public IReadOnlyList<string> FormatCriticalWarnings()
    {
        var warnings = new List<string>();
        var rates = ResourceRateSnapshot.Calculate(_world);
        var activeEnergyConsumers = CountActiveBuildings(definition => definition.EnergyConsumptionPerSecond > 0);
        var expiredCount = _world.BuildingInstances.Values.Count(instance => instance.State == BuildingState.Expired);
        var explodedCount = _world.BuildingInstances.Values.Count(instance => instance.State == BuildingState.Exploded);
        var lowLifetimeCount = CountLowLifetimeBuildings();
        var heatWarningCount = CountBuildingsInState(BuildingOperationalState.HeatWarning);
        var noHeatCoverageCount = CountBuildingsInState(BuildingOperationalState.NoHeatConversion);

        if (explodedCount > 0)
            warnings.Add($"CRITICAL: {explodedCount} BUILDING(S) EXPLODED - REPLACE OR DEMOLISH");

        if (heatWarningCount > 0)
            warnings.Add($"HEAT CRITICAL: {heatWarningCount} BUILDING(S) NEAR EXPLOSION - ADD COOLING/CONVERSION");

        if (_world.Resources.Energy <= 0.0001 && activeEnergyConsumers > 0)
            warnings.Add($"ENERGY CRITICAL: STORAGE EMPTY - {activeEnergyConsumers} CONSUMER(S) MAY STOP");
        else if (_world.Resources.MaxEnergy > 0 && _world.Resources.Energy / _world.Resources.MaxEnergy <= 0.2d && rates.EnergyPerSecond < -0.0001d)
            warnings.Add($"ENERGY WARNING: {FormatNumber(_world.Resources.Energy / _world.Resources.MaxEnergy * 100d)}% STORAGE AND {FormatSignedPerSecond(rates.EnergyPerSecond)}");

        if (noHeatCoverageCount > 0)
            warnings.Add($"HEAT WARNING: {noHeatCoverageCount} PRODUCER(S) NEED CONVERTER OR COOLING");

        if (expiredCount > 0)
            warnings.Add($"MAINTENANCE WARNING: {expiredCount} BUILDING(S) EXPIRED - REPLACE THEM");
        else if (lowLifetimeCount > 0)
            warnings.Add($"MAINTENANCE WARNING: {lowLifetimeCount} BUILDING(S) NEAR END OF LIFE");

        if (_world.Resources.Axes <= 0.0001d && HasClearableTerrain(TileType.Forest))
            warnings.Add("TOOLS WARNING: NO AXES AVAILABLE FOR FORESTS");

        if (_world.Resources.Mines <= 0.0001d && HasClearableTerrain(TileType.Mountain))
            warnings.Add("TOOLS WARNING: NO MINES AVAILABLE FOR MOUNTAINS");

        return warnings;
    }

    public IReadOnlyList<string> FormatProductionSummaryLines()
    {
        var rates = ResourceRateSnapshot.Calculate(_world);
        var heatManagedPerSecond = rates.HeatConvertedEnergyPerSecond / Math.Max(0.0001d, _world.HeatSettings.HeatEnergyConversionRate) +
                                   rates.HeatDissipatedPerSecond;
        var unmanagedHeatPerSecond = Math.Max(0d, rates.HeatProducedPerSecond - heatManagedPerSecond);
        var lifetimeDecayMultiplier = UpgradeCalculator.GetLifetimeDecayMultiplier(_world);
        var activeBuildings = _world.BuildingInstances.Values.Count(instance => instance.IsActive);
        var atRiskBuildings = CountLowLifetimeBuildings() +
                              _world.BuildingInstances.Values.Count(instance => instance.State is BuildingState.Expired or BuildingState.Exploded);

        return new[]
        {
            "GRID SUMMARY",
            $"ENERGY PROD {FormatSignedPerSecond(rates.RawEnergyProductionPerSecond + rates.HeatConvertedEnergyPerSecond)} | USE {FormatSignedPerSecond(-rates.EnergyConsumptionPerSecond)} | NET {FormatSignedPerSecond(rates.EnergyPerSecond)}",
            $"RESEARCH {FormatSignedPerSecond(rates.ResearchPerSecond)} | MONEY {FormatSignedMoneyPerSecond(rates.MoneyPerSecond)}",
            $"HEAT PROD {FormatSignedPerSecond(rates.HeatProducedPerSecond)} | MANAGED {FormatSignedPerSecond(-heatManagedPerSecond)} | FREE {FormatSignedPerSecond(unmanagedHeatPerSecond)}",
            $"MAINTENANCE ACTIVE {activeBuildings} | AT RISK {atRiskBuildings} | WEAR X{FormatNumber(lifetimeDecayMultiplier)}",
            $"TOOLS AXES {FormatNumber(_world.Resources.Axes)}/{FormatNumber(UpgradeCalculator.GetMaxAxes(_world))} {FormatSignedPerSecond(UpgradeCalculator.GetAxesPerSecond(_world))} | MINES {FormatNumber(_world.Resources.Mines)}/{FormatNumber(UpgradeCalculator.GetMaxMines(_world))} {FormatSignedPerSecond(UpgradeCalculator.GetMinesPerSecond(_world))}"
        };
    }

    public IReadOnlyList<string> FormatBuildCardDetails(string? buildingDefinitionId)
    {
        var definition = TryGetBuilding(buildingDefinitionId);
        if (definition is null)
            return new[] { "UNKNOWN BUILDING", string.IsNullOrWhiteSpace(buildingDefinitionId) ? "NO BUILDING ID" : buildingDefinitionId.ToUpperInvariant() };

        var lines = new List<string>
        {
            definition.Name.ToUpperInvariant(),
            FormatBuildAvailabilityLine(definition.Id),
            FormatBuildingEffect(definition),
            $"SIZE {definition.Width}X{definition.Height} | LIFE {FormatNumber(definition.LifetimeSeconds)}S"
        };

        if (!string.IsNullOrWhiteSpace(definition.Description))
            lines.Add(definition.Description.ToUpperInvariant());

        return lines;
    }

    public IReadOnlyList<string> FormatResearchCardDetails(string? researchId)
    {
        var definition = TryGetResearch(researchId);
        if (definition is null)
            return new[] { "UNKNOWN RESEARCH", string.IsNullOrWhiteSpace(researchId) ? "NO RESEARCH ID" : researchId.ToUpperInvariant() };

        var lines = new List<string>
        {
            definition.Name.ToUpperInvariant(),
            FormatResearchAvailabilityLine(definition.Id),
            FormatResearchUnlockSummary(definition)
        };

        if (!string.IsNullOrWhiteSpace(definition.Description))
            lines.Add(definition.Description.ToUpperInvariant());

        return lines;
    }

    public IReadOnlyList<string> FormatUpgradeCardDetails(string? upgradeId)
    {
        var definition = TryGetUpgrade(upgradeId);
        if (definition is null)
            return new[] { "UNKNOWN UPGRADE", string.IsNullOrWhiteSpace(upgradeId) ? "NO UPGRADE ID" : upgradeId.ToUpperInvariant() };

        var level = _world.Upgrades.GetLevel(definition.Id);

        return new[]
        {
            definition.Name.ToUpperInvariant(),
            FormatUpgradeAvailabilityLine(definition.Id),
            $"LEVEL {level}/{definition.MaxLevel} -> {Math.Min(level + 1, definition.MaxLevel)}/{definition.MaxLevel}",
            FormatUpgradeEffectSummary(definition),
            string.IsNullOrWhiteSpace(definition.TargetBuildingId) ? "TARGET: GLOBAL" : $"TARGET: {GetBuildingName(TryGetBuilding(definition.TargetBuildingId), definition.TargetBuildingId)}"
        };
    }


    private int CountActiveBuildings(Func<BuildingDefinition, bool> predicate)
    {
        var count = 0;

        foreach (var instance in _world.BuildingInstances.Values)
        {
            if (!instance.IsActive)
                continue;

            var definition = TryGetBuilding(instance.DefinitionId);
            if (definition is not null && predicate(definition))
                count++;
        }

        return count;
    }

    private int CountBuildingsInState(BuildingOperationalState state)
    {
        var count = 0;

        foreach (var instance in _world.BuildingInstances.Values)
        {
            if (!TryGetBuilding(instance.DefinitionId, out _))
                continue;

            var status = BuildingOperationalStatusCalculator.Calculate(_world, instance);
            if (status.State == state)
                count++;
        }

        return count;
    }

    private int CountLowLifetimeBuildings()
    {
        var count = 0;

        foreach (var instance in _world.BuildingInstances.Values)
        {
            if (!instance.IsActive)
                continue;

            if (!TryGetBuilding(instance.DefinitionId, out var definition))
                continue;

            if (definition.LifetimeSeconds <= 0)
                continue;

            var ratio = instance.RemainingLifetimeSeconds / definition.LifetimeSeconds;
            if (instance.RemainingLifetimeSeconds <= 30d || ratio <= 0.15d)
                count++;
        }

        return count;
    }

    private bool HasClearableTerrain(TileType tileType)
    {
        return _world.Map.Tiles.Any(tile => tile.Type == tileType && !tile.HasBuilding);
    }

    private BuildingDefinition? TryGetBuilding(string? buildingDefinitionId)
    {
        return !string.IsNullOrWhiteSpace(buildingDefinitionId) &&
               _world.BuildingCatalog.TryGet(buildingDefinitionId, out var definition)
            ? definition
            : null;
    }

    private bool TryGetBuilding(string? buildingDefinitionId, out BuildingDefinition definition)
    {
        if (!string.IsNullOrWhiteSpace(buildingDefinitionId) &&
            _world.BuildingCatalog.TryGet(buildingDefinitionId, out var found))
        {
            definition = found;
            return true;
        }

        definition = null!;
        return false;
    }

    private bool IsResearchUnlocked(string? researchId)
    {
        return string.IsNullOrWhiteSpace(researchId) || _world.Research.IsCompleted(researchId);
    }

    private ResearchDefinition? TryGetResearch(string? researchId)
    {
        return !string.IsNullOrWhiteSpace(researchId) &&
               _world.ResearchCatalog.TryGet(researchId, out var definition)
            ? definition
            : null;
    }

    private UpgradeDefinition? TryGetUpgrade(string? upgradeId)
    {
        return !string.IsNullOrWhiteSpace(upgradeId) &&
               _world.UpgradeCatalog.TryGet(upgradeId, out var definition)
            ? definition
            : null;
    }

    private static string GetBuildingName(BuildingDefinition? definition, string? fallbackId)
    {
        if (definition is not null)
            return definition.Name.ToUpperInvariant();

        return string.IsNullOrWhiteSpace(fallbackId) ? "BUILDING" : fallbackId.ToUpperInvariant();
    }

    private string GetResearchName(string? researchId)
    {
        if (!string.IsNullOrWhiteSpace(researchId) && _world.ResearchCatalog.TryGet(researchId, out var research))
            return research.Name.ToUpperInvariant();

        return string.IsNullOrWhiteSpace(researchId) ? "RESEARCH" : researchId.ToUpperInvariant();
    }

    private static string GetUpgradeName(UpgradeDefinition? definition, string? fallbackId)
    {
        if (definition is not null)
            return definition.Name.ToUpperInvariant();

        return string.IsNullOrWhiteSpace(fallbackId) ? "UPGRADE" : fallbackId.ToUpperInvariant();
    }

    private string FormatMissingResearch(IEnumerable<string> requiredResearchIds)
    {
        var missing = GetMissingResearchNames(requiredResearchIds);

        return missing.Count == 0 ? "PREREQUISITE RESEARCH" : FormatNameList(missing);
    }

    private List<string> GetMissingResearchNames(IEnumerable<string> requiredResearchIds)
    {
        return requiredResearchIds
            .Where(id => !_world.Research.IsCompleted(id))
            .Select(GetResearchName)
            .ToList();
    }

    private static string FormatNameList(IReadOnlyList<string> names)
    {
        if (names.Count <= 3)
            return string.Join(" + ", names);

        return string.Join(" + ", names.Take(3)) + $" + {names.Count - 3} MORE";
    }

    private string FormatUpgradeShortage(decimal moneyCost, double researchCost, decimal missingMoney, double missingResearch)
    {
        var missingMoneyText = missingMoney > 0m;
        var missingResearchText = missingResearch > 0d;

        if (missingMoneyText && missingResearchText)
        {
            return $"NEED MONEY/RESEARCH: {FormatUpgradeCost(moneyCost, researchCost)} - HAVE {FormatMoney(_world.Resources.Money)} + {FormatNumber(_world.Resources.Research)} RP - NEED {FormatMoney(missingMoney)} + {FormatNumber(missingResearch)} RP";
        }

        if (missingMoneyText)
            return $"NEED MONEY: COST {FormatMoney(moneyCost)} - HAVE {FormatMoney(_world.Resources.Money)} - NEED {FormatMoney(missingMoney)}";

        return $"NEED RESEARCH: COST {FormatNumber(researchCost)} RP - HAVE {FormatNumber(_world.Resources.Research)} - NEED {FormatNumber(missingResearch)}";
    }

    private decimal GetMissingMoney(decimal cost)
    {
        return Math.Max(0m, cost - _world.Resources.Money);
    }

    private double GetMissingResearch(double cost)
    {
        return Math.Max(0, cost - _world.Resources.Research);
    }



    private string FormatBuildingEffect(BuildingDefinition definition)
    {
        if (definition.EnergyPerSecond > 0)
            return $"PRODUCES {FormatNumber(definition.EnergyPerSecond)}/S ENERGY";
        if (definition.HeatPerSecond > 0)
            return $"PRODUCES {FormatNumber(definition.HeatPerSecond)}/S HEAT";
        if (definition.HeatConversionPerSecond > 0)
            return $"CONVERTS {FormatNumber(definition.HeatConversionPerSecond)}/S HEAT";
        if (definition.HeatDissipationPerSecond > 0)
            return $"DISSIPATES {FormatNumber(definition.HeatDissipationPerSecond)}/S HEAT";
        if (definition.ResearchPerSecond > 0)
            return $"PRODUCES {FormatNumber(definition.ResearchPerSecond)} RP/S";
        if (definition.BatteryCapacity > 0)
            return $"ADDS {FormatNumber(definition.BatteryCapacity)} ENERGY STORAGE";
        if (definition.AutoSellPerSecond > 0)
            return $"SELLS {FormatNumber(definition.AutoSellPerSecond)}/S ENERGY";
        if (definition.EnergyEfficiencyBonus > 0)
            return $"BOOSTS GRID EFFICIENCY +{FormatNumber(definition.EnergyEfficiencyBonus * 100)}%";
        if (definition.MaintenanceEfficiencyBonus > 0)
            return $"SLOWS BUILDING WEAR +{FormatNumber(definition.MaintenanceEfficiencyBonus * 100)}%";
        if (definition.ToolCapacityBonus > 0)
            return $"ADDS {FormatNumber(definition.ToolCapacityBonus)} TOOL STORAGE";

        return definition.Category.ToString().ToUpperInvariant();
    }

    private string FormatResearchUnlockSummary(ResearchDefinition definition)
    {
        if (definition.UnlockBuildingIds.Count > 0)
            return "UNLOCKS: " + string.Join(" + ", definition.UnlockBuildingIds.Select(id => GetBuildingName(TryGetBuilding(id), id)).Take(3));

        if (definition.ManagedBuildingIds.Count > 0)
            return "MANAGES: " + string.Join(" + ", definition.ManagedBuildingIds.Select(id => GetBuildingName(TryGetBuilding(id), id)).Take(3));

        return "IMPROVES GRID PROGRESSION";
    }

    private static string FormatUpgradeCost(decimal moneyCost, double researchCost)
    {
        if (moneyCost > 0 && researchCost > 0)
            return $"COST {FormatMoney(moneyCost)} + {FormatNumber(researchCost)} RP";
        if (moneyCost > 0)
            return $"COST {FormatMoney(moneyCost)}";
        return $"COST {FormatNumber(researchCost)} RP";
    }

    private static string FormatUpgradeEffectSummary(UpgradeDefinition definition)
    {
        var percent = (definition.Multiplier - 1d) * 100d;
        var amount = (percent >= 0 ? "+" : "") + FormatNumber(percent) + "%";

        return definition.EffectType switch
        {
            UpgradeEffectType.MultiplyEnergyProduction => "ENERGY PRODUCTION " + amount,
            UpgradeEffectType.MultiplyLifetime => "LIFETIME " + amount,
            UpgradeEffectType.MultiplyResearchProduction => "RESEARCH PRODUCTION " + amount,
            UpgradeEffectType.MultiplyHeatProduction => "HEAT PRODUCTION " + amount,
            UpgradeEffectType.MultiplyBatteryCapacity => "BATTERY CAPACITY " + amount,
            UpgradeEffectType.MultiplyAutoSell => "AUTO SELL " + amount,
            UpgradeEffectType.MultiplyHeatConversion => "HEAT CONVERSION " + amount,
            UpgradeEffectType.MultiplyToolAxesGeneration => "AXES GENERATION " + amount,
            UpgradeEffectType.MultiplyToolMinesGeneration => "MINES GENERATION " + amount,
            _ => definition.EffectType.ToString().ToUpperInvariant() + " " + amount
        };
    }

    private static string FormatPosition(GridPosition position)
    {
        return $"{position.X},{position.Y}";
    }

    private static string FormatMoney(decimal value)
    {
        return "$" + FormatNumber((double)value);
    }

    private static string FormatSignedPerSecond(double value)
    {
        var sign = value >= 0 ? "+" : "";
        return sign + FormatNumber(value) + "/S";
    }

    private static string FormatSignedMoneyPerSecond(decimal value)
    {
        var sign = value >= 0 ? "+" : "";
        return sign + FormatMoney(value) + "/S";
    }


    private static string FormatNumber(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return "0";

        var absolute = Math.Abs(value);
        if (absolute >= 1_000_000_000)
            return (value / 1_000_000_000d).ToString("0.##", CultureInfo.InvariantCulture) + "B";

        if (absolute >= 1_000_000)
            return (value / 1_000_000d).ToString("0.##", CultureInfo.InvariantCulture) + "M";

        if (absolute >= 1_000)
            return (value / 1_000d).ToString("0.##", CultureInfo.InvariantCulture) + "K";

        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
