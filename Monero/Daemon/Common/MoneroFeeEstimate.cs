using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroFeeEstimate : MoneroRpcResponse
{
    [JsonPropertyName("fee")]
    [JsonInclude]
    private ulong? _fee { get; set; }
    [JsonPropertyName("fees")]
    [JsonInclude]
    private List<ulong> _fees { get; set; }
    [JsonPropertyName("quantization_mask")]
    [JsonInclude]
    private ulong? _quantizationMask { get; set; }

    public MoneroFeeEstimate()
    {
        _fees = [];
    }

    public MoneroFeeEstimate(ulong? fee, List<ulong> fees, ulong? quantizationMask)
    {
        _fee = fee;
        _fees = fees;
        _quantizationMask = quantizationMask;
    }

    public MoneroFeeEstimate(MoneroFeeEstimate feeEstimate)
    {
        _fee = feeEstimate._fee;
        _fees = feeEstimate._fees;
        _quantizationMask = feeEstimate._quantizationMask;
    }

    public ulong? GetFee()
    {
        return _fee;
    }

    public MoneroFeeEstimate SetFee(ulong? fee)
    {
        _fee = fee;
        return this;
    }

    public List<ulong> GetFees()
    {
        return _fees;
    }

    public MoneroFeeEstimate SetFees(List<ulong> fees)
    {
        _fees = fees;
        return this;
    }

    public ulong? GetQuantizationMask()
    {
        return _quantizationMask;
    }

    public MoneroFeeEstimate SetQuantizationMask(ulong? quantizationMask)
    {
        _quantizationMask = quantizationMask;
        return this;
    }

    public MoneroFeeEstimate Clone()
    {
        return new MoneroFeeEstimate(this);
    }
}