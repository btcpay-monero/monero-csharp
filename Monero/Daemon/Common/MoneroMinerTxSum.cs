using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroMinerTxSum : MoneroRpcResponse
{
    [JsonPropertyName("emission_amount")]
    public ulong? EmissionSum { get; set; }

    [JsonPropertyName("fee_amount")]
    public ulong? FeeSum { get; set; }
}