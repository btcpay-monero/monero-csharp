using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroPeer
{
    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("avg_download")]
    public ulong? AvgDownload { get; set; }

    [JsonPropertyName("avg_upload")]
    public ulong? AvgUpload { get; set; }

    [JsonPropertyName("current_download")]
    public ulong? CurrentDownload { get; set; }

    [JsonPropertyName("current_upload")]
    public ulong? CurrentUpload { get; set; }

    [JsonPropertyName("connection_id")]
    public string? Hash { get; set; }

    [JsonPropertyName("height")]
    public ulong? Height { get; set; }

    [JsonPropertyName("host")]
    public string? Host { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("peer_id")]
    public string? PeerId { get; set; }

    [JsonPropertyName("incoming")]
    public bool IsIncoming { get; set; }

    [JsonPropertyName("localhost")]
    public bool IsLocalHost { get; set; }

    [JsonPropertyName("local_ip")]
    public bool IsLocalIp { get; set; }

    [JsonPropertyName("online")]
    public bool IsOnline { get; set; }

    [JsonPropertyName("last_seen")]
    public ulong? LastSeenTimestamp { get; set; }

    [JsonPropertyName("live_time")]
    public ulong? LiveTime { get; set; }

    [JsonPropertyName("recv_count")]
    public int? NumReceives { get; set; }

    [JsonPropertyName("send_count")]
    public int? NumSends { get; set; }

    [JsonPropertyName("support_flags")]
    public int? NumSupportFlags { get; set; }

    [JsonPropertyName("port")]
    public string? Port { get; set; }

    [JsonPropertyName("pruning_seed")]
    public int? PruningSeed { get; set; }

    [JsonPropertyName("recv_idle_time")]
    public ulong? ReceiveIdleTime { get; set; }

    [JsonPropertyName("rpc_credits_per_hash")]
    public ulong? RpcCreditsPerHash { get; set; }

    [JsonPropertyName("rpc_port")]
    public int? RpcPort { get; set; }

    [JsonPropertyName("send_idle_time")]
    public ulong? SendIdleTime { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }
}