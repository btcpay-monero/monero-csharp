using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroRpcPaymentInfo : MoneroRpcResponse
{
    [JsonPropertyName("credits")]
    public ulong? Credits { get; set; }

    [JsonPropertyName("top_block_hash")]
    public string? TopBlockHash { get; set; }
}