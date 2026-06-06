using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Save;

public sealed record SaveLoadResult(
    GameWorld World,
    SaveGame Save,
    SaveGameSummary Summary,
    bool LoadedFromBackup);
