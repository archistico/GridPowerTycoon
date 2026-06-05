using System.Text.Json;
using System.Text.Json.Serialization;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Heat;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Research;
using GridPowerTycoon.Core.Tools;

namespace GridPowerTycoon.Core.Data;

public sealed class GameDataLoader
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public BuildingCatalog LoadBuildingCatalog(string path)
    {
        var data = LoadJson<BuildingCatalogData>(path, "buildings");
        return BuildingCatalog.FromDefinitions(data.Buildings);
    }

    public EconomySettings LoadEconomySettings(string path)
    {
        var settings = LoadJson<EconomySettings>(path, "economy settings");
        ValidateEconomySettings(settings);
        return settings;
    }

    public ResearchCatalog LoadResearchCatalog(string path)
    {
        var data = LoadJson<ResearchCatalogData>(path, "research");
        return ResearchCatalog.FromDefinitions(data.Researches);
    }

    public HeatSettings LoadHeatSettings(string path)
    {
        var settings = LoadJson<HeatSettings>(path, "heat settings");
        ValidateHeatSettings(settings);
        return settings;
    }

    public ToolSettings LoadToolSettings(string path)
    {
        var settings = LoadJson<ToolSettings>(path, "tool settings");
        ValidateToolSettings(settings);
        return settings;
    }

    public GridMap LoadMap(string path)
    {
        var definition = LoadJson<MapDefinition>(path, "map");
        return MapDefinitionConverter.ToGridMap(definition);
    }

    private T LoadJson<T>(string path, string description)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Game data file for {description} was not found.", path);

        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<T>(json, _options);

        if (data is null)
            throw new InvalidOperationException($"Unable to read game data file for {description}.");

        return data;
    }


    private static void ValidateToolSettings(ToolSettings settings)
    {
        if (settings.AxesPerSecond < 0)
            throw new InvalidOperationException("axesPerSecond cannot be negative.");

        if (settings.MinesPerSecond < 0)
            throw new InvalidOperationException("minesPerSecond cannot be negative.");

        if (settings.MaxAxes < 0)
            throw new InvalidOperationException("maxAxes cannot be negative.");

        if (settings.MaxMines < 0)
            throw new InvalidOperationException("maxMines cannot be negative.");

        if (settings.ForestClearAxesCost < 0)
            throw new InvalidOperationException("forestClearAxesCost cannot be negative.");

        if (settings.MountainClearMinesCost < 0)
            throw new InvalidOperationException("mountainClearMinesCost cannot be negative.");
    }

    private static void ValidateHeatSettings(HeatSettings settings)
    {
        if (settings.HeatWarningThreshold < 0)
            throw new InvalidOperationException("heatWarningThreshold cannot be negative.");

        if (settings.HeatExplosionThreshold <= 0)
            throw new InvalidOperationException("heatExplosionThreshold must be greater than zero.");

        if (settings.HeatWarningThreshold >= settings.HeatExplosionThreshold)
            throw new InvalidOperationException("heatWarningThreshold must be lower than heatExplosionThreshold.");

        if (settings.HeatEnergyConversionRate <= 0)
            throw new InvalidOperationException("heatEnergyConversionRate must be greater than zero.");
    }

    private static void ValidateEconomySettings(EconomySettings settings)
    {
        if (settings.StartingMaxEnergy <= 0)
            throw new InvalidOperationException("startingMaxEnergy must be greater than zero.");

        if (settings.StartingMoney < 0)
            throw new InvalidOperationException("startingMoney cannot be negative.");

        if (settings.EnergySellValue <= 0)
            throw new InvalidOperationException("energySellValue must be greater than zero.");

        if (settings.ManualSellMultiplier <= 0)
            throw new InvalidOperationException("manualSellMultiplier must be greater than zero.");

        if (settings.AutoSellMultiplier <= 0)
            throw new InvalidOperationException("autoSellMultiplier must be greater than zero.");

        if (settings.MaxOfflineSeconds < 0)
            throw new InvalidOperationException("maxOfflineSeconds cannot be negative.");
    }
}
