using System.Text.Json.Serialization;

using Monero.Common;

namespace Monero.Wallet.Common;

public class MoneroCheckTx : MoneroCheck
{
    [JsonPropertyName("in_pool")]
    public bool? InTxPool { get; set; }

    [JsonPropertyName("confirmations")]
    public ulong? NumConfirmations { get; set; }

    [JsonPropertyName("received")]
    public ulong? ReceivedAmount { get; set; }
}