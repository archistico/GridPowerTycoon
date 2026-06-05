namespace GridPowerTycoon.Core.Simulation;

public sealed class OfflineProgressResult
{
    public static OfflineProgressResult None { get; } = new(0, 0, 0, 0, 0m, 0, 0, 0, 0);

    public double RealSecondsAway { get; }
    public double AppliedSeconds { get; }
    public double EnergyDelta { get; }
    public double ResearchDelta { get; }
    public decimal MoneyDelta { get; }
    public double AxesDelta { get; }
    public double MinesDelta { get; }
    public int BuildingsExpired { get; }
    public int BuildingsExploded { get; }

    public bool HasProgress => AppliedSeconds > 0;

    public OfflineProgressResult(
        double realSecondsAway,
        double appliedSeconds,
        double energyDelta,
        double researchDelta,
        decimal moneyDelta,
        double axesDelta,
        double minesDelta,
        int buildingsExpired,
        int buildingsExploded)
    {
        RealSecondsAway = Math.Max(0, realSecondsAway);
        AppliedSeconds = Math.Max(0, appliedSeconds);
        EnergyDelta = energyDelta;
        ResearchDelta = researchDelta;
        MoneyDelta = moneyDelta;
        AxesDelta = axesDelta;
        MinesDelta = minesDelta;
        BuildingsExpired = Math.Max(0, buildingsExpired);
        BuildingsExploded = Math.Max(0, buildingsExploded);
    }
}
