using Monero.Common;

namespace Monero.Wallet.Common;

public class MoneroCheckTx : MoneroCheck
{
    public bool inTxPool;
    public ulong numConfirmations;
    public ulong receivedAmount;

    public bool InTxPool()
    {
        return inTxPool;
    }

    public MoneroCheckTx SetInTxPool(bool inTxPool)
    {
        this.inTxPool = inTxPool;
        return this;
    }

    public ulong GetNumConfirmations()
    {
        return numConfirmations;
    }

    public MoneroCheckTx SetNumConfirmations(ulong numConfirmations)
    {
        this.numConfirmations = numConfirmations;
        return this;
    }

    public ulong GetReceivedAmount()
    {
        return receivedAmount;
    }

    public MoneroCheckTx SetReceivedAmount(ulong receivedAmount)
    {
        this.receivedAmount = receivedAmount;
        return this;
    }
}