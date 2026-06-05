namespace GridPowerTycoon.Core.Map;

public static class MapGenerator
{
    public static GridMap CreateDebugMap(int width = 20, int height = 12)
    {
        var map = new GridMap(width, height, TileType.Land);

        for (var x = 0; x < width; x++)
        {
            map.GetTile(new GridPosition(x, 0)).SetType(TileType.Water);
            map.GetTile(new GridPosition(x, height - 1)).SetType(TileType.Water);
        }

        for (var y = 0; y < height; y++)
        {
            map.GetTile(new GridPosition(0, y)).SetType(TileType.Water);
            map.GetTile(new GridPosition(width - 1, y)).SetType(TileType.Water);
        }

        SetIfInside(map, new GridPosition(4, 3), TileType.Forest);
        SetIfInside(map, new GridPosition(5, 3), TileType.Forest);
        SetIfInside(map, new GridPosition(6, 4), TileType.Forest);
        SetIfInside(map, new GridPosition(11, 5), TileType.Mountain);
        SetIfInside(map, new GridPosition(12, 5), TileType.Mountain);
        SetIfInside(map, new GridPosition(14, 7), TileType.Cloud);
        SetIfInside(map, new GridPosition(15, 7), TileType.Cloud);

        return map;
    }

    private static void SetIfInside(GridMap map, GridPosition position, TileType tileType)
    {
        if (map.Contains(position))
            map.GetTile(position).SetType(tileType);
    }
}
