using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Tools;
using GridPowerTycoon.Core.World;
using GridPowerTycoon.MonoGame.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GridPowerTycoon.MonoGame.Input;

public sealed class MapInputController
{
    private readonly GameWorld _world;
    private readonly Camera2D _camera;
    private readonly InputManager _input;
    private readonly BuildSystem _buildSystem;
    private readonly Func<Point, bool> _isMouseOverUi;

    public GridPosition? HoveredTile { get; private set; }
    public Guid? SelectedMapBuildingId { get; private set; }
    public GridPosition? SelectedTerrainPosition { get; private set; }
    public BuildResult? LastBuildResult { get; private set; }
    public TerrainClearResult? LastTerrainClearResult { get; private set; }

    public MapInputController(
        GameWorld world,
        Camera2D camera,
        InputManager input,
        BuildSystem buildSystem,
        Func<Point, bool> isMouseOverUi)
    {
        _world = world;
        _camera = camera;
        _input = input;
        _buildSystem = buildSystem;
        _isMouseOverUi = isMouseOverUi;
    }

    public void Update(Viewport viewport, string? selectedBuildingId)
    {
        var mousePoint = new Point(_input.CurrentMouse.X, _input.CurrentMouse.Y);
        var isOverUi = _isMouseOverUi(mousePoint);

        HoveredTile = isOverUi
            ? null
            : ScreenToTile(mousePoint);

        if (isOverUi || !_input.IsLeftClickPressed())
            return;

        var tilePosition = ScreenToTile(mousePoint);
        if (!_world.Map.Contains(tilePosition))
        {
            SelectedMapBuildingId = null;
            SelectedTerrainPosition = null;
            return;
        }

        var tile = _world.Map.GetTile(tilePosition);
        if (tile.BuildingId.HasValue)
        {
            SelectedMapBuildingId = tile.BuildingId.Value;
            SelectedTerrainPosition = null;
            LastBuildResult = null;
            LastTerrainClearResult = null;
            return;
        }

        SelectedMapBuildingId = null;

        if (tile.Type == TileType.Forest || tile.Type == TileType.Mountain)
        {
            SelectedTerrainPosition = tilePosition;
            LastBuildResult = null;
            LastTerrainClearResult = null;
            return;
        }

        SelectedTerrainPosition = null;

        if (selectedBuildingId is null)
            return;

        LastBuildResult = _buildSystem.Build(selectedBuildingId, tilePosition);
        LastTerrainClearResult = null;
    }

    public void SetLastBuildResult(BuildResult result)
    {
        LastBuildResult = result;
    }

    public void ClearLastBuildResult()
    {
        LastBuildResult = null;
    }

    public void SetLastTerrainClearResult(TerrainClearResult result)
    {
        LastTerrainClearResult = result;
        LastBuildResult = null;

        if (result.Success)
            SelectedTerrainPosition = null;
    }

    public void ClearLastTerrainClearResult()
    {
        LastTerrainClearResult = null;
    }

    private GridPosition ScreenToTile(Point screenPoint)
    {
        var world = _camera.ScreenToWorld(screenPoint.ToVector2());
        var x = (int)Math.Floor(world.X / MapRenderer.TileSize);
        var y = (int)Math.Floor(world.Y / MapRenderer.TileSize);

        return new GridPosition(x, y);
    }
}
