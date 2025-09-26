using System.Text.Json.Serialization;

using Monero.Daemon.Rpc;

namespace Monero.Daemon.Common;

public class MoneroDaemonSyncInfo : MoneroRpcPaymentInfo
{
    [JsonPropertyName("height")]
    [JsonInclude]
    private ulong? _height { get; set; }
    [JsonPropertyName("next_needed_pruning_seed")]
    [JsonInclude]
    private uint? _nextNeededPruningSeed { get; set; }

    [JsonPropertyName("overview")]
    [JsonInclude]
    private string? _overview { get; set; } = "";
    [JsonPropertyName("peers")]
    [JsonInclude]
    private List<MoneroPeerInfo>? _peers { get; set; }
    [JsonPropertyName("spans")]
    [JsonInclude]
    private List<MoneroConnectionSpan>? _spans { get; set; }
    [JsonPropertyName("target_height")]
    [JsonInclude]
    private ulong? _targetHeight { get; set; }

    public ulong? GetHeight()
    {
        return _height;
    }

    public void SetHeight(ulong? height)
    {
        _height = height;
    }

    public List<MoneroPeerInfo>? GetPeers()
    {
        return _peers;
    }

    public void SetPeers(List<MoneroPeerInfo>? peers)
    {
        _peers = peers;
    }

    public List<MoneroConnectionSpan>? GetSpans()
    {
        return _spans;
    }

    public void SetSpans(List<MoneroConnectionSpan>? spans)
    {
        _spans = spans;
    }

    public ulong? GetTargetHeight()
    {
        return _targetHeight;
    }

    public void SetTargetHeight(ulong? targetHeight)
    {
        _targetHeight = targetHeight;
    }

    public uint? GetNextNeededPruningSeed()
    {
        return _nextNeededPruningSeed;
    }

    public void SetNextNeededPruningSeed(uint? nextNeededPruningSeed)
    {
        _nextNeededPruningSeed = nextNeededPruningSeed;
    }

    public string? GetOverview()
    {
        return _overview;
    }

    public void SetOverview(string? overview)
    {
        _overview = overview;
    }
}