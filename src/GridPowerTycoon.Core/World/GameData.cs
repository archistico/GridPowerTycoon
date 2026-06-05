using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;

namespace GridPowerTycoon.Core.World;

public sealed class GameData
{
    public BuildingCatalog Buildings { get; }
    public EconomySettings Economy { get; }

    public GameData(BuildingCatalog buildings, EconomySettings economy)
    {
        Buildings = buildings;
        Economy = economy;
    }
}
