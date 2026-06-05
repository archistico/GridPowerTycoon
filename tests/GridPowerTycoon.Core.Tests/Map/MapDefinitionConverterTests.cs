using GridPowerTycoon.Core.Map;

namespace GridPowerTycoon.Core.Tests.Map;

public sealed class MapDefinitionConverterTests
{
    [Fact]
    public void ToGridMap_ShouldCreateMapFromRows()
    {
        var definition = new MapDefinition
        {
            Width = 5,
            Height = 3,
            Rows =
            {
                "~~~~~",
                "~.FM~",
                "~C..~"
            }
        };

        var map = MapDefinitionConverter.ToGridMap(definition);

        Assert.Equal(5, map.Width);
        Assert.Equal(3, map.Height);
        Assert.Equal(TileType.Water, map.GetTile(new GridPosition(0, 0)).Type);
        Assert.Equal(TileType.Land, map.GetTile(new GridPosition(1, 1)).Type);
        Assert.Equal(TileType.Forest, map.GetTile(new GridPosition(2, 1)).Type);
        Assert.Equal(TileType.Mountain, map.GetTile(new GridPosition(3, 1)).Type);
        Assert.Equal(TileType.Cloud, map.GetTile(new GridPosition(1, 2)).Type);
    }

    [Fact]
    public void ToGridMap_WhenHiddenRowsExist_ShouldKeepCloudVisibleAndStoreCoveredType()
    {
        var definition = new MapDefinition
        {
            Width = 5,
            Height = 3,
            Rows =
            {
                "~~~~~",
                "~.CC~",
                "~~~~~"
            },
            HiddenRows =
            {
                "~~~~~",
                "~.FM~",
                "~~~~~"
            }
        };

        var map = MapDefinitionConverter.ToGridMap(definition);

        var forestCloud = map.GetTile(new GridPosition(2, 1));
        var mountainCloud = map.GetTile(new GridPosition(3, 1));

        Assert.Equal(TileType.Cloud, forestCloud.Type);
        Assert.Equal(TileType.Forest, forestCloud.CoveredType);
        Assert.Equal(TileType.Cloud, mountainCloud.Type);
        Assert.Equal(TileType.Mountain, mountainCloud.CoveredType);
    }

    [Fact]
    public void ToGridMap_WhenRowCountDoesNotMatchHeight_ShouldThrow()
    {
        var definition = new MapDefinition
        {
            Width = 5,
            Height = 3,
            Rows =
            {
                "~~~~~",
                "~...~"
            }
        };

        Assert.Throws<InvalidOperationException>(() => MapDefinitionConverter.ToGridMap(definition));
    }

    [Fact]
    public void ToGridMap_WhenRowWidthDoesNotMatchWidth_ShouldThrow()
    {
        var definition = new MapDefinition
        {
            Width = 5,
            Height = 1,
            Rows =
            {
                "~~~~"
            }
        };

        Assert.Throws<InvalidOperationException>(() => MapDefinitionConverter.ToGridMap(definition));
    }

    [Fact]
    public void ToGridMap_WhenUnknownTileCodeExists_ShouldThrow()
    {
        var definition = new MapDefinition
        {
            Width = 5,
            Height = 1,
            Rows =
            {
                "~~X~~"
            }
        };

        Assert.Throws<InvalidOperationException>(() => MapDefinitionConverter.ToGridMap(definition));
    }
}
