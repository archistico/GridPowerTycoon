namespace GridPowerTycoon.Core.Build;

public sealed class BuildResult
{
    public bool Success { get; }
    public BuildFailureReason FailureReason { get; }
    public Guid? BuildingId { get; }

    private BuildResult(bool success, BuildFailureReason failureReason, Guid? buildingId)
    {
        Success = success;
        FailureReason = failureReason;
        BuildingId = buildingId;
    }

    public static BuildResult Ok(Guid buildingId)
    {
        return new BuildResult(true, BuildFailureReason.None, buildingId);
    }

    public static BuildResult Fail(BuildFailureReason reason)
    {
        if (reason == BuildFailureReason.None)
            throw new ArgumentException("A failed build result must specify a failure reason.", nameof(reason));

        return new BuildResult(false, reason, null);
    }
}
