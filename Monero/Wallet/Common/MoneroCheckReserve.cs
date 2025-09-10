using Monero.Common;

namespace Monero.Wallet.Common;

public class MoneroCheckReserve : MoneroCheck
{
    private ulong totalAmount;
    private ulong unconfirmedSpentAmount;

    public MoneroCheckReserve(bool isGood = false, ulong totalAmount = 0, ulong unconfirmedSpentAmount = 0)
        : base(isGood)
    {
        this.totalAmount = totalAmount;
        this.unconfirmedSpentAmount = unconfirmedSpentAmount;
    }

    public ulong GetTotalAmount()
    {
        return totalAmount;
    }

    public MoneroCheckReserve SetTotalAmount(ulong totalAmount)
    {
        this.totalAmount = totalAmount;
        return this;
    }

    public ulong GetUnconfirmedSpentAmount()
    {
        return unconfirmedSpentAmount;
    }

    public MoneroCheckReserve SetUnconfirmedSpentAmount(ulong unconfirmedSpentAmount)
    {
        this.unconfirmedSpentAmount = unconfirmedSpentAmount;
        return this;
    }
}