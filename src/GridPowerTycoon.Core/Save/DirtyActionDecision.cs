namespace GridPowerTycoon.Core.Save;

public enum DirtyActionDecision
{
    ExecuteImmediately = 0,
    NeedsConfirmation = 1,
    Confirmed = 2
}
