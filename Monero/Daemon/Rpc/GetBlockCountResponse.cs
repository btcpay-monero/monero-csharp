using System.Text.Json.Serialization;

namespace Monero.Daemon.Rpc;

public class GetBlockCountResponse
{
    [JsonPropertyName("count")]
    public uint BlockCount { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("untrusted")]
    public bool IsUntrusted { get; set; }
}