using GridPowerTycoon.Core.Map;

namespace GridPowerTycoon.Core.Expansion;

public sealed class AreaUnlockResult
{
    public bool Success { get; }
    public AreaUnlockFailureReason FailureReason { get; }
    public GridPosition Position { get; }
    public TileType? RevealedTileType { get; }
    public IReadOnlyList<AreaUnlockRevealedTile> RevealedTiles { get; }
    public int TilesUnlocked => RevealedTiles.Count;

    private AreaUnlockResult(
        bool success,
        AreaUnlockFailureReason failureReason,
        GridPosition position,
        TileType? revealedTileType,
        IReadOnlyList<AreaUnlockRevealedTile> revealedTiles)
    {
        Success = success;
        FailureReason = failureReason;
        Position = position;
        RevealedTileType = revealedTileType;
        RevealedTiles = revealedTiles;
    }

    public static AreaUnlockResult Ok(GridPosition position, IReadOnlyList<AreaUnlockRevealedTile> revealedTiles)
    {
        if (revealedTiles.Count == 0)
            throw new ArgumentException("At least one tile must be revealed.", nameof(revealedTiles));

        return new AreaUnlockResult(true, AreaUnlockFailureReason.None, position, revealedTiles[0].RevealedTileType, revealedTiles);
    }

    public static AreaUnlockResult Ok(GridPosition position, TileType revealedTileType)
    {
        return Ok(position, new[] { new AreaUnlockRevealedTile(position, revealedTileType) });
    }

    public static AreaUnlockResult Fail(GridPosition position, AreaUnlockFailureReason reason)
    {
        return new AreaUnlockResult(false, reason, position, null, Array.Empty<AreaUnlockRevealedTile>());
    }
}
