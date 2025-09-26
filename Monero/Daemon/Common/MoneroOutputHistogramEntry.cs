using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroOutputHistogramEntry
{
    [JsonPropertyName("amount")]
    [JsonInclude]
    private ulong? _amount { get; set; }
    [JsonPropertyName("total_instances")]
    [JsonInclude]
    private ulong? _numInstances { get; set; }
    [JsonPropertyName("recent_instances")]
    [JsonInclude]
    private ulong? _numRecentInstances { get; set; }
    [JsonPropertyName("unlocked_instances")]
    [JsonInclude]
    private ulong? _numUnlockedInstances { get; set; }

    public ulong? GetAmount()
    {
        return _amount;
    }

    public void SetAmount(ulong? amount)
    {
        _amount = amount;
    }

    public ulong? GetNumInstances()
    {
        return _numInstances;
    }

    public void SetNumInstances(ulong? numInstances)
    {
        _numInstances = numInstances;
    }

    public ulong? GetNumUnlockedInstances()
    {
        return _numUnlockedInstances;
    }

    public void SetNumUnlockedInstances(ulong? numUnlockedInstances)
    {
        _numUnlockedInstances = numUnlockedInstances;
    }

    public ulong? GetNumRecentInstances()
    {
        return _numRecentInstances;
    }

    public void SetNumRecentInstances(ulong? numRecentInstances)
    {
        _numRecentInstances = numRecentInstances;
    }
}