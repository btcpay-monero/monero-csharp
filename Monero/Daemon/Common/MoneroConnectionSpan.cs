using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroConnectionSpan
{
    [JsonPropertyName("connection_id")]
    public string? ConnectionId { get; set; }

    [JsonPropertyName("nblocks")]
    public ulong? NumBlocks { get; set; }

    [JsonPropertyName("rate")]
    public ulong? Rate { get; set; }

    [JsonPropertyName("remote_address")]
    public string? RemoteAddress { get; set; }

    [JsonPropertyName("size")]
    public ulong? Size { get; set; }

    [JsonPropertyName("speed")]
    public ulong? Speed { get; set; }

    [JsonPropertyName("start_block_height")]
    public ulong? StartHeight { get; set; }
}