using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Expansion;
using GridPowerTycoon.Core.Research;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Managers;
using GridPowerTycoon.Core.Operations;
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
    private const int MenuButtonHeight = 66;
    private const int MenuButtonStride = 74;

    private static readonly string[] BuildButtonIds =
    {
        "wind_turbine",
        "battery_small",
        "office_small",
        "research_small",
        "solar_panel",
        "generator_small",
        "coal_power_plant",
        "office_large",
        "generator_medium",
        "gas_power_plant",
        "research_large"
    };

    private static readonly string[] ResearchButtonIds =
    {
        "battery",
        "office_small",
        "solar_power",
        "generator_small",
        "wind_turbine_manager",
        "solar_panel_manager",
        "coal_power",
        "office_large",
        "generator_medium",
        "gas_power",
        "coal_power_manager",
        "gas_power_manager",
        "research_large"
    };

    private static readonly string[] UpgradeButtonIds =
    {
        "wind_turbine_energy_1",
        "wind_turbine_lifetime_1",
        "research_small_output_1",
        "battery_small_capacity_1",
        "office_small_sell_1",
        "generator_small_conversion_1",
        "solar_panel_heat_1",
        "solar_panel_lifetime_1",
        "axes_generation_1",
        "mines_generation_1",
        "coal_heat_1",
        "coal_lifetime_1",
        "office_large_sell_1",
        "generator_medium_conversion_1",
        "gas_heat_1",
        "gas_lifetime_1",
        "research_large_output_1"
    };

    private readonly GameWorld _world;
    private readonly Texture2D _pixel;
    private readonly PixelTextRenderer _text;

    private int _buildScrollOffset;
    private int _researchScrollOffset;
    private int _upgradeScrollOffset;

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
        GridPosition? selectedTilePosition,
        Guid? selectedMapBuildingId,
        GridPosition? selectedTerrainPosition,
        GridPosition? selectedCloudPosition,
        TerrainClearResult? lastTerrainClearResult,
        AreaUnlockResult? lastAreaUnlockResult,
        UpgradeResult? lastUpgradeResult,
        string? saveLoadMessage,
        Guid? pendingDemolishBuildingId)
    {
        var topBar = GetTopBarRectangle(viewport);
        var sideMenu = GetSideMenuRectangle(viewport);

        spriteBatch.Draw(_pixel, topBar, new Color(32, 39, 52));
        spriteBatch.Draw(_pixel, sideMenu, new Color(38, 48, 62));

        DrawTopBar(spriteBatch, topBar, viewport);
        DrawBuildMenu(spriteBatch, viewport, selectedBuildingId);
        DrawResearchMenu(spriteBatch, viewport);
        DrawUpgradeMenu(spriteBatch, viewport);
        DrawPropertiesPanel(spriteBatch, viewport, selectedBuildingId, selectedTilePosition, selectedMapBuildingId, selectedTerrainPosition, selectedCloudPosition, pendingDemolishBuildingId);
        DrawStatus(spriteBatch, viewport, selectedBuildingId, lastBuildResult, lastResearchResult, lastTerrainClearResult, lastAreaUnlockResult, lastUpgradeResult, saveLoadMessage, pendingDemolishBuildingId);
    }

    public bool IsMouseOverUi(Point mousePosition, Viewport viewport)
    {
        return GetTopBarRectangle(viewport).Contains(mousePosition) ||
               GetSideMenuRectangle(viewport).Contains(mousePosition) ||
               GetPropertiesPanelRectangle(viewport).Contains(mousePosition);
    }

    public void HandleScroll(Point mousePosition, int scrollDelta, Viewport viewport)
    {
        if (scrollDelta == 0)
            return;

        if (!GetSideMenuRectangle(viewport).Contains(mousePosition))
            return;

        const int scrollStep = 90;
        var delta = scrollDelta > 0 ? -scrollStep : scrollStep;

        if (mousePosition.X >= GetBuildColumnX() && mousePosition.X < GetBuildColumnX() + ColumnWidth)
        {
            _buildScrollOffset = ClampScrollOffset(_buildScrollOffset + delta, BuildButtonIds.Length, MenuButtonStride, viewport);
            return;
        }

        if (mousePosition.X >= GetResearchColumnX() && mousePosition.X < GetResearchColumnX() + ColumnWidth)
        {
            _researchScrollOffset = ClampScrollOffset(_researchScrollOffset + delta, ResearchButtonIds.Length, MenuButtonStride, viewport);
            return;
        }

        if (mousePosition.X >= GetUpgradeColumnX() && mousePosition.X < GetUpgradeColumnX() + ColumnWidth)
            _upgradeScrollOffset = ClampScrollOffset(_upgradeScrollOffset + delta, UpgradeButtonIds.Length, MenuButtonStride, viewport);
    }

    public bool IsSellButtonAt(Point mousePosition, Viewport viewport)
    {
        return GetSellButtonRectangle(viewport).Contains(mousePosition);
    }

    public bool IsSaveButtonAt(Point mousePosition, Viewport viewport)
    {
        return GetSaveButtonRectangle(viewport).Contains(mousePosition);
    }

    public bool IsLoadButtonAt(Point mousePosition, Viewport viewport)
    {
        return GetLoadButtonRectangle(viewport).Contains(mousePosition);
    }

    public bool IsNewGameButtonAt(Point mousePosition, Viewport viewport)
    {
        return GetNewGameButtonRectangle(viewport).Contains(mousePosition);
    }

    public bool IsToggleFullscreenButtonAt(Point mousePosition, Viewport viewport)
    {
        return GetToggleFullscreenButtonRectangle(viewport).Contains(mousePosition);
    }

    public bool IsExitButtonAt(Point mousePosition, Viewport viewport)
    {
        return GetExitButtonRectangle(viewport).Contains(mousePosition);
    }

    public bool IsReplaceButtonAt(Point mousePosition, Viewport viewport, Guid? selectedMapBuildingId)
    {
        if (!selectedMapBuildingId.HasValue)
            return false;

        if (!CanReplaceSelectedBuilding(selectedMapBuildingId.Value))
            return false;

        return GetReplaceButtonRectangle(viewport).Contains(mousePosition);
    }

    public bool IsDemolishButtonAt(Point mousePosition, Viewport viewport, Guid? selectedMapBuildingId)
    {
        if (!selectedMapBuildingId.HasValue)
            return false;

        if (!_world.TryGetBuilding(selectedMapBuildingId.Value, out _))
            return false;

        return GetDemolishButtonRectangle(viewport).Contains(mousePosition);
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

    public bool TryGetBuildingButtonAt(Point mousePosition, Viewport viewport, out string? buildingId)
    {
        for (var i = 0; i < BuildButtonIds.Length; i++)
        {
            var rect = GetBuildButtonRectangle(i);
            if (IsMenuButtonVisible(rect, viewport) && rect.Contains(mousePosition))
            {
                buildingId = BuildButtonIds[i];
                return true;
            }
        }

        buildingId = null;
        return false;
    }

    public bool TryGetResearchButtonAt(Point mousePosition, Viewport viewport, out string researchId)
    {
        for (var i = 0; i < ResearchButtonIds.Length; i++)
        {
            var rect = GetResearchButtonRectangle(i);
            if (IsMenuButtonVisible(rect, viewport) && rect.Contains(mousePosition))
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
            var rect = GetUpgradeButtonRectangle(i);
            if (IsMenuButtonVisible(rect, viewport) && rect.Contains(mousePosition))
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

    private void DrawBuildMenu(SpriteBatch spriteBatch, Viewport viewport, string? selectedBuildingId)
    {
        var headerX = GetBuildColumnX();
        _text.DrawString(spriteBatch, "BUILD", new Vector2(headerX, MenuHeaderY), new Color(230, 238, 245), 2);

        DrawColumnScrollHint(spriteBatch, viewport, headerX, _buildScrollOffset, BuildButtonIds.Length, MenuButtonStride);

        for (var i = 0; i < BuildButtonIds.Length; i++)
        {
            var id = BuildButtonIds[i];
            var rect = GetBuildButtonRectangle(i);
            if (!IsMenuButtonVisible(rect, viewport))
                continue;
            var isSelected = string.Equals(selectedBuildingId, id, StringComparison.OrdinalIgnoreCase);

            _world.BuildingCatalog.TryGet(id, out var definition);
            var isLocked = definition is not null && !IsBuildingUnlocked(definition);
            var canAfford = definition is null || _world.Resources.Money >= definition.Cost;

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
            var textColor = isLocked ? new Color(150, 155, 165) : new Color(235, 240, 245);
            var costColor = isLocked
                ? new Color(255, 150, 120)
                : canAfford
                    ? new Color(255, 225, 120)
                    : new Color(255, 110, 90);
            var costText = definition is null
                ? "BUILD COST ?"
                : isLocked
                    ? "LOCKED"
                    : canAfford
                        ? $"BUILD COST ${FormatNumber((double)definition.Cost)}"
                        : $"NEED ${FormatNumber((double)definition.Cost)}";
            var netText = definition is null ? "NET ENERGY ?" : $"NET ENERGY {FormatNetEnergy(definition)}";
            var heatText = definition is null ? "HEAT ?" : GetBuildButtonHeatText(definition);
            var purposeText = definition is null ? id : GetBuildingPurposeText(definition);

            _text.DrawString(spriteBatch, $"{indexText} {Shorten(name, 17)}", new Vector2(rect.X + 44, rect.Y + 6), textColor, 1);
            _text.DrawString(spriteBatch, Shorten(costText, 24), new Vector2(rect.X + 44, rect.Y + 21), costColor, 1);
            _text.DrawString(spriteBatch, Shorten($"{netText} | {heatText}", 30), new Vector2(rect.X + 8, rect.Y + 39), new Color(170, 210, 235), 1);
            _text.DrawString(spriteBatch, Shorten(purposeText, 30), new Vector2(rect.X + 8, rect.Y + 53), isLocked ? new Color(135, 140, 150) : new Color(175, 188, 205), 1);
        }
    }

    private void DrawResearchMenu(SpriteBatch spriteBatch, Viewport viewport)
    {
        var headerX = GetResearchColumnX();
        _text.DrawString(spriteBatch, "RESEARCH", new Vector2(headerX, MenuHeaderY), new Color(230, 238, 245), 2);

        DrawColumnScrollHint(spriteBatch, viewport, headerX, _researchScrollOffset, ResearchButtonIds.Length, MenuButtonStride);

        for (var i = 0; i < ResearchButtonIds.Length; i++)
        {
            var id = ResearchButtonIds[i];
            var rect = GetResearchButtonRectangle(i);
            if (!IsMenuButtonVisible(rect, viewport))
                continue;

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
            var status = completed ? "DONE" : missingPrereq ? "REQ RESEARCH" : canAfford ? $"COST R{cost}" : $"NEED R{cost}";
            var unlockText = definition is null ? "" : GetResearchUnlockText(definition);
            var description = definition is null ? id : GetResearchPurposeText(definition);

            _text.DrawString(spriteBatch, Shorten(name, 23), new Vector2(rect.X + 8, rect.Y + 6), new Color(235, 240, 245), 1);
            _text.DrawString(spriteBatch, Shorten(status, 26), new Vector2(rect.X + 8, rect.Y + 21), completed ? new Color(160, 245, 175) : new Color(210, 190, 255), 1);
            _text.DrawString(spriteBatch, Shorten(unlockText, 30), new Vector2(rect.X + 8, rect.Y + 39), new Color(190, 215, 255), 1);
            _text.DrawString(spriteBatch, Shorten(description, 30), new Vector2(rect.X + 8, rect.Y + 53), new Color(175, 188, 205), 1);
        }
    }

    private void DrawUpgradeMenu(SpriteBatch spriteBatch, Viewport viewport)
    {
        var headerX = GetUpgradeColumnX();
        _text.DrawString(spriteBatch, "UPGRADE", new Vector2(headerX, MenuHeaderY), new Color(230, 238, 245), 2);

        DrawColumnScrollHint(spriteBatch, viewport, headerX, _upgradeScrollOffset, UpgradeButtonIds.Length, MenuButtonStride);

        for (var i = 0; i < UpgradeButtonIds.Length; i++)
        {
            var id = UpgradeButtonIds[i];
            var rect = GetUpgradeButtonRectangle(i);
            if (!IsMenuButtonVisible(rect, viewport))
                continue;

            _world.UpgradeCatalog.TryGet(id, out var definition);
            var level = definition is null ? 0 : _world.Upgrades.GetLevel(id);
            var completed = definition is not null && level >= definition.MaxLevel;
            var missingResearch = definition is not null &&
                                  !string.IsNullOrWhiteSpace(definition.RequiredResearchId) &&
                                  !_world.Research.IsCompleted(definition.RequiredResearchId);
            var nextMoneyCost = definition is null ? 0m : UpgradeSystem.GetMoneyCost(definition, level);
            var nextResearchCost = definition is null ? 0d : UpgradeSystem.GetResearchCost(definition, level);
            var canAfford = definition is not null &&
                            _world.Resources.Money >= nextMoneyCost &&
                            _world.Resources.Research >= nextResearchCost;

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
                    ? $"MAX LV {level}"
                    : missingResearch
                        ? "REQ RESEARCH"
                        : GetUpgradeCostText(definition, level);

            var levelText = definition is null ? "" : $"LV {level}/{definition.MaxLevel}";
            _text.DrawString(spriteBatch, Shorten(name, 18), new Vector2(rect.X + 8, rect.Y + 6), new Color(235, 240, 245), 1);
            _text.DrawString(spriteBatch, levelText, new Vector2(rect.Right - 62, rect.Y + 6), new Color(180, 210, 240), 1);
            _text.DrawString(spriteBatch, Shorten(effect, 24), new Vector2(rect.X + 8, rect.Y + 21), new Color(190, 215, 255), 1);
            _text.DrawString(spriteBatch, Shorten(status, 27), new Vector2(rect.X + 8, rect.Y + 39), completed ? new Color(160, 245, 175) : new Color(255, 225, 120), 1);
            _text.DrawString(spriteBatch, Shorten(GetUpgradePurposeText(definition), 30), new Vector2(rect.X + 8, rect.Y + 53), new Color(175, 188, 205), 1);
        }
    }


    private void DrawPropertiesPanel(
        SpriteBatch spriteBatch,
        Viewport viewport,
        string? selectedBuildingId,
        GridPosition? selectedTilePosition,
        Guid? selectedMapBuildingId,
        GridPosition? selectedTerrainPosition,
        GridPosition? selectedCloudPosition,
        Guid? pendingDemolishBuildingId)
    {
        var panel = GetPropertiesPanelRectangle(viewport);
        spriteBatch.Draw(_pixel, panel, new Color(30, 38, 52, 240));
        DrawOutline(spriteBatch, panel, new Color(95, 115, 140), 2);

        var title = "PROPERTIES";
        var subtitle = selectedTilePosition.HasValue
            ? $"CELL {selectedTilePosition.Value.X},{selectedTilePosition.Value.Y}"
            : "NO CELL SELECTED";

        var rows = CreateEmptyPropertyRows();
        Color stateColor = new Color(210, 222, 235);
        var action = "-";

        if (selectedMapBuildingId.HasValue &&
            _world.TryGetBuilding(selectedMapBuildingId.Value, out var instance) &&
            _world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
        {
            title = Shorten(definition.Name.ToUpperInvariant(), 26);
            FillBuildingPropertyRows(rows, instance, definition, out stateColor, out action);
        }
        else if (selectedTerrainPosition.HasValue && _world.Map.Contains(selectedTerrainPosition.Value))
        {
            var tile = _world.Map.GetTile(selectedTerrainPosition.Value);
            FillTerrainPropertyRows(rows, tile, out stateColor, out action);
            title = tile.Type == TileType.Forest ? "FOREST" : tile.Type == TileType.Mountain ? "MOUNTAIN" : "TERRAIN";
        }
        else if (selectedCloudPosition.HasValue && _world.Map.Contains(selectedCloudPosition.Value))
        {
            var tile = _world.Map.GetTile(selectedCloudPosition.Value);
            FillCloudPropertyRows(rows, tile, selectedCloudPosition.Value, out stateColor, out action);
            title = "CLOUD AREA";
        }
        else if (selectedTilePosition.HasValue && _world.Map.Contains(selectedTilePosition.Value))
        {
            var tile = _world.Map.GetTile(selectedTilePosition.Value);
            FillEmptyTilePropertyRows(rows, tile, selectedBuildingId, out stateColor, out action);
            title = GetTileDisplayName(tile.Type).ToUpperInvariant();
        }
        else if (!string.IsNullOrWhiteSpace(selectedBuildingId) &&
                 _world.BuildingCatalog.TryGet(selectedBuildingId, out var selectedDefinition))
        {
            FillBuildToolPropertyRows(rows, selectedDefinition, out stateColor, out action);
            title = "BUILD TOOL";
            subtitle = "RIGHT CLICK TO CANCEL";
        }

        _text.DrawString(spriteBatch, title, new Vector2(panel.X + 14, panel.Y + 14), new Color(235, 240, 245), 2);
        _text.DrawString(spriteBatch, subtitle, new Vector2(panel.X + 14, panel.Y + 48), new Color(170, 185, 205), 1);

        var y = selectedMapBuildingId.HasValue ? panel.Y + 116 : panel.Y + 74;
        var rowIndex = 0;
        foreach (var key in PropertyRowKeys)
        {
            var value = rows.TryGetValue(key, out var rowValue) ? rowValue : "-";
            var valueColor = key == "STATE" ? stateColor : GetPropertyValueColor(key);
            DrawPropertyRow(spriteBatch, panel, ref y, rowIndex++, key, value, valueColor);
        }

        DrawPropertyRow(spriteBatch, panel, ref y, rowIndex, "ACTION", action, new Color(255, 225, 120));
        DrawContextActionButton(spriteBatch, viewport, selectedMapBuildingId, selectedTerrainPosition, selectedCloudPosition, pendingDemolishBuildingId);
    }

    private static readonly string[] PropertyRowKeys =
    {
        "TYPE",
        "STATE",
        "BUILD TOOL",
        "BUILD COST",
        "NEXT UPGRADE",
        "MONEY/S",
        "NET ENERGY",
        "PAYBACK",
        "LIFE",
        "MANAGED",
        "ENERGY IN",
        "ENERGY OUT",
        "HEAT OUT",
        "HEAT STORED",
        "HEAT IN",
        "HEAT TO ENERGY",
        "RESEARCH OUT",
        "AUTO SELL",
        "BATTERY",
        "SIZE",
        "REVEAL",
        "CLEAR COST",
        "UNLOCK COST"
    };

    private static Dictionary<string, string> CreateEmptyPropertyRows()
    {
        return PropertyRowKeys.ToDictionary(key => key, _ => "-");
    }

    private void FillBuildingPropertyRows(
        Dictionary<string, string> rows,
        BuildingInstance instance,
        BuildingDefinition definition,
        out Color stateColor,
        out string action)
    {
        var status = BuildingOperationalStatusCalculator.Calculate(_world, instance);
        var effectiveLifetime = UpgradeCalculator.GetLifetimeSeconds(_world, definition);
        var managed = ManagerSystem.IsManaged(_world, definition.Id);

        rows["TYPE"] = definition.Category.ToString().ToUpperInvariant();
        rows["STATE"] = status.Label;
        rows["BUILD COST"] = "$" + FormatNumber((double)definition.Cost);
        rows["NEXT UPGRADE"] = GetNextUpgradeCostText(definition);
        rows["MONEY/S"] = FormatEstimatedMoneyPerSecond(GetEstimatedMoneyPerSecond(status));
        rows["NET ENERGY"] = FormatNetEnergy(status);
        rows["PAYBACK"] = FormatPayback(definition.Cost, GetEstimatedMoneyPerSecond(status));
        rows["LIFE"] = FormatLifetime(instance.RemainingLifetimeSeconds, effectiveLifetime);
        rows["MANAGED"] = managed ? "YES" : "NO";
        rows["ENERGY IN"] = status.EnergyInputPerSecond > 0 ? $"-{FormatNumber(status.EnergyInputPerSecond)}/S" : "-";
        rows["ENERGY OUT"] = FormatEffectiveGross(status.EnergyOutputPerSecond, UpgradeCalculator.GetEnergyPerSecond(_world, definition));
        rows["HEAT OUT"] = FormatEffectiveGross(status.HeatOutputPerSecond, UpgradeCalculator.GetHeatPerSecond(_world, definition));
        rows["HEAT STORED"] = (instance.AccumulatedHeat > 0 || UpgradeCalculator.GetHeatPerSecond(_world, definition) > 0)
            ? $"{FormatNumber(status.HeatStored)} / {FormatNumber(status.HeatExplosionThreshold)}"
            : "-";
        rows["HEAT IN"] = UpgradeCalculator.GetHeatConversionPerSecond(_world, definition) > 0
            ? $"ABSORBS {FormatNumber(status.HeatConversionInputPerSecond)}/S, RANGE {definition.HeatRange} CELLS"
            : "-";
        rows["HEAT TO ENERGY"] = UpgradeCalculator.GetHeatConversionPerSecond(_world, definition) > 0
            ? $"PRODUCES {FormatNumber(status.HeatConversionEnergyOutputPerSecond)}/S ENERGY"
            : "-";
        rows["RESEARCH OUT"] = FormatEffectiveGross(status.ResearchOutputPerSecond, UpgradeCalculator.GetResearchPerSecond(_world, definition));
        rows["AUTO SELL"] = FormatEffectiveGross(status.AutoSellInputPerSecond, UpgradeCalculator.GetAutoSellPerSecond(_world, definition));
        rows["BATTERY"] = status.BatteryCapacity > 0 ? "+" + FormatNumber(status.BatteryCapacity) : "-";
        rows["SIZE"] = $"{definition.Width} X {definition.Height}";

        stateColor = GetOperationalStateColor(status.State);
        if (instance.State == BuildingState.Exploded)
            action = _world.Resources.Money >= definition.Cost ? "RESTORE OR DEMOLISH" : "NEED MONEY TO RESTORE";
        else if (instance.State == BuildingState.Expired)
            action = _world.Resources.Money >= definition.Cost ? "REPLACE OR DEMOLISH" : "NEED MONEY TO REPLACE";
        else
            action = "DEMOLISH AVAILABLE";
    }

    private void FillTerrainPropertyRows(Dictionary<string, string> rows, Tile tile, out Color stateColor, out string action)
    {
        rows["TYPE"] = tile.Type.ToString().ToUpperInvariant();
        rows["STATE"] = "BLOCKED";
        rows["SIZE"] = "1 X 1";

        if (tile.Type == TileType.Forest)
        {
            rows["CLEAR COST"] = $"{_world.ToolSettings.ForestClearAxesCost} AXES";
            action = _world.Resources.Axes >= _world.ToolSettings.ForestClearAxesCost ? "CLEAR AVAILABLE" : "NEED AXES";
        }
        else if (tile.Type == TileType.Mountain)
        {
            rows["CLEAR COST"] = $"{_world.ToolSettings.MountainClearMinesCost} MINES";
            action = _world.Resources.Mines >= _world.ToolSettings.MountainClearMinesCost ? "CLEAR AVAILABLE" : "NEED MINES";
        }
        else
        {
            action = "-";
        }

        stateColor = new Color(255, 210, 95);
    }

    private void FillCloudPropertyRows(Dictionary<string, string> rows, Tile tile, GridPosition position, out Color stateColor, out string action)
    {
        var revealText = tile.CoveredType.HasValue
            ? tile.CoveredType.Value.ToString().ToUpperInvariant()
            : "UNKNOWN";
        var tilesToUnlock = CountUnlockableCloudTiles(position);

        rows["TYPE"] = "CLOUD";
        rows["STATE"] = "LOCKED";
        rows["SIZE"] = "1 X 1";
        rows["REVEAL"] = $"{revealText}, {tilesToUnlock} TILE(S)";
        rows["UNLOCK COST"] = $"${FormatNumber((double)_world.AreaUnlockSettings.CloudUnlockMoneyCost)} + R{FormatNumber(_world.AreaUnlockSettings.CloudUnlockResearchCost)}";
        action = CanUnlockCloud() ? "UNLOCK AVAILABLE" : "NEED RESOURCES";
        stateColor = new Color(145, 155, 170);
    }

    private void FillBuildToolPropertyRows(Dictionary<string, string> rows, BuildingDefinition definition, out Color stateColor, out string action)
    {
        rows["TYPE"] = definition.Category.ToString().ToUpperInvariant();
        rows["BUILD TOOL"] = Shorten(definition.Name.ToUpperInvariant(), 24);
        rows["BUILD COST"] = "$" + FormatNumber((double)definition.Cost);
        rows["NEXT UPGRADE"] = GetNextUpgradeCostText(definition);
        rows["MONEY/S"] = FormatEstimatedMoneyPerSecond(GetEstimatedMoneyPerSecond(definition));
        rows["NET ENERGY"] = FormatNetEnergy(definition);
        rows["PAYBACK"] = FormatPayback(definition.Cost, GetEstimatedMoneyPerSecond(definition));
        rows["SIZE"] = $"{definition.Width} X {definition.Height}";
        action = _world.Resources.Money >= definition.Cost
            ? "LEFT CLICK PLAIN CELL"
            : "NEED MONEY TO BUILD";
        stateColor = new Color(255, 220, 80);
    }

    private void FillEmptyTilePropertyRows(Dictionary<string, string> rows, Tile tile, string? selectedBuildingId, out Color stateColor, out string action)
    {
        rows["TYPE"] = GetTileDisplayName(tile.Type).ToUpperInvariant();
        rows["STATE"] = tile.Type == TileType.Land ? "FREE / BUILDABLE" : "NOT BUILDABLE";

        if (!string.IsNullOrWhiteSpace(selectedBuildingId) &&
            _world.BuildingCatalog.TryGet(selectedBuildingId, out var selectedDefinition))
        {
            rows["BUILD TOOL"] = Shorten(selectedDefinition.Name.ToUpperInvariant(), 24);
            rows["BUILD COST"] = "$" + FormatNumber((double)selectedDefinition.Cost);
            rows["MONEY/S"] = FormatEstimatedMoneyPerSecond(GetEstimatedMoneyPerSecond(selectedDefinition));
            rows["NET ENERGY"] = FormatNetEnergy(selectedDefinition);
            rows["PAYBACK"] = FormatPayback(selectedDefinition.Cost, GetEstimatedMoneyPerSecond(selectedDefinition));
            rows["SIZE"] = $"{selectedDefinition.Width} X {selectedDefinition.Height}";

            if (tile.Type != TileType.Land)
                action = "TOOL ACTIVE - NOT BUILDABLE";
            else if (_world.Resources.Money < selectedDefinition.Cost)
                action = "TOOL ACTIVE - NEED MONEY";
            else
                action = "TOOL ACTIVE - CLICK TO BUILD";
        }
        else
        {
            rows["SIZE"] = "1 X 1";
            action = tile.Type == TileType.Land
                ? "SELECT A BUILDING TO BUILD HERE"
                : "NO BUILD ACTION";
        }

        stateColor = tile.Type == TileType.Land
            ? new Color(150, 235, 150)
            : new Color(210, 222, 235);
    }

    private static string GetTileDisplayName(TileType type)
    {
        return type switch
        {
            TileType.Land => "Plain",
            TileType.Forest => "Forest",
            TileType.Mountain => "Mountain",
            TileType.Water => "Water",
            TileType.Cloud => "Cloud",
            _ => type.ToString()
        };
    }

    private string GetBuildButtonHeatText(BuildingDefinition definition)
    {
        var heatOut = UpgradeCalculator.GetHeatPerSecond(_world, definition);
        var heatIn = UpgradeCalculator.GetHeatConversionPerSecond(_world, definition);

        if (heatOut > 0)
            return $"HEAT +{FormatNumber(heatOut)}/S";

        if (heatIn > 0)
            return $"HEAT IN {FormatNumber(heatIn)}/S";

        return "NO HEAT";
    }

    private static string GetBuildingPurposeText(BuildingDefinition definition)
    {
        if (!string.IsNullOrWhiteSpace(definition.Description))
            return definition.Description;

        return definition.Category switch
        {
            BuildingCategory.PowerProducer => "Produces energy.",
            BuildingCategory.Storage => "Increases energy storage.",
            BuildingCategory.Automation => "Sells energy automatically.",
            BuildingCategory.Research => "Produces research points.",
            BuildingCategory.HeatProducer => "Produces heat for generators.",
            BuildingCategory.HeatConverter => "Converts heat into energy.",
            BuildingCategory.Corporation => "Advanced economic building.",
            _ => "Building."
        };
    }

    private string GetResearchUnlockText(ResearchDefinition definition)
    {
        if (definition.UnlockBuildingIds.Count > 0)
        {
            var firstId = definition.UnlockBuildingIds[0];
            var firstName = _world.BuildingCatalog.TryGet(firstId, out var building) ? building.Name : firstId;
            return definition.UnlockBuildingIds.Count == 1
                ? "UNLOCKS " + firstName
                : $"UNLOCKS {firstName} +{definition.UnlockBuildingIds.Count - 1}";
        }

        if (definition.ManagedBuildingIds.Count > 0)
        {
            var firstId = definition.ManagedBuildingIds[0];
            var firstName = _world.BuildingCatalog.TryGet(firstId, out var building) ? building.Name : firstId;
            return definition.ManagedBuildingIds.Count == 1
                ? "MANAGES " + firstName
                : $"MANAGES {firstName} +{definition.ManagedBuildingIds.Count - 1}";
        }

        return "IMPROVES GRID";
    }

    private static string GetResearchPurposeText(ResearchDefinition definition)
    {
        return string.IsNullOrWhiteSpace(definition.Description)
            ? "Research upgrade."
            : definition.Description;
    }

    private static string GetUpgradePurposeText(UpgradeDefinition? definition)
    {
        if (definition is null)
            return "Upgrade.";

        return string.IsNullOrWhiteSpace(definition.Description)
            ? "Improves selected building."
            : definition.Description;
    }

    private string GetNextUpgradeCostText(BuildingDefinition definition)
    {
        var candidates = _world.UpgradeCatalog.All
            .Where(upgrade => string.Equals(upgrade.TargetBuildingId, definition.Id, StringComparison.OrdinalIgnoreCase))
            .Where(upgrade => _world.Upgrades.GetLevel(upgrade.Id) < upgrade.MaxLevel)
            .OrderBy(upgrade => UpgradeSystem.GetMoneyCost(upgrade, _world.Upgrades.GetLevel(upgrade.Id)))
            .ThenBy(upgrade => UpgradeSystem.GetResearchCost(upgrade, _world.Upgrades.GetLevel(upgrade.Id)))
            .ToList();

        if (candidates.Count == 0)
            return "NO UPGRADE";

        var next = candidates[0];
        var level = _world.Upgrades.GetLevel(next.Id);

        if (!string.IsNullOrWhiteSpace(next.RequiredResearchId) &&
            !_world.Research.IsCompleted(next.RequiredResearchId))
        {
            return "REQ RESEARCH";
        }

        var money = UpgradeSystem.GetMoneyCost(next, level);
        var research = UpgradeSystem.GetResearchCost(next, level);

        if (money > 0 && research > 0)
            return $"${FormatNumber((double)money)} + R{FormatNumber(research)}";

        if (money > 0)
            return "$" + FormatNumber((double)money);

        if (research > 0)
            return "R" + FormatNumber(research);

        return "FREE";
    }

    private double GetEstimatedMoneyPerSecond(BuildingOperationalStatus status)
    {
        var netEnergy = GetNetEnergyPerSecond(status);
        var sellableEnergy = Math.Max(0, netEnergy);
        var manualSellMoney = sellableEnergy * (double)_world.EconomySettings.EnergySellValue * _world.EconomySettings.ManualSellMultiplier;
        var autoSellMoney = Math.Max(0, status.AutoSellInputPerSecond) * (double)_world.EconomySettings.EnergySellValue * _world.EconomySettings.AutoSellMultiplier;
        return manualSellMoney + autoSellMoney;
    }

    private double GetEstimatedMoneyPerSecond(BuildingDefinition definition)
    {
        var energyOut = UpgradeCalculator.GetEnergyPerSecond(_world, definition);
        var heatConversionOut = UpgradeCalculator.GetHeatConversionPerSecond(_world, definition);
        var energyIn = UpgradeCalculator.GetEnergyConsumptionPerSecond(_world, definition);
        var autoSell = UpgradeCalculator.GetAutoSellPerSecond(_world, definition);
        var netEnergy = energyOut + heatConversionOut - energyIn - autoSell;
        var sellableEnergy = Math.Max(0, netEnergy);
        var manualSellMoney = sellableEnergy * (double)_world.EconomySettings.EnergySellValue * _world.EconomySettings.ManualSellMultiplier;
        var autoSellMoney = Math.Max(0, autoSell) * (double)_world.EconomySettings.EnergySellValue * _world.EconomySettings.AutoSellMultiplier;
        return manualSellMoney + autoSellMoney;
    }

    private static double GetNetEnergyPerSecond(BuildingOperationalStatus status)
    {
        return status.EnergyOutputPerSecond +
               status.HeatConversionEnergyOutputPerSecond -
               status.EnergyInputPerSecond -
               status.AutoSellInputPerSecond;
    }

    private static string FormatEstimatedMoneyPerSecond(double moneyPerSecond)
    {
        return moneyPerSecond > 0
            ? "+$" + FormatNumber(moneyPerSecond) + "/S APPROX"
            : "-";
    }

    private static string FormatNetEnergy(BuildingOperationalStatus status)
    {
        return FormatSignedPerSecond(GetNetEnergyPerSecond(status));
    }

    private string FormatNetEnergy(BuildingDefinition definition)
    {
        var netEnergy = UpgradeCalculator.GetEnergyPerSecond(_world, definition) +
                        UpgradeCalculator.GetHeatConversionPerSecond(_world, definition) -
                        UpgradeCalculator.GetEnergyConsumptionPerSecond(_world, definition) -
                        UpgradeCalculator.GetAutoSellPerSecond(_world, definition);

        return FormatSignedPerSecond(netEnergy);
    }

    private static string FormatSignedPerSecond(double value)
    {
        if (Math.Abs(value) < 0.0001)
            return "0/S";

        return (value > 0 ? "+" : "-") + FormatNumber(Math.Abs(value)) + "/S";
    }

    private static string FormatLifetime(double remainingSeconds, double effectiveLifetimeSeconds)
    {
        return effectiveLifetimeSeconds <= 0
            ? "-"
            : $"{Math.Ceiling(remainingSeconds):0} S / {effectiveLifetimeSeconds:0} S";
    }

    private static string FormatPayback(decimal cost, double moneyPerSecond)
    {
        if (cost <= 0 || moneyPerSecond <= 0)
            return "-";

        var seconds = (double)cost / moneyPerSecond;
        if (seconds < 1)
            return "ABOUT 1 SEC";

        if (seconds < 60)
            return "ABOUT " + Math.Ceiling(seconds).ToString("0") + " SEC";

        var minutes = seconds / 60d;
        if (minutes < 60)
            return "ABOUT " + Math.Ceiling(minutes).ToString("0") + " MIN";

        var hours = minutes / 60d;
        return "ABOUT " + Math.Ceiling(hours).ToString("0") + " H";
    }

    private static string FormatEffectiveGross(double effective, double gross)
    {
        if (gross <= 0 && effective <= 0)
            return "-";

        if (Math.Abs(effective - gross) > 0.0001)
            return $"+{FormatNumber(effective)}/S ({FormatNumber(gross)})";

        return $"+{FormatNumber(effective)}/S";
    }

    private void DrawPropertyRow(SpriteBatch spriteBatch, Rectangle panel, ref int y, int rowIndex, string label, string value, Color valueColor)
    {
        var row = new Rectangle(panel.X + 10, y - 3, panel.Width - 20, 18);
        if (rowIndex % 2 == 0)
            spriteBatch.Draw(_pixel, row, new Color(36, 45, 60, 120));

        _text.DrawString(spriteBatch, label, new Vector2(panel.X + 14, y), new Color(155, 170, 190), 1);
        _text.DrawString(spriteBatch, Shorten(value, 27), new Vector2(panel.X + 146, y), valueColor, 1);
        y += 20;
    }

    private void DrawContextActionButton(SpriteBatch spriteBatch, Viewport viewport, Guid? selectedMapBuildingId, GridPosition? selectedTerrainPosition, GridPosition? selectedCloudPosition, Guid? pendingDemolishBuildingId)
    {
        if (selectedMapBuildingId.HasValue && _world.TryGetBuilding(selectedMapBuildingId.Value, out var instance))
        {
            var demolishButton = GetDemolishButtonRectangle(viewport);
            var isConfirmingDemolish = pendingDemolishBuildingId == instance.Id;
            spriteBatch.Draw(_pixel, demolishButton, isConfirmingDemolish ? new Color(145, 35, 35) : new Color(96, 58, 58));
            DrawOutline(spriteBatch, demolishButton, isConfirmingDemolish ? new Color(255, 230, 120) : new Color(210, 120, 120), 2);
            _text.DrawString(spriteBatch, isConfirmingDemolish ? "CONFIRM DEMOLISH" : "DEMOLISH", new Vector2(demolishButton.X + 12, demolishButton.Y + 10), new Color(245, 225, 225), 1);

            if (instance.State == BuildingState.Expired || instance.State == BuildingState.Exploded)
            {
                var replaceButton = GetReplaceButtonRectangle(viewport);
                var canReplace = CanReplaceSelectedBuilding(instance.Id);
                var isRestore = instance.State == BuildingState.Exploded;
                var label = isRestore ? "RESTORE" : "REPLACE";
                var background = canReplace
                    ? isRestore ? new Color(118, 72, 72) : new Color(88, 118, 72)
                    : new Color(70, 70, 76);
                var outline = canReplace
                    ? isRestore ? new Color(245, 150, 140) : new Color(180, 235, 135)
                    : new Color(115, 115, 125);
                var text = canReplace ? label : label + " NEED $" + GetSelectedBuildingCostText(instance.Id);
                var textColor = canReplace ? new Color(235, 250, 220) : new Color(165, 165, 175);

                spriteBatch.Draw(_pixel, replaceButton, background);
                DrawOutline(spriteBatch, replaceButton, outline, 2);
                _text.DrawString(spriteBatch, Shorten(text, 31), new Vector2(replaceButton.X + 12, replaceButton.Y + 10), textColor, 1);
            }

            return;
        }

        if (selectedTerrainPosition.HasValue && _world.Map.Contains(selectedTerrainPosition.Value))
        {
            var tile = _world.Map.GetTile(selectedTerrainPosition.Value);
            if (tile.Type == TileType.Forest || tile.Type == TileType.Mountain)
            {
                var clearButton = GetClearTerrainButtonRectangle(viewport);
                spriteBatch.Draw(_pixel, clearButton, new Color(88, 118, 72));
                DrawOutline(spriteBatch, clearButton, new Color(180, 235, 135), 2);
                _text.DrawString(spriteBatch, "CLEAR", new Vector2(clearButton.X + 12, clearButton.Y + 10), new Color(235, 250, 220), 1);
            }

            return;
        }

        if (selectedCloudPosition.HasValue && _world.Map.Contains(selectedCloudPosition.Value))
        {
            var tile = _world.Map.GetTile(selectedCloudPosition.Value);
            if (tile.Type == TileType.Cloud)
            {
                var unlockButton = GetUnlockCloudButtonRectangle(viewport);
                spriteBatch.Draw(_pixel, unlockButton, CanUnlockCloud() ? new Color(88, 118, 72) : new Color(80, 80, 84));
                DrawOutline(spriteBatch, unlockButton, CanUnlockCloud() ? new Color(180, 235, 135) : new Color(120, 120, 130), 2);
                _text.DrawString(spriteBatch, "UNLOCK", new Vector2(unlockButton.X + 12, unlockButton.Y + 10), new Color(235, 250, 220), 1);
            }
        }
    }

    private bool CanReplaceSelectedBuilding(Guid buildingId)
    {
        if (!_world.TryGetBuilding(buildingId, out var instance))
            return false;

        if (!_world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
            return false;

        if (instance.State != BuildingState.Expired && instance.State != BuildingState.Exploded)
            return false;

        return _world.Resources.Money >= definition.Cost;
    }

    private string GetSelectedBuildingCostText(Guid buildingId)
    {
        if (!_world.TryGetBuilding(buildingId, out var instance))
            return "?";

        if (!_world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
            return "?";

        return FormatNumber((double)definition.Cost);
    }

    private static Color GetPropertyValueColor(string key)
    {
        return key switch
        {
            "BUILD COST" or "UNLOCK COST" or "ACTION" => new Color(255, 225, 120),
            "NEXT UPGRADE" => new Color(210, 190, 255),
            "MONEY/S" or "PAYBACK" => new Color(180, 225, 190),
            "NET ENERGY" => new Color(135, 210, 255),
            "BUILD TOOL" => new Color(255, 220, 80),
            "ENERGY IN" => new Color(255, 165, 120),
            "ENERGY OUT" or "HEAT TO ENERGY" => new Color(135, 210, 255),
            "HEAT OUT" or "HEAT STORED" => new Color(245, 145, 55),
            "HEAT IN" => new Color(70, 220, 190),
            "RESEARCH OUT" => new Color(210, 190, 255),
            "AUTO SELL" => new Color(180, 225, 190),
            "BATTERY" => new Color(240, 205, 70),
            _ => new Color(210, 222, 235)
        };
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

        var status = BuildingOperationalStatusCalculator.Calculate(_world, instance);

        _text.DrawString(spriteBatch, Shorten(definition.Name.ToUpperInvariant(), 28), new Vector2(panel.X + 12, panel.Y + 12), new Color(235, 240, 245), 2);
        _text.DrawString(spriteBatch, $"STATE {status.Label}", new Vector2(panel.X + 12, panel.Y + 46), GetOperationalStateColor(status.State), 1);

        var y = panel.Y + 66;
        var effectiveLifetime = UpgradeCalculator.GetLifetimeSeconds(_world, definition);
        var lifetimeText = effectiveLifetime <= 0
            ? "LIFE -"
            : $"LIFE {Math.Ceiling(instance.RemainingLifetimeSeconds):0} S / {effectiveLifetime:0} S";

        DrawDetailLine(spriteBatch, panel.X + 12, ref y, lifetimeText, new Color(210, 222, 235));
        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"COST ${FormatNumber((double)definition.Cost)}", new Color(255, 225, 120));
        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"SIZE {definition.Width}X{definition.Height}", new Color(180, 195, 215));
        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"MANAGED {(ManagerSystem.IsManaged(_world, definition.Id) ? "YES" : "NO")}", ManagerSystem.IsManaged(_world, definition.Id) ? new Color(150, 235, 150) : new Color(150, 160, 175));

        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"ENERGY IN -{FormatNumber(status.EnergyInputPerSecond)}/S", new Color(255, 165, 120));

        var grossEnergy = UpgradeCalculator.GetEnergyPerSecond(_world, definition);
        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"ENERGY OUT +{FormatNumber(status.EnergyOutputPerSecond)}/S", new Color(135, 210, 255));
        if (Math.Abs(status.EnergyOutputPerSecond - grossEnergy) > 0.0001)
            DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"ENERGY OUT GROSS +{FormatNumber(grossEnergy)}/S", new Color(100, 145, 180));

        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"HEAT OUT +{FormatNumber(status.HeatOutputPerSecond)}/S", new Color(245, 145, 55));
        var grossHeat = UpgradeCalculator.GetHeatPerSecond(_world, definition);
        if (Math.Abs(status.HeatOutputPerSecond - grossHeat) > 0.0001)
            DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"HEAT OUT GROSS +{FormatNumber(grossHeat)}/S", new Color(180, 105, 65));

        var effectiveHeatConversion = UpgradeCalculator.GetHeatConversionPerSecond(_world, definition);
        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"HEAT IN ABSORBS {FormatNumber(status.HeatConversionInputPerSecond)}/S, RANGE {definition.HeatRange} CELLS", new Color(70, 220, 190));
        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"HEAT TO ENERGY PRODUCES {FormatNumber(status.HeatConversionEnergyOutputPerSecond)}/S ENERGY", new Color(135, 210, 255));
        if (Math.Abs(status.HeatConversionInputPerSecond - effectiveHeatConversion) > 0.0001)
            DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"HEAT IN GROSS ABSORBS {FormatNumber(effectiveHeatConversion)}/S", new Color(55, 160, 145));

        var grossResearch = UpgradeCalculator.GetResearchPerSecond(_world, definition);
        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"RESEARCH OUT +{FormatNumber(status.ResearchOutputPerSecond)}/S", new Color(210, 190, 255));
        if (Math.Abs(status.ResearchOutputPerSecond - grossResearch) > 0.0001)
            DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"RESEARCH OUT GROSS +{FormatNumber(grossResearch)}/S", new Color(150, 125, 200));

        var grossAutoSell = UpgradeCalculator.GetAutoSellPerSecond(_world, definition);
        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"AUTO SELL ENERGY {FormatNumber(status.AutoSellInputPerSecond)}/S", new Color(180, 225, 190));
        if (Math.Abs(status.AutoSellInputPerSecond - grossAutoSell) > 0.0001)
            DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"AUTO SELL GROSS {FormatNumber(grossAutoSell)}/S", new Color(120, 175, 135));

        DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"BATTERY CAP +{FormatNumber(status.BatteryCapacity)}", new Color(240, 205, 70));

        if (instance.AccumulatedHeat > 0 || UpgradeCalculator.GetHeatPerSecond(_world, definition) > 0)
        {
            var heatText = $"HEAT STORED {FormatNumber(status.HeatStored)}/{FormatNumber(status.HeatExplosionThreshold)}";
            DrawDetailLine(spriteBatch, panel.X + 12, ref y, heatText, GetHeatTextColor(status.HeatStored));
            DrawDetailLine(spriteBatch, panel.X + 12, ref y, $"HEAT CONVERTER {(status.HasHeatConverterInRange ? "YES" : "NO")}", status.HasHeatConverterInRange ? new Color(150, 235, 150) : new Color(255, 150, 120));
        }

        if (instance.State == BuildingState.Expired || instance.State == BuildingState.Exploded)
        {
            var replaceButton = GetReplaceButtonRectangle(viewport);
            spriteBatch.Draw(_pixel, replaceButton, instance.State == BuildingState.Exploded ? new Color(118, 72, 72) : new Color(88, 118, 72));
            DrawOutline(spriteBatch, replaceButton, instance.State == BuildingState.Exploded ? new Color(245, 150, 140) : new Color(180, 235, 135), 2);
            var label = instance.State == BuildingState.Exploded ? "RESTORE" : "REPLACE";
            _text.DrawString(spriteBatch, $"{label} ${FormatNumber((double)definition.Cost)}", new Vector2(replaceButton.X + 12, replaceButton.Y + 10), new Color(235, 250, 220), 1);
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
        var tilesToUnlock = CountUnlockableCloudTiles(selectedCloudPosition.Value);

        _text.DrawString(spriteBatch, $"REVEALS {revealText}", new Vector2(panel.X + 12, panel.Y + 70), new Color(210, 222, 235), 1);
        _text.DrawString(spriteBatch, $"UNLOCKS UP TO {tilesToUnlock} TILES", new Vector2(panel.X + 12, panel.Y + 92), new Color(210, 222, 235), 1);
        _text.DrawString(spriteBatch, $"RADIUS {_world.AreaUnlockSettings.CloudUnlockRadius} MAX {_world.AreaUnlockSettings.MaxCloudTilesPerUnlock}", new Vector2(panel.X + 12, panel.Y + 114), new Color(170, 185, 205), 1);
        _text.DrawString(spriteBatch, $"COST ${FormatNumber((double)_world.AreaUnlockSettings.CloudUnlockMoneyCost)}", new Vector2(panel.X + 12, panel.Y + 136), new Color(255, 225, 120), 1);
        _text.DrawString(spriteBatch, $"COST R{FormatNumber(_world.AreaUnlockSettings.CloudUnlockResearchCost)}", new Vector2(panel.X + 12, panel.Y + 158), new Color(210, 190, 255), 1);

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

    private int CountUnlockableCloudTiles(GridPosition start)
    {
        if (!_world.Map.Contains(start))
            return 0;

        var startTile = _world.Map.GetTile(start);
        if (startTile.Type != TileType.Cloud || !startTile.CoveredType.HasValue)
            return 0;

        var radius = Math.Max(0, _world.AreaUnlockSettings.CloudUnlockRadius);
        var maxTiles = Math.Max(1, _world.AreaUnlockSettings.MaxCloudTilesPerUnlock);
        var count = 0;
        var visited = new HashSet<GridPosition>();
        var queue = new Queue<GridPosition>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0 && count < maxTiles)
        {
            var current = queue.Dequeue();
            var distance = Math.Abs(current.X - start.X) + Math.Abs(current.Y - start.Y);
            if (distance > radius)
                continue;

            if (_world.Map.Contains(current))
            {
                var tile = _world.Map.GetTile(current);
                if (tile.Type == TileType.Cloud && tile.CoveredType.HasValue && !tile.HasBuilding)
                    count++;
            }

            foreach (var neighbor in GetCardinalNeighbors(current))
            {
                if (visited.Contains(neighbor) || !_world.Map.Contains(neighbor))
                    continue;

                var neighborDistance = Math.Abs(neighbor.X - start.X) + Math.Abs(neighbor.Y - start.Y);
                if (neighborDistance > radius)
                    continue;

                if (_world.Map.GetTile(neighbor).Type != TileType.Cloud)
                    continue;

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        return count;
    }

    private static IEnumerable<GridPosition> GetCardinalNeighbors(GridPosition position)
    {
        yield return new GridPosition(position.X + 1, position.Y);
        yield return new GridPosition(position.X - 1, position.Y);
        yield return new GridPosition(position.X, position.Y + 1);
        yield return new GridPosition(position.X, position.Y - 1);
    }

    private void DrawStatus(SpriteBatch spriteBatch, Viewport viewport, string? selectedBuildingId, BuildResult? lastBuildResult, ResearchResult? lastResearchResult, TerrainClearResult? lastTerrainClearResult, AreaUnlockResult? lastAreaUnlockResult, UpgradeResult? lastUpgradeResult, string? saveLoadMessage, Guid? pendingDemolishBuildingId)
    {
        var y = viewport.Height - 34;
        var message = selectedBuildingId is null
            ? "SELECT BUILDING"
            : $"BUILD TOOL {selectedBuildingId} - LEFT CLICK BUILD, RIGHT CLICK CANCEL";

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
                ? $"AREA UNLOCKED {lastAreaUnlockResult.TilesUnlocked} TILES"
                : $"UNLOCK FAILED {lastAreaUnlockResult.FailureReason}";
        }

        if (lastUpgradeResult is not null)
        {
            message = lastUpgradeResult.Success
                ? $"UPGRADE OK {lastUpgradeResult.UpgradeId} LV {lastUpgradeResult.NewLevel}"
                : $"UPGRADE FAILED {lastUpgradeResult.FailureReason}";
        }

        if (!string.IsNullOrWhiteSpace(saveLoadMessage))
            message = saveLoadMessage;

        if (pendingDemolishBuildingId.HasValue)
            message = "DEMOLISH REQUIRES CONFIRMATION: CLICK CONFIRM DEMOLISH";

        var properties = GetPropertiesPanelRectangle(viewport);
        spriteBatch.Draw(_pixel, new Rectangle(SideMenuWidth, viewport.Height - 44, Math.Max(0, properties.X - SideMenuWidth), 44), new Color(25, 31, 42));
        _text.DrawString(spriteBatch, Shorten(message, 68), new Vector2(SideMenuWidth + 14, y), new Color(230, 238, 245), 1);

        DrawGameCommandButtons(spriteBatch, viewport);
    }

    private void DrawGameCommandButtons(SpriteBatch spriteBatch, Viewport viewport)
    {
        DrawSmallCommandButton(spriteBatch, GetSaveButtonRectangle(viewport), "SAVE", new Color(54, 78, 103));
        DrawSmallCommandButton(spriteBatch, GetLoadButtonRectangle(viewport), "LOAD", new Color(54, 78, 103));
        DrawSmallCommandButton(spriteBatch, GetNewGameButtonRectangle(viewport), "NEW", new Color(74, 64, 96));
        DrawSmallCommandButton(spriteBatch, GetToggleFullscreenButtonRectangle(viewport), "VIEW", new Color(66, 82, 78));
        DrawSmallCommandButton(spriteBatch, GetExitButtonRectangle(viewport), "EXIT", new Color(96, 58, 58));
    }

    private void DrawSmallCommandButton(SpriteBatch spriteBatch, Rectangle rect, string label, Color background)
    {
        spriteBatch.Draw(_pixel, rect, background);
        DrawOutline(spriteBatch, rect, new Color(120, 140, 165), 1);
        _text.DrawString(spriteBatch, label, new Vector2(rect.X + 8, rect.Y + 8), new Color(235, 240, 245), 1);
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
        var properties = GetPropertiesPanelRectangle(viewport);
        return new Rectangle(Math.Max(10, properties.X - 104), 19, 88, 36);
    }

    private static Rectangle GetEnergyFillBarRectangle(Viewport viewport)
    {
        var sellButton = GetSellButtonRectangle(viewport);
        var width = Math.Clamp(viewport.Width / 7, 120, 190);
        return new Rectangle(Math.Max(10, sellButton.X - width - 14), 21, width, 32);
    }

    private static Rectangle GetSaveButtonRectangle(Viewport viewport)
    {
        var x = GetGameCommandButtonsStartX(viewport);
        return new Rectangle(x, viewport.Height - 36, 56, 28);
    }

    private static Rectangle GetLoadButtonRectangle(Viewport viewport)
    {
        var save = GetSaveButtonRectangle(viewport);
        return new Rectangle(save.Right + 8, save.Y, 56, 28);
    }

    private static Rectangle GetNewGameButtonRectangle(Viewport viewport)
    {
        var load = GetLoadButtonRectangle(viewport);
        return new Rectangle(load.Right + 8, load.Y, 48, 28);
    }

    private static Rectangle GetToggleFullscreenButtonRectangle(Viewport viewport)
    {
        var newGame = GetNewGameButtonRectangle(viewport);
        return new Rectangle(newGame.Right + 8, newGame.Y, 56, 28);
    }

    private static Rectangle GetExitButtonRectangle(Viewport viewport)
    {
        var view = GetToggleFullscreenButtonRectangle(viewport);
        return new Rectangle(view.Right + 8, view.Y, 56, 28);
    }

    private static int GetGameCommandButtonsStartX(Viewport viewport)
    {
        var properties = GetPropertiesPanelRectangle(viewport);
        const int totalWidth = 56 + 8 + 56 + 8 + 48 + 8 + 56 + 8 + 56;
        return Math.Max(SideMenuWidth + 360, properties.X - totalWidth - 12);
    }

    public const int PropertiesPanelWidth = 380;

    private static Rectangle GetPropertiesPanelRectangle(Viewport viewport)
    {
        var width = Math.Min(PropertiesPanelWidth, Math.Max(280, viewport.Width - SideMenuWidth - 120));
        var x = Math.Max(SideMenuWidth + 20, viewport.Width - width);
        var y = TopBarHeight;
        var height = Math.Max(0, viewport.Height - TopBarHeight - 44);
        return new Rectangle(x, y, width, height);
    }

    private static Rectangle GetSelectedBuildingPanelRectangle(Viewport viewport) => GetPropertiesPanelRectangle(viewport);
    private static Rectangle GetSelectedTerrainPanelRectangle(Viewport viewport) => GetPropertiesPanelRectangle(viewport);
    private static Rectangle GetSelectedCloudPanelRectangle(Viewport viewport) => GetPropertiesPanelRectangle(viewport);

    private static Rectangle GetReplaceButtonRectangle(Viewport viewport)
    {
        var panel = GetSelectedBuildingPanelRectangle(viewport);
        return new Rectangle(panel.X + 12, panel.Bottom - 44, panel.Width - 24, 32);
    }

    private static Rectangle GetDemolishButtonRectangle(Viewport viewport)
    {
        var panel = GetSelectedBuildingPanelRectangle(viewport);
        return new Rectangle(panel.X + 12, panel.Y + 74, panel.Width - 24, 32);
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

    private Rectangle GetBuildButtonRectangle(int index)
    {
        return new Rectangle(GetBuildColumnX(), MenuButtonsY + index * MenuButtonStride - _buildScrollOffset, ColumnWidth, MenuButtonHeight);
    }

    private Rectangle GetResearchButtonRectangle(int index)
    {
        return new Rectangle(GetResearchColumnX(), MenuButtonsY + index * MenuButtonStride - _researchScrollOffset, ColumnWidth, MenuButtonHeight);
    }

    private Rectangle GetUpgradeButtonRectangle(int index)
    {
        return new Rectangle(GetUpgradeColumnX(), MenuButtonsY + index * MenuButtonStride - _upgradeScrollOffset, ColumnWidth, MenuButtonHeight);
    }

    private static bool IsMenuButtonVisible(Rectangle rect, Viewport viewport)
    {
        return rect.Top >= MenuButtonsY && rect.Bottom <= viewport.Height - 8;
    }

    private static int ClampScrollOffset(int offset, int itemCount, int itemStride, Viewport viewport)
    {
        var contentHeight = itemCount * itemStride;
        var visibleHeight = Math.Max(1, viewport.Height - MenuButtonsY - 14);
        var maxOffset = Math.Max(0, contentHeight - visibleHeight);
        return Math.Clamp(offset, 0, maxOffset);
    }

    private void DrawColumnScrollHint(SpriteBatch spriteBatch, Viewport viewport, int x, int offset, int itemCount, int itemStride)
    {
        var maxOffset = ClampScrollOffset(int.MaxValue, itemCount, itemStride, viewport);
        if (maxOffset <= 0)
            return;

        var text = offset <= 0
            ? "MORE"
            : offset >= maxOffset
                ? "TOP"
                : "SCROLL";

        _text.DrawString(spriteBatch, text, new Vector2(x + ColumnWidth - 54, MenuHeaderY + 9), new Color(165, 180, 200), 1);
    }

    private static string GetUpgradeCostText(UpgradeDefinition definition, int currentLevel)
    {
        var moneyCost = UpgradeSystem.GetMoneyCost(definition, currentLevel);
        var researchCost = UpgradeSystem.GetResearchCost(definition, currentLevel);

        if (moneyCost > 0 && researchCost > 0)
            return $"NEXT ${FormatNumber((double)moneyCost)} R{FormatNumber(researchCost)}";

        if (moneyCost > 0)
            return $"NEXT ${FormatNumber((double)moneyCost)}";

        return $"NEXT R{FormatNumber(researchCost)}";
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
            UpgradeEffectType.MultiplyHeatProduction => $"HEAT OUT {amount}",
            UpgradeEffectType.MultiplyBatteryCapacity => $"BATTERY CAP {amount}",
            UpgradeEffectType.MultiplyAutoSell => $"AUTO SELL {amount}",
            UpgradeEffectType.MultiplyHeatConversion => $"HEAT TO ENERGY {amount}",
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

    private static Color GetOperationalStateColor(BuildingOperationalState state)
    {
        return state switch
        {
            BuildingOperationalState.Active => new Color(150, 235, 150),
            BuildingOperationalState.NoEnergy => new Color(255, 165, 120),
            BuildingOperationalState.Expired => new Color(255, 210, 95),
            BuildingOperationalState.Exploded => new Color(255, 110, 90),
            BuildingOperationalState.HeatWarning => new Color(245, 145, 55),
            BuildingOperationalState.NoHeatConversion => new Color(255, 150, 120),
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
        return FormatSiNumber(value, "0.##");
    }

    private static string FormatNumberFixed2(double value)
    {
        return FormatSiNumber(value, "0.00");
    }

    private static string FormatSiNumber(double value, string format)
    {
        var sign = value < 0 ? "-" : "";
        var absolute = Math.Abs(value);

        var units = new[]
        {
            (Value: 1_000_000_000_000_000_000_000_000d, Suffix: "Y"),
            (Value: 1_000_000_000_000_000_000_000d, Suffix: "Z"),
            (Value: 1_000_000_000_000_000_000d, Suffix: "E"),
            (Value: 1_000_000_000_000_000d, Suffix: "P"),
            (Value: 1_000_000_000_000d, Suffix: "T"),
            (Value: 1_000_000_000d, Suffix: "G"),
            (Value: 1_000_000d, Suffix: "M"),
            (Value: 1_000d, Suffix: "k")
        };

        foreach (var unit in units)
        {
            if (absolute >= unit.Value)
                return sign + (absolute / unit.Value).ToString(format) + unit.Suffix;
        }

        return value.ToString(format);
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
