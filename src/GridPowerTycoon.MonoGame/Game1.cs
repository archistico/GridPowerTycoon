using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Data;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Expansion;
using GridPowerTycoon.Core.Research;
using GridPowerTycoon.Core.Simulation;
using GridPowerTycoon.Core.Save;
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
    private SaveGameService _saveGameService = null!;
    private GameData _gameData = null!;
    private string _savePath = string.Empty;
    private string _dataDirectory = string.Empty;
    private string? _lastSaveLoadMessage;

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
        _dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
        var dataDirectory = _dataDirectory;
        var buildings = loader.LoadBuildingCatalog(Path.Combine(dataDirectory, "buildings.json"));
        var economy = loader.LoadEconomySettings(Path.Combine(dataDirectory, "economy.json"));
        var research = loader.LoadResearchCatalog(Path.Combine(dataDirectory, "research.json"));
        var heat = loader.LoadHeatSettings(Path.Combine(dataDirectory, "heat.json"));
        var tools = loader.LoadToolSettings(Path.Combine(dataDirectory, "tools.json"));
        var upgrades = loader.LoadUpgradeCatalog(Path.Combine(dataDirectory, "upgrades.json"));
        var areaUnlock = loader.LoadAreaUnlockSettings(Path.Combine(dataDirectory, "area-unlock.json"));
        var map = loader.LoadMap(Path.Combine(dataDirectory, "maps", "default-map.json"));
        _gameData = new GameData(buildings, economy, research, heat, tools, upgrades, areaUnlock);
        _saveGameService = new SaveGameService();
        _savePath = Path.Combine(AppContext.BaseDirectory, "Saves", "savegame.json");

        var world = new GameWorld(map, _gameData);
        OfflineProgressResult? offlineProgress = null;

        if (File.Exists(_savePath))
        {
            var save = _saveGameService.LoadSaveFromFile(_savePath);
            world = _saveGameService.RestoreWorld(save, _gameData);
            offlineProgress = new OfflineProgressSystem(world, new SellSystem(world))
                .Apply(save.SavedAt, DateTimeOffset.UtcNow);
        }

        ConfigureWorld(world);

        if (offlineProgress is not null && offlineProgress.HasProgress)
        {
            _lastSaveLoadMessage = FormatOfflineProgress(offlineProgress);
            _saveGameService.SaveToFile(_world, _savePath);
        }

        _camera = new Camera2D();
        CenterCameraOnInitialIsland();
        _input = new InputManager();
        _cameraInput = new CameraInputController(
            _camera,
            _input,
            point => _uiRenderer?.IsMouseOverUi(point, GraphicsDevice.Viewport) == true);

        base.Initialize();
    }

    private void ConfigureWorld(GameWorld world)
    {
        _world = world;
        _buildSystem = new BuildSystem(_world);
        _sellSystem = new SellSystem(_world);
        _researchSystem = new ResearchSystem(_world);
        _terrainClearSystem = new TerrainClearSystem(_world);
        _upgradeSystem = new UpgradeSystem(_world);
        _areaUnlockSystem = new AreaUnlockSystem(_world);
        _simulation = new GameSimulation(_world, _sellSystem);

        if (_pixel is not null)
        {
            _mapRenderer = new MapRenderer(_world, _pixel);
            _uiRenderer = new UiRenderer(_world, _pixel);
            _mapInput = new MapInputController(
                _world,
                _camera,
                _input,
                _buildSystem,
                point => _uiRenderer.IsMouseOverUi(point, GraphicsDevice.Viewport));
        }
    }


    private void CenterCameraOnInitialIsland()
    {
        var worldCenter = new Vector2(
            _world.Map.Width * MapRenderer.TileSize / 2f,
            _world.Map.Height * MapRenderer.TileSize / 2f);

        var viewportSize = new Vector2(
            _graphics.PreferredBackBufferWidth,
            _graphics.PreferredBackBufferHeight);

        _camera.SetPosition(worldCenter - viewportSize / (2f * _camera.Zoom));
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        ConfigureWorld(_world);
    }

    protected override void Update(GameTime gameTime)
    {
        _input.Update();

        if (_input.IsKeyPressed(Keys.Escape))
        {
            SaveCurrentGame();
            Exit();
        }

        if (_input.IsKeyPressed(Keys.F5))
            SaveCurrentGame();

        if (_input.IsKeyPressed(Keys.F9))
            LoadCurrentGame();

        _uiRenderer.HandleScroll(new Point(_input.CurrentMouse.X, _input.CurrentMouse.Y), _input.CurrentMouse.ScrollWheelValue - _input.PreviousMouse.ScrollWheelValue, GraphicsDevice.Viewport);

        HandleBuildSelectionInput();

        _cameraInput.Update(gameTime);
        _mapInput.Update(GraphicsDevice.Viewport, _selectedBuildingId);

        _simulation.Update(gameTime.ElapsedGameTime.TotalSeconds);
        if (_simulation.LastManagerRenewalResult.HasRenewals)
        {
            _lastSaveLoadMessage = $"MANAGER RENEWED {_simulation.LastManagerRenewalResult.RenewedCount} BUILDING(S) -${FormatCompactMoney((double)_simulation.LastManagerRenewalResult.MoneySpent)}";
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(18, 24, 34));

        _spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: _camera.GetTransformMatrix());

        _mapRenderer.Draw(_spriteBatch, _mapInput.HoveredTile, _mapInput.SelectedTilePosition, _selectedBuildingId, _buildSystem);

        _spriteBatch.End();

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _uiRenderer.Draw(
            _spriteBatch,
            GraphicsDevice.Viewport,
            _selectedBuildingId,
            _mapInput.LastBuildResult,
            _lastResearchResult,
            _mapInput.SelectedTilePosition,
            _mapInput.SelectedMapBuildingId,
            _mapInput.SelectedTerrainPosition,
            _mapInput.SelectedCloudPosition,
            _mapInput.LastTerrainClearResult,
            _mapInput.LastAreaUnlockResult,
            _lastUpgradeResult,
            _lastSaveLoadMessage);

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

    private void SaveCurrentGame()
    {
        _saveGameService.SaveToFile(_world, _savePath);
        _lastSaveLoadMessage = "GAME SAVED";
    }

    private void LoadCurrentGame()
    {
        if (!File.Exists(_savePath))
        {
            _lastSaveLoadMessage = "NO SAVE FOUND";
            return;
        }

        ConfigureWorld(_saveGameService.LoadFromFile(_savePath, _gameData));
        CenterCameraOnInitialIsland();
        _lastResearchResult = null;
        _lastUpgradeResult = null;
        _lastSaveLoadMessage = "GAME LOADED";
    }

    private void StartNewGame()
    {
        var loader = new GameDataLoader();
        var map = loader.LoadMap(Path.Combine(_dataDirectory, "maps", "default-map.json"));

        ConfigureWorld(new GameWorld(map, _gameData));
        CenterCameraOnInitialIsland();
        _selectedBuildingId = "wind_turbine";
        _lastResearchResult = null;
        _lastUpgradeResult = null;
        _lastSaveLoadMessage = "NEW GAME STARTED";
    }

    private void ToggleFullscreen()
    {
        if (_graphics.IsFullScreen)
        {
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
            _lastSaveLoadMessage = "WINDOWED MODE";
        }
        else
        {
            StartFullscreen();
            _lastSaveLoadMessage = "FULLSCREEN MODE";
        }

        CenterCameraOnInitialIsland();
    }

    private static string FormatOfflineProgress(OfflineProgressResult result)
    {
        var minutes = Math.Floor(result.AppliedSeconds / 60d);
        var seconds = Math.Floor(result.AppliedSeconds % 60d);

        var duration = minutes >= 60
            ? $"{Math.Floor(minutes / 60d):0}H {minutes % 60:0}M"
            : $"{minutes:0}M {seconds:0}S";

        return $"OFFLINE {duration}: ENERGY {FormatDelta(result.EnergyDelta)} RESEARCH {FormatDelta(result.ResearchDelta)} MONEY {FormatMoneyDelta(result.MoneyDelta)} AXES {FormatDelta(result.AxesDelta)} MINES {FormatDelta(result.MinesDelta)} EXPIRED {result.BuildingsExpired}";
    }

    private static string FormatDelta(double value)
    {
        return value >= 0 ? $"+{value:0.00}" : $"{value:0.00}";
    }

    private static string FormatMoneyDelta(decimal value)
    {
        return value >= 0 ? $"+${value:0.00}" : $"-${Math.Abs(value):0.00}";
    }

    private static string FormatCompactMoney(double value)
    {
        var abs = Math.Abs(value);
        var units = new[] { "", "k", "M", "G", "T", "P", "E", "Z", "Y" };
        var unitIndex = 0;

        while (abs >= 1000d && unitIndex < units.Length - 1)
        {
            abs /= 1000d;
            unitIndex++;
        }

        var sign = value < 0 ? "-" : "";
        return $"{sign}{abs:0.##}{units[unitIndex]}";
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

        if (_input.IsKeyPressed(Keys.D7) || _input.IsKeyPressed(Keys.NumPad7))
            _selectedBuildingId = buildIds.Count > 6 ? buildIds[6] : null;

        if (_input.IsKeyPressed(Keys.D8) || _input.IsKeyPressed(Keys.NumPad8))
            _selectedBuildingId = buildIds.Count > 7 ? buildIds[7] : null;

        if (_input.IsKeyPressed(Keys.D9) || _input.IsKeyPressed(Keys.NumPad9))
            _selectedBuildingId = buildIds.Count > 8 ? buildIds[8] : null;

        if (_input.IsKeyPressed(Keys.D0) || _input.IsKeyPressed(Keys.NumPad0))
            _selectedBuildingId = buildIds.Count > 9 ? buildIds[9] : null;

        var mousePoint = new Point(_input.CurrentMouse.X, _input.CurrentMouse.Y);
        if (_input.IsLeftClickPressed() && _uiRenderer.IsSellButtonAt(mousePoint, GraphicsDevice.Viewport))
        {
            _sellSystem.SellAll();
            return;
        }

        if (_input.IsLeftClickPressed() && _uiRenderer.IsSaveButtonAt(mousePoint, GraphicsDevice.Viewport))
        {
            SaveCurrentGame();
            return;
        }

        if (_input.IsLeftClickPressed() && _uiRenderer.IsLoadButtonAt(mousePoint, GraphicsDevice.Viewport))
        {
            LoadCurrentGame();
            return;
        }

        if (_input.IsLeftClickPressed() && _uiRenderer.IsNewGameButtonAt(mousePoint, GraphicsDevice.Viewport))
        {
            StartNewGame();
            return;
        }

        if (_input.IsLeftClickPressed() && _uiRenderer.IsToggleFullscreenButtonAt(mousePoint, GraphicsDevice.Viewport))
        {
            ToggleFullscreen();
            return;
        }

        if (_input.IsLeftClickPressed() && _uiRenderer.IsExitButtonAt(mousePoint, GraphicsDevice.Viewport))
        {
            SaveCurrentGame();
            Exit();
            return;
        }

        if (_input.IsLeftClickPressed() &&
            _uiRenderer.TryGetResearchButtonAt(mousePoint, GraphicsDevice.Viewport, out var clickedResearchId))
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

        if (_input.IsLeftClickPressed() && _uiRenderer.TryGetBuildingButtonAt(mousePoint, GraphicsDevice.Viewport, out var clickedBuildingId))
        {
            _selectedBuildingId = clickedBuildingId;
            _lastResearchResult = null;
            _lastUpgradeResult = null;
            _mapInput.ClearLastAreaUnlockResult();
        }
    }
}
