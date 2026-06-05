using GridPowerTycoon.Core.Economy;

namespace GridPowerTycoon.Core.Tests.Economy;

public sealed class ResourceStateTests
{
    [Fact]
    public void TrySpendMoney_ShouldDecreaseMoney()
    {
        var resources = new ResourceState(new EconomySettings { StartingMoney = 100, StartingMaxEnergy = 100 });

        var result = resources.TrySpendMoney(25);

        Assert.True(result);
        Assert.Equal(75, resources.Money);
    }

    [Fact]
    public void TrySpendMoney_ShouldFailWhenNotEnoughMoney()
    {
        var resources = new ResourceState(new EconomySettings { StartingMoney = 10, StartingMaxEnergy = 100 });

        var result = resources.TrySpendMoney(25);

        Assert.False(result);
        Assert.Equal(10, resources.Money);
    }

    [Fact]
    public void AddEnergy_ShouldRespectMaxEnergy()
    {
        var resources = new ResourceState(new EconomySettings { StartingMoney = 0, StartingMaxEnergy = 100 });

        resources.AddEnergy(150);

        Assert.Equal(100, resources.Energy);
    }

    [Fact]
    public void IncreaseMaxEnergy_ShouldIncreaseCapacity()
    {
        var resources = new ResourceState(new EconomySettings { StartingMoney = 0, StartingMaxEnergy = 100 });

        resources.IncreaseMaxEnergy(500);

        Assert.Equal(600, resources.MaxEnergy);
    }
}
