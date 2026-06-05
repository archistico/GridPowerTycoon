using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Heat;
using GridPowerTycoon.Core.Research;
using GridPowerTycoon.Core.Tools;

namespace GridPowerTycoon.Core.World;

public sealed class GameData
{
    public BuildingCatalog Buildings { get; }
    public EconomySettings Economy { get; }
    public ResearchCatalog Research { get; }
    public HeatSettings Heat { get; }
    public ToolSettings Tools { get; }

    public GameData(BuildingCatalog buildings, EconomySettings economy)
        : this(buildings, economy, ResearchCatalog.Empty, new HeatSettings(), new ToolSettings())
    {
    }

    public GameData(BuildingCatalog buildings, EconomySettings economy, ResearchCatalog research)
        : this(buildings, economy, research, new HeatSettings(), new ToolSettings())
    {
    }

    public GameData(BuildingCatalog buildings, EconomySettings economy, ResearchCatalog research, HeatSettings heat)
        : this(buildings, economy, research, heat, new ToolSettings())
    {
    }

    public GameData(BuildingCatalog buildings, EconomySettings economy, ResearchCatalog research, HeatSettings heat, ToolSettings tools)
    {
        Buildings = buildings;
        Economy = economy;
        Research = research;
        Heat = heat;
        Tools = tools;
    }
}
