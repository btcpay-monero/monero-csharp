using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroPruneResult : MoneroRpcResponse
{
    [JsonPropertyName("pruned")]
    public bool? IsPruned { get; set; }

    [JsonPropertyName("pruning_seed")]
    public int? PruningSeed { get; set; }
}