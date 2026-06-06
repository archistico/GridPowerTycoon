namespace GridPowerTycoon.Core.Buildings;

public sealed class BuildingDefinition
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";

    public BuildingCategory Category { get; init; }

    public decimal Cost { get; init; }

    public int Width { get; init; } = 1;
    public int Height { get; init; } = 1;

    public double EnergyPerSecond { get; init; }
    public double HeatPerSecond { get; init; }
    public double ResearchPerSecond { get; init; }
    public double EnergyConsumptionPerSecond { get; init; }

    public double BatteryCapacity { get; init; }
    public double AutoSellPerSecond { get; init; }

    public double HeatConversionPerSecond { get; init; }
    public double HeatDissipationPerSecond { get; init; }
    public int HeatRange { get; init; }

    public double EnergyEfficiencyBonus { get; init; }
    public double MaintenanceEfficiencyBonus { get; init; }
    public double ToolCapacityBonus { get; init; }

    public double LifetimeSeconds { get; init; }

    public string? RequiredResearchId { get; init; }
    public int UnlockLevel { get; init; } = 1;
}
