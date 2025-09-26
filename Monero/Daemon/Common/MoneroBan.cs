using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroBan
{
    [JsonPropertyName("host")]
    [JsonInclude]
    private string? _host { get; set; }
    [JsonPropertyName("ip")]
    [JsonInclude]
    private uint? _ip { get; set; } // integer formatted IP
    [JsonPropertyName("ban")]
    [JsonInclude]
    private bool? _isBanned { get; set; }
    [JsonPropertyName("seconds")]
    [JsonInclude]
    private ulong? _seconds { get; set; }

    public string? GetHost()
    {
        return _host;
    }

    public MoneroBan SetHost(string? host)
    {
        _host = host;
        return this;
    }

    public uint? GetIp()
    {
        return _ip;
    }

    public MoneroBan SetIp(uint? ip)
    {
        _ip = ip;
        return this;
    }

    public bool? IsBanned()
    {
        return _isBanned;
    }

    public MoneroBan SetIsBanned(bool? isBanned)
    {
        _isBanned = isBanned;
        return this;
    }

    public ulong? GetSeconds()
    {
        return _seconds;
    }

    public MoneroBan SetSeconds(ulong? seconds)
    {
        _seconds = seconds;
        return this;
    }
}