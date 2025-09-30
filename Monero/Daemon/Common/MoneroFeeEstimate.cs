using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroFeeEstimate : MoneroRpcResponse
{
    [JsonPropertyName("fee")]
    public ulong? Fee { get; set; }

    [JsonPropertyName("fees")]
    public List<ulong>? Fees { get; set; }

    [JsonPropertyName("quantization_mask")]
    public ulong? QuantizationMask { get; set; }
}