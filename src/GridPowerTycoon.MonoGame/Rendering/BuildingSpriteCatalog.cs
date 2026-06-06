using System.Diagnostics.CodeAnalysis;
using GridPowerTycoon.Core.Buildings;
using Microsoft.Xna.Framework.Graphics;

namespace GridPowerTycoon.MonoGame.Rendering;

public sealed class BuildingSpriteCatalog : IDisposable
{
    private readonly Dictionary<string, Texture2D> _sprites = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    public static BuildingSpriteCatalog Load(GraphicsDevice graphicsDevice, string contentRootDirectory)
    {
        var catalog = new BuildingSpriteCatalog();
        var buildingsRoot = Path.Combine(contentRootDirectory, "Sprites", "Buildings");

        if (!Directory.Exists(buildingsRoot))
            return catalog;

        foreach (var buildingDirectory in Directory.EnumerateDirectories(buildingsRoot).OrderBy(static directory => directory, StringComparer.OrdinalIgnoreCase))
        {
            var buildingId = Path.GetFileName(buildingDirectory);
            if (string.IsNullOrWhiteSpace(buildingId))
                continue;

            catalog.LoadBuildingSprites(graphicsDevice, buildingId, buildingDirectory);
        }

        return catalog;
    }

    public bool TryGet(string buildingId, BuildingState state, [NotNullWhen(true)] out Texture2D? texture)
    {
        var stateName = GetStateName(state);
        if (_sprites.TryGetValue(GetKey(buildingId, stateName), out var stateTexture))
        {
            texture = stateTexture;
            return true;
        }

        if (_sprites.TryGetValue(GetKey(buildingId, "idle"), out var idleTexture))
        {
            texture = idleTexture;
            return true;
        }

        texture = null;
        return false;
    }

    private static string GetKey(string buildingId, string state) => buildingId + ":" + state;

    private void LoadBuildingSprites(GraphicsDevice graphicsDevice, string buildingId, string buildingDirectory)
    {
        foreach (var file in Directory.EnumerateFiles(buildingDirectory, "*.png", SearchOption.TopDirectoryOnly).OrderBy(static file => file, StringComparer.OrdinalIgnoreCase))
        {
            var state = GetStateFromFileName(buildingId, Path.GetFileNameWithoutExtension(file));
            if (string.IsNullOrWhiteSpace(state))
                continue;

            using var stream = File.OpenRead(file);
            _sprites[GetKey(buildingId, state)] = Texture2D.FromStream(graphicsDevice, stream);
        }
    }

    private static string GetStateFromFileName(string buildingId, string fileName)
    {
        if (fileName.Equals(buildingId, StringComparison.OrdinalIgnoreCase))
            return "idle";

        var prefix = buildingId + "_";
        if (!fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        var suffix = fileName[prefix.Length..];
        return suffix switch
        {
            "idle" => "idle",
            "active" => "active",
            "damaged" => "damaged",
            "expired" => "expired",
            "exploded" => "exploded",
            _ => string.Empty
        };
    }

    private static string GetStateName(BuildingState state)
    {
        return state switch
        {
            BuildingState.Active => "active",
            BuildingState.Expired => "expired",
            BuildingState.Exploded => "exploded",
            _ => "idle"
        };
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        foreach (var sprite in _sprites.Values)
            sprite.Dispose();

        _sprites.Clear();
        _disposed = true;
    }
}
