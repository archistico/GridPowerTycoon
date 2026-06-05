namespace GridPowerTycoon.Core.Research;

public sealed class ResearchState
{
    private readonly HashSet<string> _completedResearchIds = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<string> CompletedResearchIds => _completedResearchIds;

    public bool IsCompleted(string researchId)
    {
        return _completedResearchIds.Contains(researchId);
    }

    public void Complete(string researchId)
    {
        if (string.IsNullOrWhiteSpace(researchId))
            throw new ArgumentException("Research id cannot be empty.", nameof(researchId));

        _completedResearchIds.Add(researchId);
    }
}
