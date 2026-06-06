using GridPowerTycoon.Core.Build;
using GridPowerTycoon.Core.Data;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Expansion;
using GridPowerTycoon.Core.Managers;
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

    private string? _selectedBuildingId;
    private LeftPanelMode _activeLeftPanelMode = LeftPanelMode.Build;
    private Guid? _pendingDemolishBuildingId;
    private ResearchResult? _lastResearchResult;
    private UpgradeResult? _lastUpgradeResult;
    private string? _lastManagerRenewalSignature;

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

        _uiRenderer.HandleScroll(new Point(_input.CurrentMouse.X, _input.CurrentMouse.Y), _input.CurrentMouse.ScrollWheelValue - _input.PreviousMouse.ScrollWheelValue, _activeLeftPanelMode, GraphicsDevice.Viewport);

        HandleBuildSelectionInput();

        _cameraInput.Update(gameTime);
        _mapInput.Update(GraphicsDevice.Viewport, _selectedBuildingId);
        if (_input.IsLeftClickPressed() && !_uiRenderer.IsMouseOverUi(new Point(_input.CurrentMouse.X, _input.CurrentMouse.Y), GraphicsDevice.Viewport))
        {
            if (_pendingDemolishBuildingId.HasValue)
                _lastSaveLoadMessage = null;

            _pendingDemolishBuildingId = null;
        }

        if (_mapInput.LastClickSelectedExistingBuilding)
        {
            _selectedBuildingId = null;
        }

        _simulation.Update(gameTime.ElapsedGameTime.TotalSeconds);
        UpdateManagerRenewalFeedback(_simulation.LastManagerRenewalResult);

        base.Update(gameTime);
    }

    private void UpdateManagerRenewalFeedback(ManagerRenewalResult result)
    {
        if (result.RenewedCount <= 0 && result.NotEnoughMoneyCount <= 0)
        {
            _lastManagerRenewalSignature = null;
            return;
        }

        var signature = $"{result.RenewedCount}:{result.NotEnoughMoneyCount}:{result.MoneySpent}";
        if (string.Equals(signature, _lastManagerRenewalSignature, StringComparison.Ordinal))
            return;

        _lastManagerRenewalSignature = signature;
        _lastSaveLoadMessage = FormatManagerRenewal(result);
    }

    private static string FormatManagerRenewal(ManagerRenewalResult result)
    {
        if (result.RenewedCount > 0 && result.NotEnoughMoneyCount > 0)
        {
            return $"MANAGER RENEWED {result.RenewedCount} -${FormatCompactMoney((double)result.MoneySpent)} | NEED MONEY FOR {result.NotEnoughMoneyCount}";
        }

        if (result.RenewedCount > 0)
            return $"MANAGER RENEWED {result.RenewedCount} BUILDING(S) -${FormatCompactMoney((double)result.MoneySpent)}";

        return $"MANAGER NEEDS MONEY FOR {result.NotEnoughMoneyCount} EXPIRED BUILDING(S)";
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(18, 24, 34));

        _spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: _camera.GetTransformMatrix());

        _mapRenderer.Draw(
            _spriteBatch,
            _mapInput.HoveredTile,
            _mapInput.SelectedTilePosition,
            _selectedBuildingId,
            _buildSystem,
            _mapInput.LastBuildFailurePosition,
            _mapInput.LastBuildFailureReason);

        _spriteBatch.End();

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _uiRenderer.Draw(
            _spriteBatch,
            GraphicsDevice.Viewport,
            _activeLeftPanelMode,
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
            _lastSaveLoadMessage,
            _pendingDemolishBuildingId);

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
        _lastManagerRenewalSignature = null;
        _lastSaveLoadMessage = "GAME LOADED";
    }

    private void StartNewGame()
    {
        var loader = new GameDataLoader();
        var map = loader.LoadMap(Path.Combine(_dataDirectory, "maps", "default-map.json"));

        ConfigureWorld(new GameWorld(map, _gameData));
        CenterCameraOnInitialIsland();
        _selectedBuildingId = null;
        _activeLeftPanelMode = LeftPanelMode.Build;
        _pendingDemolishBuildingId = null;
        _lastResearchResult = null;
        _lastUpgradeResult = null;
        _lastManagerRenewalSignature = null;
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
            ToggleBuildTool(buildIds, 0);

        if (_input.IsKeyPressed(Keys.D2) || _input.IsKeyPressed(Keys.NumPad2))
            ToggleBuildTool(buildIds, 1);

        if (_input.IsKeyPressed(Keys.D3) || _input.IsKeyPressed(Keys.NumPad3))
            ToggleBuildTool(buildIds, 2);

        if (_input.IsKeyPressed(Keys.D4) || _input.IsKeyPressed(Keys.NumPad4))
            ToggleBuildTool(buildIds, 3);

        if (_input.IsKeyPressed(Keys.D5) || _input.IsKeyPressed(Keys.NumPad5))
            ToggleBuildTool(buildIds, 4);

        if (_input.IsKeyPressed(Keys.D6) || _input.IsKeyPressed(Keys.NumPad6))
            ToggleBuildTool(buildIds, 5);

        if (_input.IsKeyPressed(Keys.D7) || _input.IsKeyPressed(Keys.NumPad7))
            ToggleBuildTool(buildIds, 6);

        if (_input.IsKeyPressed(Keys.D8) || _input.IsKeyPressed(Keys.NumPad8))
            ToggleBuildTool(buildIds, 7);

        if (_input.IsKeyPressed(Keys.D9) || _input.IsKeyPressed(Keys.NumPad9))
            ToggleBuildTool(buildIds, 8);

        if (_input.IsKeyPressed(Keys.D0) || _input.IsKeyPressed(Keys.NumPad0))
            ToggleBuildTool(buildIds, 9);

        var mousePoint = new Point(_input.CurrentMouse.X, _input.CurrentMouse.Y);

        if (_input.IsLeftClickPressed() &&
            _uiRenderer.TryGetLeftPanelModeButtonAt(mousePoint, GraphicsDevice.Viewport, out var clickedLeftPanelMode))
        {
            _activeLeftPanelMode = clickedLeftPanelMode;
            if (clickedLeftPanelMode != LeftPanelMode.Build)
                _selectedBuildingId = null;

            _pendingDemolishBuildingId = null;
            _mapInput.ClearLastBuildResult();
            _mapInput.ClearLastAreaUnlockResult();
            return;
        }

        if (_input.IsRightClickPressed() && !string.IsNullOrWhiteSpace(_selectedBuildingId))
        {
            _selectedBuildingId = null;
            _pendingDemolishBuildingId = null;
            _lastSaveLoadMessage = "BUILD TOOL CANCELED";
            _mapInput.ClearLastBuildResult();
            return;
        }
        if (_input.IsLeftClickPressed() && _uiRenderer.IsSellButtonAt(mousePoint, GraphicsDevice.Viewport))
        {
            _sellSystem.SellAll();
            _pendingDemolishBuildingId = null;
            return;
        }

        if (_input.IsLeftClickPressed() && _uiRenderer.IsSaveButtonAt(mousePoint, GraphicsDevice.Viewport))
        {
            SaveCurrentGame();
            _pendingDemolishBuildingId = null;
            return;
        }

        if (_input.IsLeftClickPressed() && _uiRenderer.IsLoadButtonAt(mousePoint, GraphicsDevice.Viewport))
        {
            LoadCurrentGame();
            _pendingDemolishBuildingId = null;
            return;
        }

        if (_input.IsLeftClickPressed() && _uiRenderer.IsNewGameButtonAt(mousePoint, GraphicsDevice.Viewport))
        {
            StartNewGame();
            _pendingDemolishBuildingId = null;
            return;
        }

        if (_input.IsLeftClickPressed() && _uiRenderer.IsToggleFullscreenButtonAt(mousePoint, GraphicsDevice.Viewport))
        {
            ToggleFullscreen();
            _pendingDemolishBuildingId = null;
            return;
        }

        if (_input.IsLeftClickPressed() && _uiRenderer.IsExitButtonAt(mousePoint, GraphicsDevice.Viewport))
        {
            SaveCurrentGame();
            Exit();
            return;
        }

        if (_input.IsLeftClickPressed() &&
            _uiRenderer.TryGetResearchButtonAt(mousePoint, GraphicsDevice.Viewport, _activeLeftPanelMode, out var clickedResearchId))
        {
            _lastResearchResult = _researchSystem.Complete(clickedResearchId);
            _lastUpgradeResult = null;
            _pendingDemolishBuildingId = null;
            _mapInput.ClearLastBuildResult();
            _mapInput.ClearLastAreaUnlockResult();
            return;
        }

        if (_input.IsLeftClickPressed() &&
            _uiRenderer.TryGetUpgradeButtonAt(mousePoint, GraphicsDevice.Viewport, _activeLeftPanelMode, out var clickedUpgradeId))
        {
            _lastUpgradeResult = _upgradeSystem.Purchase(clickedUpgradeId);
            _lastResearchResult = null;
            _pendingDemolishBuildingId = null;
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
            _pendingDemolishBuildingId = null;
            return;
        }

        if (_input.IsLeftClickPressed() &&
            _uiRenderer.IsDemolishButtonAt(mousePoint, GraphicsDevice.Viewport, _mapInput.SelectedMapBuildingId))
        {
            var buildingId = _mapInput.SelectedMapBuildingId!.Value;
            if (_pendingDemolishBuildingId != buildingId)
            {
                _pendingDemolishBuildingId = buildingId;
                _lastSaveLoadMessage = "CLICK DEMOLISH AGAIN TO CONFIRM";
                _lastResearchResult = null;
                _lastUpgradeResult = null;
                return;
            }

            var result = _buildSystem.Demolish(buildingId);
            _mapInput.SetLastBuildResult(result);
            if (result.Success)
                _mapInput.ClearSelectedBuilding();
            _pendingDemolishBuildingId = null;
            _lastResearchResult = null;
            _lastUpgradeResult = null;
            _lastSaveLoadMessage = result.Success ? "BUILDING DEMOLISHED" : null;
            return;
        }

        if (_input.IsLeftClickPressed() &&
            _uiRenderer.IsClearTerrainButtonAt(mousePoint, GraphicsDevice.Viewport, _mapInput.SelectedTerrainPosition))
        {
            var result = _terrainClearSystem.Clear(_mapInput.SelectedTerrainPosition!.Value);
            _mapInput.SetLastTerrainClearResult(result);
            _lastResearchResult = null;
            _lastUpgradeResult = null;
            _pendingDemolishBuildingId = null;
            return;
        }

        if (_input.IsLeftClickPressed() &&
            _uiRenderer.IsUnlockCloudButtonAt(mousePoint, GraphicsDevice.Viewport, _mapInput.SelectedCloudPosition))
        {
            var result = _areaUnlockSystem.UnlockCloud(_mapInput.SelectedCloudPosition!.Value);
            _mapInput.SetLastAreaUnlockResult(result);
            _lastResearchResult = null;
            _lastUpgradeResult = null;
            _pendingDemolishBuildingId = null;
            return;
        }

        if (_input.IsLeftClickPressed() && _uiRenderer.TryGetBuildingButtonAt(mousePoint, GraphicsDevice.Viewport, _activeLeftPanelMode, out var clickedBuildingId))
        {
            if (string.Equals(_selectedBuildingId, clickedBuildingId, StringComparison.OrdinalIgnoreCase))
            {
                _selectedBuildingId = null;
                _lastSaveLoadMessage = "BUILD TOOL CANCELED";
            }
            else
            {
                _selectedBuildingId = clickedBuildingId;
                _lastSaveLoadMessage = null;
            }

            _lastResearchResult = null;
            _lastUpgradeResult = null;
            _pendingDemolishBuildingId = null;
            _mapInput.ClearLastBuildResult();
            _mapInput.ClearLastAreaUnlockResult();
        }
    }

    private void ToggleBuildTool(IReadOnlyList<string> buildIds, int index)
    {
        if (buildIds.Count <= index)
        {
            _selectedBuildingId = null;
            _lastSaveLoadMessage = "BUILD TOOL CANCELED";
            _mapInput.ClearLastBuildResult();
            return;
        }

        var buildingId = buildIds[index];
        _activeLeftPanelMode = LeftPanelMode.Build;

        if (string.Equals(_selectedBuildingId, buildingId, StringComparison.OrdinalIgnoreCase))
        {
            _selectedBuildingId = null;
            _lastSaveLoadMessage = "BUILD TOOL CANCELED";
        }
        else
        {
            _selectedBuildingId = buildingId;
            _lastSaveLoadMessage = null;
        }

        _lastResearchResult = null;
        _lastUpgradeResult = null;
        _pendingDemolishBuildingId = null;
        _mapInput.ClearLastBuildResult();
        _mapInput.ClearLastAreaUnlockResult();
    }
}
