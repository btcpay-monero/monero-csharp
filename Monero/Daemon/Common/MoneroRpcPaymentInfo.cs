using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroRpcPaymentInfo : MoneroRpcResponse
{
    [JsonPropertyName("credits")]
    [JsonInclude]
    protected ulong? _credits { get; set; }
    [JsonPropertyName("top_block_hash")]
    [JsonInclude]
    protected string? _topBlockHash { get; set; }

    public ulong? GetCredits()
    {
        return _credits;
    }

    public void SetCredits(ulong? credits)
    {
        _credits = credits;
    }

    public string? GetTopBlockHash()
    {
        return _topBlockHash;
    }

    public void SetTopBlockHash(string? topBlockHash)
    {
        _topBlockHash = topBlockHash;
    }
}