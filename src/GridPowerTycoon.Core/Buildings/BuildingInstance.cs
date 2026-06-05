using GridPowerTycoon.Core.Map;

namespace GridPowerTycoon.Core.Buildings;

public sealed class BuildingInstance
{
    public Guid Id { get; }
    public string DefinitionId { get; }
    public GridPosition Position { get; }
    public double RemainingLifetimeSeconds { get; private set; }
    public double AccumulatedHeat { get; private set; }
    public BuildingState State { get; private set; }

    public bool IsActive => State == BuildingState.Active;

    public BuildingInstance(Guid id, string definitionId, GridPosition position, double lifetimeSeconds)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Building instance id cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(definitionId))
            throw new ArgumentException("Building definition id cannot be empty.", nameof(definitionId));

        if (lifetimeSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(lifetimeSeconds));

        Id = id;
        DefinitionId = definitionId;
        Position = position;
        RemainingLifetimeSeconds = lifetimeSeconds;
        State = BuildingState.Active;
    }

    public void ReduceLifetime(double deltaSeconds)
    {
        if (deltaSeconds <= 0)
            return;

        if (State != BuildingState.Active)
            return;

        if (RemainingLifetimeSeconds <= 0)
            return;

        RemainingLifetimeSeconds -= deltaSeconds;

        if (RemainingLifetimeSeconds <= 0)
        {
            RemainingLifetimeSeconds = 0;
            State = BuildingState.Expired;
        }
    }

    public void AddHeat(double amount)
    {
        if (amount > 0)
            AccumulatedHeat += amount;
    }

    public void RemoveHeat(double amount)
    {
        if (amount > 0)
            AccumulatedHeat = Math.Max(0, AccumulatedHeat - amount);
    }

    public void MarkExploded()
    {
        State = BuildingState.Exploded;
    }

    public void Replace(double lifetimeSeconds)
    {
        if (lifetimeSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(lifetimeSeconds));

        RemainingLifetimeSeconds = lifetimeSeconds;
        AccumulatedHeat = 0;
        State = BuildingState.Active;
    }

    public static BuildingInstance Restore(
        Guid id,
        string definitionId,
        GridPosition position,
        double remainingLifetimeSeconds,
        double accumulatedHeat,
        BuildingState state)
    {
        if (remainingLifetimeSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(remainingLifetimeSeconds));

        if (accumulatedHeat < 0)
            throw new ArgumentOutOfRangeException(nameof(accumulatedHeat));

        var instance = new BuildingInstance(id, definitionId, position, remainingLifetimeSeconds);
        instance.AccumulatedHeat = accumulatedHeat;
        instance.State = state;
        return instance;
    }
}
