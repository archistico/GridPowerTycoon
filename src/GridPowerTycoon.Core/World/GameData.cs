using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Expansion;
using GridPowerTycoon.Core.Heat;
using GridPowerTycoon.Core.Research;
using GridPowerTycoon.Core.Tools;
using GridPowerTycoon.Core.Upgrades;

namespace GridPowerTycoon.Core.World;

public sealed class GameData
{
    public const int CurrentVersion = 1;

    public BuildingCatalog Buildings { get; }
    public EconomySettings Economy { get; }
    public ResearchCatalog Research { get; }
    public HeatSettings Heat { get; }
    public ToolSettings Tools { get; }
    public UpgradeCatalog Upgrades { get; }
    public AreaUnlockSettings AreaUnlock { get; }

    public GameData(BuildingCatalog buildings, EconomySettings economy)
        : this(buildings, economy, ResearchCatalog.Empty, new HeatSettings(), new ToolSettings(), UpgradeCatalog.Empty, new AreaUnlockSettings())
    {
    }

    public GameData(BuildingCatalog buildings, EconomySettings economy, ResearchCatalog research)
        : this(buildings, economy, research, new HeatSettings(), new ToolSettings(), UpgradeCatalog.Empty, new AreaUnlockSettings())
    {
    }

    public GameData(BuildingCatalog buildings, EconomySettings economy, ResearchCatalog research, HeatSettings heat)
        : this(buildings, economy, research, heat, new ToolSettings(), UpgradeCatalog.Empty, new AreaUnlockSettings())
    {
    }

    public GameData(BuildingCatalog buildings, EconomySettings economy, ResearchCatalog research, HeatSettings heat, ToolSettings tools)
        : this(buildings, economy, research, heat, tools, UpgradeCatalog.Empty, new AreaUnlockSettings())
    {
    }

    public GameData(BuildingCatalog buildings, EconomySettings economy, ResearchCatalog research, HeatSettings heat, ToolSettings tools, UpgradeCatalog upgrades)
        : this(buildings, economy, research, heat, tools, upgrades, new AreaUnlockSettings())
    {
    }

    public GameData(BuildingCatalog buildings, EconomySettings economy, ResearchCatalog research, HeatSettings heat, ToolSettings tools, UpgradeCatalog upgrades, AreaUnlockSettings areaUnlock)
    {
        Buildings = buildings;
        Economy = economy;
        Research = research;
        Heat = heat;
        Tools = tools;
        Upgrades = upgrades;
        AreaUnlock = areaUnlock;
    }
}
