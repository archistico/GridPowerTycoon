namespace GridPowerTycoon.Core.Save;

public sealed class SaveDirtyState
{
    public bool IsDirty { get; private set; }
    public PendingDirtyAction PendingAction { get; private set; } = PendingDirtyAction.None;

    public void MarkDirty()
    {
        IsDirty = true;
    }

    public void MarkClean()
    {
        IsDirty = false;
        PendingAction = PendingDirtyAction.None;
    }

    public void ClearPendingAction()
    {
        PendingAction = PendingDirtyAction.None;
    }

    public DirtyActionDecision Request(PendingDirtyAction action)
    {
        if (action == PendingDirtyAction.None)
            throw new ArgumentException("A real dirty action is required.", nameof(action));

        if (!IsDirty)
        {
            PendingAction = PendingDirtyAction.None;
            return DirtyActionDecision.ExecuteImmediately;
        }

        if (PendingAction == action)
        {
            PendingAction = PendingDirtyAction.None;
            return DirtyActionDecision.Confirmed;
        }

        PendingAction = action;
        return DirtyActionDecision.NeedsConfirmation;
    }
}
