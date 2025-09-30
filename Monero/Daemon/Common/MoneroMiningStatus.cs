using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroMiningStatus : MoneroRpcResponse
{
    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("active")]
    public bool? IsActive { get; set; }

    [JsonPropertyName("is_background_mining_enabled")]
    public bool? IsBackground { get; set; }

    [JsonPropertyName("threads_count")]
    public uint? NumThreads { get; set; }

    [JsonPropertyName("speed")]
    public ulong? Speed { get; set; }
}