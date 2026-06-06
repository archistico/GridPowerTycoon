namespace GridPowerTycoon.Core.Save;

public sealed class AutoSaveState
{
    private readonly TimeSpan _interval;
    private TimeSpan _elapsedDirtyTime;

    public AutoSaveState(TimeSpan interval, bool isEnabled = true)
    {
        if (interval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(interval), "Autosave interval must be positive.");

        _interval = interval;
        IsEnabled = isEnabled;
    }

    public bool IsEnabled { get; private set; }
    public TimeSpan Interval => _interval;
    public TimeSpan ElapsedDirtyTime => _elapsedDirtyTime;

    public void SetEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
        ResetTimer();
    }

    public void ResetTimer()
    {
        _elapsedDirtyTime = TimeSpan.Zero;
    }

    public bool Tick(TimeSpan elapsed, bool isDirty)
    {
        if (elapsed < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(elapsed), "Elapsed time cannot be negative.");

        if (!IsEnabled || !isDirty || elapsed == TimeSpan.Zero)
        {
            if (!isDirty)
                ResetTimer();

            return false;
        }

        _elapsedDirtyTime += elapsed;
        if (_elapsedDirtyTime < _interval)
            return false;

        ResetTimer();
        return true;
    }
}
