using GridPowerTycoon.Core.Map;
using Microsoft.Xna.Framework.Graphics;

namespace GridPowerTycoon.MonoGame.Rendering;

public sealed class TerrainSpriteCatalog : IDisposable
{
    private readonly Dictionary<TileType, List<Texture2D>> _tileSprites = new();
    private bool _disposed;

    public static TerrainSpriteCatalog Load(GraphicsDevice graphicsDevice, string contentRootDirectory)
    {
        var catalog = new TerrainSpriteCatalog();
        var spritesRoot = Path.Combine(contentRootDirectory, "Sprites");

        catalog.LoadTileSet(graphicsDevice, TileType.Land, Path.Combine(spritesRoot, "Terrain", "grass"));
        catalog.LoadTileSet(graphicsDevice, TileType.Forest, Path.Combine(spritesRoot, "Nature", "forest"));
        catalog.LoadTileSet(graphicsDevice, TileType.Mountain, Path.Combine(spritesRoot, "Nature", "mountain"));
        catalog.LoadTileSet(graphicsDevice, TileType.Cloud, Path.Combine(spritesRoot, "Terrain", "cloud"));

        return catalog;
    }

    public bool TryGet(TileType tileType, GridPosition position, out Texture2D texture)
    {
        texture = null!;

        if (!_tileSprites.TryGetValue(tileType, out var sprites) || sprites.Count == 0)
            return false;

        texture = sprites[GetStableVariantIndex(tileType, position, sprites.Count)];
        return true;
    }

    private void LoadTileSet(GraphicsDevice graphicsDevice, TileType tileType, string directory)
    {
        if (!Directory.Exists(directory))
            return;

        var files = Directory
            .EnumerateFiles(directory, "*.png", SearchOption.TopDirectoryOnly)
            .OrderBy(static file => file, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (files.Count == 0)
            return;

        var sprites = new List<Texture2D>(files.Count);
        foreach (var file in files)
        {
            using var stream = File.OpenRead(file);
            sprites.Add(Texture2D.FromStream(graphicsDevice, stream));
        }

        _tileSprites[tileType] = sprites;
    }

    private static int GetStableVariantIndex(TileType tileType, GridPosition position, int count)
    {
        var hash = HashCode.Combine((int)tileType, position.X, position.Y);
        return Math.Abs(hash % count);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        foreach (var sprites in _tileSprites.Values)
        {
            foreach (var sprite in sprites)
                sprite.Dispose();
        }

        _tileSprites.Clear();
        _disposed = true;
    }
}
