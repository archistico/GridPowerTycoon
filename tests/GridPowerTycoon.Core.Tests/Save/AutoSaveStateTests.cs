using GridPowerTycoon.Core.Save;

namespace GridPowerTycoon.Core.Tests.Save;

public sealed class AutoSaveStateTests
{
    [Fact]
    public void Tick_WhenDirtyTimeIsBelowInterval_ShouldNotAutosave()
    {
        var state = new AutoSaveState(TimeSpan.FromSeconds(60));

        var shouldAutosave = state.Tick(TimeSpan.FromSeconds(59), isDirty: true);

        Assert.False(shouldAutosave);
        Assert.Equal(TimeSpan.FromSeconds(59), state.ElapsedDirtyTime);
    }

    [Fact]
    public void Tick_WhenDirtyTimeReachesInterval_ShouldAutosaveAndResetTimer()
    {
        var state = new AutoSaveState(TimeSpan.FromSeconds(60));

        state.Tick(TimeSpan.FromSeconds(30), isDirty: true);
        var shouldAutosave = state.Tick(TimeSpan.FromSeconds(30), isDirty: true);

        Assert.True(shouldAutosave);
        Assert.Equal(TimeSpan.Zero, state.ElapsedDirtyTime);
    }

    [Fact]
    public void Tick_WhenWorldIsClean_ShouldResetTimerAndSkipAutosave()
    {
        var state = new AutoSaveState(TimeSpan.FromSeconds(60));

        state.Tick(TimeSpan.FromSeconds(30), isDirty: true);
        var shouldAutosave = state.Tick(TimeSpan.FromSeconds(10), isDirty: false);

        Assert.False(shouldAutosave);
        Assert.Equal(TimeSpan.Zero, state.ElapsedDirtyTime);
    }

    [Fact]
    public void Tick_WhenAutosaveIsDisabled_ShouldSkipAutosaveAndNotAccumulateTime()
    {
        var state = new AutoSaveState(TimeSpan.FromSeconds(60), isEnabled: false);

        var shouldAutosave = state.Tick(TimeSpan.FromSeconds(90), isDirty: true);

        Assert.False(shouldAutosave);
        Assert.Equal(TimeSpan.Zero, state.ElapsedDirtyTime);
    }

    [Fact]
    public void SetEnabled_ShouldResetTimer()
    {
        var state = new AutoSaveState(TimeSpan.FromSeconds(60));
        state.Tick(TimeSpan.FromSeconds(30), isDirty: true);

        state.SetEnabled(false);

        Assert.False(state.IsEnabled);
        Assert.Equal(TimeSpan.Zero, state.ElapsedDirtyTime);
    }
}
