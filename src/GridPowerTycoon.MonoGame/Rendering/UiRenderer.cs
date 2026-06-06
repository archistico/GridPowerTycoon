using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Expansion;
using GridPowerTycoon.Core.Feedback;
using GridPowerTycoon.Core.Research;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Managers;
using GridPowerTycoon.Core.Operations;
using GridPowerTycoon.Core.Progression;
using GridPowerTycoon.Core.Tools;
using GridPowerTycoon.Core.Upgrades;
using GridPowerTycoon.Core.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GridPowerTycoon.MonoGame.Rendering;

public enum LeftPanelMode
{
    Build,
    Research,
    Upgrade
}

public sealed class UiRenderer
{
    public const int TopBarHeight = 74;
    public const int SideMenuWidth = 430;
    private const int MenuStripHeight = 38;
    private const int StatusBarHeight = 44;

    private const int PanelMargin = 12;
    private const int ColumnWidth = SideMenuWidth - (PanelMargin * 2);
    private const int ColumnGap = 12;
    private const int MenuHeaderY = TopBarHeight + MenuStripHeight + 8;
    private const int MenuButtonsY = TopBarHeight + MenuStripHeight + 10;
    private const int MenuButtonHeight = 96;
    private const int MenuButtonStride = 104;
    private const int MenuButtonTextX = 10;
    private const int MenuButtonTitleY = 7;
    private const int MenuButtonMetaY = 31;
    private const int MenuButtonPrimaryY = 49;
    private const int MenuButtonPurposeY = 66;
    private const int MenuButtonDetailY = 82;

    private static readonly string[] BuildButtonIds =
    {
        "wind_turbine",
        "battery_small",
        "office_small",
        "research_small",
        "solar_panel",
        "generator_small",
        "heat_sink_small",
        "substation_small",
        "maintenance_center_small",
        "tool_warehouse_small",
        "coal_power_plant",
        "geothermal_plant",
        "office_large",
        "generator_medium",
        "gas_power_plant",
        "research_large",
        "data_center",
        "nuclear_reactor"
    };

    private static readonly string[] ResearchButtonIds =
    {
        "battery",
        "office_small",
        "solar_power",
        "generator_small",
        "heat_management",
        "grid_substation",
        "maintenance_center",
        "tool_storage",
        "wind_turbine_manager",
        "solar_panel_manager",
        "coal_power",
        "geothermal_power",
        "office_large",
        "generator_medium",
        "gas_power",
        "coal_power_manager",
        "gas_power_manager",
        "research_large",
        "data_center",
        "nuclear_power"
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
        "research_large_output_1",
        "nuclear_heat_1",
        "nuclear_lifetime_1"
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
        LeftPanelMode activeLeftPanelMode,
        string? selectedBuildingId,
        BuildResult? lastBuildResult,
        ResearchResult? lastResearchResult,
        GridPosition? lastBuildFailurePosition,
        GridPosition? selectedTilePosition,
        Guid? selectedMapBuildingId,
        GridPosition? selectedTerrainPosition,
        GridPosition? selectedCloudPosition,
        TerrainClearResult? lastTerrainClearResult,
        AreaUnlockResult? lastAreaUnlockResult,
        UpgradeResult? lastUpgradeResult,
        string? saveLoadMessage,
        string saveDataInfo,
        bool showHelpPanel,
        Guid? pendingDemolishBuildingId,
        Point mousePosition)
    {
        var topBar = GetTopBarRectangle(viewport);
        var sideMenu = GetSideMenuRectangle(viewport);

        spriteBatch.Draw(_pixel, topBar, new Color(32, 39, 52));
        spriteBatch.Draw(_pixel, sideMenu, new Color(38, 48, 62));

        DrawActiveLeftMenu(spriteBatch, viewport, activeLeftPanelMode, selectedBuildingId);

        // Draw the top bars after the scrollable menu. They act as a hard visual mask
        // if a scrolled card would otherwise spill outside the left list area.
        spriteBatch.Draw(_pixel, topBar, new Color(32, 39, 52));
        DrawTopBar(spriteBatch, topBar, viewport);
        DrawMenuStrip(spriteBatch, viewport, activeLeftPanelMode);

        DrawPropertiesPanel(spriteBatch, viewport, selectedBuildingId, selectedTilePosition, selectedMapBuildingId, selectedTerrainPosition, selectedCloudPosition, pendingDemolishBuildingId);
        DrawProductionSummaryPanel(spriteBatch, viewport);
        DrawEarlyChecklist(spriteBatch, viewport);
        DrawHelpPanel(spriteBatch, viewport, showHelpPanel);
        DrawStatus(spriteBatch, viewport, selectedBuildingId, selectedMapBuildingId, lastBuildResult, lastResearchResult, lastBuildFailurePosition, lastTerrainClearResult, lastAreaUnlockResult, lastUpgradeResult, saveLoadMessage, saveDataInfo, pendingDemolishBuildingId);
        DrawHoveredCardDetails(spriteBatch, viewport, activeLeftPanelMode, mousePosition);
    }

    public bool IsMouseOverUi(Point mousePosition, Viewport viewport)
    {
        return GetTopBarRectangle(viewport).Contains(mousePosition) ||
               GetMenuStripRectangle(viewport).Contains(mousePosition) ||
               GetSideMenuRectangle(viewport).Contains(mousePosition) ||
               GetPropertiesPanelRectangle(viewport).Contains(mousePosition) ||
               GetStatusBarRectangle(viewport).Contains(mousePosition);
    }

    public void HandleScroll(Point mousePosition, int scrollDelta, LeftPanelMode activeLeftPanelMode, Viewport viewport)
    {
        if (scrollDelta == 0)
            return;

        if (!GetSideMenuRectangle(viewport).Contains(mousePosition))
            return;

        var delta = scrollDelta > 0 ? -MenuButtonStride : MenuButtonStride;

        switch (activeLeftPanelMode)
        {
            case LeftPanelMode.Build:
                _buildScrollOffset = ClampScrollOffset(_buildScrollOffset + delta, BuildButtonIds.Length, MenuButtonStride, viewport);
                break;
            case LeftPanelMode.Research:
                _researchScrollOffset = ClampScrollOffset(_researchScrollOffset + delta, ResearchButtonIds.Length, MenuButtonStride, viewport);
                break;
            case LeftPanelMode.Upgrade:
                _upgradeScrollOffset = ClampScrollOffset(_upgradeScrollOffset + delta, UpgradeButtonIds.Length, MenuButtonStride, viewport);
                break;
        }
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

    public bool IsHelpButtonAt(Point mousePosition, Viewport viewport)
    {
        return GetHelpButtonRectangle(viewport).Contains(mousePosition);
    }

    public bool IsExitButtonAt(Point mousePosition, Viewport viewport)
    {
        return GetExitButtonRectangle(viewport).Contains(mousePosition);
    }

    public bool TryGetLeftPanelModeButtonAt(Point mousePosition, Viewport viewport, out LeftPanelMode mode)
    {
        if (GetBuildTabButtonRectangle(viewport).Contains(mousePosition))
        {
            mode = LeftPanelMode.Build;
            return true;
        }

        if (GetResearchTabButtonRectangle(viewport).Contains(mousePosition))
        {
            mode = LeftPanelMode.Research;
            return true;
        }

        if (GetUpgradeTabButtonRectangle(viewport).Contains(mousePosition))
        {
            mode = LeftPanelMode.Upgrade;
            return true;
        }

        mode = LeftPanelMode.Build;
        return false;
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

    public bool TryGetBuildingButtonAt(Point mousePosition, Viewport viewport, LeftPanelMode activeLeftPanelMode, out string? buildingId)
    {
        if (activeLeftPanelMode != LeftPanelMode.Build)
        {
            buildingId = null;
            return false;
        }

        var formatter = new GameplayFeedbackFormatter(_world);

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

    public bool TryGetResearchButtonAt(Point mousePosition, Viewport viewport, LeftPanelMode activeLeftPanelMode, out string researchId)
    {
        if (activeLeftPanelMode != LeftPanelMode.Research)
        {
            researchId = "";
            return false;
        }

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

    public bool TryGetUpgradeButtonAt(Point mousePosition, Viewport viewport, LeftPanelMode activeLeftPanelMode, out string upgradeId)
    {
        if (activeLeftPanelMode != LeftPanelMode.Upgrade)
        {
            upgradeId = "";
            return false;
        }

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

    private void DrawMenuStrip(SpriteBatch spriteBatch, Viewport viewport, LeftPanelMode activeLeftPanelMode)
    {
        var strip = GetMenuStripRectangle(viewport);
        spriteBatch.Draw(_pixel, strip, new Color(28, 36, 48));
        DrawOutline(spriteBatch, strip, new Color(70, 84, 104), 1);

        DrawTabButton(spriteBatch, GetBuildTabButtonRectangle(viewport), "BUILD", activeLeftPanelMode == LeftPanelMode.Build);
        DrawTabButton(spriteBatch, GetResearchTabButtonRectangle(viewport), "RESEARCH", activeLeftPanelMode == LeftPanelMode.Research);
        DrawTabButton(spriteBatch, GetUpgradeTabButtonRectangle(viewport), "UPGRADE", activeLeftPanelMode == LeftPanelMode.Upgrade);
        DrawFutureSectionButtons(spriteBatch, viewport);

        DrawSmallCommandButton(spriteBatch, GetNewGameButtonRectangle(viewport), "NEW", new Color(74, 64, 96));
        DrawSmallCommandButton(spriteBatch, GetLoadButtonRectangle(viewport), "LOAD", new Color(54, 78, 103));
        DrawSmallCommandButton(spriteBatch, GetSaveButtonRectangle(viewport), "SAVE", new Color(54, 78, 103));
        DrawSmallCommandButton(spriteBatch, GetToggleFullscreenButtonRectangle(viewport), "VIEW", new Color(66, 82, 78));
        DrawSmallCommandButton(spriteBatch, GetExitButtonRectangle(viewport), "EXIT", new Color(96, 58, 58));
    }

    private void DrawTabButton(SpriteBatch spriteBatch, Rectangle rect, string label, bool isActive)
    {
        spriteBatch.Draw(_pixel, rect, isActive ? new Color(67, 86, 110) : new Color(48, 60, 76));
        DrawOutline(spriteBatch, rect, isActive ? new Color(255, 220, 80) : new Color(90, 104, 124), isActive ? 2 : 1);
        _text.DrawString(spriteBatch, label, new Vector2(rect.X + 10, rect.Y + 9), new Color(235, 240, 245), 1);
    }

    private void DrawActiveLeftMenu(SpriteBatch spriteBatch, Viewport viewport, LeftPanelMode activeLeftPanelMode, string? selectedBuildingId)
    {
        switch (activeLeftPanelMode)
        {
            case LeftPanelMode.Build:
                DrawBuildMenu(spriteBatch, viewport, selectedBuildingId);
                break;
            case LeftPanelMode.Research:
                DrawResearchMenu(spriteBatch, viewport);
                break;
            case LeftPanelMode.Upgrade:
                DrawUpgradeMenu(spriteBatch, viewport);
                break;
        }
    }

    private void DrawFutureSectionButtons(SpriteBatch spriteBatch, Viewport viewport)
    {
        var x = GetFutureSectionButtonsStartX(viewport);
        var maxRight = GetFutureSectionButtonsMaxRight(viewport);
        var y = TopBarHeight + 5;
        const int gap = 8;

        DrawFutureSectionButtonIfFits(spriteBatch, "STATS", 72, ref x, y, maxRight, gap);
        DrawHelpSectionButtonIfFits(spriteBatch, ref x, y, maxRight, gap);
        DrawFutureSectionButtonIfFits(spriteBatch, "SETTINGS", 96, ref x, y, maxRight, gap);
    }

    private void DrawHelpSectionButtonIfFits(SpriteBatch spriteBatch, ref int x, int y, int maxRight, int gap)
    {
        var rect = new Rectangle(x, y, 64, 28);
        if (rect.Right > maxRight)
            return;

        DrawSmallCommandButton(spriteBatch, rect, "HELP", new Color(70, 70, 96));
        x = rect.Right + gap;
    }

    private void DrawFutureSectionButtonIfFits(SpriteBatch spriteBatch, string label, int width, ref int x, int y, int maxRight, int gap)
    {
        var rect = new Rectangle(x, y, width, 28);
        if (rect.Right > maxRight)
            return;

        DrawDisabledCommandButton(spriteBatch, rect, label);
        x = rect.Right + gap;
    }

    private void DrawDisabledCommandButton(SpriteBatch spriteBatch, Rectangle rect, string label)
    {
        spriteBatch.Draw(_pixel, rect, new Color(42, 48, 58));
        DrawOutline(spriteBatch, rect, new Color(70, 80, 96), 1);
        _text.DrawString(spriteBatch, label, new Vector2(rect.X + 8, rect.Y + 8), new Color(120, 132, 150), 1);
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
        _text.DrawString(spriteBatch, label, new Vector2(x, 7), new Color(230, 238, 245), 2);
        _text.DrawString(spriteBatch, value, new Vector2(x, 29), accentColor, 2);
        _text.DrawString(spriteBatch, rate, new Vector2(x, 56), accentColor, 1);
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
        var formatter = new GameplayFeedbackFormatter(_world);
        var headerX = GetBuildColumnX();
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

            var categoryColor = definition is null ? Color.Magenta : GetBuildingColor(definition.Category);
            if (isLocked)
                categoryColor = new Color(90, 95, 105);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, 5, rect.Height), categoryColor);

            var name = definition?.Name ?? id;
            var textColor = isLocked ? new Color(150, 155, 165) : new Color(235, 240, 245);
            var costColor = isSelected
                ? new Color(255, 220, 80)
                : isLocked
                    ? new Color(255, 150, 120)
                    : canAfford
                        ? new Color(255, 225, 120)
                        : new Color(255, 110, 90);
            var costText = definition is null
                ? formatter.FormatBuildAvailabilityLine(id)
                : isSelected
                    ? "ACTIVE - " + formatter.FormatBuildAvailabilityLine(id)
                    : formatter.FormatBuildAvailabilityLine(id);
            var primaryText = definition is null ? "MAIN EFFECT ?" : GetBuildButtonMainEffectText(definition);
            var supportText = definition is null ? id : GetBuildButtonSupportText(definition);

            _text.DrawString(spriteBatch, Shorten(name, 28), new Vector2(rect.X + MenuButtonTextX, rect.Y + MenuButtonTitleY), textColor, 2);
            _text.DrawString(spriteBatch, Shorten(costText, 60), new Vector2(rect.X + MenuButtonTextX, rect.Y + MenuButtonMetaY), costColor, 1);
            _text.DrawString(spriteBatch, Shorten(primaryText, 62), new Vector2(rect.X + MenuButtonTextX, rect.Y + MenuButtonPrimaryY), new Color(170, 210, 235), 1);
            _text.DrawString(spriteBatch, Shorten(supportText, 62), new Vector2(rect.X + MenuButtonTextX, rect.Y + MenuButtonPurposeY), isLocked ? new Color(135, 140, 150) : new Color(175, 188, 205), 1);
            if (definition is not null)
                _text.DrawString(spriteBatch, Shorten(GetBuildButtonDetailText(definition), 62), new Vector2(rect.X + MenuButtonTextX, rect.Y + MenuButtonDetailY), isLocked ? new Color(120, 128, 138) : new Color(145, 160, 178), 1);
        }
    }

    private void DrawResearchMenu(SpriteBatch spriteBatch, Viewport viewport)
    {
        var headerX = GetBuildColumnX();
        DrawColumnScrollHint(spriteBatch, viewport, headerX, _researchScrollOffset, ResearchButtonIds.Length, MenuButtonStride);
        var formatter = new GameplayFeedbackFormatter(_world);

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

            var stripeColor = definition is null
                ? Color.Magenta
                : completed
                    ? new Color(90, 175, 105)
                    : missingPrereq
                        ? new Color(90, 95, 105)
                        : definition.ManagedBuildingIds.Count > 0
                            ? new Color(120, 105, 220)
                            : definition.UnlockBuildingIds.Count > 0
                                ? new Color(95, 165, 230)
                                : new Color(180, 150, 230);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, 5, rect.Height), stripeColor);

            var name = definition?.Name ?? id;
            var status = formatter.FormatResearchAvailabilityLine(id);
            var unlockText = definition is null ? "" : GetResearchUnlockText(definition);
            var description = definition is null ? id : GetResearchActionText(definition);
            var detail = definition is null ? "RESEARCH" : GetResearchButtonDetailText(definition);

            _text.DrawString(spriteBatch, Shorten(name, 30), new Vector2(rect.X + MenuButtonTextX, rect.Y + MenuButtonTitleY), new Color(235, 240, 245), 2);
            var statusColor = completed
                ? new Color(160, 245, 175)
                : missingPrereq
                    ? new Color(255, 170, 130)
                    : canAfford
                        ? new Color(210, 190, 255)
                        : new Color(255, 150, 120);
            _text.DrawString(spriteBatch, Shorten(status, 58), new Vector2(rect.X + MenuButtonTextX, rect.Y + MenuButtonMetaY), statusColor, 1);
            _text.DrawString(spriteBatch, Shorten(unlockText, 60), new Vector2(rect.X + MenuButtonTextX, rect.Y + MenuButtonPrimaryY), new Color(190, 215, 255), 1);
            _text.DrawString(spriteBatch, Shorten(description, 60), new Vector2(rect.X + MenuButtonTextX, rect.Y + MenuButtonPurposeY), new Color(175, 188, 205), 1);
            _text.DrawString(spriteBatch, Shorten(detail, 62), new Vector2(rect.X + MenuButtonTextX, rect.Y + MenuButtonDetailY), completed ? new Color(130, 190, 145) : new Color(145, 160, 178), 1);
        }
    }

    private void DrawUpgradeMenu(SpriteBatch spriteBatch, Viewport viewport)
    {
        var headerX = GetBuildColumnX();
        DrawColumnScrollHint(spriteBatch, viewport, headerX, _upgradeScrollOffset, UpgradeButtonIds.Length, MenuButtonStride);
        var formatter = new GameplayFeedbackFormatter(_world);

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

            var accentColor = definition is null ? Color.Magenta : GetUpgradeAccentColor(definition);
            if (completed)
                accentColor = new Color(80, 175, 95);
            else if (missingResearch)
                accentColor = new Color(90, 95, 105);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, 5, rect.Height), accentColor);

            var name = definition?.Name ?? id;
            var effect = definition is null ? "?" : GetUpgradeEffectText(definition);
            var targetText = definition is null ? "" : GetUpgradeTargetText(definition);
            var status = formatter.FormatUpgradeAvailabilityLine(id);
            var detailText = definition is null ? id : GetUpgradeButtonDetailText(definition, level, completed);

            _text.DrawString(spriteBatch, Shorten(name, 28), new Vector2(rect.X + MenuButtonTextX, rect.Y + MenuButtonTitleY), new Color(235, 240, 245), 2);
            var statusColor = completed
                ? new Color(160, 245, 175)
                : missingResearch
                    ? new Color(255, 170, 130)
                    : canAfford
                        ? new Color(255, 225, 120)
                        : new Color(255, 150, 120);
            _text.DrawString(spriteBatch, Shorten(status, 58), new Vector2(rect.X + MenuButtonTextX, rect.Y + MenuButtonMetaY), statusColor, 1);
            _text.DrawString(spriteBatch, Shorten(effect, 58), new Vector2(rect.X + MenuButtonTextX, rect.Y + MenuButtonPrimaryY), new Color(190, 215, 255), 1);
            _text.DrawString(spriteBatch, Shorten(targetText, 58), new Vector2(rect.X + MenuButtonTextX, rect.Y + MenuButtonPurposeY), new Color(175, 188, 205), 1);
            _text.DrawString(spriteBatch, Shorten(detailText, 58), new Vector2(rect.X + MenuButtonTextX, rect.Y + MenuButtonDetailY), completed ? new Color(140, 210, 155) : new Color(145, 160, 178), 1);
        }
    }




    private void DrawHoveredCardDetails(SpriteBatch spriteBatch, Viewport viewport, LeftPanelMode activeLeftPanelMode, Point mousePosition)
    {
        if (!TryGetHoveredCardDetails(mousePosition, viewport, activeLeftPanelMode, out var lines))
            return;

        var visibleLines = lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Take(7)
            .ToArray();

        if (visibleLines.Length == 0)
            return;

        var width = 410;
        var lineHeight = 17;
        var height = 18 + visibleLines.Length * lineHeight + 12;
        var x = mousePosition.X + 18;
        var y = mousePosition.Y + 18;

        if (x + width > viewport.Width - 12)
            x = mousePosition.X - width - 18;
        if (y + height > viewport.Height - StatusBarHeight - 8)
            y = viewport.Height - StatusBarHeight - height - 8;
        if (x < SideMenuWidth + 8)
            x = SideMenuWidth + 8;
        if (y < TopBarHeight + MenuStripHeight + 8)
            y = TopBarHeight + MenuStripHeight + 8;

        var panel = new Rectangle(x, y, width, height);
        spriteBatch.Draw(_pixel, panel, new Color(24, 30, 42, 238));
        DrawOutline(spriteBatch, panel, new Color(120, 140, 170), 2);

        for (var i = 0; i < visibleLines.Length; i++)
        {
            var color = i == 0
                ? new Color(255, 225, 120)
                : i == 1 && visibleLines[i].StartsWith("LOCKED", StringComparison.OrdinalIgnoreCase)
                    ? new Color(255, 170, 130)
                    : i == 1 && visibleLines[i].StartsWith("NEED", StringComparison.OrdinalIgnoreCase)
                        ? new Color(255, 150, 120)
                        : i == 1 && visibleLines[i].StartsWith("READY", StringComparison.OrdinalIgnoreCase)
                            ? new Color(160, 245, 175)
                            : new Color(220, 230, 240);

            _text.DrawString(spriteBatch, Shorten(visibleLines[i], 64), new Vector2(panel.X + 12, panel.Y + 11 + i * lineHeight), color, 1);
        }
    }

    private bool TryGetHoveredCardDetails(Point mousePosition, Viewport viewport, LeftPanelMode activeLeftPanelMode, out IReadOnlyList<string> lines)
    {
        var formatter = new GameplayFeedbackFormatter(_world);

        switch (activeLeftPanelMode)
        {
            case LeftPanelMode.Build:
                for (var i = 0; i < BuildButtonIds.Length; i++)
                {
                    var rect = GetBuildButtonRectangle(i);
                    if (IsMenuButtonVisible(rect, viewport) && rect.Contains(mousePosition))
                    {
                        lines = formatter.FormatBuildCardDetails(BuildButtonIds[i]);
                        return true;
                    }
                }
                break;

            case LeftPanelMode.Research:
                for (var i = 0; i < ResearchButtonIds.Length; i++)
                {
                    var rect = GetResearchButtonRectangle(i);
                    if (IsMenuButtonVisible(rect, viewport) && rect.Contains(mousePosition))
                    {
                        lines = formatter.FormatResearchCardDetails(ResearchButtonIds[i]);
                        return true;
                    }
                }
                break;

            case LeftPanelMode.Upgrade:
                for (var i = 0; i < UpgradeButtonIds.Length; i++)
                {
                    var rect = GetUpgradeButtonRectangle(i);
                    if (IsMenuButtonVisible(rect, viewport) && rect.Contains(mousePosition))
                    {
                        lines = formatter.FormatUpgradeCardDetails(UpgradeButtonIds[i]);
                        return true;
                    }
                }
                break;
        }

        lines = Array.Empty<string>();
        return false;
    }

    private void DrawMenuStateBadge(SpriteBatch spriteBatch, Rectangle buttonRect, string text, Color fillColor, Color textColor)
    {
        var badgeWidth = Math.Clamp(text.Length * 7 + 10, 42, 82);
        var badge = new Rectangle(buttonRect.Right - badgeWidth - 6, buttonRect.Y + 6, badgeWidth, 15);

        spriteBatch.Draw(_pixel, badge, fillColor);
        DrawOutline(spriteBatch, badge, new Color(205, 215, 225), 1);
        _text.DrawString(spriteBatch, text, new Vector2(badge.X + 5, badge.Y + 4), textColor, 1);
    }


    private void DrawEarlyChecklist(SpriteBatch spriteBatch, Viewport viewport)
    {
        var items = GetEarlyChecklistItems();
        if (items.All(item => item.Done))
            return;

        var properties = GetPropertiesPanelRectangle(viewport);
        var x = SideMenuWidth + 16;
        var width = properties.X - x - 16;
        if (width < 320)
            return;

        var itemHeight = 16;
        var height = 32 + items.Count * itemHeight + 10;
        var panel = new Rectangle(x, viewport.Height - StatusBarHeight - height - 10, Math.Min(width, 430), height);

        if (panel.Y < TopBarHeight + MenuStripHeight + 8)
            return;

        spriteBatch.Draw(_pixel, panel, new Color(25, 31, 42, 225));
        DrawOutline(spriteBatch, panel, new Color(90, 110, 135, 220), 2);

        _text.DrawString(spriteBatch, "EARLY CHECKLIST", new Vector2(panel.X + 12, panel.Y + 10), new Color(235, 240, 245), 1);

        var y = panel.Y + 31;
        foreach (var item in items)
        {
            var marker = item.Done ? "OK" : "--";
            var markerColor = item.Done ? new Color(150, 235, 150) : new Color(255, 225, 120);
            var textColor = item.Done ? new Color(145, 170, 145) : new Color(220, 230, 240);

            _text.DrawString(spriteBatch, marker, new Vector2(panel.X + 12, y), markerColor, 1);
            _text.DrawString(spriteBatch, Shorten(item.Text, 48), new Vector2(panel.X + 39, y), textColor, 1);
            y += itemHeight;
        }
    }

    private void DrawProductionSummaryPanel(SpriteBatch spriteBatch, Viewport viewport)
    {
        var properties = GetPropertiesPanelRectangle(viewport);
        var x = SideMenuWidth + 16;
        var width = properties.X - x - 16;
        if (width < 440)
            return;

        var lines = CreateFeedbackFormatter().FormatProductionSummaryLines();
        var lineHeight = 17;
        var height = 32 + (lines.Count - 1) * lineHeight + 12;
        var panel = new Rectangle(x, TopBarHeight + MenuStripHeight + 16, Math.Min(width, 620), height);

        if (panel.Bottom > viewport.Height - StatusBarHeight - 12)
            return;

        spriteBatch.Draw(_pixel, panel, new Color(24, 31, 42, 225));
        DrawOutline(spriteBatch, panel, new Color(85, 108, 132, 220), 2);

        _text.DrawString(spriteBatch, lines[0], new Vector2(panel.X + 12, panel.Y + 10), new Color(235, 240, 245), 1);

        var y = panel.Y + 31;
        for (var i = 1; i < lines.Count; i++)
        {
            var color = i switch
            {
                1 => new Color(135, 210, 255),
                2 => new Color(255, 225, 120),
                3 => new Color(255, 175, 120),
                4 => new Color(180, 220, 185),
                _ => new Color(210, 225, 238)
            };

            _text.DrawString(spriteBatch, Shorten(lines[i], 92), new Vector2(panel.X + 12, y), color, 1);
            y += lineHeight;
        }
    }

    private IReadOnlyList<EarlyChecklistItem> GetEarlyChecklistItems()
    {
        return new[]
        {
            new EarlyChecklistItem("Build wind turbine", HasBuilt("wind_turbine")),
            new EarlyChecklistItem("Build small office", HasBuilt("office_small")),
            new EarlyChecklistItem("Build research center", HasBuilt("research_small")),
            new EarlyChecklistItem("Build small battery", HasBuilt("battery_small")),
            new EarlyChecklistItem("Build solar + generator", HasBuilt("solar_panel") && HasBuilt("generator_small") && !HasHeatProducerWithoutConverter())
        };
    }

    private readonly record struct EarlyChecklistItem(string Text, bool Done);

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
        "PURPOSE",
        "STATE",
        "ISSUE",
        "BUILD TOOL",
        "COST",
        "SIZE",
        "REQUIRES",
        "PRODUCES",
        "CONSUMES",
        "STORAGE",
        "HEAT",
        "MAINTENANCE",
        "LIFETIME",
        "MANAGER",
        "NEXT UPGRADE",
        "ECONOMY",
        "PAYBACK",
        "REVEAL",
        "TERRAIN COST",
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
        rows["PURPOSE"] = GetPropertyPurposeText(definition);
        rows["STATE"] = status.Label;
        rows["ISSUE"] = GetOperationalIssueText(status, definition);
        rows["COST"] = "$" + FormatNumber((double)definition.Cost);
        rows["SIZE"] = $"{definition.Width} X {definition.Height}";
        rows["REQUIRES"] = GetBuildingRequirementText(definition);
        rows["PRODUCES"] = FormatBuildingProduction(status, definition);
        rows["CONSUMES"] = FormatBuildingConsumption(status);
        rows["STORAGE"] = FormatBuildingStorage(status, definition);
        rows["HEAT"] = FormatBuildingHeat(status, definition);
        rows["MAINTENANCE"] = FormatBuildingMaintenance(definition);
        rows["LIFETIME"] = FormatLifetime(instance.RemainingLifetimeSeconds, effectiveLifetime);
        rows["MANAGER"] = GetManagerStatusText(definition.Id, managed);
        rows["NEXT UPGRADE"] = GetNextUpgradeCostText(definition);
        rows["ECONOMY"] = FormatEstimatedMoneyPerSecond(GetEstimatedMoneyPerSecond(status));
        rows["PAYBACK"] = FormatPayback(definition.Cost, GetEstimatedMoneyPerSecond(status));

        stateColor = GetOperationalStateColor(status.State);
        if (instance.State == BuildingState.Exploded)
            action = _world.Resources.Money >= definition.Cost ? "RESTORE OR DEMOLISH" : "NEED MONEY TO RESTORE";
        else if (instance.State == BuildingState.Expired)
            action = _world.Resources.Money >= definition.Cost ? "REPLACE OR DEMOLISH" : "NEED MONEY TO REPLACE";
        else
            action = "DEMOLISH AVAILABLE";
    }

    private string GetBuildingRequirementText(BuildingDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(definition.RequiredResearchId))
            return "NONE";

        var researchName = _world.ResearchCatalog.TryGet(definition.RequiredResearchId, out var research)
            ? research.Name.ToUpperInvariant()
            : definition.RequiredResearchId.ToUpperInvariant();

        return _world.Research.IsCompleted(definition.RequiredResearchId)
            ? "DONE: " + researchName
            : "LOCKED: " + researchName;
    }

    private string FormatBuildingProduction(BuildingOperationalStatus status, BuildingDefinition definition)
    {
        var parts = new List<string>();

        if (status.EnergyOutputPerSecond > 0 || UpgradeCalculator.GetEnergyPerSecond(_world, definition) > 0)
            parts.Add("E " + FormatEffectiveGross(status.EnergyOutputPerSecond, UpgradeCalculator.GetEnergyPerSecond(_world, definition)));

        if (status.HeatConversionEnergyOutputPerSecond > 0 || UpgradeCalculator.GetHeatConversionPerSecond(_world, definition) > 0)
            parts.Add("HEAT->E +" + FormatNumber(status.HeatConversionEnergyOutputPerSecond) + "/S");

        if (status.ResearchOutputPerSecond > 0 || UpgradeCalculator.GetResearchPerSecond(_world, definition) > 0)
            parts.Add("R " + FormatEffectiveGross(status.ResearchOutputPerSecond, UpgradeCalculator.GetResearchPerSecond(_world, definition)));

        var moneyPerSecond = GetEstimatedMoneyPerSecond(status);
        if (moneyPerSecond > 0)
            parts.Add("$ +" + FormatNumber(moneyPerSecond) + "/S");

        return parts.Count == 0 ? "-" : string.Join(" | ", parts);
    }

    private string FormatBuildingProduction(BuildingDefinition definition)
    {
        var parts = new List<string>();
        var energy = UpgradeCalculator.GetEnergyPerSecond(_world, definition);
        var heatConversion = UpgradeCalculator.GetHeatConversionPerSecond(_world, definition);
        var research = UpgradeCalculator.GetResearchPerSecond(_world, definition);
        var moneyPerSecond = GetEstimatedMoneyPerSecond(definition);

        if (energy > 0)
            parts.Add("E +" + FormatNumber(energy) + "/S");

        if (heatConversion > 0)
            parts.Add("HEAT->E +" + FormatNumber(heatConversion) + "/S");

        if (research > 0)
            parts.Add("R +" + FormatNumber(research) + "/S");

        if (moneyPerSecond > 0)
            parts.Add("$ +" + FormatNumber(moneyPerSecond) + "/S");

        return parts.Count == 0 ? "-" : string.Join(" | ", parts);
    }

    private string FormatBuildingConsumption(BuildingOperationalStatus status)
    {
        var parts = new List<string>();

        if (status.EnergyInputPerSecond > 0)
            parts.Add("E -" + FormatNumber(status.EnergyInputPerSecond) + "/S");

        if (status.AutoSellInputPerSecond > 0)
            parts.Add("AUTOSELL -" + FormatNumber(status.AutoSellInputPerSecond) + "/S");

        return parts.Count == 0 ? "-" : string.Join(" | ", parts);
    }

    private string FormatBuildingConsumption(BuildingDefinition definition)
    {
        var parts = new List<string>();
        var energyIn = UpgradeCalculator.GetEnergyConsumptionPerSecond(_world, definition);
        var autoSell = UpgradeCalculator.GetAutoSellPerSecond(_world, definition);

        if (energyIn > 0)
            parts.Add("E -" + FormatNumber(energyIn) + "/S");

        if (autoSell > 0)
            parts.Add("AUTOSELL -" + FormatNumber(autoSell) + "/S");

        return parts.Count == 0 ? "-" : string.Join(" | ", parts);
    }

    private static string FormatBuildingStorage(BuildingOperationalStatus status, BuildingDefinition definition)
    {
        var parts = new List<string>();

        if (status.BatteryCapacity > 0)
            parts.Add("BATTERY +" + FormatNumber(status.BatteryCapacity));

        if (definition.ToolCapacityBonus > 0)
            parts.Add("TOOLS +" + FormatNumber(definition.ToolCapacityBonus));

        if (definition.EnergyEfficiencyBonus > 0)
            parts.Add("GRID +" + FormatPercent(definition.EnergyEfficiencyBonus));

        return parts.Count == 0 ? "-" : string.Join(" | ", parts);
    }

    private static string FormatBuildingStorage(BuildingDefinition definition)
    {
        var parts = new List<string>();

        if (definition.BatteryCapacity > 0)
            parts.Add("BATTERY +" + FormatNumber(definition.BatteryCapacity));

        if (definition.ToolCapacityBonus > 0)
            parts.Add("TOOLS +" + FormatNumber(definition.ToolCapacityBonus));

        if (definition.EnergyEfficiencyBonus > 0)
            parts.Add("GRID +" + FormatPercent(definition.EnergyEfficiencyBonus));

        return parts.Count == 0 ? "-" : string.Join(" | ", parts);
    }

    private string FormatBuildingHeat(BuildingOperationalStatus status, BuildingDefinition definition)
    {
        var parts = new List<string>();

        if (status.HeatOutputPerSecond > 0 || UpgradeCalculator.GetHeatPerSecond(_world, definition) > 0)
            parts.Add("OUT " + FormatEffectiveGross(status.HeatOutputPerSecond, UpgradeCalculator.GetHeatPerSecond(_world, definition)));

        if (UpgradeCalculator.GetHeatConversionPerSecond(_world, definition) > 0)
            parts.Add("IN " + FormatNumber(status.HeatConversionInputPerSecond) + "/S R" + definition.HeatRange);

        if (UpgradeCalculator.GetHeatDissipationPerSecond(_world, definition) > 0)
            parts.Add("COOL " + FormatNumber(UpgradeCalculator.GetHeatDissipationPerSecond(_world, definition)) + "/S R" + definition.HeatRange);

        if (status.HeatStored > 0 || UpgradeCalculator.GetHeatPerSecond(_world, definition) > 0)
            parts.Add("STORED " + FormatNumber(status.HeatStored) + "/" + FormatNumber(status.HeatExplosionThreshold));

        var risk = GetHeatRiskText(status);
        if (risk != "-")
            parts.Add(risk);

        return parts.Count == 0 ? "-" : string.Join(" | ", parts);
    }

    private string FormatBuildingHeat(BuildingDefinition definition)
    {
        var parts = new List<string>();
        var heatOut = UpgradeCalculator.GetHeatPerSecond(_world, definition);
        var heatIn = UpgradeCalculator.GetHeatConversionPerSecond(_world, definition);
        var heatDissipation = UpgradeCalculator.GetHeatDissipationPerSecond(_world, definition);

        if (heatOut > 0)
            parts.Add("OUT +" + FormatNumber(heatOut) + "/S");

        if (heatIn > 0)
            parts.Add("IN " + FormatNumber(heatIn) + "/S R" + definition.HeatRange);

        if (heatDissipation > 0)
            parts.Add("COOL " + FormatNumber(heatDissipation) + "/S R" + definition.HeatRange);

        return parts.Count == 0 ? "-" : string.Join(" | ", parts);
    }

    private static string FormatBuildingMaintenance(BuildingDefinition definition)
    {
        return definition.MaintenanceEfficiencyBonus > 0
            ? "WEAR -" + FormatPercent(definition.MaintenanceEfficiencyBonus)
            : "-";
    }

    private string GetManagerStatusText(string buildingDefinitionId, bool managed)
    {
        var managerResearch = _world.ResearchCatalog.All
            .FirstOrDefault(research => research.ManagedBuildingIds.Any(id =>
                string.Equals(id, buildingDefinitionId, StringComparison.OrdinalIgnoreCase)));

        if (managerResearch is null)
            return "-";

        return managed
            ? "ACTIVE: " + managerResearch.Name
            : "UNLOCK: " + managerResearch.Name;
    }

    private static string GetHeatRiskText(BuildingOperationalStatus status)
    {
        if (status.HeatOutputPerSecond <= 0 && status.HeatStored <= 0)
            return "-";

        if (status.HeatOutputPerSecond <= 0)
            return "NO NEW HEAT";

        if (status.HasHeatConverterInRange && status.HeatStored < status.HeatWarningThreshold)
            return "CONTROLLED";

        var remainingToExplosion = status.HeatExplosionThreshold - status.HeatStored;
        if (remainingToExplosion <= 0)
            return "EXPLOSION";

        var seconds = remainingToExplosion / Math.Max(0.0001, status.HeatOutputPerSecond);
        return seconds < 60
            ? $"EXPLODES IN {Math.Ceiling(seconds):0} S"
            : "RISK LOW";
    }

    private static string GetOperationalIssueText(BuildingOperationalStatus status, BuildingDefinition definition)
    {
        return status.State switch
        {
            BuildingOperationalState.Active => GetActiveOperationalNote(status, definition),
            BuildingOperationalState.HeatWarning => "HEAT ABOVE WARNING",
            BuildingOperationalState.NoEnergy => "NEEDS STORED ENERGY",
            BuildingOperationalState.NoHeatConversion => "PLACE GENERATOR IN RANGE",
            BuildingOperationalState.Expired => "REPLACE OR MANAGE",
            BuildingOperationalState.Exploded => "RESTORE OR DEMOLISH",
            _ => status.Label
        };
    }

    private static string GetActiveOperationalNote(BuildingOperationalStatus status, BuildingDefinition definition)
    {
        if (status.HeatOutputPerSecond > 0)
            return status.HasHeatConverterInRange ? "HEAT COVERAGE OK" : "NEEDS HEAT COVERAGE";

        if (status.EnergyInputPerSecond > 0)
            return "ENERGY SUPPLY OK";

        if (status.HeatConversionInputPerSecond > 0)
            return "ABSORBING HEAT IN RANGE";

        if (status.AutoSellInputPerSecond > 0)
            return "SELLING ENERGY";

        if (status.ResearchOutputPerSecond > 0)
            return "PRODUCING RESEARCH";

        if (status.EnergyOutputPerSecond > 0)
            return "PRODUCING ENERGY";

        if (status.BatteryCapacity > 0)
            return "STORING ENERGY";

        return definition.Category switch
        {
            BuildingCategory.Storage => "STORAGE READY",
            BuildingCategory.Automation => "WAITING FOR ENERGY",
            BuildingCategory.HeatConverter => "WAITING FOR HEAT IN RANGE",
            _ => "READY"
        };
    }

    private void FillTerrainPropertyRows(Dictionary<string, string> rows, Tile tile, out Color stateColor, out string action)
    {
        rows["TYPE"] = tile.Type.ToString().ToUpperInvariant();
        rows["PURPOSE"] = tile.Type == TileType.Forest ? "BLOCKS BUILDING, CLEAR WITH AXES" : tile.Type == TileType.Mountain ? "BLOCKS BUILDING, CLEAR WITH MINES" : "TERRAIN OBSTACLE";
        rows["STATE"] = "BLOCKED";
        rows["SIZE"] = "1 X 1";

        if (tile.Type == TileType.Forest)
        {
            var required = _world.ToolSettings.ForestClearAxesCost;
            var available = _world.Resources.Axes;
            rows["ISSUE"] = available >= required ? "READY TO CLEAR" : $"NEED {required - available:0} AXES";
            rows["TERRAIN COST"] = $"{available:0} / {required} AXES";
            action = available >= required ? "CLEAR AVAILABLE" : "NEED AXES";
        }
        else if (tile.Type == TileType.Mountain)
        {
            var required = _world.ToolSettings.MountainClearMinesCost;
            var available = _world.Resources.Mines;
            rows["ISSUE"] = available >= required ? "READY TO CLEAR" : $"NEED {required - available:0} MINES";
            rows["TERRAIN COST"] = $"{available:0} / {required} MINES";
            action = available >= required ? "CLEAR AVAILABLE" : "NEED MINES";
        }
        else
        {
            rows["ISSUE"] = "NOT CLEARABLE";
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
        var moneyCost = _world.AreaUnlockSettings.CloudUnlockMoneyCost;
        var researchCost = _world.AreaUnlockSettings.CloudUnlockResearchCost;
        var hasMoney = _world.Resources.Money >= moneyCost;
        var hasResearch = _world.Resources.Research >= researchCost;
        var canUnlock = CanUnlockCloud();

        rows["TYPE"] = "CLOUD";
        rows["PURPOSE"] = "UNLOCKS MORE MAP AREA";
        rows["STATE"] = canUnlock ? "READY TO UNLOCK" : "LOCKED";
        rows["ISSUE"] = GetCloudUnlockIssueText(hasMoney, hasResearch, tile.CoveredType.HasValue, tilesToUnlock);
        rows["SIZE"] = "1 X 1";
        rows["REVEAL"] = $"{revealText}, {tilesToUnlock} TILE(S)";
        rows["UNLOCK COST"] = $"${FormatNumber((double)_world.Resources.Money)} / ${FormatNumber((double)moneyCost)} + R{FormatNumber(_world.Resources.Research)} / R{FormatNumber(researchCost)}";
        action = canUnlock ? "UNLOCK AVAILABLE" : "NEED RESOURCES";
        stateColor = canUnlock ? new Color(150, 220, 245) : new Color(145, 155, 170);
    }

    private static string GetCloudUnlockIssueText(bool hasMoney, bool hasResearch, bool hasHiddenTile, int tilesToUnlock)
    {
        if (!hasHiddenTile || tilesToUnlock <= 0)
            return "NOTHING TO REVEAL";

        if (!hasMoney && !hasResearch)
            return "NEED MONEY AND RESEARCH";

        if (!hasMoney)
            return "NEED MONEY";

        if (!hasResearch)
            return "NEED RESEARCH";

        return "READY TO REVEAL";
    }

    private string GetBuildToolHintText(BuildingDefinition definition)
    {
        var heatOut = UpgradeCalculator.GetHeatPerSecond(_world, definition);
        var heatIn = UpgradeCalculator.GetHeatConversionPerSecond(_world, definition);

        if (heatIn > 0 && definition.HeatRange > 0)
        {
            return HasBuiltHeatProducer()
                ? $"PLACE WITH HEAT SOURCE IN RANGE {definition.HeatRange}"
                : "BUILD HEAT SOURCE FIRST";
        }

        if (heatOut > 0)
        {
            return HasBuiltHeatConverter()
                ? "PLACE INSIDE GENERATOR RANGE"
                : "BUILD GENERATOR AFTER THIS";
        }

        return "-";
    }

    private string GetBuildToolActionText(BuildingDefinition definition)
    {
        var heatOut = UpgradeCalculator.GetHeatPerSecond(_world, definition);
        var heatIn = UpgradeCalculator.GetHeatConversionPerSecond(_world, definition);

        if (heatIn > 0 && definition.HeatRange > 0)
            return "CLICK CELL, CHECK RANGE PREVIEW";

        if (heatOut > 0)
            return "CLICK CELL, THEN ADD GENERATOR";

        return "LEFT CLICK PLAIN CELL";
    }

    private bool HasBuiltHeatProducer()
    {
        return _world.BuildingInstances.Values.Any(instance =>
            _world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition) &&
            UpgradeCalculator.GetHeatPerSecond(_world, definition) > 0);
    }

    private bool HasBuiltHeatConverter()
    {
        return _world.BuildingInstances.Values.Any(instance =>
            _world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition) &&
            definition.HeatRange > 0 &&
            UpgradeCalculator.GetHeatConversionPerSecond(_world, definition) > 0);
    }

    private void FillBuildToolPropertyRows(Dictionary<string, string> rows, BuildingDefinition definition, out Color stateColor, out string action)
    {
        rows["TYPE"] = definition.Category.ToString().ToUpperInvariant();
        rows["PURPOSE"] = GetPropertyPurposeText(definition);
        rows["ISSUE"] = GetBuildToolHintText(definition);
        rows["BUILD TOOL"] = Shorten(definition.Name.ToUpperInvariant(), 24);
        rows["COST"] = "$" + FormatNumber((double)definition.Cost);
        rows["SIZE"] = $"{definition.Width} X {definition.Height}";
        rows["REQUIRES"] = GetBuildingRequirementText(definition);
        rows["PRODUCES"] = FormatBuildingProduction(definition);
        rows["CONSUMES"] = FormatBuildingConsumption(definition);
        rows["STORAGE"] = FormatBuildingStorage(definition);
        rows["HEAT"] = FormatBuildingHeat(definition);
        rows["MAINTENANCE"] = FormatBuildingMaintenance(definition);
        rows["NEXT UPGRADE"] = GetNextUpgradeCostText(definition);
        rows["ECONOMY"] = FormatEstimatedMoneyPerSecond(GetEstimatedMoneyPerSecond(definition));
        rows["PAYBACK"] = FormatPayback(definition.Cost, GetEstimatedMoneyPerSecond(definition));
        action = _world.Resources.Money >= definition.Cost
            ? GetBuildToolActionText(definition)
            : "NEED MONEY TO BUILD";
        stateColor = new Color(255, 220, 80);
    }

    private void FillEmptyTilePropertyRows(Dictionary<string, string> rows, Tile tile, string? selectedBuildingId, out Color stateColor, out string action)
    {
        rows["TYPE"] = GetTileDisplayName(tile.Type).ToUpperInvariant();
        rows["PURPOSE"] = tile.Type == TileType.Land ? "EMPTY BUILDABLE TERRAIN" : "TERRAIN BLOCKS BUILDING";
        rows["STATE"] = tile.Type == TileType.Land ? "FREE / BUILDABLE" : "NOT BUILDABLE";

        if (!string.IsNullOrWhiteSpace(selectedBuildingId) &&
            _world.BuildingCatalog.TryGet(selectedBuildingId, out var selectedDefinition))
        {
            rows["PURPOSE"] = GetPropertyPurposeText(selectedDefinition);
            rows["ISSUE"] = GetBuildToolHintText(selectedDefinition);
            rows["BUILD TOOL"] = Shorten(selectedDefinition.Name.ToUpperInvariant(), 24);
            rows["COST"] = "$" + FormatNumber((double)selectedDefinition.Cost);
            rows["SIZE"] = $"{selectedDefinition.Width} X {selectedDefinition.Height}";
            rows["REQUIRES"] = GetBuildingRequirementText(selectedDefinition);
            rows["PRODUCES"] = FormatBuildingProduction(selectedDefinition);
            rows["CONSUMES"] = FormatBuildingConsumption(selectedDefinition);
            rows["STORAGE"] = FormatBuildingStorage(selectedDefinition);
            rows["HEAT"] = FormatBuildingHeat(selectedDefinition);
            rows["MAINTENANCE"] = FormatBuildingMaintenance(selectedDefinition);
            rows["ECONOMY"] = FormatEstimatedMoneyPerSecond(GetEstimatedMoneyPerSecond(selectedDefinition));
            rows["PAYBACK"] = FormatPayback(selectedDefinition.Cost, GetEstimatedMoneyPerSecond(selectedDefinition));

            if (tile.Type != TileType.Land)
                action = "TOOL ACTIVE - NOT BUILDABLE";
            else if (_world.Resources.Money < selectedDefinition.Cost)
                action = "TOOL ACTIVE - NEED MONEY";
            else
                action = "TOOL ACTIVE - " + GetBuildToolActionText(selectedDefinition);
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


    private string GetBuildLockedText(BuildingDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(definition.RequiredResearchId))
            return "LOCKED";

        return _world.ResearchCatalog.TryGet(definition.RequiredResearchId, out var research)
            ? "REQ " + research.Name
            : "REQ RESEARCH";
    }

    private string GetBuildButtonMainEffectText(BuildingDefinition definition)
    {
        var energyOut = UpgradeCalculator.GetEnergyPerSecond(_world, definition);
        var heatOut = UpgradeCalculator.GetHeatPerSecond(_world, definition);
        var researchOut = UpgradeCalculator.GetResearchPerSecond(_world, definition);
        var batteryCapacity = UpgradeCalculator.GetBatteryCapacity(_world, definition);
        var autoSell = UpgradeCalculator.GetAutoSellPerSecond(_world, definition);
        var heatIn = UpgradeCalculator.GetHeatConversionPerSecond(_world, definition);
        var heatDissipation = UpgradeCalculator.GetHeatDissipationPerSecond(_world, definition);
        var energyIn = UpgradeCalculator.GetEnergyConsumptionPerSecond(_world, definition);

        if (definition.EnergyEfficiencyBonus > 0)
            return $"BOOSTS GRID +{FormatPercent(definition.EnergyEfficiencyBonus)}";

        if (definition.MaintenanceEfficiencyBonus > 0)
            return $"SLOWS WEAR {FormatPercent(definition.MaintenanceEfficiencyBonus)}";

        if (definition.ToolCapacityBonus > 0)
            return $"STORES TOOLS +{FormatNumber(definition.ToolCapacityBonus)}";

        if (energyOut > 0)
            return $"PRODUCES {FormatNumber(energyOut)}/S ENERGY";

        if (heatIn > 0)
            return $"CONVERTS {FormatNumber(heatIn)}/S HEAT";

        if (heatDissipation > 0)
            return $"DISSIPATES {FormatNumber(heatDissipation)}/S HEAT";

        if (heatOut > 0)
            return $"PRODUCES {FormatNumber(heatOut)}/S HEAT";

        if (researchOut > 0)
            return $"PRODUCES R{FormatNumber(researchOut)}/S";

        if (batteryCapacity > 0)
            return $"ADDS {FormatNumber(batteryCapacity)} STORAGE";

        if (autoSell > 0)
            return $"SELLS {FormatNumber(autoSell)}/S ENERGY";

        if (energyIn > 0)
            return $"USES {FormatNumber(energyIn)}/S ENERGY";

        return $"NET ENERGY {FormatNetEnergy(definition)}";
    }

    private static string GetBuildButtonDetailText(BuildingDefinition definition)
    {
        var category = definition.Category switch
        {
            BuildingCategory.PowerProducer => "POWER",
            BuildingCategory.Storage => "STORAGE",
            BuildingCategory.Automation => "AUTO SELL",
            BuildingCategory.Maintenance => "MAINTENANCE",
            BuildingCategory.ToolStorage => "TOOL STORAGE",
            BuildingCategory.Research => "RESEARCH",
            BuildingCategory.HeatProducer => "HEAT SOURCE",
            BuildingCategory.HeatConverter => "HEAT CONVERTER",
            BuildingCategory.HeatSink => "HEAT SINK",
            BuildingCategory.Corporation => "CORPORATION",
            BuildingCategory.Special when definition.EnergyEfficiencyBonus > 0 => "GRID SUPPORT",
            _ => "BUILDING"
        };

        return $"SIZE {definition.Width} X {definition.Height} | {category}";
    }

    private string GetBuildButtonSupportText(BuildingDefinition definition)
    {
        var energyIn = UpgradeCalculator.GetEnergyConsumptionPerSecond(_world, definition);
        var heatOut = UpgradeCalculator.GetHeatPerSecond(_world, definition);
        var heatIn = UpgradeCalculator.GetHeatConversionPerSecond(_world, definition);
        var heatDissipation = UpgradeCalculator.GetHeatDissipationPerSecond(_world, definition);
        var netEnergy = FormatNetEnergy(definition);

        if (definition.MaintenanceEfficiencyBonus > 0)
            return "EXTENDS BUILDING LIFETIME";

        if (definition.ToolCapacityBonus > 0)
            return "INCREASES AXE AND MINE CAPACITY";

        if (heatIn > 0)
        {
            var range = Math.Max(0, definition.HeatRange);
            return range > 0
                ? $"PLACE NEAR HEAT SOURCE, RANGE {range}"
                : "NEEDS STORED HEAT";
        }

        if (heatDissipation > 0)
        {
            var range = Math.Max(0, definition.HeatRange);
            return range > 0
                ? $"COOLS HEAT WITHOUT ENERGY, RANGE {range}"
                : "COOLS STORED HEAT";
        }

        if (heatOut > 0)
            return "PLACE GENERATOR IN RANGE";

        if (energyIn > 0)
            return $"USES {FormatNumber(energyIn)}/S ENERGY";

        if (definition.BatteryCapacity > 0)
            return "PREVENTS ENERGY WASTE";

        if (definition.AutoSellPerSecond > 0)
            return "TURNS STORED ENERGY INTO MONEY";

        if (definition.ResearchPerSecond > 0)
            return "UNLOCKS NEW TECHNOLOGIES";

        return $"NET ENERGY {netEnergy} | NO HEAT";
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
            BuildingCategory.Maintenance => "Slows operational wear.",
            BuildingCategory.ToolStorage => "Increases tool storage capacity.",
            BuildingCategory.Research => "Produces research points.",
            BuildingCategory.HeatProducer => "Produces heat for generators.",
            BuildingCategory.HeatConverter => "Converts heat into energy.",
            BuildingCategory.HeatSink => "Dissipates heat without producing energy.",
            BuildingCategory.Corporation => "Advanced economic building.",
            BuildingCategory.Special when definition.EnergyEfficiencyBonus > 0 => "Improves grid energy efficiency.",
            _ => "Building."
        };
    }

    private static string GetPropertyPurposeText(BuildingDefinition definition)
    {
        return definition.Category switch
        {
            BuildingCategory.PowerProducer => "PRODUCES ELECTRIC ENERGY",
            BuildingCategory.Storage => "STORES UNUSED ENERGY",
            BuildingCategory.Automation => "SELLS ENERGY FOR MONEY",
            BuildingCategory.Maintenance => "SLOWS BUILDING WEAR",
            BuildingCategory.ToolStorage => "EXPANDS TOOL CAPACITY",
            BuildingCategory.Research => "PRODUCES RESEARCH POINTS",
            BuildingCategory.HeatProducer => "PRODUCES HEAT FOR GENERATORS",
            BuildingCategory.HeatConverter => "TURNS HEAT INTO ENERGY",
            BuildingCategory.HeatSink => "DISSIPATES HEAT SAFELY",
            BuildingCategory.Corporation => "ADVANCED ECONOMIC BUILDING",
            BuildingCategory.Special when definition.EnergyEfficiencyBonus > 0 => "BOOSTS GRID EFFICIENCY",
            _ => string.IsNullOrWhiteSpace(definition.Description) ? "BUILDING" : definition.Description.ToUpperInvariant()
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


    private string GetResearchPrerequisiteText(ResearchDefinition definition)
    {
        var missingId = definition.RequiredResearchIds.FirstOrDefault(x => !_world.Research.IsCompleted(x));
        if (string.IsNullOrWhiteSpace(missingId))
            return "REQ RESEARCH";

        return _world.ResearchCatalog.TryGet(missingId, out var research)
            ? "REQ " + research.Name
            : "REQ RESEARCH";
    }

    private string GetResearchActionText(ResearchDefinition definition)
    {
        if (definition.UnlockBuildingIds.Count > 0)
        {
            var firstId = definition.UnlockBuildingIds[0];
            var firstName = _world.BuildingCatalog.TryGet(firstId, out var building) ? building.Name : firstId;
            return definition.UnlockBuildingIds.Count == 1
                ? "NEW BUILDING " + firstName
                : $"NEW BUILDINGS {definition.UnlockBuildingIds.Count}";
        }

        if (definition.ManagedBuildingIds.Count > 0)
        {
            var firstId = definition.ManagedBuildingIds[0];
            var firstName = _world.BuildingCatalog.TryGet(firstId, out var building) ? building.Name : firstId;
            return definition.ManagedBuildingIds.Count == 1
                ? "AUTO MANAGES " + firstName
                : $"AUTO MANAGES {definition.ManagedBuildingIds.Count} BUILDINGS";
        }

        return string.IsNullOrWhiteSpace(definition.Description)
            ? "IMPROVES THE GRID"
            : definition.Description;
    }

    private string GetResearchButtonDetailText(ResearchDefinition definition)
    {
        if (definition.UnlockBuildingIds.Count > 0)
        {
            var names = definition.UnlockBuildingIds
                .Select(id => _world.BuildingCatalog.TryGet(id, out var building) ? building.Name : id)
                .Take(2)
                .ToList();

            if (definition.UnlockBuildingIds.Count > 2)
                return "UNLOCK LIST " + string.Join(", ", names) + $" +{definition.UnlockBuildingIds.Count - 2}";

            return "UNLOCK LIST " + string.Join(", ", names);
        }

        if (definition.ManagedBuildingIds.Count > 0)
            return GetManagerResearchDetailText(definition);

        if (!string.IsNullOrWhiteSpace(definition.Description))
            return definition.Description.ToUpperInvariant();

        return "RESEARCH IMPROVEMENT";
    }

    private string GetManagerResearchDetailText(ResearchDefinition definition)
    {
        var builtCount = _world.BuildingInstances.Values.Count(instance =>
            definition.ManagedBuildingIds.Any(id =>
                string.Equals(id, instance.DefinitionId, StringComparison.OrdinalIgnoreCase)));

        var expiredCount = _world.BuildingInstances.Values.Count(instance =>
            instance.State == BuildingState.Expired &&
            definition.ManagedBuildingIds.Any(id =>
                string.Equals(id, instance.DefinitionId, StringComparison.OrdinalIgnoreCase)));

        var completed = _world.Research.IsCompleted(definition.Id);
        var prefix = completed ? "MANAGING" : "WILL MANAGE";

        if (builtCount <= 0)
            return prefix + " 0 BUILT";

        if (expiredCount > 0)
            return $"{prefix} {builtCount} BUILT | EXPIRED {expiredCount}";

        return $"{prefix} {builtCount} BUILT";
    }

    private string GetUpgradeTargetText(UpgradeDefinition definition)
    {
        if (!string.IsNullOrWhiteSpace(definition.TargetBuildingId) &&
            _world.BuildingCatalog.TryGet(definition.TargetBuildingId, out var building))
        {
            return "TARGET " + building.Name;
        }

        return definition.EffectType switch
        {
            UpgradeEffectType.MultiplyToolAxesGeneration => "TARGET AXES PRODUCTION",
            UpgradeEffectType.MultiplyToolMinesGeneration => "TARGET MINES PRODUCTION",
            _ => string.IsNullOrWhiteSpace(definition.Description) ? "IMPROVES THE GRID" : definition.Description
        };
    }

    private string GetUpgradeButtonDetailText(UpgradeDefinition definition, int level, bool completed)
    {
        var levelText = $"LV {level} / {definition.MaxLevel}";
        if (completed)
            return levelText + " | UPGRADE COMPLETE";

        if (!string.IsNullOrWhiteSpace(definition.RequiredResearchId) &&
            !_world.Research.IsCompleted(definition.RequiredResearchId))
        {
            var researchName = _world.ResearchCatalog.TryGet(definition.RequiredResearchId, out var research)
                ? research.Name
                : "RESEARCH";
            return levelText + " | REQUIRES " + researchName;
        }

        var nextLevel = Math.Min(level + 1, definition.MaxLevel);
        return levelText + $" | NEXT LEVEL {nextLevel}";
    }

    private static Color GetUpgradeAccentColor(UpgradeDefinition definition)
    {
        return definition.EffectType switch
        {
            UpgradeEffectType.MultiplyEnergyProduction => new Color(135, 210, 255),
            UpgradeEffectType.MultiplyLifetime => new Color(255, 210, 95),
            UpgradeEffectType.MultiplyResearchProduction => new Color(210, 190, 255),
            UpgradeEffectType.MultiplyHeatProduction => new Color(245, 145, 55),
            UpgradeEffectType.MultiplyBatteryCapacity => new Color(240, 205, 70),
            UpgradeEffectType.MultiplyAutoSell => new Color(180, 225, 190),
            UpgradeEffectType.MultiplyHeatConversion => new Color(70, 220, 190),
            UpgradeEffectType.MultiplyToolAxesGeneration => new Color(210, 235, 190),
            UpgradeEffectType.MultiplyToolMinesGeneration => new Color(230, 210, 170),
            _ => new Color(190, 215, 255)
        };
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
        if (IsPropertyGroupStart(label) && rowIndex > 0)
        {
            var separator = new Rectangle(panel.X + 10, y - 6, panel.Width - 20, 1);
            spriteBatch.Draw(_pixel, separator, new Color(70, 86, 108, 150));
        }

        var row = new Rectangle(panel.X + 10, y - 3, panel.Width - 20, 18);
        if (rowIndex % 2 == 0)
            spriteBatch.Draw(_pixel, row, new Color(36, 45, 60, 120));

        var displayLabel = GetPropertyDisplayLabel(label);
        _text.DrawString(spriteBatch, displayLabel, new Vector2(panel.X + 14, y), GetPropertyLabelColor(label), 1);
        _text.DrawString(spriteBatch, Shorten(value, 34), new Vector2(panel.X + 136, y), valueColor, 1);
        y += 20;
    }

    private static bool IsPropertyGroupStart(string label)
    {
        return label is "COST" or "PRODUCES" or "LIFETIME" or "REVEAL";
    }

    private static string GetPropertyDisplayLabel(string label)
    {
        return label switch
        {
            "ECONOMY" => "MONEY / S",
            "SIZE" => "FOOTPRINT",
            "REVEAL" => "REVEALS",
            "TERRAIN COST" => "CLEAR COST",
            _ => label
        };
    }

    private static Color GetPropertyLabelColor(string label)
    {
        return label switch
        {
            "TYPE" or "PURPOSE" or "STATE" or "ISSUE" or "BUILD TOOL" or "REQUIRES" => new Color(175, 190, 210),
            "COST" or "NEXT UPGRADE" or "ECONOMY" or "PAYBACK" => new Color(205, 190, 150),
            "LIFETIME" or "MANAGER" or "MAINTENANCE" => new Color(170, 195, 170),
            "PRODUCES" or "CONSUMES" or "STORAGE" => new Color(145, 195, 225),
            "HEAT" => new Color(220, 150, 95),
            "REVEAL" or "TERRAIN COST" or "UNLOCK COST" => new Color(190, 190, 170),
            "ACTION" => new Color(255, 225, 120),
            _ => new Color(155, 170, 190)
        };
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
            "COST" or "UNLOCK COST" or "TERRAIN COST" or "ACTION" => new Color(255, 225, 120),
            "PURPOSE" => new Color(185, 205, 225),
            "ISSUE" or "REQUIRES" => new Color(255, 225, 120),
            "NEXT UPGRADE" => new Color(210, 190, 255),
            "ECONOMY" or "PAYBACK" => new Color(180, 225, 190),
            "PRODUCES" => new Color(135, 210, 255),
            "CONSUMES" => new Color(255, 165, 120),
            "BUILD TOOL" => new Color(255, 220, 80),
            "HEAT" => new Color(245, 145, 55),
            "STORAGE" => new Color(240, 205, 70),
            "MANAGER" or "MAINTENANCE" or "LIFETIME" => new Color(170, 230, 170),
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

    private void DrawHelpPanel(SpriteBatch spriteBatch, Viewport viewport, bool showHelpPanel)
    {
        if (!showHelpPanel)
            return;

        var panel = GetHelpPanelRectangle(viewport);
        if (panel.Width <= 0 || panel.Height <= 0)
            return;

        spriteBatch.Draw(_pixel, panel, new Color(24, 31, 42, 242));
        DrawOutline(spriteBatch, panel, new Color(120, 145, 175, 230), 2);

        var x = panel.X + 16;
        var y = panel.Y + 14;
        _text.DrawString(spriteBatch, "QUICK GUIDE", new Vector2(x, y), new Color(235, 240, 245), 2);
        _text.DrawString(spriteBatch, "H OR HELP TO TOGGLE", new Vector2(panel.Right - 150, y + 5), new Color(170, 185, 205), 1);

        y += 36;
        _text.DrawString(spriteBatch, "CURRENT", new Vector2(x, y), new Color(255, 225, 120), 1);
        y += 18;
        DrawHelpLine(spriteBatch, x, ref y, ">", GetCurrentObjectiveHint().Replace("OBJECTIVE: ", ""));
        DrawHelpLine(spriteBatch, x, ref y, "NEXT", GetCurrentObjectiveDetailHint());
        DrawHelpLine(spriteBatch, x, ref y, "BOT", GetCurrentBottleneckHint());

        foreach (var item in GetEarlyChecklistItems())
        {
            var marker = item.Done ? "OK" : "--";
            DrawHelpLine(spriteBatch, x, ref y, marker, item.Text.ToUpperInvariant());
        }

        y += 10;
        _text.DrawString(spriteBatch, "BASICS", new Vector2(x, y), new Color(255, 225, 120), 1);
        y += 18;
        DrawHelpLine(spriteBatch, x, ref y, "1", "BUILD WIND TURBINE TO PRODUCE ENERGY");
        DrawHelpLine(spriteBatch, x, ref y, "2", "SELL ENERGY, THEN BUILD OFFICE FOR AUTO MONEY");
        DrawHelpLine(spriteBatch, x, ref y, "3", "RESEARCH UNLOCKS BUILDINGS AND MANAGERS");
        DrawHelpLine(spriteBatch, x, ref y, "4", "GENERATORS CONVERT HEAT INSIDE THEIR RANGE");
        DrawHelpLine(spriteBatch, x, ref y, "5", "CLEAR OBSTACLES AND UNLOCK CLOUDS TO EXPAND");

        y += 10;
        _text.DrawString(spriteBatch, "CONTROLS", new Vector2(x, y), new Color(255, 225, 120), 1);
        y += 18;
        DrawHelpLine(spriteBatch, x, ref y, "-", "LEFT CLICK SELECT / BUILD / ACTION BUTTONS");
        DrawHelpLine(spriteBatch, x, ref y, "-", "RIGHT CLICK CANCELS ACTIVE BUILD TOOL");
        DrawHelpLine(spriteBatch, x, ref y, "-", "1-0 SELECT BUILDINGS, F5 SAVE, F6 AUTOSAVE, F9 LOAD");
        DrawHelpLine(spriteBatch, x, ref y, "-", "VIEW TO TOGGLE FULLSCREEN");
    }

    private void DrawHelpLine(SpriteBatch spriteBatch, int x, ref int y, string bullet, string text)
    {
        _text.DrawString(spriteBatch, bullet, new Vector2(x, y), new Color(150, 210, 255), 1);
        _text.DrawString(spriteBatch, Shorten(text, 62), new Vector2(x + 22, y), new Color(215, 226, 238), 1);
        y += 18;
    }

    private void DrawStatus(SpriteBatch spriteBatch, Viewport viewport, string? selectedBuildingId, Guid? selectedMapBuildingId, BuildResult? lastBuildResult, ResearchResult? lastResearchResult, GridPosition? lastBuildFailurePosition, TerrainClearResult? lastTerrainClearResult, AreaUnlockResult? lastAreaUnlockResult, UpgradeResult? lastUpgradeResult, string? saveLoadMessage, string saveDataInfo, Guid? pendingDemolishBuildingId)
    {
        var statusBar = GetStatusBarRectangle(viewport);
        var y = statusBar.Y + 14;
        var selectedStatusMessage = GetSelectedBuildingStatusMessage(selectedMapBuildingId);
        var objectiveHint = GetCurrentObjectiveHint();
        var criticalWarning = CreateFeedbackFormatter().FormatCriticalWarning();
        var message = selectedStatusMessage ??
                      (selectedBuildingId is null
                          ? criticalWarning ?? objectiveHint
                          : $"BUILD TOOL {selectedBuildingId} - LEFT CLICK BUILD, RIGHT CLICK CANCEL");

        if (lastResearchResult is not null)
        {
            message = lastResearchResult.Success
                ? $"RESEARCH OK {lastResearchResult.ResearchId}"
                : CreateFeedbackFormatter().FormatResearchFailure(lastResearchResult);
        }

        if (lastBuildResult is not null)
        {
            message = lastBuildResult.Success
                ? "BUILD OK"
                : CreateFeedbackFormatter().FormatBuildFailure(lastBuildResult.FailureReason, selectedBuildingId, lastBuildFailurePosition);
        }

        if (lastTerrainClearResult is not null)
        {
            message = lastTerrainClearResult.Success
                ? "TERRAIN CLEARED"
                : GetTerrainClearFailureMessage(lastTerrainClearResult.FailureReason);
        }

        if (lastAreaUnlockResult is not null)
        {
            message = lastAreaUnlockResult.Success
                ? GetAreaUnlockSuccessMessage(lastAreaUnlockResult)
                : GetAreaUnlockFailureMessage(lastAreaUnlockResult.FailureReason);
        }

        if (lastUpgradeResult is not null)
        {
            message = lastUpgradeResult.Success
                ? $"UPGRADE OK {lastUpgradeResult.UpgradeId} LV {lastUpgradeResult.NewLevel}"
                : CreateFeedbackFormatter().FormatUpgradeFailure(lastUpgradeResult);
        }

        if (!string.IsNullOrWhiteSpace(saveLoadMessage))
            message = saveLoadMessage;

        if (pendingDemolishBuildingId.HasValue)
            message = "DEMOLISH REQUIRES CONFIRMATION: CLICK CONFIRM DEMOLISH";

        spriteBatch.Draw(_pixel, statusBar, new Color(25, 31, 42));
        DrawOutline(spriteBatch, statusBar, new Color(55, 67, 84), 1);
        _text.DrawString(spriteBatch, Shorten(message, 82), new Vector2(statusBar.X + 14, y), new Color(230, 238, 245), 1);
        DrawSaveDataInfo(spriteBatch, statusBar, saveDataInfo);
        DrawStatusBadgeLegend(spriteBatch, statusBar);
    }


    private void DrawSaveDataInfo(SpriteBatch spriteBatch, Rectangle statusBar, string saveDataInfo)
    {
        if (string.IsNullOrWhiteSpace(saveDataInfo) || statusBar.Width < 980)
            return;

        var text = Shorten(saveDataInfo, 46);
        var estimatedWidth = text.Length * 6;
        var x = Math.Max(statusBar.X + 520, statusBar.Right - estimatedWidth - 18);
        var y = statusBar.Y + 14;

        _text.DrawString(spriteBatch, text, new Vector2(x, y), new Color(165, 180, 200), 1);
    }

    private GameplayFeedbackFormatter CreateFeedbackFormatter()
    {
        return new GameplayFeedbackFormatter(_world);
    }

    private string GetCurrentBottleneckHint()
    {
        return CreateProgressionAdvisor().GetCurrentBottleneckHint();
    }

    private string GetCurrentObjectiveDetailHint()
    {
        return CreateProgressionAdvisor().GetCurrentObjectiveDetailHint();
    }

    private string GetCurrentObjectiveHint()
    {
        return CreateProgressionAdvisor().GetCurrentObjectiveHint();
    }

    private bool HasBuilt(string buildingDefinitionId)
    {
        return CreateProgressionAdvisor().HasBuilt(buildingDefinitionId);
    }

    private bool HasHeatProducerWithoutConverter()
    {
        return CreateProgressionAdvisor().HasHeatProducerWithoutConverter();
    }

    private ProgressionAdvisor CreateProgressionAdvisor()
    {
        return new ProgressionAdvisor(_world);
    }

    private string? GetSelectedBuildingStatusMessage(Guid? selectedMapBuildingId)
    {
        if (!selectedMapBuildingId.HasValue)
            return null;

        if (!_world.TryGetBuilding(selectedMapBuildingId.Value, out var instance))
            return null;

        if (!_world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
            return null;

        var status = BuildingOperationalStatusCalculator.Calculate(_world, instance);
        var name = definition.Name.ToUpperInvariant();

        return status.State switch
        {
            BuildingOperationalState.Active => GetActiveSelectedBuildingMessage(name, status),
            BuildingOperationalState.NoEnergy => $"{name}: NEED STORED ENERGY",
            BuildingOperationalState.NoHeatConversion => $"{name}: PLACE GENERATOR IN RANGE",
            BuildingOperationalState.HeatWarning => $"{name}: HEAT WARNING {FormatNumber(status.HeatStored)}/{FormatNumber(status.HeatExplosionThreshold)}",
            BuildingOperationalState.Expired => $"{name}: EXPIRED - REPLACE OR DEMOLISH",
            BuildingOperationalState.Exploded => $"{name}: EXPLODED - RESTORE OR DEMOLISH",
            _ => $"{name}: {status.Label}"
        };
    }

    private static string GetActiveSelectedBuildingMessage(string name, BuildingOperationalStatus status)
    {
        if (status.HeatOutputPerSecond > 0)
            return status.HasHeatConverterInRange
                ? $"{name}: ACTIVE - HEAT COVERAGE OK"
                : $"{name}: ACTIVE - NEED HEAT COVERAGE";

        if (status.HeatConversionInputPerSecond > 0)
            return $"{name}: ACTIVE - ABSORBING HEAT";

        if (status.AutoSellInputPerSecond > 0)
            return $"{name}: ACTIVE - SELLING ENERGY";

        if (status.ResearchOutputPerSecond > 0)
            return $"{name}: ACTIVE - PRODUCING RESEARCH";

        if (status.EnergyOutputPerSecond > 0)
            return $"{name}: ACTIVE - PRODUCING ENERGY";

        if (status.BatteryCapacity > 0)
            return $"{name}: ACTIVE - STORING ENERGY";

        return $"{name}: ACTIVE";
    }

    private static string GetBuildFailureMessage(BuildFailureReason reason)
    {
        return reason switch
        {
            BuildFailureReason.NotEnoughMoney => "BUILD FAILED: NEED MONEY",
            BuildFailureReason.ResearchRequired => "BUILD FAILED: RESEARCH REQUIRED",
            BuildFailureReason.TileAlreadyOccupied => "BUILD FAILED: CELL OCCUPIED",
            BuildFailureReason.TileNotBuildable => "BUILD FAILED: INVALID TERRAIN",
            BuildFailureReason.OutOfMap => "BUILD FAILED: OUT OF MAP",
            BuildFailureReason.UnknownBuilding => "BUILD FAILED: UNKNOWN BUILDING",
            BuildFailureReason.InvalidBuildingSize => "BUILD FAILED: NOT ENOUGH SPACE",
            BuildFailureReason.BuildingNotFound => "BUILD FAILED: BUILDING NOT FOUND",
            BuildFailureReason.BuildingNotExpired => "BUILD FAILED: BUILDING STILL ACTIVE",
            _ => $"BUILD FAILED: {reason}"
        };
    }

    private static string GetResearchFailureMessage(ResearchFailureReason reason)
    {
        return reason switch
        {
            ResearchFailureReason.NotEnoughResearch => "RESEARCH FAILED: NEED RESEARCH POINTS",
            ResearchFailureReason.MissingPrerequisite => "RESEARCH FAILED: MISSING PREREQUISITE",
            ResearchFailureReason.AlreadyCompleted => "RESEARCH FAILED: ALREADY COMPLETED",
            ResearchFailureReason.UnknownResearch => "RESEARCH FAILED: UNKNOWN RESEARCH",
            _ => $"RESEARCH FAILED: {reason}"
        };
    }

    private static string GetTerrainClearFailureMessage(TerrainClearFailureReason reason)
    {
        return reason switch
        {
            TerrainClearFailureReason.NotEnoughAxes => "CLEAR FAILED: NEED AXES",
            TerrainClearFailureReason.NotEnoughMines => "CLEAR FAILED: NEED MINES",
            TerrainClearFailureReason.TileHasBuilding => "CLEAR FAILED: CELL HAS BUILDING",
            TerrainClearFailureReason.NotClearableTerrain => "CLEAR FAILED: TERRAIN NOT CLEARABLE",
            TerrainClearFailureReason.OutOfMap => "CLEAR FAILED: OUT OF MAP",
            _ => $"CLEAR FAILED: {reason}"
        };
    }

    private static string GetAreaUnlockSuccessMessage(AreaUnlockResult result)
    {
        if (result.RevealedTiles.Count == 0)
            return "AREA UNLOCKED";

        var summary = string.Join(", ",
            result.RevealedTiles
                .GroupBy(tile => tile.RevealedTileType)
                .OrderBy(group => group.Key.ToString())
                .Select(group => $"{group.Key.ToString().ToUpperInvariant()} {group.Count()}"));

        return $"AREA UNLOCKED {result.TilesUnlocked}: {summary}";
    }

    private static string GetAreaUnlockFailureMessage(AreaUnlockFailureReason reason)
    {
        return reason switch
        {
            AreaUnlockFailureReason.NotEnoughMoney => "UNLOCK FAILED: NEED MONEY",
            AreaUnlockFailureReason.NotEnoughResearch => "UNLOCK FAILED: NEED RESEARCH",
            AreaUnlockFailureReason.TileAlreadyOccupied => "UNLOCK FAILED: CELL OCCUPIED",
            AreaUnlockFailureReason.TileNotCloud => "UNLOCK FAILED: SELECT CLOUD",
            AreaUnlockFailureReason.MissingHiddenTile => "UNLOCK FAILED: NOTHING TO REVEAL",
            AreaUnlockFailureReason.OutOfMap => "UNLOCK FAILED: OUT OF MAP",
            _ => $"UNLOCK FAILED: {reason}"
        };
    }

    private static string GetUpgradeFailureMessage(UpgradeFailureReason reason)
    {
        return reason switch
        {
            UpgradeFailureReason.NotEnoughMoney => "UPGRADE FAILED: NEED MONEY",
            UpgradeFailureReason.NotEnoughResearch => "UPGRADE FAILED: NEED RESEARCH",
            UpgradeFailureReason.MissingResearch => "UPGRADE FAILED: RESEARCH REQUIRED",
            UpgradeFailureReason.MaxLevelReached => "UPGRADE FAILED: MAX LEVEL",
            UpgradeFailureReason.UnknownUpgrade => "UPGRADE FAILED: UNKNOWN UPGRADE",
            _ => $"UPGRADE FAILED: {reason}"
        };
    }

    private void DrawStatusBadgeLegend(SpriteBatch spriteBatch, Rectangle statusBar)
    {
        const int itemWidth = 78;
        const int itemGap = 8;
        const int itemCount = 5;
        var totalWidth = itemWidth * itemCount + itemGap * (itemCount - 1);

        if (statusBar.Width < totalWidth + 560)
            return;

        var x = statusBar.Right - totalWidth - 14;
        var y = statusBar.Y + 11;

        DrawStatusBadgeLegendItem(spriteBatch, ref x, y, "E", "ENERGY", new Color(105, 60, 35), new Color(255, 165, 120), itemWidth, itemGap);
        DrawStatusBadgeLegendItem(spriteBatch, ref x, y, "G", "GEN", new Color(110, 45, 35), new Color(255, 150, 120), itemWidth, itemGap);
        DrawStatusBadgeLegendItem(spriteBatch, ref x, y, "H", "HEAT", new Color(110, 68, 20), new Color(245, 145, 55), itemWidth, itemGap);
        DrawStatusBadgeLegendItem(spriteBatch, ref x, y, "T", "TIME", new Color(90, 85, 55), new Color(255, 210, 95), itemWidth, itemGap);
        DrawStatusBadgeLegendItem(spriteBatch, ref x, y, "X", "BOOM", new Color(110, 25, 20), new Color(255, 85, 75), itemWidth, itemGap);
    }

    private void DrawStatusBadgeLegendItem(SpriteBatch spriteBatch, ref int x, int y, string badge, string label, Color background, Color outline, int itemWidth, int itemGap)
    {
        var badgeRect = new Rectangle(x, y, 18, 18);
        spriteBatch.Draw(_pixel, badgeRect, background);
        DrawOutline(spriteBatch, badgeRect, outline, 1);
        _text.DrawString(spriteBatch, badge, new Vector2(badgeRect.X + 5, badgeRect.Y + 6), new Color(255, 240, 220), 1);
        _text.DrawString(spriteBatch, label, new Vector2(badgeRect.Right + 5, y + 6), new Color(185, 198, 215), 1);
        x += itemWidth + itemGap;
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

    private static Rectangle GetMenuStripRectangle(Viewport viewport)
    {
        return new Rectangle(0, TopBarHeight, viewport.Width, MenuStripHeight);
    }

    private static Rectangle GetStatusBarRectangle(Viewport viewport)
    {
        return new Rectangle(0, Math.Max(0, viewport.Height - StatusBarHeight), viewport.Width, StatusBarHeight);
    }

    private static Rectangle GetSideMenuRectangle(Viewport viewport)
    {
        return new Rectangle(0, TopBarHeight + MenuStripHeight, SideMenuWidth, Math.Max(0, viewport.Height - TopBarHeight - MenuStripHeight - StatusBarHeight));
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

    private static Rectangle GetBuildTabButtonRectangle(Viewport viewport)
    {
        return new Rectangle(PanelMargin, TopBarHeight + 5, 70, 28);
    }

    private static Rectangle GetResearchTabButtonRectangle(Viewport viewport)
    {
        var build = GetBuildTabButtonRectangle(viewport);
        return new Rectangle(build.Right + 8, build.Y, 96, 28);
    }

    private static Rectangle GetUpgradeTabButtonRectangle(Viewport viewport)
    {
        var research = GetResearchTabButtonRectangle(viewport);
        return new Rectangle(research.Right + 8, research.Y, 90, 28);
    }

    private static Rectangle GetSaveButtonRectangle(Viewport viewport)
    {
        var load = GetLoadButtonRectangle(viewport);
        return new Rectangle(load.Right + 8, load.Y, 56, 28);
    }

    private static Rectangle GetLoadButtonRectangle(Viewport viewport)
    {
        var newGame = GetNewGameButtonRectangle(viewport);
        return new Rectangle(newGame.Right + 8, newGame.Y, 56, 28);
    }

    private static Rectangle GetNewGameButtonRectangle(Viewport viewport)
    {
        var x = GetGameCommandButtonsStartX(viewport);
        return new Rectangle(x, TopBarHeight + 5, 48, 28);
    }

    private static Rectangle GetToggleFullscreenButtonRectangle(Viewport viewport)
    {
        var save = GetSaveButtonRectangle(viewport);
        return new Rectangle(save.Right + 8, save.Y, 56, 28);
    }

    private static Rectangle GetHelpButtonRectangle(Viewport viewport)
    {
        var x = GetFutureSectionButtonsStartX(viewport);
        var y = TopBarHeight + 5;
        var maxRight = GetFutureSectionButtonsMaxRight(viewport);
        const int gap = 8;

        var stats = new Rectangle(x, y, 72, 28);
        if (stats.Right <= maxRight)
            x = stats.Right + gap;

        var help = new Rectangle(x, y, 64, 28);
        return help.Right <= maxRight ? help : Rectangle.Empty;
    }

    private static Rectangle GetExitButtonRectangle(Viewport viewport)
    {
        var view = GetToggleFullscreenButtonRectangle(viewport);
        return new Rectangle(view.Right + 8, view.Y, 56, 28);
    }

    private static int GetGameCommandButtonsStartX(Viewport viewport)
    {
        var properties = GetPropertiesPanelRectangle(viewport);
        const int totalWidth = 48 + 8 + 56 + 8 + 56 + 8 + 56 + 8 + 56;
        return Math.Max(GetUpgradeTabButtonRectangle(viewport).Right + 24, properties.X - totalWidth - 16);
    }

    private static int GetFutureSectionButtonsStartX(Viewport viewport)
    {
        return GetUpgradeTabButtonRectangle(viewport).Right + 12;
    }

    private static int GetFutureSectionButtonsMaxRight(Viewport viewport)
    {
        return GetNewGameButtonRectangle(viewport).X - 12;
    }

    public const int PropertiesPanelWidth = 380;

    private static Rectangle GetHelpPanelRectangle(Viewport viewport)
    {
        var properties = GetPropertiesPanelRectangle(viewport);
        var x = SideMenuWidth + 20;
        var right = properties.X - 20;
        var width = Math.Min(560, Math.Max(0, right - x));
        var height = Math.Min(448, Math.Max(0, viewport.Height - TopBarHeight - MenuStripHeight - StatusBarHeight - 40));
        var y = TopBarHeight + MenuStripHeight + 18;
        return new Rectangle(x, y, width, height);
    }

    private static Rectangle GetPropertiesPanelRectangle(Viewport viewport)
    {
        var width = Math.Min(PropertiesPanelWidth, Math.Max(280, viewport.Width - SideMenuWidth - 120));
        var x = Math.Max(SideMenuWidth + 20, viewport.Width - width);
        var y = TopBarHeight + MenuStripHeight;
        var height = Math.Max(0, viewport.Height - TopBarHeight - MenuStripHeight - StatusBarHeight);
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

    private static Rectangle GetLeftMenuListRectangle(Viewport viewport)
    {
        var top = MenuButtonsY;
        var bottom = Math.Max(top, viewport.Height - StatusBarHeight - 8);
        return new Rectangle(0, top, SideMenuWidth, bottom - top);
    }

    private static int GetBuildColumnX() => PanelMargin;
    private static int GetResearchColumnX() => GetBuildColumnX();
    private static int GetUpgradeColumnX() => GetBuildColumnX();

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
        var list = GetLeftMenuListRectangle(viewport);
        return rect.Top >= list.Top && rect.Bottom <= list.Bottom;
    }

    private static int ClampScrollOffset(int offset, int itemCount, int itemStride, Viewport viewport)
    {
        var visibleHeight = Math.Max(1, GetLeftMenuListRectangle(viewport).Height);
        var visibleItemCount = Math.Max(1, visibleHeight / itemStride);
        var maxFirstVisibleIndex = Math.Max(0, itemCount - visibleItemCount);
        var requestedFirstVisibleIndex = (int)Math.Round(offset / (double)itemStride, MidpointRounding.AwayFromZero);
        var clampedIndex = Math.Clamp(requestedFirstVisibleIndex, 0, maxFirstVisibleIndex);
        return clampedIndex * itemStride;
    }

    private void DrawColumnScrollHint(SpriteBatch spriteBatch, Viewport viewport, int x, int offset, int itemCount, int itemStride)
    {
        // Intentionally hidden: the list already responds to mouse wheel scrolling.
        // Keeping text hints between the tab bar and the left panel made the layout feel cluttered.
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

    private static string FormatEnergyEfficiencyBonus(BuildingDefinition definition)
    {
        return definition.EnergyEfficiencyBonus > 0
            ? "+" + FormatPercent(definition.EnergyEfficiencyBonus) + " ENERGY OUTPUT"
            : "-";
    }

    private static string FormatMaintenanceEfficiencyBonus(BuildingDefinition definition)
    {
        return definition.MaintenanceEfficiencyBonus > 0
            ? "-" + FormatPercent(definition.MaintenanceEfficiencyBonus) + " LIFETIME WEAR"
            : "-";
    }

    private static string FormatToolCapacityBonus(BuildingDefinition definition)
    {
        return definition.ToolCapacityBonus > 0
            ? "+" + FormatNumber(definition.ToolCapacityBonus) + " AXES/MINES CAP"
            : "-";
    }

    private static string FormatPercent(double value)
    {
        return (value * 100d).ToString("0.#") + "%";
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
