using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroPruneResult : MoneroRpcResponse
{
    [JsonPropertyName("pruned")]
    [JsonInclude]
    private bool? _isPruned { get; set; }
    [JsonPropertyName("pruning_seed")]
    [JsonInclude]
    private int? _pruningSeed { get; set; }

    public bool? IsPruned()
    {
        return _isPruned;
    }

    public void SetIsPruned(bool? isPruned)
    {
        _isPruned = isPruned;
    }

    public int? GetPruningSeed()
    {
        return _pruningSeed;
    }

    public void SetPruningSeed(int? pruningSeed)
    {
        _pruningSeed = pruningSeed;
    }
}