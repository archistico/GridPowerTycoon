using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Tools;

public sealed class ToolGenerationSystem
{
    private readonly GameWorld _world;

    public ToolGenerationSystem(GameWorld world)
    {
        _world = world;
    }

    public void Update(double deltaSeconds)
    {
        if (deltaSeconds <= 0)
            return;

        var settings = _world.ToolSettings;

        _world.Resources.AddAxes(settings.AxesPerSecond * deltaSeconds, settings.MaxAxes);
        _world.Resources.AddMines(settings.MinesPerSecond * deltaSeconds, settings.MaxMines);
    }
}
