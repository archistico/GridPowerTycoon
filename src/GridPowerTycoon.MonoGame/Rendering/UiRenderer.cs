using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Expansion;
using GridPowerTycoon.Core.Research;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Tools;
using GridPowerTycoon.Core.Upgrades;
using GridPowerTycoon.Core.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GridPowerTycoon.MonoGame.Rendering;

public sealed class UiRenderer
{
    public const int TopBarHeight = 74;
    public const int SideMenuWidth = 650;

    private const int PanelMargin = 12;
    private const int ColumnWidth = 200;
    private const int ColumnGap = 12;
    private const int MenuHeaderY = TopBarHeight + 14;
    private const int MenuButtonsY = TopBarHeight + 48;

    private static readonly string[] BuildButtonIds =
    {
        "wind_turbine",
        "battery_small",
        "office_small",
        "research_small",
        "solar_panel",
        "generator_small"
    };

    private static readonly string[] ResearchButtonIds =
    {
        "battery",
        "office_small",
        "solar_power",
        "generator_small"
    };

    private static readonly string[] UpgradeButtonIds =
    {
        "wind_turbine_energy_1",
        "wind_turbine_lifetime_1",
        "research_small_output_1",
        "battery_small_capacity_1",
        "office_small_sell_1",
        "generator_small_conversion_1",
        "axes_generation_1",
        "mines_generation_1"
    };

    private readonly GameWorld _world;
    private readonly Texture2D _pixel;
    private readonly PixelTextRenderer _text;

    public UiRenderer(GameWorld world, Texture2D pixel)
    {
        _world = world;
        _pixel = pixel;
        _text = new PixelTextRenderer(pixel);
    }

    public void Draw(
        SpriteBatch spriteBatch,
        Viewport viewport,
        string? selectedBuildingId,
        BuildResult? lastBuildResult,
        ResearchResult? lastResearchResult,
        Guid? selectedMapBuildingId,
        GridPosition? selectedTerrainPosition,
        GridPosition? selectedCloudPosition,
        TerrainClearResult? lastTerrainClearResult,
        AreaUnlockResult? lastAreaUnlockResult,
        UpgradeResult? lastUpgradeResult,
        string? saveLoadMessage)
    {
        var topBar = GetTopBarRectangle(viewport);
        var sideMenu = GetSideMenuRectangle(viewport);

        spriteBatch.Draw(_pixel, topBar, new Color(32, 39, 52));
        spriteBatch.Draw(_pixel, sideMenu, new Color(38, 48, 62));

        DrawTopBar(spriteBatch, topBar, viewport);
        DrawBuildMenu(spriteBatch, selectedBuildingId);
        DrawResearchMenu(spriteBatch);
        DrawUpgradeMenu(spriteBatch);
        DrawSelectedBuildingPanel(spriteBatch, viewport, selectedMapBuildingId);
        DrawSelectedTerrainPanel(spriteBatch, viewport, selectedTerrainPosition);
        DrawSelectedCloudPanel(spriteBatch, viewport, selectedCloudPosition);
        DrawStatus(spriteBatch, viewport, selectedBuildingId, lastBuildResult, lastResearchResult, lastTerrainClearResult, lastAreaUnlockResult, lastUpgradeResult, saveLoadMessage);
    }

    public bool IsMouseOverUi(Point mousePosition, Viewport viewport)
    {
        return GetTopBarRectangle(viewport).Contains(mousePosition) ||
               GetSideMenuRectangle(viewport).Contains(mousePosition) ||
               GetSelectedBuildingPanelRectangle(viewport).Contains(mousePosition) ||
               GetSelectedTerrainPanelRectangle(viewport).Contains(mousePosition) ||
               GetSelectedCloudPanelRectangle(viewport).Contains(mousePosition);
    }

    public bool IsSellButtonAt(Point mousePosition, Viewport viewport)
    {
        return GetSellButtonRectangle(viewport).Contains(mousePosition);
    }

    public bool IsReplaceButtonAt(Point mousePosition, Viewport viewport, Guid? selectedMapBuildingId)
    {
        if (!selectedMapBuildingId.HasValue)
            return false;

        if (!_world.TryGetBuilding(selectedMapBuildingId.Value, out var instance))
            return false;

        if (instance.State != BuildingState.Expired)
            return false;

        return GetReplaceButtonRectangle(viewport).Contains(mousePosition);
    }

    public bool IsClearTerrainButtonAt(Point mousePosition, Viewport viewport, GridPosition? selectedTerrainPosition)
    {
        if (!selectedTerrainPosition.HasValue)
            return false;

        if (!_world.Map.Contains(selectedTerrainPosition.Value))
            return false;

        var tile = _world.Map.GetTile(selectedTerrainPosition.Value);
        if (tile.Type != TileType.Forest && tile.Type != TileType.Mountain)
            return false;

        return GetClearTerrainButtonRectangle(viewport).Contains(mousePosition);
    }

    public bool IsUnlockCloudButtonAt(Point mousePosition, Viewport viewport, GridPosition? selectedCloudPosition)
    {
        if (!selectedCloudPosition.HasValue)
            return false;

        if (!_world.Map.Contains(selectedCloudPosition.Value))
            return false;

        var tile = _world.Map.GetTile(selectedCloudPosition.Value);
        if (tile.Type != TileType.Cloud)
            return false;

        return GetUnlockCloudButtonRectangle(viewport).Contains(mousePosition);
    }

    public bool TryGetBuildingButtonAt(Point mousePosition, out string? buildingId)
    {
        for (var i = 0; i < BuildButtonIds.Length; i++)
        {
            if (GetBuildButtonRectangle(i).Contains(mousePosition))
            {
                buildingId = BuildButtonIds[i];
                return true;
            }
        }

        buildingId = null;
        return false;
    }

    public bool TryGetResearchButtonAt(Point mousePosition, out string researchId)
    {
        for (var i = 0; i < ResearchButtonIds.Length; i++)
        {
            if (GetResearchButtonRectangle(i).Contains(mousePosition))
            {
                researchId = ResearchButtonIds[i];
                return true;
            }
        }

        researchId = "";
        return false;
    }

    public bool TryGetUpgradeButtonAt(Point mousePosition, Viewport viewport, out string upgradeId)
    {
        for (var i = 0; i < UpgradeButtonIds.Length; i++)
        {
            if (GetUpgradeButtonRectangle(i).Contains(mousePosition))
            {
                upgradeId = UpgradeButtonIds[i];
                return true;
            }
        }

        upgradeId = "";
        return false;
    }

    public IReadOnlyList<string> GetBuildButtonIds()
    {
        return BuildButtonIds;
    }

    private void DrawTopBar(SpriteBatch spriteBatch, Rectangle topBar, Viewport viewport)
    {
        var resources = _world.Resources;
        var rates = ResourceRateSnapshot.Calculate(_world);
        var axesPerSecond = UpgradeCalculator.GetAxesPerSecond(_world);
        var minesPerSecond = UpgradeCalculator.GetMinesPerSecond(_world);

        var sellButton = GetSellButtonRectangle(viewport);
        var energyFillBar = GetEnergyFillBarRectangle(viewport);
        var availableWidth = Math.Max(360, energyFillBar.X - 18);
        var sectionWidth = Math.Max(130, availableWidth / 5);

        DrawTopMetric(spriteBatch, 14 + sectionWidth * 0, "ENERGY", $"{FormatNumberFixed2(resources.Energy)}/{FormatNumberFixed2(resources.MaxEnergy)}", $"{FormatSignedNumberFixed2(rates.EnergyPerSecond)}/S", new Color(135, 210, 255));
        DrawTopMetric(spriteBatch, 14 + sectionWidth * 1, "RESEARCH", FormatNumberFixed2(resources.Research), $"{FormatSignedNumberFixed2(rates.ResearchPerSecond)}/S", new Color(210, 190, 255));
        DrawTopMetric(spriteBatch, 14 + sectionWidth * 2, "MONEY", "$" + FormatNumberFixed2((double)resources.Money), FormatSignedMoneyFixed2((double)rates.MoneyPerSecond) + "/S", new Color(255, 225, 120));
        DrawTopMetric(spriteBatch, 14 + sectionWidth * 3, "AXES", $"{FormatNumberFixed2(resources.Axes)}/{FormatNumberFixed2(_world.ToolSettings.MaxAxes)}", $"{FormatSignedNumberFixed2(axesPerSecond)}/S", new Color(210, 235, 190));
        DrawTopMetric(spriteBatch, 14 + sectionWidth * 4, "MINES", $"{FormatNumberFixed2(resources.Mines)}/{FormatNumberFixed2(_world.ToolSettings.MaxMines)}", $"{FormatSignedNumberFixed2(minesPerSecond)}/S", new Color(230, 210, 170));

        DrawEnergyFillBar(spriteBatch, energyFillBar);

        spriteBatch.Draw(_pixel, sellButton, new Color(70, 120, 72));
        DrawOutline(spriteBatch, sellButton, new Color(150, 235, 150), 2);
        _text.DrawString(spriteBatch, "SELL", new Vector2(sellButton.X + 16, sellButton.Y + 13), new Color(235, 250, 235), 2);
    }

    private void DrawTopMetric(SpriteBatch spriteBatch, int x, string label, string value, string rate, Color accentColor)
    {
        _text.DrawString(spriteBatch, label, new Vector2(x, 9), new Color(230, 238, 245), 1);
        _text.DrawString(spriteBatch, value, new Vector2(x, 25), accentColor, 2);
        _text.DrawString(spriteBatch, rate, new Vector2(x, 54), accentColor, 1);
    }

    private void DrawEnergyFillBar(SpriteBatch spriteBatch, Rectangle rect)
    {
        var maxEnergy = Math.Max(1d, _world.Resources.MaxEnergy);
        var ratio = MathHelper.Clamp((float)(_world.Resources.Energy / maxEnergy), 0f, 1f);

        spriteBatch.Draw(_pixel, rect, new Color(18, 25, 36));
        DrawOutline(spriteBatch, rect, new Color(90, 150, 190), 2);

        var inner = new Rectangle(rect.X + 3, rect.Y + 3, Math.Max(0, rect.Width - 6), Math.Max(0, rect.Height - 6));
        var fill = new Rectangle(inner.X, inner.Y, (int)Math.Round(inner.Width * ratio), inner.Height);

        if (fill.Width > 0)
            spriteBatch.Draw(_pixel, fill, new Color(70, 155, 215));

        var percentText = $"FILL {ratio * 100f:0}%";
        _text.DrawString(spriteBatch, percentText, new Vector2(rect.X + 10, rect.Y + 11), new Color(235, 245, 255), 1);
    }

    private void DrawBuildMenu(SpriteBatch spriteBatch, string? selectedBuildingId)
    {
        var headerX = GetBuildColumnX();
        _text.DrawString(spriteBatch, "BUILD", new Vector2(headerX, MenuHeaderY), new Color(230, 238, 245), 2);

        for (var i = 0; i < BuildButtonIds.Length; i++)
        {
            var id = BuildButtonIds[i];
            var rect = GetBuildButtonRectangle(i);
            var isSelected = string.Equals(selectedBuildingId, id, StringComparison.OrdinalIgnoreCase);

            _world.BuildingCatalog.TryGet(id, out var definition);
            var isLocked = definition is not null && !IsBuildingUnlocked(definition);

            spriteBatch.Draw(_pixel, rect, isSelected ? new Color(67, 86, 110) : new Color(48, 60, 76));
            DrawOutline(spriteBatch, rect, isSelected ? new Color(255, 220, 80) : new Color(74, 88, 108), isSelected ? 3 : 1);

            var iconRect = new Rectangle(rect.X + 8, rect.Y + 8, 28, 28);
            var iconColor = definition is null ? Color.Magenta : GetBuildingColor(definition.Category);
            if (isLocked)
                iconColor = new Color(90, 95, 105);
            spriteBatch.Draw(_pixel, iconRect, iconColor);
            DrawOutline(spriteBatch, iconRect, new Color(15, 20, 28), 1);

            var indexText = (i + 1).ToString();
            var name = definition?.Name ?? id;
            var cost = definition is null ? "?" : FormatNumber((double)definition.Cost);
            var textColor = isLocked ? new Color(150, 155, 165) : new Color(235, 240, 245);
            _text.DrawString(spriteBatch, $"{indexText} {Shorten(name, 16)}", new Vector2(rect.X + 44, rect.Y + 7), textColor, 1);
            _text.DrawString(spriteBatch, isLocked ? "LOCKED" : $"COST ${cost}", new Vector2(rect.X + 44, rect.Y + 24), isLocked ? new Color(255, 150, 120) : new Color(255, 225, 120), 1);
        }
    }

    private void DrawResearchMenu(SpriteBatch spriteBatch)
    {
        var headerX = GetResearchColumnX();
        _text.DrawString(spriteBatch, "RESEARCH", new Vector2(headerX, MenuHeaderY), new Color(230, 238, 245), 2);

        for (var i = 0; i < ResearchButtonIds.Length; i++)
        {
            var id = ResearchButtonIds[i];
            var rect = GetResearchButtonRectangle(i);

            _world.ResearchCatalog.TryGet(id, out var definition);
            var completed = _world.Research.IsCompleted(id);
            var canAfford = definition is not null && _world.Resources.Research >= definition.Cost;
            var missingPrereq = definition is not null && definition.RequiredResearchIds.Any(x => !_world.Research.IsCompleted(x));

            var background = completed
                ? new Color(50, 85, 62)
                : canAfford && !missingPrereq
                    ? new Color(64, 58, 86)
                    : new Color(45, 49, 62);

            spriteBatch.Draw(_pixel, rect, background);
            DrawOutline(spriteBatch, rect, completed ? new Color(130, 230, 150) : new Color(80, 86, 105), 1);

            var name = definition?.Name ?? id;
            var cost = definition is null ? "?" : FormatNumber(definition.Cost);
            var status = completed ? "DONE" : missingPrereq ? "REQ" : $"COST R{cost}";

            _text.DrawString(spriteBatch, Shorten(name, 22), new Vector2(rect.X + 8, rect.Y + 6), new Color(235, 240, 245), 1);
            _text.DrawString(spriteBatch, status, new Vector2(rect.X + 8, rect.Y + 24), completed ? new Color(160, 245, 175) : new Color(210, 190, 255), 1);
        }
    }

    private void DrawUpgradeMenu(SpriteBatch spriteBatch)
    {
        var headerX = GetUpgradeColumnX();
        _text.DrawString(spriteBatch, "UPGRADE", new Vector2(headerX, MenuHeaderY), new Color(230, 238, 245), 2);

        for (var i = 0; i < UpgradeButtonIds.Length; i++)
        {
            var id = UpgradeButtonIds[i];
            var rect = GetUpgradeButtonRectangle(i);

            _world.UpgradeCatalog.TryGet(id, out var definition);
            var level = definition is null ? 0 : _world.Upgrades.GetLevel(id);
            var completed = definition is not null && level >= definition.MaxLevel;
            var missingResearch = definition is not null &&
                                  !string.IsNullOrWhiteSpace(definition.RequiredResearchId) &&
                                  !_world.Research.IsCompleted(definition.RequiredResearchId);
            var canAfford = definition is not null &&
                            _world.Resources.Money >= definition.CostMoney &&
                            _world.Resources.Research >= definition.CostResearch;

            var background = completed
                ? new Color(50, 85, 62)
                : missingResearch
                    ? new Color(50, 50, 58)
                    : canAfford
                        ? new Color(70, 64, 90)
                        : new Color(45, 49, 62);

            spriteBatch.Draw(_pixel, rect, background);
            DrawOutline(spriteBatch, rect, completed ? new Color(130, 230, 150) : new Color(80, 86, 105), 1);

            var name = definition?.Name ?? id;
            var effect = definition is null ? "?" : GetUpgradeEffectText(definition);
            var status = definition is null
                ? "?"
                : completed
                    ? "DONE"
                    : missingResearch
                        ? "REQ RESEARCH"
                        : GetUpgradeCostText(definition);

            _text.DrawString(spriteBatch, Shorten(name, 22), new Vector2(rect.X + 8, rect.Y + 5), new Color(235, 240, 245), 1);
            _text.DrawString(spriteBatch, Shorten(effect, 24), new Vector2(rect.X + 8, rect.Y + 22), new Color(190, 215, 255), 1);
            _text.DrawString(spriteBatch, status, new Vector2(rect.X + 8, rect.Y + 39), completed ? new Color(160, 245, 175) : new Color(255, 225, 120), 1);
        }
    }

    private void DrawSelectedBuildingPanel(SpriteBatch spriteBatch, Viewport viewport, Guid? selectedMapBuildingId)
    {
        if (!selectedMapBuildingId.HasValue)
            return;

        if (!_world.TryGetBuilding(selectedMapBuildingId.Value, out var instance))
            return;

        if (!_world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
            return;

        var panel = GetSelectedBuildingPanelRectangle(viewport);
        spriteBatch.Draw(_pixel, panel, new Color(30, 38, 52, 235));
        DrawOutline(spriteBatch, panel, new Color(95, 115, 140), 2);

        _text.DrawString(spriteBatch, Shorten(definition.Name.ToUpperInvariant(), 28), new Vector2(panel.X + 12, panel.Y + 12), new Color(235, 240, 245), 2);
        _text.DrawString(spriteBatch, $"STATE {instance.State.ToString().ToUpperInvariant()}", new Vector2(panel.X + 12, panel.Y + 46), GetStateColor(instance.State), 1);

        var y = panel.Y + 66;
        var effectiveLifetime = UpgradeCalculator.GetLifetimeSeconds(_world, definition);
        var lifetimeText = effectiveLifetime <= 0
            ? "LIFE -"
            : $"LIFE {Math.Ceiling(instance.RemainingLifetimeSeconds):0}S/{effectiveLifetime:0}S";

        DrawDetailLine(spriteBatch, panel.X + 12, ref y, lifetimeText, new Color(210, 222, 235));
        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"COST ${FormatNumber((double)definition.Cost)}", new Color(255, 225, 120));
        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"SIZE {definition.Width}X{definition.Height}", new Color(180, 195, 215));

        var energyConsumption = UpgradeCalculator.GetEnergyConsumptionPerSecond(_world, definition);
        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"ENERGY IN -{FormatNumber(energyConsumption)}/S", new Color(255, 165, 120));

        var effectiveEnergy = UpgradeCalculator.GetEnergyPerSecond(_world, definition);
        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"ENERGY OUT +{FormatNumber(effectiveEnergy)}/S", new Color(135, 210, 255));

        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"HEAT OUT +{FormatNumber(definition.HeatPerSecond)}/S", new Color(245, 145, 55));

        var effectiveHeatConversion = UpgradeCalculator.GetHeatConversionPerSecond(_world, definition);
        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"HEAT IN {FormatNumber(effectiveHeatConversion)}/S R{definition.HeatRange}", new Color(70, 220, 190));
        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"ENERGY OUT FROM HEAT {FormatNumber(effectiveHeatConversion * _world.HeatSettings.HeatEnergyConversionRate)}/S", new Color(135, 210, 255));

        var effectiveResearch = UpgradeCalculator.GetResearchPerSecond(_world, definition);
        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"RESEARCH OUT +{FormatNumber(effectiveResearch)}/S", new Color(210, 190, 255));

        var effectiveAutoSell = UpgradeCalculator.GetAutoSellPerSecond(_world, definition);
        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"AUTO SELL IN {FormatNumber(effectiveAutoSell)}/S", new Color(180, 225, 190));

        var effectiveBattery = UpgradeCalculator.GetBatteryCapacity(_world, definition);
        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"BATTERY CAP +{FormatNumber(effectiveBattery)}", new Color(240, 205, 70));

        if (instance.AccumulatedHeat > 0 || definition.HeatPerSecond > 0)
        {
            var heatText = $"HEAT STORED {FormatNumber(instance.AccumulatedHeat)}/{FormatNumber(_world.HeatSettings.HeatExplosionThreshold)}";
            DrawDetailLine(spriteBatch, panel.X + 12, ref y, heatText, GetHeatTextColor(instance.AccumulatedHeat));
        }

        if (instance.State == BuildingState.Expired)
        {
            var replaceButton = GetReplaceButtonRectangle(viewport);
            spriteBatch.Draw(_pixel, replaceButton, new Color(88, 118, 72));
            DrawOutline(spriteBatch, replaceButton, new Color(180, 235, 135), 2);
            _text.DrawString(spriteBatch, $"REPLACE ${FormatNumber((double)definition.Cost)}", new Vector2(replaceButton.X + 12, replaceButton.Y + 10), new Color(235, 250, 220), 1);
        }
    }

    private void DrawDetailLine(SpriteBatch spriteBatch, int x, ref int y, string text, Color color)
    {
        _text.DrawString(spriteBatch, text, new Vector2(x, y), color, 1);
        y += 18;
    }

    private void DrawSelectedTerrainPanel(SpriteBatch spriteBatch, Viewport viewport, GridPosition? selectedTerrainPosition)
    {
        if (!selectedTerrainPosition.HasValue)
            return;

        if (!_world.Map.Contains(selectedTerrainPosition.Value))
            return;

        var tile = _world.Map.GetTile(selectedTerrainPosition.Value);
        if (tile.Type != TileType.Forest && tile.Type != TileType.Mountain)
            return;

        var panel = GetSelectedTerrainPanelRectangle(viewport);
        spriteBatch.Draw(_pixel, panel, new Color(30, 38, 52, 235));
        DrawOutline(spriteBatch, panel, new Color(95, 115, 140), 2);

        var title = tile.Type == TileType.Forest ? "FOREST" : "MOUNTAIN";
        var costText = tile.Type == TileType.Forest
            ? $"COST {_world.ToolSettings.ForestClearAxesCost} AXES"
            : $"COST {_world.ToolSettings.MountainClearMinesCost} MINES";

        var availableText = tile.Type == TileType.Forest
            ? $"HAVE {FormatNumberFixed2(_world.Resources.Axes)} AXES"
            : $"HAVE {FormatNumberFixed2(_world.Resources.Mines)} MINES";

        _text.DrawString(spriteBatch, title, new Vector2(panel.X + 12, panel.Y + 12), new Color(235, 240, 245), 2);
        _text.DrawString(spriteBatch, $"CELL {tile.Position.X},{tile.Position.Y}", new Vector2(panel.X + 12, panel.Y + 48), new Color(210, 222, 235), 1);
        _text.DrawString(spriteBatch, costText, new Vector2(panel.X + 12, panel.Y + 70), new Color(255, 225, 120), 1);
        _text.DrawString(spriteBatch, availableText, new Vector2(panel.X + 12, panel.Y + 92), new Color(210, 235, 190), 1);

        var clearButton = GetClearTerrainButtonRectangle(viewport);
        spriteBatch.Draw(_pixel, clearButton, CanClearTerrain(tile.Type) ? new Color(88, 118, 72) : new Color(80, 80, 84));
        DrawOutline(spriteBatch, clearButton, CanClearTerrain(tile.Type) ? new Color(180, 235, 135) : new Color(120, 120, 130), 2);
        _text.DrawString(spriteBatch, "CLEAR", new Vector2(clearButton.X + 12, clearButton.Y + 10), new Color(235, 250, 220), 1);
    }

    private bool CanClearTerrain(TileType type)
    {
        return type switch
        {
            TileType.Forest => _world.Resources.Axes >= _world.ToolSettings.ForestClearAxesCost,
            TileType.Mountain => _world.Resources.Mines >= _world.ToolSettings.MountainClearMinesCost,
            _ => false
        };
    }

    private void DrawSelectedCloudPanel(SpriteBatch spriteBatch, Viewport viewport, GridPosition? selectedCloudPosition)
    {
        if (!selectedCloudPosition.HasValue)
            return;

        if (!_world.Map.Contains(selectedCloudPosition.Value))
            return;

        var tile = _world.Map.GetTile(selectedCloudPosition.Value);
        if (tile.Type != TileType.Cloud)
            return;

        var panel = GetSelectedCloudPanelRectangle(viewport);
        spriteBatch.Draw(_pixel, panel, new Color(30, 38, 52, 235));
        DrawOutline(spriteBatch, panel, new Color(145, 155, 170), 2);

        var revealText = tile.CoveredType.HasValue
            ? tile.CoveredType.Value.ToString().ToUpperInvariant()
            : "UNKNOWN";

        _text.DrawString(spriteBatch, "CLOUD AREA", new Vector2(panel.X + 12, panel.Y + 12), new Color(235, 240, 245), 2);
        _text.DrawString(spriteBatch, $"CELL {tile.Position.X},{tile.Position.Y}", new Vector2(panel.X + 12, panel.Y + 48), new Color(210, 222, 235), 1);
        _text.DrawString(spriteBatch, $"REVEALS {revealText}", new Vector2(panel.X + 12, panel.Y + 70), new Color(210, 222, 235), 1);
        _text.DrawString(spriteBatch, $"COST ${FormatNumber((double)_world.AreaUnlockSettings.CloudUnlockMoneyCost)}", new Vector2(panel.X + 12, panel.Y + 92), new Color(255, 225, 120), 1);
        _text.DrawString(spriteBatch, $"COST R{FormatNumber(_world.AreaUnlockSettings.CloudUnlockResearchCost)}", new Vector2(panel.X + 12, panel.Y + 114), new Color(210, 190, 255), 1);

        var unlockButton = GetUnlockCloudButtonRectangle(viewport);
        spriteBatch.Draw(_pixel, unlockButton, CanUnlockCloud() ? new Color(88, 118, 72) : new Color(80, 80, 84));
        DrawOutline(spriteBatch, unlockButton, CanUnlockCloud() ? new Color(180, 235, 135) : new Color(120, 120, 130), 2);
        _text.DrawString(spriteBatch, "UNLOCK", new Vector2(unlockButton.X + 12, unlockButton.Y + 10), new Color(235, 250, 220), 1);
    }

    private bool CanUnlockCloud()
    {
        return _world.Resources.Money >= _world.AreaUnlockSettings.CloudUnlockMoneyCost &&
               _world.Resources.Research >= _world.AreaUnlockSettings.CloudUnlockResearchCost;
    }

    private void DrawStatus(SpriteBatch spriteBatch, Viewport viewport, string? selectedBuildingId, BuildResult? lastBuildResult, ResearchResult? lastResearchResult, TerrainClearResult? lastTerrainClearResult, AreaUnlockResult? lastAreaUnlockResult, UpgradeResult? lastUpgradeResult, string? saveLoadMessage)
    {
        var y = viewport.Height - 34;
        var message = selectedBuildingId is null
            ? "SELECT BUILDING 1-6"
            : $"SELECTED {selectedBuildingId}";

        if (lastResearchResult is not null)
        {
            message = lastResearchResult.Success
                ? $"RESEARCH OK {lastResearchResult.ResearchId}"
                : $"RESEARCH FAILED {lastResearchResult.FailureReason}";
        }

        if (lastBuildResult is not null)
        {
            message = lastBuildResult.Success
                ? "BUILD OK"
                : $"BUILD FAILED {lastBuildResult.FailureReason}";
        }

        if (lastTerrainClearResult is not null)
        {
            message = lastTerrainClearResult.Success
                ? "TERRAIN CLEARED"
                : $"CLEAR FAILED {lastTerrainClearResult.FailureReason}";
        }

        if (lastAreaUnlockResult is not null)
        {
            message = lastAreaUnlockResult.Success
                ? $"AREA UNLOCKED {lastAreaUnlockResult.RevealedTileType}"
                : $"UNLOCK FAILED {lastAreaUnlockResult.FailureReason}";
        }

        if (lastUpgradeResult is not null)
        {
            message = lastUpgradeResult.Success
                ? $"UPGRADE OK {lastUpgradeResult.UpgradeId}"
                : $"UPGRADE FAILED {lastUpgradeResult.FailureReason}";
        }

        if (!string.IsNullOrWhiteSpace(saveLoadMessage))
            message = saveLoadMessage;

        spriteBatch.Draw(_pixel, new Rectangle(SideMenuWidth, viewport.Height - 44, Math.Max(0, viewport.Width - SideMenuWidth), 44), new Color(25, 31, 42));
        _text.DrawString(spriteBatch, message, new Vector2(SideMenuWidth + 14, y), new Color(230, 238, 245), 1);
        _text.DrawString(spriteBatch, "F5 SAVE  F9 LOAD  ESC SAVE+EXIT", new Vector2(Math.Max(SideMenuWidth + 360, viewport.Width - 300), y), new Color(170, 185, 205), 1);
    }

    private bool IsBuildingUnlocked(BuildingDefinition definition)
    {
        return string.IsNullOrWhiteSpace(definition.RequiredResearchId) ||
               _world.Research.IsCompleted(definition.RequiredResearchId);
    }

    private static Rectangle GetTopBarRectangle(Viewport viewport)
    {
        return new Rectangle(0, 0, viewport.Width, TopBarHeight);
    }

    private static Rectangle GetSideMenuRectangle(Viewport viewport)
    {
        return new Rectangle(0, TopBarHeight, SideMenuWidth, Math.Max(0, viewport.Height - TopBarHeight));
    }

    private static Rectangle GetSellButtonRectangle(Viewport viewport)
    {
        return new Rectangle(Math.Max(10, viewport.Width - 104), 19, 88, 36);
    }

    private static Rectangle GetEnergyFillBarRectangle(Viewport viewport)
    {
        var sellButton = GetSellButtonRectangle(viewport);
        var width = Math.Clamp(viewport.Width / 7, 120, 190);
        return new Rectangle(Math.Max(10, sellButton.X - width - 14), 21, width, 32);
    }

    private static Rectangle GetSelectedBuildingPanelRectangle(Viewport viewport)
    {
        var width = 360;
        var height = 286;
        return new Rectangle(Math.Max(SideMenuWidth + 10, viewport.Width - width - 20), Math.Max(TopBarHeight + 10, viewport.Height - height - 58), width, height);
    }

    private static Rectangle GetSelectedTerrainPanelRectangle(Viewport viewport)
    {
        var width = 330;
        var height = 170;
        return new Rectangle(Math.Max(SideMenuWidth + 10, viewport.Width - width - 20), Math.Max(TopBarHeight + 10, viewport.Height - height - 58), width, height);
    }

    private static Rectangle GetSelectedCloudPanelRectangle(Viewport viewport)
    {
        var width = 330;
        var height = 190;
        return new Rectangle(Math.Max(SideMenuWidth + 10, viewport.Width - width - 20), Math.Max(TopBarHeight + 10, viewport.Height - height - 58), width, height);
    }

    private static Rectangle GetReplaceButtonRectangle(Viewport viewport)
    {
        var panel = GetSelectedBuildingPanelRectangle(viewport);
        return new Rectangle(panel.X + 12, panel.Bottom - 44, panel.Width - 24, 32);
    }

    private static Rectangle GetClearTerrainButtonRectangle(Viewport viewport)
    {
        var panel = GetSelectedTerrainPanelRectangle(viewport);
        return new Rectangle(panel.X + 12, panel.Bottom - 44, panel.Width - 24, 32);
    }

    private static Rectangle GetUnlockCloudButtonRectangle(Viewport viewport)
    {
        var panel = GetSelectedCloudPanelRectangle(viewport);
        return new Rectangle(panel.X + 12, panel.Bottom - 44, panel.Width - 24, 32);
    }

    private static int GetBuildColumnX() => PanelMargin;
    private static int GetResearchColumnX() => PanelMargin + ColumnWidth + ColumnGap;
    private static int GetUpgradeColumnX() => PanelMargin + (ColumnWidth + ColumnGap) * 2;

    private static Rectangle GetBuildButtonRectangle(int index)
    {
        return new Rectangle(GetBuildColumnX(), MenuButtonsY + index * 58, ColumnWidth, 50);
    }

    private static Rectangle GetResearchButtonRectangle(int index)
    {
        return new Rectangle(GetResearchColumnX(), MenuButtonsY + index * 58, ColumnWidth, 50);
    }

    private static Rectangle GetUpgradeButtonRectangle(int index)
    {
        return new Rectangle(GetUpgradeColumnX(), MenuButtonsY + index * 60, ColumnWidth, 56);
    }

    private static string GetUpgradeCostText(UpgradeDefinition definition)
    {
        if (definition.CostMoney > 0 && definition.CostResearch > 0)
            return $"COST ${FormatNumber((double)definition.CostMoney)} R{FormatNumber(definition.CostResearch)}";

        if (definition.CostMoney > 0)
            return $"COST ${FormatNumber((double)definition.CostMoney)}";

        return $"COST R{FormatNumber(definition.CostResearch)}";
    }

    private static string GetUpgradeEffectText(UpgradeDefinition definition)
    {
        var percent = (definition.Multiplier - 1d) * 100d;
        var sign = percent >= 0 ? "+" : "";
        var amount = $"{sign}{percent:0.#}%";

        return definition.EffectType switch
        {
            UpgradeEffectType.MultiplyEnergyProduction => $"ENERGY {amount}",
            UpgradeEffectType.MultiplyLifetime => $"LIFE {amount}",
            UpgradeEffectType.MultiplyResearchProduction => $"RESEARCH {amount}",
            UpgradeEffectType.MultiplyBatteryCapacity => $"BATTERY CAP {amount}",
            UpgradeEffectType.MultiplyAutoSell => $"AUTO SELL {amount}",
            UpgradeEffectType.MultiplyHeatConversion => $"HEAT CONV {amount}",
            UpgradeEffectType.MultiplyToolAxesGeneration => $"AXES RATE {amount}",
            UpgradeEffectType.MultiplyToolMinesGeneration => $"MINES RATE {amount}",
            _ => definition.EffectType.ToString().ToUpperInvariant()
        };
    }

    private static Color GetBuildingColor(BuildingCategory category)
    {
        return category switch
        {
            BuildingCategory.PowerProducer => new Color(230, 238, 245),
            BuildingCategory.Storage => new Color(240, 205, 70),
            BuildingCategory.Automation => new Color(70, 165, 225),
            BuildingCategory.Research => new Color(160, 110, 230),
            BuildingCategory.HeatProducer => new Color(235, 130, 55),
            BuildingCategory.HeatConverter => new Color(70, 220, 190),
            BuildingCategory.Corporation => new Color(210, 210, 245),
            _ => new Color(220, 220, 220)
        };
    }

    private static Color GetStateColor(BuildingState state)
    {
        return state switch
        {
            BuildingState.Active => new Color(150, 235, 150),
            BuildingState.Expired => new Color(255, 210, 95),
            BuildingState.Exploded => new Color(255, 110, 90),
            _ => new Color(230, 238, 245)
        };
    }

    private Color GetHeatTextColor(double accumulatedHeat)
    {
        if (accumulatedHeat >= _world.HeatSettings.HeatExplosionThreshold)
            return new Color(255, 110, 90);

        if (accumulatedHeat >= _world.HeatSettings.HeatWarningThreshold)
            return new Color(245, 145, 55);

        return new Color(255, 210, 90);
    }

    private void DrawOutline(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
    {
        spriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Top, rect.Width, thickness), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Bottom - thickness, rect.Width, thickness), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Top, thickness, rect.Height), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Right - thickness, rect.Top, thickness, rect.Height), color);
    }

    private static string Shorten(string value, int maxLength)
    {
        if (value.Length <= maxLength)
            return value;

        return value[..Math.Max(0, maxLength - 1)] + ".";
    }

    private static string FormatNumber(double value)
    {
        if (value >= 1_000_000_000)
            return (value / 1_000_000_000d).ToString("0.##") + "B";

        if (value >= 1_000_000)
            return (value / 1_000_000d).ToString("0.##") + "M";

        if (value >= 1_000)
            return (value / 1_000d).ToString("0.##") + "K";

        return value.ToString("0.##");
    }

    private static string FormatNumberFixed2(double value)
    {
        if (value >= 1_000_000_000)
            return (value / 1_000_000_000d).ToString("0.00") + "B";

        if (value >= 1_000_000)
            return (value / 1_000_000d).ToString("0.00") + "M";

        if (value >= 1_000)
            return (value / 1_000d).ToString("0.00") + "K";

        return value.ToString("0.00");
    }


    private static string FormatSignedMoneyFixed2(double value)
    {
        if (value > 0)
            return "+$" + FormatNumberFixed2(value);

        if (value < 0)
            return "-$" + FormatNumberFixed2(Math.Abs(value));

        return "+$0.00";
    }

    private static string FormatSignedNumberFixed2(double value)
    {
        if (value > 0)
            return "+" + FormatNumberFixed2(value);

        if (value < 0)
            return "-" + FormatNumberFixed2(Math.Abs(value));

        return "+0.00";
    }
}
