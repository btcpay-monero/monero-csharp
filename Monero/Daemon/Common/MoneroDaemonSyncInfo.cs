using System.Text.Json.Serialization;

using Monero.Daemon.Rpc;

namespace Monero.Daemon.Common;

public class MoneroDaemonSyncInfo : MoneroRpcPaymentInfo
{
    [JsonPropertyName("height")]
    public ulong? Height { get; set; }

    [JsonPropertyName("next_needed_pruning_seed")]
    public uint? NextNeededPruningSeed { get; set; }

    [JsonPropertyName("overview")]
    public string? Overview { get; set; } = "";

    [JsonPropertyName("peers")]
    public List<MoneroPeerInfo>? Peers { get; set; }

    [JsonPropertyName("spans")]
    public List<MoneroConnectionSpan>? Spans { get; set; }

    [JsonPropertyName("target_height")]
    public ulong? TargetHeight { get; set; }
}