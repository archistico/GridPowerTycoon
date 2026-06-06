using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Research;
using GridPowerTycoon.Core.Upgrades;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Progression;

public sealed class ProgressionAdvisor
{
    private readonly GameWorld _world;

    public ProgressionAdvisor(GameWorld world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
    }

    public string GetCurrentObjectiveHint()
    {
        if (!HasBuilt("wind_turbine"))
            return "OBJECTIVE: BUILD WIND TURBINE - START ENERGY PRODUCTION";

        if (!HasBuilt("office_small"))
        {
            return HasEnoughMoneyFor("office_small")
                ? "OBJECTIVE: BUILD SMALL OFFICE - SELL ENERGY AUTOMATICALLY"
                : "OBJECTIVE: SELL STORED ENERGY, THEN BUILD SMALL OFFICE";
        }

        if (!HasBuilt("research_small"))
        {
            return HasEnoughMoneyFor("research_small")
                ? "OBJECTIVE: BUILD SMALL RESEARCH CENTER - UNLOCK PROGRESS"
                : "OBJECTIVE: EARN MONEY WITH OFFICE, THEN BUILD RESEARCH CENTER";
        }

        if (!HasBuilt("battery_small"))
        {
            return HasEnoughMoneyFor("battery_small")
                ? "OBJECTIVE: BUILD SMALL BATTERY - STORE MORE ENERGY"
                : "OBJECTIVE: SAVE MONEY, THEN BUILD SMALL BATTERY";
        }

        if (!HasBuilt("solar_panel"))
        {
            return HasEnoughMoneyFor("solar_panel")
                ? "OBJECTIVE: BUILD SOLAR PANEL - PRODUCE HEAT POWER"
                : "OBJECTIVE: EARN MONEY, THEN BUILD SOLAR PANEL";
        }

        if (HasHeatProducerWithoutConverter())
            return "OBJECTIVE: PLACE GENERATOR IN RANGE OF HEAT SOURCE";

        if (!HasBuilt("generator_small"))
        {
            return HasEnoughMoneyFor("generator_small")
                ? "OBJECTIVE: BUILD SMALL GENERATOR NEAR SOLAR PANEL"
                : "OBJECTIVE: EARN MONEY, THEN BUILD SMALL GENERATOR";
        }

        return GetMidGameObjectiveHint();
    }

    public string GetCurrentObjectiveDetailHint()
    {
        if (!HasBuilt("wind_turbine"))
            return GetBuildObjectiveDetail("wind_turbine", "OPEN BUILD TAB AND PLACE IT ON PLAIN LAND");

        if (!HasBuilt("office_small"))
            return GetBuildObjectiveDetail("office_small", "SELL ENERGY IF NEEDED, THEN PLACE OFFICE");

        if (!HasBuilt("research_small"))
            return GetBuildObjectiveDetail("research_small", "USE OFFICE INCOME, THEN PLACE RESEARCH CENTER");

        if (!HasBuilt("battery_small"))
            return GetBuildObjectiveDetail("battery_small", "BUILD IT TO INCREASE ENERGY STORAGE");

        if (!HasBuilt("solar_panel"))
            return GetBuildObjectiveDetail("solar_panel", "PLACE IT, THEN ADD A GENERATOR NEARBY");

        if (HasHeatProducerWithoutConverter())
            return GetBuildObjectiveDetail("generator_small", "PLACE GENERATOR SO HEAT SOURCE IS INSIDE RANGE");

        if (!HasBuilt("generator_small"))
            return GetBuildObjectiveDetail("generator_small", "PLACE IT NEAR SOLAR PANEL");

        return GetMidGameObjectiveDetailHint();
    }

    public string GetCurrentBottleneckHint()
    {
        var rates = ResourceRateSnapshot.Calculate(_world);

        if (HasHeatProducerWithoutConverter())
            return "HEAT: PRODUCER WITHOUT GENERATOR COVERAGE";

        if (rates.EnergyPerSecond < -0.01)
            return "ENERGY: GRID IS LOSING STORED ENERGY";

        if (rates.RawEnergyProductionPerSecond <= 0)
            return "ENERGY: BUILD MORE POWER PRODUCTION";

        if (rates.MoneyPerSecond <= 0 && HasBuilt("office_small"))
            return "MONEY: OFFICE HAS NO ENERGY TO SELL";

        if (rates.MoneyPerSecond <= 0 && !HasBuilt("office_small"))
            return "MONEY: BUILD OFFICE TO AUTOMATE SALES";

        if (rates.ResearchPerSecond <= 0 && HasBuilt("research_small"))
            return "RESEARCH: CENTER NEEDS ENERGY SUPPLY";

        if (rates.ResearchPerSecond <= 0 && !HasBuilt("research_small"))
            return "RESEARCH: BUILD RESEARCH CENTER";

        if (HasCloudTiles() && !CanAffordCloudUnlock())
            return "EXPANSION: NEED MONEY/RESEARCH FOR CLOUDS";

        if (HasClearableTerrain() && !HasEnoughToolsToClearTerrain())
            return "EXPANSION: WAIT FOR AXES OR MINES";

        if (IsEnergyStorageNearlyFull() && rates.EnergyPerSecond > 0.01)
            return "STORAGE: ADD BATTERY OR SELL MORE ENERGY";

        if (TryGetAffordableUpgrade(out _))
            return "UPGRADE: AFFORDABLE UPGRADE AVAILABLE";

        if (TryGetAffordableResearch(out _))
            return "RESEARCH: AFFORDABLE RESEARCH AVAILABLE";

        return "STABLE: CHECK NEXT OBJECTIVE";
    }

    public bool HasBuilt(string buildingDefinitionId)
    {
        return _world.BuildingInstances.Values.Any(instance =>
            string.Equals(instance.DefinitionId, buildingDefinitionId, StringComparison.OrdinalIgnoreCase));
    }

    public bool HasHeatProducerWithoutConverter()
    {
        foreach (var instance in _world.BuildingInstances.Values)
        {
            if (!instance.IsActive)
                continue;

            if (!_world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            if (UpgradeCalculator.GetHeatPerSecond(_world, definition) <= 0)
                continue;

            if (!HasActiveHeatConverterInRange(instance))
                return true;
        }

        return false;
    }

    private string GetMidGameObjectiveHint()
    {
        if (!HasAnyUpgradePurchased())
        {
            return TryGetAffordableUpgrade(out var affordableUpgrade)
                ? $"OBJECTIVE: BUY FIRST UPGRADE - {Shorten(affordableUpgrade.Name.ToUpperInvariant(), 32)}"
                : "OBJECTIVE: SAVE MONEY FOR YOUR FIRST UPGRADE";
        }

        if (TryGetAffordableResearch(out var affordableResearch))
            return $"OBJECTIVE: COMPLETE RESEARCH - {Shorten(affordableResearch.Name.ToUpperInvariant(), 34)}";

        if (TryGetResearchToSaveFor(out var nextResearch))
            return $"OBJECTIVE: PRODUCE RESEARCH FOR {Shorten(nextResearch.Name.ToUpperInvariant(), 30)}";

        if (HasCloudTiles())
        {
            return CanAffordCloudUnlock()
                ? "OBJECTIVE: UNLOCK A CLOUD AREA TO EXPAND"
                : "OBJECTIVE: SAVE MONEY AND RESEARCH TO UNLOCK CLOUDS";
        }

        if (HasClearableTerrain())
        {
            return HasEnoughToolsToClearTerrain()
                ? "OBJECTIVE: CLEAR FOREST OR MOUNTAIN TO FREE SPACE"
                : "OBJECTIVE: WAIT FOR AXES OR MINES TO CLEAR TERRAIN";
        }

        if (!HasAnyManagerResearchCompleted())
        {
            return TryGetAffordableManagerResearch(out var managerResearch)
                ? $"OBJECTIVE: UNLOCK MANAGER - {Shorten(managerResearch.Name.ToUpperInvariant(), 32)}"
                : "OBJECTIVE: PRODUCE RESEARCH TO UNLOCK A MANAGER";
        }

        if (CanBuildUnlockedNewHeatTier())
            return "OBJECTIVE: BUILD NEXT HEAT POWER TIER";

        if (TryGetAffordableUpgrade(out var nextUpgrade))
            return $"OBJECTIVE: BUY NEXT UPGRADE - {Shorten(nextUpgrade.Name.ToUpperInvariant(), 33)}";

        return "OBJECTIVE: EXPAND, UPGRADE, AND KEEP ENERGY FLOWING";
    }

    private string GetMidGameObjectiveDetailHint()
    {
        if (!HasAnyUpgradePurchased())
        {
            return TryGetAffordableUpgrade(out var affordableUpgrade)
                ? $"OPEN UPGRADE TAB AND BUY {Shorten(affordableUpgrade.Name.ToUpperInvariant(), 30)}"
                : GetUpgradeSavingDetail("SAVE FOR FIRST USEFUL UPGRADE");
        }

        if (TryGetAffordableResearch(out var affordableResearch))
            return $"OPEN RESEARCH TAB AND COMPLETE {Shorten(affordableResearch.Name.ToUpperInvariant(), 30)}";

        if (TryGetResearchToSaveFor(out var nextResearch))
            return $"NEED R{FormatNumber(Math.Max(0, nextResearch.Cost - _world.Resources.Research))} MORE RESEARCH";

        if (HasCloudTiles())
        {
            if (CanAffordCloudUnlock())
                return "SELECT A CLOUD CELL AND CLICK UNLOCK";

            var moneyMissing = Math.Max(0m, _world.AreaUnlockSettings.CloudUnlockMoneyCost - _world.Resources.Money);
            var researchMissing = Math.Max(0d, _world.AreaUnlockSettings.CloudUnlockResearchCost - _world.Resources.Research);
            return $"NEED ${FormatNumber((double)moneyMissing)} AND R{FormatNumber(researchMissing)} FOR CLOUD";
        }

        if (HasClearableTerrain())
        {
            if (HasEnoughToolsToClearTerrain())
                return "SELECT FOREST OR MOUNTAIN AND CLICK CLEAR";

            return $"NEED AXES {FormatNumber(_world.Resources.Axes)}/{FormatNumber(_world.ToolSettings.ForestClearAxesCost)} OR MINES {FormatNumber(_world.Resources.Mines)}/{FormatNumber(_world.ToolSettings.MountainClearMinesCost)}";
        }

        if (!HasAnyManagerResearchCompleted())
        {
            return TryGetAffordableManagerResearch(out var managerResearch)
                ? $"COMPLETE MANAGER RESEARCH {Shorten(managerResearch.Name.ToUpperInvariant(), 26)}"
                : "PRODUCE MORE RESEARCH FOR FIRST MANAGER";
        }

        if (CanBuildUnlockedNewHeatTier())
            return "BUILD UNLOCKED COAL/GAS OR MEDIUM GENERATOR TIER";

        if (TryGetAffordableUpgrade(out var nextUpgrade))
            return $"OPEN UPGRADE TAB AND BUY {Shorten(nextUpgrade.Name.ToUpperInvariant(), 30)}";

        return "CHECK BOTTLENECKS: ENERGY, MONEY, RESEARCH, HEAT OR SPACE";
    }

    private string GetBuildObjectiveDetail(string buildingDefinitionId, string readyText)
    {
        if (!_world.BuildingCatalog.TryGet(buildingDefinitionId, out var definition))
            return readyText;

        if (!string.IsNullOrWhiteSpace(definition.RequiredResearchId) &&
            !_world.Research.IsCompleted(definition.RequiredResearchId))
        {
            return _world.ResearchCatalog.TryGet(definition.RequiredResearchId, out var research)
                ? $"COMPLETE RESEARCH {Shorten(research.Name.ToUpperInvariant(), 30)}"
                : "COMPLETE REQUIRED RESEARCH";
        }

        if (_world.Resources.Money < definition.Cost)
            return $"NEED ${FormatNumber((double)(definition.Cost - _world.Resources.Money))} MORE MONEY";

        return readyText;
    }

    private string GetUpgradeSavingDetail(string fallback)
    {
        if (!TryGetNextRelevantUpgradeToSaveFor(out var definition))
            return fallback;

        var currentLevel = _world.Upgrades.GetLevel(definition.Id);
        var moneyCost = UpgradeSystem.GetMoneyCost(definition, currentLevel);
        var researchCost = UpgradeSystem.GetResearchCost(definition, currentLevel);
        var missingMoney = Math.Max(0m, moneyCost - _world.Resources.Money);
        var missingResearch = Math.Max(0d, researchCost - _world.Resources.Research);

        if (missingMoney > 0 && missingResearch > 0)
            return $"NEED ${FormatNumber((double)missingMoney)} AND R{FormatNumber(missingResearch)} FOR UPGRADE";

        if (missingMoney > 0)
            return $"NEED ${FormatNumber((double)missingMoney)} MORE MONEY FOR UPGRADE";

        if (missingResearch > 0)
            return $"NEED R{FormatNumber(missingResearch)} MORE RESEARCH FOR UPGRADE";

        return fallback;
    }

    private bool HasEnoughMoneyFor(string buildingDefinitionId)
    {
        return _world.BuildingCatalog.TryGet(buildingDefinitionId, out var definition) &&
               _world.Resources.Money >= definition.Cost;
    }

    private bool HasAnyUpgradePurchased()
    {
        return _world.Upgrades.Levels.Any(level => level.Value > 0);
    }

    private bool TryGetAffordableUpgrade(out UpgradeDefinition definition)
    {
        definition = null!;

        foreach (var candidate in _world.UpgradeCatalog.All.OrderBy(upgrade => upgrade.CostMoney).ThenBy(upgrade => upgrade.CostResearch))
        {
            var currentLevel = _world.Upgrades.GetLevel(candidate.Id);
            if (currentLevel >= candidate.MaxLevel)
                continue;

            if (!IsUpgradeTargetRelevant(candidate))
                continue;

            if (!string.IsNullOrWhiteSpace(candidate.RequiredResearchId) &&
                !_world.Research.IsCompleted(candidate.RequiredResearchId))
                continue;

            if (_world.Resources.Money < UpgradeSystem.GetMoneyCost(candidate, currentLevel))
                continue;

            if (_world.Resources.Research < UpgradeSystem.GetResearchCost(candidate, currentLevel))
                continue;

            definition = candidate;
            return true;
        }

        return false;
    }

    private bool TryGetNextRelevantUpgradeToSaveFor(out UpgradeDefinition definition)
    {
        definition = null!;

        foreach (var candidate in _world.UpgradeCatalog.All.OrderBy(upgrade => upgrade.CostMoney).ThenBy(upgrade => upgrade.CostResearch))
        {
            var currentLevel = _world.Upgrades.GetLevel(candidate.Id);
            if (currentLevel >= candidate.MaxLevel)
                continue;

            if (!IsUpgradeTargetRelevant(candidate))
                continue;

            if (!string.IsNullOrWhiteSpace(candidate.RequiredResearchId) &&
                !_world.Research.IsCompleted(candidate.RequiredResearchId))
                continue;

            definition = candidate;
            return true;
        }

        return false;
    }

    private bool IsUpgradeTargetRelevant(UpgradeDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(definition.TargetBuildingId))
            return true;

        return HasBuilt(definition.TargetBuildingId);
    }

    private bool TryGetAffordableResearch(out ResearchDefinition definition)
    {
        definition = null!;

        foreach (var candidate in GetAvailableResearchDefinitions())
        {
            if (_world.Resources.Research < candidate.Cost)
                continue;

            definition = candidate;
            return true;
        }

        return false;
    }

    private bool TryGetResearchToSaveFor(out ResearchDefinition definition)
    {
        definition = null!;

        foreach (var candidate in GetAvailableResearchDefinitions())
        {
            definition = candidate;
            return true;
        }

        return false;
    }

    private IEnumerable<ResearchDefinition> GetAvailableResearchDefinitions()
    {
        return _world.ResearchCatalog.All
            .Where(research => !_world.Research.IsCompleted(research.Id))
            .Where(research => research.RequiredResearchIds.All(required => _world.Research.IsCompleted(required)))
            .OrderBy(research => research.Cost);
    }

    private bool HasCloudTiles()
    {
        return _world.Map.Tiles.Any(tile => tile.Type == TileType.Cloud && tile.CoveredType.HasValue);
    }

    private bool CanAffordCloudUnlock()
    {
        return _world.Resources.Money >= _world.AreaUnlockSettings.CloudUnlockMoneyCost &&
               _world.Resources.Research >= _world.AreaUnlockSettings.CloudUnlockResearchCost;
    }

    private bool HasClearableTerrain()
    {
        return _world.Map.Tiles.Any(tile => tile.Type is TileType.Forest or TileType.Mountain);
    }

    private bool HasEnoughToolsToClearTerrain()
    {
        return _world.Map.Tiles.Any(tile =>
            tile.Type == TileType.Forest && _world.Resources.Axes >= _world.ToolSettings.ForestClearAxesCost ||
            tile.Type == TileType.Mountain && _world.Resources.Mines >= _world.ToolSettings.MountainClearMinesCost);
    }

    private bool HasAnyManagerResearchCompleted()
    {
        return _world.ResearchCatalog.All.Any(research =>
            research.ManagedBuildingIds.Count > 0 &&
            _world.Research.IsCompleted(research.Id));
    }

    private bool TryGetAffordableManagerResearch(out ResearchDefinition definition)
    {
        definition = null!;

        foreach (var candidate in GetAvailableResearchDefinitions().Where(research => research.ManagedBuildingIds.Count > 0))
        {
            if (_world.Resources.Research < candidate.Cost)
                continue;

            definition = candidate;
            return true;
        }

        return false;
    }

    private bool CanBuildUnlockedNewHeatTier()
    {
        return HasUnlockedAndUnbuilt("coal_power_plant") ||
               HasUnlockedAndUnbuilt("generator_medium") ||
               HasUnlockedAndUnbuilt("gas_power_plant");
    }

    private bool HasUnlockedAndUnbuilt(string buildingDefinitionId)
    {
        if (HasBuilt(buildingDefinitionId))
            return false;

        if (!_world.BuildingCatalog.TryGet(buildingDefinitionId, out var definition))
            return false;

        return string.IsNullOrWhiteSpace(definition.RequiredResearchId) ||
               _world.Research.IsCompleted(definition.RequiredResearchId);
    }

    private bool HasActiveHeatConverterInRange(BuildingInstance producer)
    {
        foreach (var instance in _world.BuildingInstances.Values)
        {
            if (!instance.IsActive || instance.Id == producer.Id)
                continue;

            if (!_world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            if (definition.HeatRange <= 0)
                continue;

            if (UpgradeCalculator.GetHeatConversionPerSecond(_world, definition) <= 0)
                continue;

            if (GetChebyshevDistance(instance, producer) <= definition.HeatRange)
                return true;
        }

        return false;
    }

    private bool IsEnergyStorageNearlyFull()
    {
        if (_world.Resources.MaxEnergy <= 0)
            return false;

        return _world.Resources.Energy / _world.Resources.MaxEnergy >= 0.9;
    }

    private static int GetChebyshevDistance(BuildingInstance a, BuildingInstance b)
    {
        var dx = Math.Abs(a.Position.X - b.Position.X);
        var dy = Math.Abs(a.Position.Y - b.Position.Y);
        return Math.Max(dx, dy);
    }

    private static string Shorten(string text, int maxCharacters)
    {
        if (string.IsNullOrEmpty(text) || maxCharacters <= 0)
            return string.Empty;

        return text.Length <= maxCharacters ? text : text[..Math.Max(0, maxCharacters - 3)] + "...";
    }

    private static string FormatNumber(double value)
    {
        if (Math.Abs(value) >= 1000000)
            return (value / 1000000d).ToString("0.##") + "M";

        if (Math.Abs(value) >= 1000)
            return (value / 1000d).ToString("0.##") + "K";

        if (Math.Abs(value - Math.Round(value)) < 0.0001)
            return Math.Round(value).ToString("0");

        return value.ToString("0.##");
    }
}
