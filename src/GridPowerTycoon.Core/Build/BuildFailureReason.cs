namespace GridPowerTycoon.Core.Build;

public enum BuildFailureReason
{
    None,
    UnknownBuilding,
    OutOfMap,
    TileNotBuildable,
    TileAlreadyOccupied,
    NotEnoughMoney,
    ResearchRequired,
    InvalidBuildingSize,
    BuildingNotFound,
    BuildingNotExpired
}
