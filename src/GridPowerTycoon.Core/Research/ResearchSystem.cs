using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Research;

public sealed class ResearchSystem
{
    private readonly GameWorld _world;

    public ResearchSystem(GameWorld world)
    {
        _world = world;
    }

    public ResearchFailureReason CanComplete(string researchId)
    {
        if (!_world.ResearchCatalog.TryGet(researchId, out var definition))
            return ResearchFailureReason.UnknownResearch;

        if (_world.Research.IsCompleted(researchId))
            return ResearchFailureReason.AlreadyCompleted;

        foreach (var requiredId in definition.RequiredResearchIds)
        {
            if (!_world.Research.IsCompleted(requiredId))
                return ResearchFailureReason.MissingPrerequisite;
        }

        if (_world.Resources.Research < definition.Cost)
            return ResearchFailureReason.NotEnoughResearch;

        return ResearchFailureReason.None;
    }

    public ResearchResult Complete(string researchId)
    {
        var validation = CanComplete(researchId);
        if (validation != ResearchFailureReason.None)
            return ResearchResult.Fail(validation);

        var definition = _world.ResearchCatalog.GetRequired(researchId);
        if (!_world.Resources.TrySpendResearch(definition.Cost))
            return ResearchResult.Fail(ResearchFailureReason.NotEnoughResearch);

        _world.Research.Complete(researchId);
        return ResearchResult.Ok(researchId);
    }
}
