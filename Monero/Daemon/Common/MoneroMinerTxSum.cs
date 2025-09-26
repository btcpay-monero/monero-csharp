using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroMinerTxSum : MoneroRpcResponse
{
    [JsonPropertyName("emission_amount")]
    [JsonInclude]
    private ulong? _emissionSum { get; set; }
    [JsonPropertyName("fee_amount")]
    [JsonInclude]
    private ulong? _feeSum { get; set; }

    public ulong? GetEmissionSum()
    {
        return _emissionSum;
    }

    public void SetEmissionSum(ulong? emissionSum)
    {
        _emissionSum = emissionSum;
    }

    public ulong? GetFeeSum()
    {
        return _feeSum;
    }

    public void SetFeeSum(ulong? feeSum)
    {
        _feeSum = feeSum;
    }
}