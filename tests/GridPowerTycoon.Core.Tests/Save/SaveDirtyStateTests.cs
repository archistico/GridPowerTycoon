using GridPowerTycoon.Core.Save;

namespace GridPowerTycoon.Core.Tests.Save;

public sealed class SaveDirtyStateTests
{
    [Fact]
    public void Request_WhenClean_ShouldExecuteImmediately()
    {
        var state = new SaveDirtyState();

        var decision = state.Request(PendingDirtyAction.NewGame);

        Assert.Equal(DirtyActionDecision.ExecuteImmediately, decision);
        Assert.False(state.IsDirty);
        Assert.Equal(PendingDirtyAction.None, state.PendingAction);
    }

    [Fact]
    public void Request_WhenDirty_ShouldRequireConfirmationFirst()
    {
        var state = new SaveDirtyState();
        state.MarkDirty();

        var decision = state.Request(PendingDirtyAction.NewGame);

        Assert.Equal(DirtyActionDecision.NeedsConfirmation, decision);
        Assert.True(state.IsDirty);
        Assert.Equal(PendingDirtyAction.NewGame, state.PendingAction);
    }

    [Fact]
    public void Request_WhenSameDirtyActionIsRequestedTwice_ShouldConfirm()
    {
        var state = new SaveDirtyState();
        state.MarkDirty();
        state.Request(PendingDirtyAction.Exit);

        var decision = state.Request(PendingDirtyAction.Exit);

        Assert.Equal(DirtyActionDecision.Confirmed, decision);
        Assert.True(state.IsDirty);
        Assert.Equal(PendingDirtyAction.None, state.PendingAction);
    }

    [Fact]
    public void Request_WhenDifferentDirtyActionIsRequested_ShouldSwitchPendingAction()
    {
        var state = new SaveDirtyState();
        state.MarkDirty();
        state.Request(PendingDirtyAction.NewGame);

        var decision = state.Request(PendingDirtyAction.Exit);

        Assert.Equal(DirtyActionDecision.NeedsConfirmation, decision);
        Assert.True(state.IsDirty);
        Assert.Equal(PendingDirtyAction.Exit, state.PendingAction);
    }

    [Fact]
    public void MarkClean_ShouldClearDirtyAndPendingState()
    {
        var state = new SaveDirtyState();
        state.MarkDirty();
        state.Request(PendingDirtyAction.NewGame);

        state.MarkClean();

        Assert.False(state.IsDirty);
        Assert.Equal(PendingDirtyAction.None, state.PendingAction);
    }

    [Fact]
    public void MarkDirty_ShouldNotClearPendingConfirmation()
    {
        var state = new SaveDirtyState();
        state.MarkDirty();
        state.Request(PendingDirtyAction.NewGame);

        state.MarkDirty();

        Assert.True(state.IsDirty);
        Assert.Equal(PendingDirtyAction.NewGame, state.PendingAction);
    }

    [Fact]
    public void Request_WithNoneAction_ShouldThrow()
    {
        var state = new SaveDirtyState();

        Assert.Throws<ArgumentException>(() => state.Request(PendingDirtyAction.None));
    }
}
