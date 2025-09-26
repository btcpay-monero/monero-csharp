using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroConnectionSpan
{
    [JsonPropertyName("connection_id")]
    [JsonInclude]
    private string? _connectionId { get; set; }
    [JsonPropertyName("nblocks")]
    [JsonInclude]
    private ulong? _numBlocks { get; set; }
    [JsonPropertyName("rate")]
    [JsonInclude]
    private ulong? _rate { get; set; }
    [JsonPropertyName("remote_address")]
    [JsonInclude]
    private string? _remoteAddress { get; set; }
    [JsonPropertyName("size")]
    [JsonInclude]
    private ulong? _size { get; set; }
    [JsonPropertyName("speed")]
    [JsonInclude]
    private ulong? _speed { get; set; }
    [JsonPropertyName("start_block_height")]
    [JsonInclude]
    private ulong? _startHeight { get; set; }

    public string? GetConnectionId()
    {
        return _connectionId;
    }

    public void SetConnectionId(string? connectionId)
    {
        _connectionId = connectionId;
    }

    public ulong? GetNumBlocks()
    {
        return _numBlocks;
    }

    public void SetNumBlocks(ulong? numBlocks)
    {
        _numBlocks = numBlocks;
    }

    public string? GetRemoteAddress()
    {
        return _remoteAddress;
    }

    public void SetRemoteAddress(string? remoteAddress)
    {
        _remoteAddress = remoteAddress;
    }

    public ulong? GetRate()
    {
        return _rate;
    }

    public void SetRate(ulong? rate)
    {
        _rate = rate;
    }

    public ulong? GetSpeed()
    {
        return _speed;
    }

    public void SetSpeed(ulong? speed)
    {
        _speed = speed;
    }

    public ulong? GetSize()
    {
        return _size;
    }

    public void SetSize(ulong? size)
    {
        _size = size;
    }

    public ulong? GetStartHeight()
    {
        return _startHeight;
    }

    public void SetStartHeight(ulong? startHeight)
    {
        _startHeight = startHeight;
    }
}