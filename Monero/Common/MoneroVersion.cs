using System.Text.Json.Serialization;

using Monero.Daemon.Common;

namespace Monero.Common;

public class MoneroVersion : MoneroRpcResponse
{
    [JsonPropertyName("release")]
    [JsonInclude]
    private bool? _isRelease { get; set; }
    [JsonPropertyName("version")]
    [JsonInclude]
    private long? _number { get; set; }

    public MoneroVersion()
    {
    }

    public MoneroVersion(long? number)
    {
        _number = number;
    }

    public MoneroVersion(long? number, bool? isRelease)
    {
        _number = number;
        _isRelease = isRelease;
    }

    public long? GetNumber() { return _number; }
    public bool? IsRelease() { return _isRelease; }

    public MoneroVersion SetNumber(long? number)
    {
        _number = number;
        return this;
    }

    public MoneroVersion SetIsRelease(bool? isRelease)
    {
        _isRelease = isRelease;
        return this;
    }
}