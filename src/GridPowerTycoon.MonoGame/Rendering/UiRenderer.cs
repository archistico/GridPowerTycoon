using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Research;
using GridPowerTycoon.Core.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GridPowerTycoon.MonoGame.Rendering;

public sealed class UiRenderer
{
    public const int TopBarHeight = 64;
    public const int SideMenuWidth = 210;

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
        Guid? selectedMapBuildingId)
    {
        var topBar = GetTopBarRectangle(viewport);
        var sideMenu = GetSideMenuRectangle(viewport);

        spriteBatch.Draw(_pixel, topBar, new Color(32, 39, 52));
        spriteBatch.Draw(_pixel, sideMenu, new Color(38, 48, 62));

        DrawTopBar(spriteBatch, topBar);
        DrawBuildMenu(spriteBatch, selectedBuildingId);
        DrawResearchMenu(spriteBatch);
        DrawSelectedBuildingPanel(spriteBatch, viewport, selectedMapBuildingId);
        DrawStatus(spriteBatch, viewport, selectedBuildingId, lastBuildResult, lastResearchResult);
    }

    public bool IsMouseOverUi(Point mousePosition, Viewport viewport)
    {
        return GetTopBarRectangle(viewport).Contains(mousePosition) ||
               GetSideMenuRectangle(viewport).Contains(mousePosition) ||
               GetSelectedBuildingPanelRectangle(viewport).Contains(mousePosition);
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

    public IReadOnlyList<string> GetBuildButtonIds()
    {
        return BuildButtonIds;
    }

    private void DrawTopBar(SpriteBatch spriteBatch, Rectangle topBar)
    {
        var resources = _world.Resources;
        var money = FormatNumber((double)resources.Money);
        var energy = $"{FormatNumber(resources.Energy)}/{FormatNumber(resources.MaxEnergy)}";
        var research = FormatNumber(resources.Research);

        _text.DrawString(spriteBatch, $"ENERGY {energy}", new Vector2(14, 16), new Color(230, 238, 245), 2);
        _text.DrawString(spriteBatch, $"RESEARCH {research}", new Vector2(330, 16), new Color(210, 190, 255), 2);
        _text.DrawString(spriteBatch, $"MONEY ${money}", new Vector2(600, 16), new Color(255, 225, 120), 2);

        var sellButton = GetSellButtonRectangle(new Viewport(0, 0, topBar.Width, topBar.Height));
        spriteBatch.Draw(_pixel, sellButton, new Color(70, 120, 72));
        DrawOutline(spriteBatch, sellButton, new Color(150, 235, 150), 2);
        _text.DrawString(spriteBatch, "SELL", new Vector2(sellButton.X + 20, sellButton.Y + 12), new Color(235, 250, 235), 2);

        DrawEnergyBar(spriteBatch, new Rectangle(topBar.Width - 260, 18, 230, 22));
    }

    private void DrawEnergyBar(SpriteBatch spriteBatch, Rectangle rect)
    {
        spriteBatch.Draw(_pixel, rect, new Color(18, 24, 34));
        var ratio = _world.Resources.MaxEnergy <= 0 ? 0 : _world.Resources.Energy / _world.Resources.MaxEnergy;
        var fillWidth = (int)Math.Round(rect.Width * Math.Clamp(ratio, 0, 1));
        if (fillWidth > 0)
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, fillWidth, rect.Height), new Color(88, 185, 255));
        DrawOutline(spriteBatch, rect, new Color(120, 135, 155), 2);
    }

    private void DrawBuildMenu(SpriteBatch spriteBatch, string? selectedBuildingId)
    {
        _text.DrawString(spriteBatch, "BUILD", new Vector2(18, 82), new Color(230, 238, 245), 2);

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
            _text.DrawString(spriteBatch, $"{indexText} {Shorten(name, 17)}", new Vector2(rect.X + 44, rect.Y + 8), textColor, 1);
            _text.DrawString(spriteBatch, isLocked ? "LOCKED" : $"${cost}", new Vector2(rect.X + 44, rect.Y + 25), isLocked ? new Color(255, 150, 120) : new Color(255, 225, 120), 1);
        }
    }

    private void DrawResearchMenu(SpriteBatch spriteBatch)
    {
        _text.DrawString(spriteBatch, "RESEARCH", new Vector2(18, 448), new Color(230, 238, 245), 2);

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
            var status = completed ? "DONE" : missingPrereq ? "REQ" : $"R {cost}";

            _text.DrawString(spriteBatch, Shorten(name, 21), new Vector2(rect.X + 8, rect.Y + 6), new Color(235, 240, 245), 1);
            _text.DrawString(spriteBatch, status, new Vector2(rect.X + 8, rect.Y + 23), completed ? new Color(160, 245, 175) : new Color(210, 190, 255), 1);
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

        _text.DrawString(spriteBatch, Shorten(definition.Name.ToUpperInvariant(), 24), new Vector2(panel.X + 12, panel.Y + 12), new Color(235, 240, 245), 2);
        _text.DrawString(spriteBatch, $"STATE {instance.State.ToString().ToUpperInvariant()}", new Vector2(panel.X + 12, panel.Y + 46), GetStateColor(instance.State), 1);

        var lifetimeText = definition.LifetimeSeconds <= 0
            ? "LIFE -"
            : $"LIFE {FormatNumber(instance.RemainingLifetimeSeconds)}S/{FormatNumber(definition.LifetimeSeconds)}S";

        _text.DrawString(spriteBatch, lifetimeText, new Vector2(panel.X + 12, panel.Y + 66), new Color(210, 222, 235), 1);

        if (definition.EnergyPerSecond > 0)
            _text.DrawString(spriteBatch, $"ENERGY +{FormatNumber(definition.EnergyPerSecond)}/S", new Vector2(panel.X + 12, panel.Y + 86), new Color(135, 210, 255), 1);

        if (definition.ResearchPerSecond > 0)
            _text.DrawString(spriteBatch, $"RESEARCH +{FormatNumber(definition.ResearchPerSecond)}/S", new Vector2(panel.X + 12, panel.Y + 106), new Color(210, 190, 255), 1);

        if (definition.AutoSellPerSecond > 0)
            _text.DrawString(spriteBatch, $"AUTO SELL {FormatNumber(definition.AutoSellPerSecond)}/S", new Vector2(panel.X + 12, panel.Y + 126), new Color(180, 225, 190), 1);

        if (instance.State == BuildingState.Expired)
        {
            var replaceButton = GetReplaceButtonRectangle(viewport);
            spriteBatch.Draw(_pixel, replaceButton, new Color(88, 118, 72));
            DrawOutline(spriteBatch, replaceButton, new Color(180, 235, 135), 2);
            _text.DrawString(spriteBatch, $"REPLACE ${FormatNumber((double)definition.Cost)}", new Vector2(replaceButton.X + 12, replaceButton.Y + 10), new Color(235, 250, 220), 1);
        }
    }

    private void DrawStatus(SpriteBatch spriteBatch, Viewport viewport, string? selectedBuildingId, BuildResult? lastBuildResult, ResearchResult? lastResearchResult)
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

        spriteBatch.Draw(_pixel, new Rectangle(SideMenuWidth, viewport.Height - 44, viewport.Width - SideMenuWidth, 44), new Color(25, 31, 42));
        _text.DrawString(spriteBatch, message, new Vector2(SideMenuWidth + 14, y), new Color(230, 238, 245), 1);
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
        return new Rectangle(0, TopBarHeight, SideMenuWidth, viewport.Height - TopBarHeight);
    }

    private static Rectangle GetSellButtonRectangle(Viewport viewport)
    {
        return new Rectangle(viewport.Width - 370, 14, 90, 34);
    }

    private static Rectangle GetSelectedBuildingPanelRectangle(Viewport viewport)
    {
        return new Rectangle(viewport.Width - 330, viewport.Height - 235, 310, 178);
    }

    private static Rectangle GetReplaceButtonRectangle(Viewport viewport)
    {
        var panel = GetSelectedBuildingPanelRectangle(viewport);
        return new Rectangle(panel.X + 12, panel.Bottom - 44, panel.Width - 24, 32);
    }

    private static Rectangle GetBuildButtonRectangle(int index)
    {
        return new Rectangle(12, 112 + index * 54, SideMenuWidth - 24, 46);
    }

    private static Rectangle GetResearchButtonRectangle(int index)
    {
        return new Rectangle(12, 482 + index * 42, SideMenuWidth - 24, 36);
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
}
