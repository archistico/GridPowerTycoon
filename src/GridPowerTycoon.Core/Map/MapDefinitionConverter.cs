namespace GridPowerTycoon.Core.Map;

public static class MapDefinitionConverter
{
    public static GridMap ToGridMap(MapDefinition definition)
    {
        Validate(definition);

        var map = new GridMap(definition.Width, definition.Height, TileType.Land);
        var hasHiddenRows = definition.HiddenRows.Count > 0;

        for (var y = 0; y < definition.Height; y++)
        {
            var row = definition.Rows[y];
            var hiddenRow = hasHiddenRows ? definition.HiddenRows[y] : null;

            for (var x = 0; x < definition.Width; x++)
            {
                var tileType = CharToTileType(row[x]);
                var tile = map.GetTile(new GridPosition(x, y));

                tile.SetType(tileType);

                if (tileType == TileType.Cloud)
                {
                    var coveredType = hiddenRow is null
                        ? TileType.Land
                        : CharToTileType(hiddenRow[x]);

                    if (coveredType == TileType.Cloud)
                        coveredType = TileType.Land;

                    tile.SetCoveredType(coveredType);
                }
                else
                {
                    tile.SetCoveredType(null);
                }
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

        ValidateRows(definition.Rows, definition.Width, definition.Height, "Map rows");

        if (definition.HiddenRows.Count > 0)
            ValidateRows(definition.HiddenRows, definition.Width, definition.Height, "Map hiddenRows");
    }

    private static void ValidateRows(IReadOnlyList<string> rows, int width, int height, string label)
    {
        if (rows.Count != height)
            throw new InvalidOperationException($"{label} count must be equal to height. Expected {height}, found {rows.Count}.");

        for (var y = 0; y < rows.Count; y++)
        {
            var row = rows[y];

            if (row.Length != width)
                throw new InvalidOperationException($"{label} row {y} must contain exactly {width} characters. Found {row.Length}.");

            for (var x = 0; x < row.Length; x++)
            {
                if (!IsKnownTileCode(row[x]))
                    throw new InvalidOperationException($"{label} contains unknown tile code '{row[x]}' at x={x}, y={y}.");
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
