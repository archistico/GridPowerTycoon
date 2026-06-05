using System.Text.Json;
using System.Text.Json.Serialization;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Save;

public sealed class SaveGameService
{
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
            Version = 1,
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

        ValidateSave(save);

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
            world.Research.Complete(researchId);

        foreach (var upgrade in save.UpgradeLevels)
            world.Upgrades.SetLevel(upgrade.Key, upgrade.Value);

        foreach (var savedBuilding in save.Buildings)
        {
            var instance = BuildingInstance.Restore(
                savedBuilding.Id,
                savedBuilding.DefinitionId,
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
        ArgumentNullException.ThrowIfNull(world);

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var save = CreateSave(world);
        var json = JsonSerializer.Serialize(save, _options);
        File.WriteAllText(path, json);
    }

    public GameWorld LoadFromFile(string path, GameData data)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Save file not found.", path);

        var json = File.ReadAllText(path);
        var save = JsonSerializer.Deserialize<SaveGame>(json, _options);

        if (save is null)
            throw new InvalidOperationException("Unable to read save file.");

        return RestoreWorld(save, data);
    }

    private static void ValidateSave(SaveGame save)
    {
        if (save.Version != 1)
            throw new InvalidOperationException($"Unsupported save version '{save.Version}'.");

        if (save.Map.Width <= 0 || save.Map.Height <= 0)
            throw new InvalidOperationException("Save map size is invalid.");

        if (save.Map.Tiles.Count != save.Map.Width * save.Map.Height)
            throw new InvalidOperationException("Save map tile count does not match its size.");

        var buildingIds = save.Buildings.Select(x => x.Id).ToHashSet();
        foreach (var tile in save.Map.Tiles.Where(x => x.BuildingId.HasValue))
        {
            if (!buildingIds.Contains(tile.BuildingId!.Value))
                throw new InvalidOperationException($"Tile {tile.X},{tile.Y} references unknown building '{tile.BuildingId}'.");
        }
    }
}
