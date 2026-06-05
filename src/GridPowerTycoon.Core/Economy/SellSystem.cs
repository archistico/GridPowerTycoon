using GridPowerTycoon.Core.World;

namespace GridPowerTycoon.Core.Economy;

public sealed class SellSystem
{
    private readonly GameWorld _world;

    public SellSystem(GameWorld world)
    {
        _world = world;
    }

    public decimal SellAll()
    {
        var energy = _world.Resources.RemoveAllEnergy();
        return ConvertEnergyToMoney(energy, _world.EconomySettings.ManualSellMultiplier);
    }

    public decimal SellAmount(double amount)
    {
        var energy = _world.Resources.RemoveEnergy(amount);
        return ConvertEnergyToMoney(energy, _world.EconomySettings.AutoSellMultiplier);
    }

    private decimal ConvertEnergyToMoney(double energy, double multiplier)
    {
        if (energy <= 0)
            return 0m;

        var safeMultiplier = Math.Max(0, multiplier);
        var money = (decimal)energy * _world.EconomySettings.EnergySellValue * (decimal)safeMultiplier;
        _world.Resources.AddMoney(money);
        return money;
    }
}
