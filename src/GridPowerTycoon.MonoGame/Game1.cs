using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Data;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Expansion;
using GridPowerTycoon.Core.Research;
using GridPowerTycoon.Core.Simulation;
using GridPowerTycoon.Core.World;
using GridPowerTycoon.Core.Tools;
using GridPowerTycoon.Core.Upgrades;
using GridPowerTycoon.MonoGame.Input;
using GridPowerTycoon.MonoGame.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GridPowerTycoon.MonoGame;

public sealed class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;

    private GameWorld _world = null!;
    private BuildSystem _buildSystem = null!;
    private SellSystem _sellSystem = null!;
    private ResearchSystem _researchSystem = null!;
    private TerrainClearSystem _terrainClearSystem = null!;
    private UpgradeSystem _upgradeSystem = null!;
    private AreaUnlockSystem _areaUnlockSystem = null!;
    private GameSimulation _simulation = null!;
    private Camera2D _camera = null!;
    private InputManager _input = null!;
    private CameraInputController _cameraInput = null!;
    private MapInputController _mapInput = null!;
    private MapRenderer _mapRenderer = null!;
    private UiRenderer _uiRenderer = null!;

    private string? _selectedBuildingId = "wind_turbine";
    private ResearchResult? _lastResearchResult;
    private UpgradeResult? _lastUpgradeResult;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720,
            SynchronizeWithVerticalRetrace = true
        };

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += HandleClientSizeChanged;
    }


    private void HandleClientSizeChanged(object? sender, EventArgs e)
    {
        var width = Math.Max(900, Window.ClientBounds.Width);
        var height = Math.Max(540, Window.ClientBounds.Height);

        if (_graphics.PreferredBackBufferWidth == width && _graphics.PreferredBackBufferHeight == height)
            return;

        _graphics.PreferredBackBufferWidth = width;
        _graphics.PreferredBackBufferHeight = height;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        Window.Title = "GridPower Tycoon";
        StartFullscreen();

        var loader = new GameDataLoader();
        var dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
        var buildings = loader.LoadBuildingCatalog(Path.Combine(dataDirectory, "buildings.json"));
        var economy = loader.LoadEconomySettings(Path.Combine(dataDirectory, "economy.json"));
        var research = loader.LoadResearchCatalog(Path.Combine(dataDirectory, "research.json"));
        var heat = loader.LoadHeatSettings(Path.Combine(dataDirectory, "heat.json"));
        var tools = loader.LoadToolSettings(Path.Combine(dataDirectory, "tools.json"));
        var upgrades = loader.LoadUpgradeCatalog(Path.Combine(dataDirectory, "upgrades.json"));
        var areaUnlock = loader.LoadAreaUnlockSettings(Path.Combine(dataDirectory, "area-unlock.json"));
        var map = loader.LoadMap(Path.Combine(dataDirectory, "maps", "default-map.json"));
        var data = new GameData(buildings, economy, research, heat, tools, upgrades, areaUnlock);

        _world = new GameWorld(map, data);
        _buildSystem = new BuildSystem(_world);
        _sellSystem = new SellSystem(_world);
        _researchSystem = new ResearchSystem(_world);
        _terrainClearSystem = new TerrainClearSystem(_world);
        _upgradeSystem = new UpgradeSystem(_world);
        _areaUnlockSystem = new AreaUnlockSystem(_world);
        _simulation = new GameSimulation(_world, _sellSystem);
        _camera = new Camera2D();
        _input = new InputManager();
        _cameraInput = new CameraInputController(_camera, _input);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _mapRenderer = new MapRenderer(_world, _pixel);
        _uiRenderer = new UiRenderer(_world, _pixel);
        _mapInput = new MapInputController(
            _world,
            _camera,
            _input,
            _buildSystem,
            point => _uiRenderer.IsMouseOverUi(point, GraphicsDevice.Viewport));
    }

    protected override void Update(GameTime gameTime)
    {
        _input.Update();

        if (_input.IsKeyPressed(Keys.Escape))
            Exit();

        HandleBuildSelectionInput();

        _cameraInput.Update(gameTime);
        _mapInput.Update(GraphicsDevice.Viewport, _selectedBuildingId);

        _simulation.Update(gameTime.ElapsedGameTime.TotalSeconds);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(18, 24, 34));

        _spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: _camera.GetTransformMatrix());

        _mapRenderer.Draw(_spriteBatch, _mapInput.HoveredTile, _selectedBuildingId, _buildSystem);

        _spriteBatch.End();

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _uiRenderer.Draw(
            _spriteBatch,
            GraphicsDevice.Viewport,
            _selectedBuildingId,
            _mapInput.LastBuildResult,
            _lastResearchResult,
            _mapInput.SelectedMapBuildingId,
            _mapInput.SelectedTerrainPosition,
            _mapInput.SelectedCloudPosition,
            _mapInput.LastTerrainClearResult,
            _mapInput.LastAreaUnlockResult,
            _lastUpgradeResult);

        _spriteBatch.End();

        base.Draw(gameTime);
    }


    private void StartFullscreen()
    {
        var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;

        _graphics.HardwareModeSwitch = false;
        _graphics.IsFullScreen = true;
        _graphics.PreferredBackBufferWidth = displayMode.Width;
        _graphics.PreferredBackBufferHeight = displayMode.Height;
        _graphics.ApplyChanges();
    }

    private void HandleBuildSelectionInput()
    {
        var buildIds = _uiRenderer.GetBuildButtonIds();

        if (_input.IsKeyPressed(Keys.D1) || _input.IsKeyPressed(Keys.NumPad1))
            _selectedBuildingId = buildIds.Count > 0 ? buildIds[0] : null;

        if (_input.IsKeyPressed(Keys.D2) || _input.IsKeyPressed(Keys.NumPad2))
            _selectedBuildingId = buildIds.Count > 1 ? buildIds[1] : null;

        if (_input.IsKeyPressed(Keys.D3) || _input.IsKeyPressed(Keys.NumPad3))
            _selectedBuildingId = buildIds.Count > 2 ? buildIds[2] : null;

        if (_input.IsKeyPressed(Keys.D4) || _input.IsKeyPressed(Keys.NumPad4))
            _selectedBuildingId = buildIds.Count > 3 ? buildIds[3] : null;

        if (_input.IsKeyPressed(Keys.D5) || _input.IsKeyPressed(Keys.NumPad5))
            _selectedBuildingId = buildIds.Count > 4 ? buildIds[4] : null;

        if (_input.IsKeyPressed(Keys.D6) || _input.IsKeyPressed(Keys.NumPad6))
            _selectedBuildingId = buildIds.Count > 5 ? buildIds[5] : null;

        var mousePoint = new Point(_input.CurrentMouse.X, _input.CurrentMouse.Y);
        if (_input.IsLeftClickPressed() && _uiRenderer.IsSellButtonAt(mousePoint, GraphicsDevice.Viewport))
        {
            _sellSystem.SellAll();
            return;
        }

        if (_input.IsLeftClickPressed() &&
            _uiRenderer.TryGetResearchButtonAt(mousePoint, out var clickedResearchId))
        {
            _lastResearchResult = _researchSystem.Complete(clickedResearchId);
            _lastUpgradeResult = null;
            _mapInput.ClearLastBuildResult();
            _mapInput.ClearLastAreaUnlockResult();
            return;
        }

        if (_input.IsLeftClickPressed() &&
            _uiRenderer.TryGetUpgradeButtonAt(mousePoint, GraphicsDevice.Viewport, out var clickedUpgradeId))
        {
            _lastUpgradeResult = _upgradeSystem.Purchase(clickedUpgradeId);
            _lastResearchResult = null;
            _mapInput.ClearLastBuildResult();
            _mapInput.ClearLastAreaUnlockResult();
            return;
        }

        if (_input.IsLeftClickPressed() &&
            _uiRenderer.IsReplaceButtonAt(mousePoint, GraphicsDevice.Viewport, _mapInput.SelectedMapBuildingId))
        {
            var result = _buildSystem.ReplaceExpired(_mapInput.SelectedMapBuildingId!.Value);
            _mapInput.SetLastBuildResult(result);
            _lastResearchResult = null;
            _lastUpgradeResult = null;
            return;
        }

        if (_input.IsLeftClickPressed() &&
            _uiRenderer.IsClearTerrainButtonAt(mousePoint, GraphicsDevice.Viewport, _mapInput.SelectedTerrainPosition))
        {
            var result = _terrainClearSystem.Clear(_mapInput.SelectedTerrainPosition!.Value);
            _mapInput.SetLastTerrainClearResult(result);
            _lastResearchResult = null;
            _lastUpgradeResult = null;
            return;
        }

        if (_input.IsLeftClickPressed() &&
            _uiRenderer.IsUnlockCloudButtonAt(mousePoint, GraphicsDevice.Viewport, _mapInput.SelectedCloudPosition))
        {
            var result = _areaUnlockSystem.UnlockCloud(_mapInput.SelectedCloudPosition!.Value);
            _mapInput.SetLastAreaUnlockResult(result);
            _lastResearchResult = null;
            _lastUpgradeResult = null;
            return;
        }

        if (_input.IsLeftClickPressed() && _uiRenderer.TryGetBuildingButtonAt(mousePoint, out var clickedBuildingId))
        {
            _selectedBuildingId = clickedBuildingId;
            _lastResearchResult = null;
            _lastUpgradeResult = null;
            _mapInput.ClearLastAreaUnlockResult();
        }
    }
}
