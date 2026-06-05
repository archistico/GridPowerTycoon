using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Research;

namespace GridPowerTycoon.Core.World;

public sealed class GameData
{
    public BuildingCatalog Buildings { get; }
    public EconomySettings Economy { get; }
    public ResearchCatalog Research { get; }

    public GameData(BuildingCatalog buildings, EconomySettings economy)
        : this(buildings, economy, ResearchCatalog.Empty)
    {
    }

    public GameData(BuildingCatalog buildings, EconomySettings economy, ResearchCatalog research)
    {
        Buildings = buildings;
        Economy = economy;
        Research = research;
    }
}
