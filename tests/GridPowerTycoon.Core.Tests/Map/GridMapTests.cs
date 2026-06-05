using GridPowerTycoon.Core.Map;

namespace GridPowerTycoon.Core.Tests.Map;

public sealed class GridMapTests
{
    [Fact]
    public void Constructor_ShouldCreateExpectedSize()
    {
        var map = new GridMap(20, 12);

        Assert.Equal(20, map.Width);
        Assert.Equal(12, map.Height);
    }

    [Fact]
    public void Contains_ShouldReturnFalseOutsideBounds()
    {
        var map = new GridMap(20, 12);

        Assert.False(map.Contains(new GridPosition(-1, 0)));
        Assert.False(map.Contains(new GridPosition(0, -1)));
        Assert.False(map.Contains(new GridPosition(20, 0)));
        Assert.False(map.Contains(new GridPosition(0, 12)));
    }

    [Fact]
    public void LandTileWithoutBuilding_ShouldBeBuildable()
    {
        var tile = new Tile(new GridPosition(1, 1), TileType.Land);

        Assert.True(tile.IsBuildable);
    }

    [Theory]
    [InlineData(TileType.Water)]
    [InlineData(TileType.Forest)]
    [InlineData(TileType.Mountain)]
    [InlineData(TileType.Cloud)]
    public void NonLandTile_ShouldNotBeBuildable(TileType type)
    {
        var tile = new Tile(new GridPosition(1, 1), type);

        Assert.False(tile.IsBuildable);
    }

    [Fact]
    public void TileWithBuilding_ShouldNotBeBuildable()
    {
        var tile = new Tile(new GridPosition(1, 1), TileType.Land);
        tile.SetBuilding(Guid.NewGuid());

        Assert.False(tile.IsBuildable);
    }
}
