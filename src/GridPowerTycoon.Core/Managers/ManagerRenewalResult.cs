namespace GridPowerTycoon.Core.Managers;

public sealed record ManagerRenewalResult(
    int RenewedCount,
    int NotEnoughMoneyCount,
    decimal MoneySpent)
{
    public static ManagerRenewalResult None { get; } = new(0, 0, 0m);

    public bool HasRenewals => RenewedCount > 0;
}
