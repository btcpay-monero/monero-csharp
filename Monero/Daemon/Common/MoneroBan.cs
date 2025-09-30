using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroBan
{
    [JsonPropertyName("host")]
    public string? Host { get; set; }

    [JsonPropertyName("ip")]
    public uint? Ip { get; set; } // integer formatted IP

    [JsonPropertyName("ban")]
    public bool? IsBanned { get; set; }

    [JsonPropertyName("seconds")]
    public ulong? Seconds { get; set; }
}