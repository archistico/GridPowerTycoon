namespace GridPowerTycoon.Core.Economy;

public sealed class ResourceState
{
    public double Energy { get; private set; }
    public double MaxEnergy { get; private set; }
    public double Research { get; private set; }
    public decimal Money { get; private set; }
    public double Axes { get; private set; }
    public double Mines { get; private set; }

    public ResourceState(EconomySettings settings)
    {
        if (settings.StartingMaxEnergy <= 0)
            throw new ArgumentOutOfRangeException(nameof(settings), "Starting max energy must be greater than zero.");

        MaxEnergy = settings.StartingMaxEnergy;
        Energy = Math.Clamp(settings.StartingEnergy, 0, MaxEnergy);
        Research = Math.Max(0, settings.StartingResearch);
        Money = settings.StartingMoney;
        Axes = Math.Max(0, settings.StartingAxes);
        Mines = Math.Max(0, settings.StartingMines);
    }

    public void AddEnergy(double amount)
    {
        if (amount <= 0)
            return;

        Energy = Math.Min(MaxEnergy, Energy + amount);
    }

    public double RemoveEnergy(double amount)
    {
        if (amount <= 0)
            return 0;

        var removed = Math.Min(Energy, amount);
        Energy -= removed;
        return removed;
    }

    public bool TrySpendEnergy(double amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        if (amount <= 0)
            return true;

        if (Energy < amount)
            return false;

        Energy -= amount;
        return true;
    }

    public double RemoveAllEnergy()
    {
        var removed = Energy;
        Energy = 0;
        return removed;
    }

    public void AddResearch(double amount)
    {
        if (amount > 0)
            Research += amount;
    }

    public bool TrySpendResearch(double amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        if (Research < amount)
            return false;

        Research -= amount;
        return true;
    }

    public void AddMoney(decimal amount)
    {
        if (amount > 0)
            Money += amount;
    }

    public bool TrySpendMoney(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        if (Money < amount)
            return false;

        Money -= amount;
        return true;
    }

    public void IncreaseMaxEnergy(double amount)
    {
        if (amount > 0)
            MaxEnergy += amount;
    }

    public void SetMaxEnergy(double value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value));

        MaxEnergy = value;
        Energy = Math.Min(Energy, MaxEnergy);
    }

    public void AddAxes(double amount, double maxAxes)
    {
        if (amount <= 0)
            return;

        Axes = Math.Min(Math.Max(0, maxAxes), Axes + amount);
    }

    public void AddMines(double amount, double maxMines)
    {
        if (amount <= 0)
            return;

        Mines = Math.Min(Math.Max(0, maxMines), Mines + amount);
    }

    public bool TrySpendAxes(double amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        if (Axes < amount)
            return false;

        Axes -= amount;
        return true;
    }

    public bool TrySpendMines(double amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        if (Mines < amount)
            return false;

        Mines -= amount;
        return true;
    }

    public void Restore(double energy, double maxEnergy, double research, decimal money, double axes, double mines)
    {
        if (maxEnergy <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxEnergy));

        MaxEnergy = maxEnergy;
        Energy = Math.Clamp(energy, 0, MaxEnergy);
        Research = Math.Max(0, research);
        Money = money < 0 ? 0 : money;
        Axes = Math.Max(0, axes);
        Mines = Math.Max(0, mines);
    }
}
