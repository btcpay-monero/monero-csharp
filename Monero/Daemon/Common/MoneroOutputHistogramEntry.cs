using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroOutputHistogramEntry
{
    [JsonPropertyName("amount")]
    public ulong? Amount { get; set; }

    [JsonPropertyName("total_instances")]
    public ulong? NumInstances { get; set; }

    [JsonPropertyName("recent_instances")]
    public ulong? NumRecentInstances { get; set; }

    [JsonPropertyName("unlocked_instances")]
    public ulong? NumUnlockedInstances { get; set; }
}