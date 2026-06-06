using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Managers;
using GridPowerTycoon.Core.Operations;
using GridPowerTycoon.Core.Upgrades;
using GridPowerTycoon.Core.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GridPowerTycoon.MonoGame.Rendering;

public sealed class MapRenderer
{
    public const int TileSize = 64;

    private readonly GameWorld _world;
    private readonly Texture2D _pixel;
    private readonly PixelTextRenderer _text;

    public MapRenderer(GameWorld world, Texture2D pixel)
    {
        _world = world;
        _pixel = pixel;
        _text = new PixelTextRenderer(pixel);
    }

    public void Draw(
        SpriteBatch spriteBatch,
        GridPosition? hoveredTile,
        GridPosition? selectedTile,
        string? selectedBuildingId,
        BuildSystem buildSystem,
        GridPosition? lastBuildFailurePosition,
        BuildFailureReason? lastBuildFailureReason)
    {
        DrawTiles(spriteBatch);
        DrawBuildings(spriteBatch);
        DrawSelectedTile(spriteBatch, selectedTile);
        DrawBuildFailureMarker(spriteBatch, lastBuildFailurePosition, lastBuildFailureReason);
        DrawHoverAndBuildPreview(spriteBatch, hoveredTile, selectedBuildingId, buildSystem);
    }

    private void DrawTiles(SpriteBatch spriteBatch)
    {
        foreach (var tile in _world.Map.Tiles)
        {
            var rect = GetTileRectangle(tile.Position);

            spriteBatch.Draw(_pixel, rect, GetTileColor(tile.Type));
            DrawOutline(spriteBatch, rect, new Color(0, 0, 0, 80), 1);
        }
    }

    private void DrawBuildings(SpriteBatch spriteBatch)
    {
        foreach (var instance in _world.BuildingInstances.Values)
        {
            if (!_world.BuildingCatalog.TryGet(instance.DefinitionId, out var definition))
                continue;

            var rect = GetBuildingRectangle(instance, definition);
            var inner = new Rectangle(rect.X + 8, rect.Y + 8, rect.Width - 16, rect.Height - 16);

            spriteBatch.Draw(_pixel, inner, GetBuildingColor(definition.Category, instance.State));
            DrawOutline(spriteBatch, inner, new Color(10, 14, 20), 2);

            DrawLifetimeBar(spriteBatch, inner, instance, definition);
            DrawHeatBar(spriteBatch, inner, instance, definition);

            var status = BuildingOperationalStatusCalculator.Calculate(_world, instance);
            DrawOperationalBadge(spriteBatch, inner, status);
            DrawManagerBadge(spriteBatch, inner, instance);

            if (instance.State == BuildingState.Expired)
            {
                DrawDiagonal(spriteBatch, inner, new Color(255, 255, 255, 180));
            }
        }
    }

    private void DrawLifetimeBar(SpriteBatch spriteBatch, Rectangle buildingRect, BuildingInstance instance, BuildingDefinition definition)
    {
        var effectiveLifetime = UpgradeCalculator.GetLifetimeSeconds(_world, definition);
        if (effectiveLifetime <= 0)
            return;

        const int margin = 4;
        const int height = 5;

        var barBackground = new Rectangle(
            buildingRect.X + margin,
            buildingRect.Bottom - margin - height,
            Math.Max(1, buildingRect.Width - margin * 2),
            height);

        spriteBatch.Draw(_pixel, barBackground, new Color(18, 24, 34, 220));

        var ratio = instance.State == BuildingState.Active
            ? instance.RemainingLifetimeSeconds / effectiveLifetime
            : 0;

        var fillWidth = (int)Math.Round(barBackground.Width * Math.Clamp(ratio, 0, 1));
        if (fillWidth > 0)
        {
            var fill = new Rectangle(barBackground.X, barBackground.Y, fillWidth, barBackground.Height);
            spriteBatch.Draw(_pixel, fill, GetLifetimeBarColor(ratio));
        }

        DrawOutline(spriteBatch, barBackground, new Color(8, 12, 18, 230), 1);
    }


    private void DrawHeatBar(SpriteBatch spriteBatch, Rectangle buildingRect, BuildingInstance instance, BuildingDefinition definition)
    {
        if (definition.HeatPerSecond <= 0 && instance.AccumulatedHeat <= 0)
            return;

        var threshold = _world.HeatSettings.HeatExplosionThreshold;
        if (threshold <= 0)
            return;

        const int margin = 4;
        const int height = 5;

        var barBackground = new Rectangle(
            buildingRect.X + margin,
            buildingRect.Y + margin,
            Math.Max(1, buildingRect.Width - margin * 2),
            height);

        spriteBatch.Draw(_pixel, barBackground, new Color(18, 24, 34, 220));

        var ratio = instance.State == BuildingState.Active
            ? instance.AccumulatedHeat / threshold
            : instance.State == BuildingState.Exploded ? 1 : 0;

        var fillWidth = (int)Math.Round(barBackground.Width * Math.Clamp(ratio, 0, 1));
        if (fillWidth > 0)
        {
            var fill = new Rectangle(barBackground.X, barBackground.Y, fillWidth, barBackground.Height);
            spriteBatch.Draw(_pixel, fill, GetHeatBarColor(ratio));
        }

        DrawOutline(spriteBatch, barBackground, new Color(8, 12, 18, 230), 1);
    }

    private Color GetHeatBarColor(double ratio)
    {
        var warningRatio = _world.HeatSettings.HeatExplosionThreshold <= 0
            ? 0.6
            : _world.HeatSettings.HeatWarningThreshold / _world.HeatSettings.HeatExplosionThreshold;

        if (ratio >= 1)
            return new Color(255, 70, 55);

        if (ratio >= warningRatio)
            return new Color(245, 145, 55);

        return new Color(255, 210, 90);
    }

    private static Color GetLifetimeBarColor(double ratio)
    {
        if (ratio <= 0.25)
            return new Color(235, 80, 65);

        if (ratio <= 0.5)
            return new Color(235, 180, 70);

        return new Color(110, 220, 120);
    }


    private void DrawManagerBadge(SpriteBatch spriteBatch, Rectangle buildingRect, BuildingInstance instance)
    {
        if (!ManagerSystem.IsManaged(_world, instance.DefinitionId))
            return;

        var rect = new Rectangle(buildingRect.X + 4, buildingRect.Bottom - 18, 14, 14);
        spriteBatch.Draw(_pixel, rect, new Color(35, 90, 55, 235));
        DrawOutline(spriteBatch, rect, new Color(145, 235, 160), 1);
        _text.DrawString(spriteBatch, "M", new Vector2(rect.X + 3, rect.Y + 4), new Color(225, 255, 225), 1);
    }

    private void DrawOperationalBadge(SpriteBatch spriteBatch, Rectangle buildingRect, BuildingOperationalStatus status)
    {
        var badge = GetOperationalBadge(status);
        if (badge is null)
            return;

        var rect = new Rectangle(buildingRect.Right - 18, buildingRect.Y + 4, 14, 14);
        spriteBatch.Draw(_pixel, rect, badge.Value.Background);
        DrawOutline(spriteBatch, rect, badge.Value.Outline, 1);
        _text.DrawString(spriteBatch, badge.Value.Text, new Vector2(rect.X + 4, rect.Y + 4), badge.Value.TextColor, 1);
    }

    private static (string Text, Color Background, Color Outline, Color TextColor)? GetOperationalBadge(BuildingOperationalStatus status)
    {
        return status.State switch
        {
            BuildingOperationalState.NoEnergy => ("E", new Color(105, 60, 35, 235), new Color(255, 165, 120), new Color(255, 225, 180)),
            BuildingOperationalState.NoHeatConversion => ("G", new Color(110, 45, 35, 235), new Color(255, 150, 120), new Color(255, 230, 210)),
            BuildingOperationalState.HeatWarning => ("H", new Color(110, 68, 20, 235), new Color(245, 145, 55), new Color(255, 235, 170)),
            BuildingOperationalState.Expired => ("T", new Color(90, 85, 55, 235), new Color(255, 210, 95), new Color(255, 245, 190)),
            BuildingOperationalState.Exploded => ("X", new Color(110, 25, 20, 235), new Color(255, 85, 75), new Color(255, 225, 220)),
            _ => null
        };
    }


    private void DrawSelectedTile(SpriteBatch spriteBatch, GridPosition? selectedTile)
    {
        if (selectedTile is null)
            return;

        var position = selectedTile.Value;
        if (!_world.Map.Contains(position))
            return;

        var rect = GetTileRectangle(position);
        DrawOutline(spriteBatch, rect, new Color(255, 240, 120, 255), 4);
        DrawOutline(spriteBatch, new Rectangle(rect.X + 4, rect.Y + 4, rect.Width - 8, rect.Height - 8), new Color(20, 24, 32, 220), 2);
    }

    private void DrawBuildFailureMarker(SpriteBatch spriteBatch, GridPosition? position, BuildFailureReason? reason)
    {
        if (!position.HasValue || reason != BuildFailureReason.NotEnoughMoney)
            return;

        if (!_world.Map.Contains(position.Value))
            return;

        var tileRect = GetTileRectangle(position.Value);
        var marker = new Rectangle(tileRect.X + 12, tileRect.Y + 8, 40, 44);

        spriteBatch.Draw(_pixel, marker, new Color(95, 25, 25, 220));
        DrawOutline(spriteBatch, marker, new Color(255, 85, 85, 245), 3);
        _text.DrawString(spriteBatch, "$", new Vector2(marker.X + 12, marker.Y + 7), new Color(255, 225, 120), 4);
        DrawDiagonal(spriteBatch, new Rectangle(marker.X + 4, marker.Y + 4, marker.Width - 8, marker.Height - 8), new Color(255, 245, 245, 245), 4);
    }

    private void DrawHoverAndBuildPreview(SpriteBatch spriteBatch, GridPosition? hoveredTile, string? selectedBuildingId, BuildSystem buildSystem)
    {
        if (hoveredTile is null)
            return;

        var position = hoveredTile.Value;
        var rect = GetTileRectangle(position);

        if (!_world.Map.Contains(position))
            return;

        var color = new Color(255, 230, 80, 220);

        if (!string.IsNullOrWhiteSpace(selectedBuildingId))
        {
            var reason = buildSystem.CanBuild(selectedBuildingId, position);
            color = reason == BuildFailureReason.None
                ? new Color(110, 255, 120, 220)
                : new Color(255, 80, 80, 220);
        }

        DrawOutline(spriteBatch, rect, color, 3);
    }

    private static Rectangle GetTileRectangle(GridPosition position)
    {
        return new Rectangle(
            position.X * TileSize,
            position.Y * TileSize,
            TileSize,
            TileSize);
    }

    private static Rectangle GetBuildingRectangle(BuildingInstance instance, BuildingDefinition definition)
    {
        return new Rectangle(
            instance.Position.X * TileSize,
            instance.Position.Y * TileSize,
            definition.Width * TileSize,
            definition.Height * TileSize);
    }

    private static Color GetTileColor(TileType type)
    {
        return type switch
        {
            TileType.Water => new Color(25, 92, 150),
            TileType.Land => new Color(74, 145, 69),
            TileType.Forest => new Color(28, 92, 45),
            TileType.Mountain => new Color(115, 115, 115),
            TileType.Cloud => new Color(185, 190, 196),
            _ => Color.Magenta
        };
    }

    private static Color GetBuildingColor(BuildingCategory category, BuildingState state)
    {
        if (state == BuildingState.Expired)
            return new Color(100, 100, 100);

        if (state == BuildingState.Exploded)
            return new Color(120, 30, 20);

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

    private void DrawOutline(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
    {
        spriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Top, rect.Width, thickness), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Bottom - thickness, rect.Width, thickness), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Top, thickness, rect.Height), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Right - thickness, rect.Top, thickness, rect.Height), color);
    }

    private void DrawDiagonal(SpriteBatch spriteBatch, Rectangle rect, Color color)
    {
        DrawDiagonal(spriteBatch, rect, color, 3);
    }

    private void DrawDiagonal(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
    {
        var steps = Math.Max(rect.Width, rect.Height);
        for (var i = 0; i < steps; i += 4)
        {
            spriteBatch.Draw(_pixel, new Rectangle(rect.Left + i, rect.Top + i, thickness, thickness), color);
        }
    }
}
