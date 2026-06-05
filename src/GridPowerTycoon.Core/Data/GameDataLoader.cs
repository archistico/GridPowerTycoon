using System.Text.Json;
using System.Text.Json.Serialization;
using GridPowerTycoon.Core.Buildings;
using GridPowerTycoon.Core.Economy;
using GridPowerTycoon.Core.Map;
using GridPowerTycoon.Core.Research;

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
