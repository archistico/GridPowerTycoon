namespace GridPowerTycoon.Core.Map;

public sealed class GridMap
{
    private readonly Tile[,] _tiles;

    public int Width { get; }
    public int Height { get; }

    public GridMap(int width, int height, TileType defaultTileType = TileType.Land)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Map width must be greater than zero.");

        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Map height must be greater than zero.");

        Width = width;
        Height = height;
        _tiles = new Tile[width, height];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                _tiles[x, y] = new Tile(new GridPosition(x, y), defaultTileType);
            }
        }
    }

    public bool Contains(GridPosition position)
    {
        return position.X >= 0 &&
               position.Y >= 0 &&
               position.X < Width &&
               position.Y < Height;
    }

    public Tile GetTile(GridPosition position)
    {
        if (!Contains(position))
            throw new ArgumentOutOfRangeException(nameof(position), "Position is outside the map.");

        return _tiles[position.X, position.Y];
    }

    public IEnumerable<Tile> Tiles
    {
        get
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                    yield return _tiles[x, y];
            }
        }
    }
}
