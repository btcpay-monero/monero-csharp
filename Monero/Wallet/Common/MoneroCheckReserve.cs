using Monero.Common;

namespace Monero.Wallet.Common;

public class MoneroCheckReserve : MoneroCheck
{
    private ulong? _totalAmount;
    private ulong? _unconfirmedSpentAmount;

    public MoneroCheckReserve()
        : base(false)
    {

    }

    public MoneroCheckReserve(bool isGood, ulong? totalAmount, ulong? unconfirmedSpentAmount)
        : base(isGood)
    {
        this._totalAmount = totalAmount;
        this._unconfirmedSpentAmount = unconfirmedSpentAmount;
    }

    public ulong? GetTotalAmount()
    {
        return _totalAmount;
    }

    public MoneroCheckReserve SetTotalAmount(ulong? totalAmount)
    {
        this._totalAmount = totalAmount;
        return this;
    }

    public ulong? GetUnconfirmedSpentAmount()
    {
        return _unconfirmedSpentAmount;
    }

    public MoneroCheckReserve SetUnconfirmedSpentAmount(ulong? unconfirmedSpentAmount)
    {
        this._unconfirmedSpentAmount = unconfirmedSpentAmount;
        return this;
    }
}