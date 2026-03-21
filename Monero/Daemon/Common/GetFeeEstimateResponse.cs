using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class GetFeeEstimateResponse : MoneroRpcResponse
{
    [JsonPropertyName("fee")]
    public uint Fee { get; set; }

    [JsonPropertyName("fees")]
    public List<uint>? Fees { get; set; }

    [JsonPropertyName("quantization_mask")]
    public uint QuantizationMask { get; set; }
}