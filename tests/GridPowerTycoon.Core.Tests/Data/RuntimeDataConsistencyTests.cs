using System.Text.RegularExpressions;
using GridPowerTycoon.Core.Data;

namespace GridPowerTycoon.Core.Tests.Data;

public sealed class RuntimeDataConsistencyTests
{
    [Fact]
    public void RuntimeData_BuildingRequiredResearchIds_ShouldExistInResearchCatalog()
    {
        var (buildings, researches, _) = LoadRuntimeData();

        var missing = buildings.All
            .Where(x => !string.IsNullOrWhiteSpace(x.RequiredResearchId))
            .Where(x => !researches.TryGet(x.RequiredResearchId!, out _))
            .Select(x => $"{x.Id} -> {x.RequiredResearchId}")
            .ToList();

        Assert.Empty(missing);
    }

    [Fact]
    public void RuntimeData_ResearchUnlockBuildingIds_ShouldExistInBuildingCatalog()
    {
        var (buildings, researches, _) = LoadRuntimeData();

        var missing = researches.All
            .SelectMany(x => x.UnlockBuildingIds.Select(id => new { ResearchId = x.Id, BuildingId = id }))
            .Where(x => !buildings.TryGet(x.BuildingId, out _))
            .Select(x => $"{x.ResearchId} -> {x.BuildingId}")
            .ToList();

        Assert.Empty(missing);
    }

    [Fact]
    public void RuntimeData_ResearchManagedBuildingIds_ShouldExistInBuildingCatalog()
    {
        var (buildings, researches, _) = LoadRuntimeData();

        var missing = researches.All
            .SelectMany(x => x.ManagedBuildingIds.Select(id => new { ResearchId = x.Id, BuildingId = id }))
            .Where(x => !buildings.TryGet(x.BuildingId, out _))
            .Select(x => $"{x.ResearchId} -> {x.BuildingId}")
            .ToList();

        Assert.Empty(missing);
    }

    [Fact]
    public void RuntimeData_ResearchPrerequisiteIds_ShouldExistInResearchCatalog()
    {
        var (_, researches, _) = LoadRuntimeData();

        var missing = researches.All
            .SelectMany(x => x.RequiredResearchIds.Select(id => new { ResearchId = x.Id, RequiredResearchId = id }))
            .Where(x => !researches.TryGet(x.RequiredResearchId, out _))
            .Select(x => $"{x.ResearchId} -> {x.RequiredResearchId}")
            .ToList();

        Assert.Empty(missing);
    }

    [Fact]
    public void RuntimeData_UpgradeTargetBuildingIds_ShouldExistInBuildingCatalog()
    {
        var (buildings, _, upgrades) = LoadRuntimeData();

        var missing = upgrades.All
            .Where(x => !string.IsNullOrWhiteSpace(x.TargetBuildingId))
            .Where(x => !buildings.TryGet(x.TargetBuildingId!, out _))
            .Select(x => $"{x.Id} -> {x.TargetBuildingId}")
            .ToList();

        Assert.Empty(missing);
    }

    [Fact]
    public void RuntimeData_UpgradeRequiredResearchIds_ShouldExistInResearchCatalog()
    {
        var (_, researches, upgrades) = LoadRuntimeData();

        var missing = upgrades.All
            .Where(x => !string.IsNullOrWhiteSpace(x.RequiredResearchId))
            .Where(x => !researches.TryGet(x.RequiredResearchId!, out _))
            .Select(x => $"{x.Id} -> {x.RequiredResearchId}")
            .ToList();

        Assert.Empty(missing);
    }

    [Fact]
    public void RuntimeData_UiBuildButtonIds_ShouldExistInBuildingCatalog()
    {
        var (buildings, _, _) = LoadRuntimeData();
        var uiBuildIds = ReadUiStringArray("BuildButtonIds");

        var missing = uiBuildIds
            .Where(id => !buildings.TryGet(id, out _))
            .ToList();

        Assert.Empty(missing);
    }

    [Fact]
    public void RuntimeData_UiResearchButtonIds_ShouldExistInResearchCatalog()
    {
        var (_, researches, _) = LoadRuntimeData();
        var uiResearchIds = ReadUiStringArray("ResearchButtonIds");

        var missing = uiResearchIds
            .Where(id => !researches.TryGet(id, out _))
            .ToList();

        Assert.Empty(missing);
    }

    [Fact]
    public void RuntimeData_AllBuildings_ShouldBeReachableFromBuildUi()
    {
        var (buildings, _, _) = LoadRuntimeData();
        var uiBuildIds = ReadUiStringArray("BuildButtonIds").ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = buildings.All
            .Where(x => !uiBuildIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToList();

        Assert.Empty(missing);
    }

    [Fact]
    public void RuntimeData_AllResearches_ShouldBeReachableFromResearchUi()
    {
        var (_, researches, _) = LoadRuntimeData();
        var uiResearchIds = ReadUiStringArray("ResearchButtonIds").ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = researches.All
            .Where(x => !uiResearchIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToList();

        Assert.Empty(missing);
    }


    [Fact]
    public void RuntimeData_UiUpgradeButtonIds_ShouldExistInUpgradeCatalog()
    {
        var (_, _, upgrades) = LoadRuntimeData();
        var uiUpgradeIds = ReadUiStringArray("UpgradeButtonIds");

        var missing = uiUpgradeIds
            .Where(id => !upgrades.TryGet(id, out _))
            .ToList();

        Assert.Empty(missing);
    }

    [Fact]
    public void RuntimeData_AllUpgrades_ShouldBeReachableFromUpgradeUi()
    {
        var (_, _, upgrades) = LoadRuntimeData();
        var uiUpgradeIds = ReadUiStringArray("UpgradeButtonIds").ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = upgrades.All
            .Where(x => !uiUpgradeIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToList();

        Assert.Empty(missing);
    }

    private static (GridPowerTycoon.Core.Buildings.BuildingCatalog Buildings, GridPowerTycoon.Core.Research.ResearchCatalog Researches, GridPowerTycoon.Core.Upgrades.UpgradeCatalog Upgrades) LoadRuntimeData()
    {
        var root = FindRepositoryRoot();
        var dataPath = Path.Combine(root, "src", "GridPowerTycoon.MonoGame", "Data");
        var loader = new GameDataLoader();

        return (
            loader.LoadBuildingCatalog(Path.Combine(dataPath, "buildings.json")),
            loader.LoadResearchCatalog(Path.Combine(dataPath, "research.json")),
            loader.LoadUpgradeCatalog(Path.Combine(dataPath, "upgrades.json")));
    }

    private static IReadOnlyList<string> ReadUiStringArray(string arrayName)
    {
        var root = FindRepositoryRoot();
        var path = Path.Combine(root, "src", "GridPowerTycoon.MonoGame", "Rendering", "UiRenderer.cs");
        var source = File.ReadAllText(path);
        var match = Regex.Match(
            source,
            $@"private\s+static\s+readonly\s+string\[\]\s+{Regex.Escape(arrayName)}\s*=\s*\{{(?<body>.*?)\}};",
            RegexOptions.Singleline);

        if (!match.Success)
            throw new InvalidOperationException($"Unable to find UI array '{arrayName}'.");

        return Regex.Matches(match.Groups["body"].Value, "\"(?<id>[^\"]+)\"")
            .Select(x => x.Groups["id"].Value)
            .ToList();
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "GridPowerTycoon.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Unable to locate GridPowerTycoon repository root.");
    }
}
