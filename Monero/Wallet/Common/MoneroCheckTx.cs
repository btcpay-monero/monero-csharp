using Monero.Common;

namespace Monero.Wallet.Common;

public class MoneroCheckTx : MoneroCheck
{
    private bool? _inTxPool;
    private ulong? _numConfirmations;
    private ulong? _receivedAmount;

    public bool? InTxPool()
    {
        return _inTxPool;
    }

    public MoneroCheckTx SetInTxPool(bool? inTxPool)
    {
        this._inTxPool = inTxPool;
        return this;
    }

    public ulong? GetNumConfirmations()
    {
        return _numConfirmations;
    }

    public MoneroCheckTx SetNumConfirmations(ulong? numConfirmations)
    {
        this._numConfirmations = numConfirmations;
        return this;
    }

    public ulong? GetReceivedAmount()
    {
        return _receivedAmount;
    }

    public MoneroCheckTx SetReceivedAmount(ulong? receivedAmount)
    {
        this._receivedAmount = receivedAmount;
        return this;
    }
}