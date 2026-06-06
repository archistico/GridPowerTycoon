using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Expansion;
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
    public GridPosition? SelectedTilePosition { get; private set; }
    public Guid? SelectedMapBuildingId { get; private set; }
    public GridPosition? SelectedTerrainPosition { get; private set; }
    public GridPosition? SelectedCloudPosition { get; private set; }
    public BuildResult? LastBuildResult { get; private set; }
    public GridPosition? LastBuildFailurePosition { get; private set; }
    public BuildFailureReason? LastBuildFailureReason { get; private set; }
    public TerrainClearResult? LastTerrainClearResult { get; private set; }
    public AreaUnlockResult? LastAreaUnlockResult { get; private set; }
    public bool LastClickSelectedExistingBuilding { get; private set; }

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
        LastClickSelectedExistingBuilding = false;

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
            ClearSelection();
            return;
        }

        SelectedTilePosition = tilePosition;
        var tile = _world.Map.GetTile(tilePosition);
        if (tile.BuildingId.HasValue)
        {
            LastClickSelectedExistingBuilding = true;
            SelectedMapBuildingId = tile.BuildingId.Value;
            SelectedTerrainPosition = null;
            SelectedCloudPosition = null;
            ClearLastBuildResult();
            LastTerrainClearResult = null;
            LastAreaUnlockResult = null;
            return;
        }

        SelectedMapBuildingId = null;

        if (tile.Type == TileType.Forest || tile.Type == TileType.Mountain)
        {
            SelectedTerrainPosition = tilePosition;
            SelectedCloudPosition = null;
            ClearLastBuildResult();
            LastTerrainClearResult = null;
            LastAreaUnlockResult = null;
            return;
        }

        if (tile.Type == TileType.Cloud)
        {
            SelectedCloudPosition = tilePosition;
            SelectedTerrainPosition = null;
            ClearLastBuildResult();
            LastTerrainClearResult = null;
            LastAreaUnlockResult = null;
            return;
        }

        SelectedTerrainPosition = null;
        SelectedCloudPosition = null;

        if (selectedBuildingId is null)
            return;

        LastBuildResult = _buildSystem.Build(selectedBuildingId, tilePosition);
        LastTerrainClearResult = null;
        LastAreaUnlockResult = null;

        if (LastBuildResult.Success && LastBuildResult.BuildingId.HasValue)
        {
            LastBuildFailurePosition = null;
            LastBuildFailureReason = null;
            SelectedMapBuildingId = LastBuildResult.BuildingId.Value;
            SelectedTerrainPosition = null;
            SelectedCloudPosition = null;
        }
        else
        {
            LastBuildFailurePosition = tilePosition;
            LastBuildFailureReason = LastBuildResult.FailureReason;
        }
    }

    public void SetLastBuildResult(BuildResult result)
    {
        LastBuildResult = result;
        LastBuildFailurePosition = null;
        LastBuildFailureReason = null;
        LastTerrainClearResult = null;
        LastAreaUnlockResult = null;
    }

    public void ClearLastBuildResult()
    {
        LastBuildResult = null;
        LastBuildFailurePosition = null;
        LastBuildFailureReason = null;
    }

    public void ClearSelectedBuilding()
    {
        SelectedMapBuildingId = null;
    }

    public void SetLastTerrainClearResult(TerrainClearResult result)
    {
        LastTerrainClearResult = result;
        ClearLastBuildResult();
        LastAreaUnlockResult = null;

        if (result.Success)
            SelectedTerrainPosition = null;
    }

    public void ClearLastTerrainClearResult()
    {
        LastTerrainClearResult = null;
    }

    public void ClearLastAreaUnlockResult()
    {
        LastAreaUnlockResult = null;
    }

    public void SetLastAreaUnlockResult(AreaUnlockResult result)
    {
        LastAreaUnlockResult = result;
        ClearLastBuildResult();
        LastTerrainClearResult = null;

        if (result.Success)
            SelectedCloudPosition = null;
    }

    private void ClearSelection()
    {
        SelectedTilePosition = null;
        SelectedMapBuildingId = null;
        SelectedTerrainPosition = null;
        SelectedCloudPosition = null;
        ClearLastBuildResult();
        LastTerrainClearResult = null;
        LastAreaUnlockResult = null;
    }

    private GridPosition ScreenToTile(Point screenPoint)
    {
        var world = _camera.ScreenToWorld(screenPoint.ToVector2());
        var x = (int)Math.Floor(world.X / MapRenderer.TileSize);
        var y = (int)Math.Floor(world.Y / MapRenderer.TileSize);

        return new GridPosition(x, y);
    }
}
