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
        _inTxPool = inTxPool;
        return this;
    }

    public ulong? GetNumConfirmations()
    {
        return _numConfirmations;
    }

    public MoneroCheckTx SetNumConfirmations(ulong? numConfirmations)
    {
        _numConfirmations = numConfirmations;
        return this;
    }

    public ulong? GetReceivedAmount()
    {
        return _receivedAmount;
    }

    public MoneroCheckTx SetReceivedAmount(ulong? receivedAmount)
    {
        _receivedAmount = receivedAmount;
        return this;
    }
}