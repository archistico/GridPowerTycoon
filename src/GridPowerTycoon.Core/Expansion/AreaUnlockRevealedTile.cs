using GridPowerTycoon.Core.Map;

namespace GridPowerTycoon.Core.Expansion;

public sealed class AreaUnlockRevealedTile
{
    public GridPosition Position { get; }
    public TileType RevealedTileType { get; }

    public AreaUnlockRevealedTile(GridPosition position, TileType revealedTileType)
    {
        Position = position;
        RevealedTileType = revealedTileType;
    }
}
