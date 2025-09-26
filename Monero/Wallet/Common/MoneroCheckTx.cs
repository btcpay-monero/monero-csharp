using System.Text.Json.Serialization;

using Monero.Common;

namespace Monero.Wallet.Common;

public class MoneroCheckTx : MoneroCheck
{
    [JsonPropertyName("in_pool")]
    private bool? _inTxPool;
    [JsonPropertyName("confirmations")]
    private ulong? _numConfirmations;
    [JsonPropertyName("received")]
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