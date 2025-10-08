using System.Text.Json.Serialization;

namespace Monero.Daemon.Rpc;

public class GetFeeEstimateRequest
{
    [JsonPropertyName("grace_blocks")] public int? GraceBlocks { get; set; }
}