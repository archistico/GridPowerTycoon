namespace GridPowerTycoon.Core.Map;

public static class MapDefinitionConverter
{
    public static GridMap ToGridMap(MapDefinition definition)
    {
        Validate(definition);

        var map = new GridMap(definition.Width, definition.Height, TileType.Land);

        for (var y = 0; y < definition.Height; y++)
        {
            var row = definition.Rows[y];

            for (var x = 0; x < definition.Width; x++)
            {
                var tileType = CharToTileType(row[x]);
                map.GetTile(new GridPosition(x, y)).SetType(tileType);
            }
        }

        return map;
    }

    public static void Validate(MapDefinition definition)
    {
        if (definition.Width <= 0)
            throw new InvalidOperationException("Map width must be greater than zero.");

        if (definition.Height <= 0)
            throw new InvalidOperationException("Map height must be greater than zero.");

        if (definition.Rows.Count != definition.Height)
            throw new InvalidOperationException($"Map rows count must be equal to height. Expected {definition.Height}, found {definition.Rows.Count}.");

        for (var y = 0; y < definition.Rows.Count; y++)
        {
            var row = definition.Rows[y];

            if (row.Length != definition.Width)
                throw new InvalidOperationException($"Map row {y} must contain exactly {definition.Width} characters. Found {row.Length}.");

            for (var x = 0; x < row.Length; x++)
            {
                if (!IsKnownTileCode(row[x]))
                    throw new InvalidOperationException($"Map contains unknown tile code '{row[x]}' at x={x}, y={y}.");
            }
        }
    }

    private static bool IsKnownTileCode(char code)
    {
        return code is '~' or '.' or 'F' or 'M' or 'C';
    }

    private static TileType CharToTileType(char code)
    {
        return code switch
        {
            '~' => TileType.Water,
            '.' => TileType.Land,
            'F' => TileType.Forest,
            'M' => TileType.Mountain,
            'C' => TileType.Cloud,
            _ => throw new InvalidOperationException($"Unknown tile code '{code}'.")
        };
    }
}
