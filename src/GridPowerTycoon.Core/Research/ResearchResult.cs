namespace GridPowerTycoon.Core.Research;

public sealed class ResearchResult
{
    public bool Success { get; }
    public ResearchFailureReason FailureReason { get; }
    public string? ResearchId { get; }

    private ResearchResult(bool success, ResearchFailureReason failureReason, string? researchId)
    {
        Success = success;
        FailureReason = failureReason;
        ResearchId = researchId;
    }

    public static ResearchResult Ok(string researchId)
    {
        return new ResearchResult(true, ResearchFailureReason.None, researchId);
    }

    public static ResearchResult Fail(ResearchFailureReason reason)
    {
        return new ResearchResult(false, reason, null);
    }
}
