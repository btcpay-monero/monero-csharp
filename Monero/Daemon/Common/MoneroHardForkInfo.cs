using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroHardForkInfo : MoneroRpcPaymentInfo
{
    [JsonPropertyName("earliest_height")]
    public ulong? EarliestHeight { get; set; }

    [JsonPropertyName("enabled")]
    public bool? IsEnabled { get; set; }

    [JsonPropertyName("votes")]
    public uint? NumVotes { get; set; }

    [JsonPropertyName("state")]
    public uint? State { get; set; }

    [JsonPropertyName("threshold")]
    public uint? Threshold { get; set; }

    [JsonPropertyName("version")]
    public uint? Version { get; set; }

    [JsonPropertyName("voting")]
    public uint? Voting { get; set; }

    [JsonPropertyName("window")]
    public uint? Window { get; set; }
}