using GridPowerTycoon.Core.Map;

namespace GridPowerTycoon.Core.Expansion;

public sealed class AreaUnlockResult
{
    public bool Success { get; }
    public AreaUnlockFailureReason FailureReason { get; }
    public GridPosition Position { get; }
    public TileType? RevealedTileType { get; }

    private AreaUnlockResult(bool success, AreaUnlockFailureReason failureReason, GridPosition position, TileType? revealedTileType)
    {
        Success = success;
        FailureReason = failureReason;
        Position = position;
        RevealedTileType = revealedTileType;
    }

    public static AreaUnlockResult Ok(GridPosition position, TileType revealedTileType)
    {
        return new AreaUnlockResult(true, AreaUnlockFailureReason.None, position, revealedTileType);
    }

    public static AreaUnlockResult Fail(GridPosition position, AreaUnlockFailureReason reason)
    {
        return new AreaUnlockResult(false, reason, position, null);
    }
}
