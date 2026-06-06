using System.Text.Json;
using System.Text.Json.Serialization;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Save;

public sealed class SaveGameService
{
    private readonly SaveIdMigrationMap _idMigration;

    public SaveGameService()
        : this(SaveIdMigrationMap.Empty)
    {
    }

    public SaveGameService(SaveIdMigrationMap idMigration)
    {
        _idMigration = idMigration ?? throw new ArgumentNullException(nameof(idMigration));
    }

    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public SaveGame CreateSave(GameWorld world, DateTimeOffset? savedAt = null)
    {
        ArgumentNullException.ThrowIfNull(world);

        return new SaveGame
        {
            Version = SaveGame.CurrentVersion,
            DataVersion = GameData.CurrentVersion,
            SavedAt = savedAt ?? DateTimeOffset.UtcNow,
            Resources = new SaveResources
            {
                Energy = world.Resources.Energy,
                MaxEnergy = world.Resources.MaxEnergy,
                Research = world.Resources.Research,
                Money = world.Resources.Money,
                Axes = world.Resources.Axes,
                Mines = world.Resources.Mines
            },
            Map = new SaveMap
            {
                Width = world.Map.Width,
                Height = world.Map.Height,
                Tiles = world.Map.Tiles
                    .Select(tile => new SaveTile
                    {
                        X = tile.Position.X,
                        Y = tile.Position.Y,
                        Type = tile.Type,
                        CoveredType = tile.CoveredType,
                        BuildingId = tile.BuildingId
                    })
                    .ToList()
            },
            Buildings = world.BuildingInstances.Values
                .Select(instance => new SaveBuildingInstance
                {
                    Id = instance.Id,
                    DefinitionId = instance.DefinitionId,
                    X = instance.Position.X,
                    Y = instance.Position.Y,
                    RemainingLifetimeSeconds = instance.RemainingLifetimeSeconds,
                    AccumulatedHeat = instance.AccumulatedHeat,
                    State = instance.State
                })
                .ToList(),
            CompletedResearchIds = world.Research.CompletedResearchIds.ToList(),
            UpgradeLevels = world.Upgrades.Levels.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase)
        };
    }

    public GameWorld RestoreWorld(SaveGame save, GameData data)
    {
        ArgumentNullException.ThrowIfNull(save);
        ArgumentNullException.ThrowIfNull(data);

        ValidateSave(save, data, _idMigration);

        var map = new GridMap(save.Map.Width, save.Map.Height, TileType.Water);

        foreach (var savedTile in save.Map.Tiles)
        {
            var position = new GridPosition(savedTile.X, savedTile.Y);
            var tile = map.GetTile(position);
            tile.SetType(savedTile.Type);
            tile.SetCoveredType(savedTile.CoveredType);
        }

        var world = new GameWorld(map, data);
        world.Resources.Restore(
            save.Resources.Energy,
            save.Resources.MaxEnergy,
            save.Resources.Research,
            save.Resources.Money,
            save.Resources.Axes,
            save.Resources.Mines);

        foreach (var researchId in save.CompletedResearchIds)
            world.Research.Complete(_idMigration.ResolveResearchId(researchId));

        foreach (var upgrade in save.UpgradeLevels)
            world.Upgrades.SetLevel(_idMigration.ResolveUpgradeId(upgrade.Key), upgrade.Value);

        foreach (var savedBuilding in save.Buildings)
        {
            var instance = BuildingInstance.Restore(
                savedBuilding.Id,
                _idMigration.ResolveBuildingDefinitionId(savedBuilding.DefinitionId),
                new GridPosition(savedBuilding.X, savedBuilding.Y),
                savedBuilding.RemainingLifetimeSeconds,
                savedBuilding.AccumulatedHeat,
                savedBuilding.State);

            world.AddBuilding(instance);
        }

        foreach (var savedTile in save.Map.Tiles.Where(x => x.BuildingId.HasValue))
        {
            var position = new GridPosition(savedTile.X, savedTile.Y);
            var tile = world.Map.GetTile(position);
            tile.SetBuilding(savedTile.BuildingId!.Value);
        }

        return world;
    }

    public void SaveToFile(GameWorld world, string path)
    {
        SaveToFile(world, path, DateTimeOffset.UtcNow);
    }

    public void SaveToFile(GameWorld world, string path, DateTimeOffset savedAt)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var save = CreateSave(world, savedAt);
        var json = JsonSerializer.Serialize(save, _options);
        WriteSaveFileWithBackup(path, json);
    }

    public static string GetBackupPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var directory = Path.GetDirectoryName(path);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        var extension = Path.GetExtension(path);
        var backupFileName = string.IsNullOrEmpty(extension)
            ? fileNameWithoutExtension + ".backup"
            : fileNameWithoutExtension + ".backup" + extension;

        return string.IsNullOrEmpty(directory)
            ? backupFileName
            : Path.Combine(directory, backupFileName);
    }

    public SaveGameSummary CreateSummary(SaveGame save)
    {
        ArgumentNullException.ThrowIfNull(save);

        return new SaveGameSummary(save.Version, save.DataVersion, save.SavedAt);
    }

    public SaveGameSummary LoadSummaryFromFile(string path)
    {
        return CreateSummary(LoadSaveFromFile(path));
    }


    private static void WriteSaveFileWithBackup(string path, string json)
    {
        var directory = Path.GetDirectoryName(path);
        var tempPath = Path.Combine(
            string.IsNullOrWhiteSpace(directory) ? "." : directory,
            Path.GetFileName(path) + ".tmp");

        try
        {
            File.WriteAllText(tempPath, json);

            if (File.Exists(path))
                File.Copy(path, GetBackupPath(path), overwrite: true);

            File.Move(tempPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    public SaveGame LoadSaveFromFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Save file not found.", path);

        var json = File.ReadAllText(path);

        try
        {
            var save = JsonSerializer.Deserialize<SaveGame>(json, _options);

            if (save is null)
                throw new InvalidOperationException("Unable to read save file.");

            return save;
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Unable to read save file. The save JSON is invalid or corrupted.", exception);
        }
    }

    public GameWorld LoadFromFile(string path, GameData data)
    {
        var save = LoadSaveFromFile(path);
        return RestoreWorld(save, data);
    }

    public SaveLoadResult LoadFromFileWithBackup(string path, GameData data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(data);

        Exception? primaryException = null;
        if (File.Exists(path))
        {
            try
            {
                return LoadPrimaryOrBackup(path, data, loadedFromBackup: false);
            }
            catch (Exception exception)
            {
                primaryException = exception;
            }
        }

        var backupPath = GetBackupPath(path);
        if (!File.Exists(backupPath))
        {
            if (primaryException is not null)
                throw new InvalidOperationException("Unable to load save file and no backup save file was found.", primaryException);

            throw new FileNotFoundException("Save file not found.", path);
        }

        try
        {
            return LoadPrimaryOrBackup(backupPath, data, loadedFromBackup: true);
        }
        catch (Exception backupException)
        {
            throw new InvalidOperationException("Unable to load save file or backup save file.",
                primaryException is null ? backupException : new AggregateException(primaryException, backupException));
        }
    }

    private SaveLoadResult LoadPrimaryOrBackup(string path, GameData data, bool loadedFromBackup)
    {
        var save = LoadSaveFromFile(path);
        var world = RestoreWorld(save, data);
        return new SaveLoadResult(world, save, CreateSummary(save), loadedFromBackup);
    }

    private static void ValidateSave(SaveGame save, GameData data, SaveIdMigrationMap idMigration)
    {
        if (save.Version != SaveGame.CurrentVersion)
            throw new InvalidOperationException($"Unsupported save version '{save.Version}'.");

        if (save.DataVersion != GameData.CurrentVersion)
            throw new InvalidOperationException($"Unsupported data version '{save.DataVersion}'.");

        if (save.Map.Width <= 0 || save.Map.Height <= 0)
            throw new InvalidOperationException("Save map size is invalid.");

        if (save.Map.Tiles.Count != save.Map.Width * save.Map.Height)
            throw new InvalidOperationException("Save map tile count does not match its size.");

        ValidateMapTiles(save);
        ValidateBuildings(save, data, idMigration);
        ValidateResearch(save, data, idMigration);
        ValidateUpgrades(save, data, idMigration);
    }

    private static void ValidateMapTiles(SaveGame save)
    {
        var positions = new HashSet<GridPosition>();

        foreach (var tile in save.Map.Tiles)
        {
            var position = new GridPosition(tile.X, tile.Y);

            if (tile.X < 0 || tile.Y < 0 || tile.X >= save.Map.Width || tile.Y >= save.Map.Height)
                throw new InvalidOperationException($"Save tile {tile.X},{tile.Y} is outside the map bounds.");

            if (!positions.Add(position))
                throw new InvalidOperationException($"Save contains duplicate tile coordinates {tile.X},{tile.Y}.");
        }

        for (var y = 0; y < save.Map.Height; y++)
        {
            for (var x = 0; x < save.Map.Width; x++)
            {
                var position = new GridPosition(x, y);
                if (!positions.Contains(position))
                    throw new InvalidOperationException($"Save is missing tile coordinates {x},{y}.");
            }
        }
    }

    private static void ValidateBuildings(SaveGame save, GameData data, SaveIdMigrationMap idMigration)
    {
        var buildingIds = new HashSet<Guid>();
        foreach (var building in save.Buildings)
        {
            if (building.Id == Guid.Empty)
                throw new InvalidOperationException("Save contains a building with an empty id.");

            if (!buildingIds.Add(building.Id))
                throw new InvalidOperationException($"Save contains duplicate building id '{building.Id}'.");

            if (string.IsNullOrWhiteSpace(building.DefinitionId))
                throw new InvalidOperationException($"Save building '{building.Id}' has an empty definition id.");

            var resolvedDefinitionId = idMigration.ResolveBuildingDefinitionId(building.DefinitionId);
            if (!data.Buildings.TryGet(resolvedDefinitionId, out var definition))
                throw new InvalidOperationException($"Save references unknown building definition '{building.DefinitionId}'.");

            if (building.RemainingLifetimeSeconds < 0)
                throw new InvalidOperationException($"Save building '{building.Id}' has negative remaining lifetime.");

            if (building.AccumulatedHeat < 0)
                throw new InvalidOperationException($"Save building '{building.Id}' has negative accumulated heat.");

            ValidateBuildingFootprint(save, building, definition);
        }

        var buildingsById = save.Buildings.ToDictionary(x => x.Id);
        foreach (var tile in save.Map.Tiles.Where(x => x.BuildingId.HasValue))
        {
            if (!buildingIds.Contains(tile.BuildingId!.Value))
                throw new InvalidOperationException($"Tile {tile.X},{tile.Y} references unknown building '{tile.BuildingId}'.");

            var building = buildingsById[tile.BuildingId.Value];
            var definition = data.Buildings.GetRequired(idMigration.ResolveBuildingDefinitionId(building.DefinitionId));
            if (!IsInsideFootprint(tile, building, definition))
                throw new InvalidOperationException($"Tile {tile.X},{tile.Y} references building '{tile.BuildingId}' but is outside its footprint.");
        }
    }

    private static void ValidateBuildingFootprint(SaveGame save, SaveBuildingInstance building, BuildingDefinition definition)
    {
        for (var y = 0; y < definition.Height; y++)
        {
            for (var x = 0; x < definition.Width; x++)
            {
                var tileX = building.X + x;
                var tileY = building.Y + y;

                if (tileX < 0 || tileY < 0 || tileX >= save.Map.Width || tileY >= save.Map.Height)
                    throw new InvalidOperationException($"Save building '{building.Id}' footprint is outside the map bounds.");

                var tile = save.Map.Tiles.Single(t => t.X == tileX && t.Y == tileY);
                if (tile.BuildingId != building.Id)
                    throw new InvalidOperationException($"Save building '{building.Id}' footprint is not fully linked on tile {tileX},{tileY}.");
            }
        }
    }


    private static bool IsInsideFootprint(SaveTile tile, SaveBuildingInstance building, BuildingDefinition definition)
    {
        return tile.X >= building.X &&
               tile.Y >= building.Y &&
               tile.X < building.X + definition.Width &&
               tile.Y < building.Y + definition.Height;
    }

    private static void ValidateResearch(SaveGame save, GameData data, SaveIdMigrationMap idMigration)
    {
        var completedResearchIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var researchId in save.CompletedResearchIds)
        {
            if (string.IsNullOrWhiteSpace(researchId))
                throw new InvalidOperationException("Save contains an empty research id.");

            var resolvedResearchId = idMigration.ResolveResearchId(researchId);
            if (!completedResearchIds.Add(resolvedResearchId))
                throw new InvalidOperationException($"Save contains duplicate completed research '{researchId}' after id migration.");

            if (!data.Research.TryGet(resolvedResearchId, out _))
                throw new InvalidOperationException($"Save references unknown research '{researchId}'.");
        }
    }

    private static void ValidateUpgrades(SaveGame save, GameData data, SaveIdMigrationMap idMigration)
    {
        var upgradeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var upgrade in save.UpgradeLevels)
        {
            if (string.IsNullOrWhiteSpace(upgrade.Key))
                throw new InvalidOperationException("Save contains an empty upgrade id.");

            var resolvedUpgradeId = idMigration.ResolveUpgradeId(upgrade.Key);
            if (!upgradeIds.Add(resolvedUpgradeId))
                throw new InvalidOperationException($"Save contains duplicate upgrade '{upgrade.Key}' after id migration.");

            if (!data.Upgrades.TryGet(resolvedUpgradeId, out var definition))
                throw new InvalidOperationException($"Save references unknown upgrade '{upgrade.Key}'.");

            if (upgrade.Value < 0)
                throw new InvalidOperationException($"Save upgrade '{upgrade.Key}' has negative level.");

            if (upgrade.Value > definition.MaxLevel)
                throw new InvalidOperationException($"Save upgrade '{upgrade.Key}' level {upgrade.Value} exceeds max level {definition.MaxLevel}.");
        }
    }
}
