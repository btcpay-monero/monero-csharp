using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroPeer
{
    [JsonPropertyName("address")]
    [JsonInclude]
    private string? _address { get; set; }
    [JsonPropertyName("avg_download")]
    [JsonInclude]
    private ulong? _avgDownload { get; set; }
    [JsonPropertyName("avg_upload")]
    [JsonInclude]
    private ulong? _avgUpload { get; set; }
    [JsonPropertyName("current_download")]
    [JsonInclude]
    private ulong? _currentDownload { get; set; }
    [JsonPropertyName("current_upload")]
    [JsonInclude]
    private ulong? _currentUpload { get; set; }
    [JsonPropertyName("connection_id")]
    [JsonInclude]
    private string? _hash { get; set; }
    [JsonPropertyName("height")]
    [JsonInclude]
    private ulong? _height { get; set; }
    [JsonPropertyName("host")]
    [JsonInclude]
    private string? _host { get; set; }
    [JsonPropertyName("id")]
    [JsonInclude]
    private string? _id { get; set; }
    [JsonPropertyName("peer_id")]
    [JsonInclude]
    private string? _peerId { get; set; }
    [JsonPropertyName("incoming")]
    [JsonInclude]
    private bool _isIncoming { get; set; }
    [JsonPropertyName("localhost")]
    [JsonInclude]
    private bool _isLocalHost { get; set; }
    [JsonPropertyName("local_ip")]
    [JsonInclude]
    private bool _isLocalIp { get; set; }
    [JsonPropertyName("online")]
    [JsonInclude]
    private bool _isOnline { get; set; }
    [JsonPropertyName("last_seen")]
    [JsonInclude]
    private ulong? _lastSeenTimestamp { get; set; }
    [JsonPropertyName("live_time")]
    [JsonInclude]
    private ulong? _liveTime { get; set; }
    [JsonPropertyName("recv_count")]
    [JsonInclude]
    private int? _numReceives { get; set; }
    [JsonPropertyName("send_count")]
    [JsonInclude]
    private int? _numSends { get; set; }
    [JsonPropertyName("support_flags")]
    [JsonInclude]
    private int? _numSupportFlags { get; set; }
    [JsonPropertyName("port")]
    [JsonInclude]
    private object? _port { get; set; }
    [JsonPropertyName("pruning_seed")]
    [JsonInclude]
    private int? _pruningSeed { get; set; }
    [JsonPropertyName("recv_idle_time")]
    [JsonInclude]
    private ulong? _receiveIdleTime { get; set; }
    [JsonPropertyName("rpc_credits_per_hash")]
    [JsonInclude]
    private ulong? _rpcCreditsPerHash { get; set; }
    [JsonPropertyName("rpc_port")]
    [JsonInclude]
    private int? _rpcPort { get; set; }
    [JsonPropertyName("send_idle_time")]
    [JsonInclude]
    private ulong? _sendIdleTime { get; set; }
    [JsonPropertyName("state")]
    [JsonInclude]
    private string? _state { get; set; }

    public string? GetId()
    {
        return _id;
    }

    public MoneroPeer SetId(string? id)
    {
        _id = id;
        return this;
    }

    public MoneroPeer SetPeerId(string? id)
    {
        _peerId = id;
        return this;
    }

    public string? GetPeerId()
    {
        return _peerId;
    }

    public string? GetAddress()
    {
        return _address;
    }

    public MoneroPeer SetAddress(string? address)
    {
        _address = address;
        return this;
    }

    public string? GetHost()
    {
        return _host;
    }

    public MoneroPeer SetHost(string? host)
    {
        _host = host;
        return this;
    }

    public int? GetPort()
    {
        if (_port is string p)
        {
            return Convert.ToInt32(_port);
        }
        if (_port is JsonElement r)
        {
            return Convert.ToInt32(_port.ToString());
        }
        return (int?)_port;
    }

    public MoneroPeer SetPort(int? port)
    {
        _port = port;
        return this;
    }

    public bool IsOnline()
    {
        return _isOnline;
    }

    public MoneroPeer SetIsOnline(bool isOnline)
    {
        _isOnline = isOnline;
        return this;
    }

    public ulong? GetLastSeenTimestamp()
    {
        return _lastSeenTimestamp;
    }

    public MoneroPeer SetLastSeenTimestamp(ulong? lastSeenTimestamp)
    {
        _lastSeenTimestamp = lastSeenTimestamp;
        return this;
    }

    public int? GetPruningSeed()
    {
        return _pruningSeed;
    }

    public MoneroPeer SetPruningSeed(int? pruningSeed)
    {
        _pruningSeed = pruningSeed;
        return this;
    }

    public int? GetRpcPort()
    {
        return _rpcPort;
    }

    public MoneroPeer SetRpcPort(int? rpcPort)
    {
        _rpcPort = rpcPort;
        return this;
    }

    public ulong? GetRpcCreditsPerHash()
    {
        return _rpcCreditsPerHash;
    }

    public MoneroPeer SetRpcCreditsPerHash(ulong? rpcCreditsPerHash)
    {
        _rpcCreditsPerHash = rpcCreditsPerHash;
        return this;
    }

    public string? GetHash()
    {
        return _hash;
    }

    public void SetHash(string? hash)
    {
        _hash = hash;
    }

    public ulong? GetAvgDownload()
    {
        return _avgDownload;
    }

    public void SetAvgDownload(ulong? avgDownload)
    {
        _avgDownload = avgDownload;
    }

    public ulong? GetAvgUpload()
    {
        return _avgUpload;
    }

    public void SetAvgUpload(ulong? avgUpload)
    {
        _avgUpload = avgUpload;
    }

    public ulong? GetCurrentDownload()
    {
        return _currentDownload;
    }

    public void SetCurrentDownload(ulong? currentDownload)
    {
        _currentDownload = currentDownload;
    }

    public ulong? GetCurrentUpload()
    {
        return _currentUpload;
    }

    public void SetCurrentUpload(ulong? currentUpload)
    {
        _currentUpload = currentUpload;
    }

    public ulong? GetHeight()
    {
        return _height;
    }

    public void SetHeight(ulong? height)
    {
        _height = height;
    }

    public bool IsIncoming()
    {
        return _isIncoming;
    }

    public void SetIsIncoming(bool isIncoming)
    {
        _isIncoming = isIncoming;
    }

    public ulong? GetLiveTime()
    {
        return _liveTime;
    }

    public void SetLiveTime(ulong? liveTime)
    {
        _liveTime = liveTime;
    }

    public bool IsLocalIp()
    {
        return _isLocalIp;
    }

    public void SetIsLocalIp(bool isLocalIp)
    {
        _isLocalIp = isLocalIp;
    }

    public bool IsLocalHost()
    {
        return _isLocalHost;
    }

    public void SetIsLocalHost(bool isLocalHost)
    {
        _isLocalHost = isLocalHost;
    }

    public int? GetNumReceives()
    {
        return _numReceives;
    }

    public void SetNumReceives(int? numReceives)
    {
        _numReceives = numReceives;
    }

    public int? GetNumSends()
    {
        return _numSends;
    }

    public void SetNumSends(int? numSends)
    {
        _numSends = numSends;
    }

    public ulong? GetReceiveIdleTime()
    {
        return _receiveIdleTime;
    }

    public void SetReceiveIdleTime(ulong? receiveIdleTime)
    {
        _receiveIdleTime = receiveIdleTime;
    }

    public ulong? GetSendIdleTime()
    {
        return _sendIdleTime;
    }

    public void SetSendIdleTime(ulong? sendIdleTime)
    {
        _sendIdleTime = sendIdleTime;
    }

    public string? GetState()
    {
        return _state;
    }

    public void SetState(string? state)
    {
        _state = state;
    }

    public int? GetNumSupportFlags()
    {
        return _numSupportFlags;
    }

    public void SetNumSupportFlags(int? numSupportFlags)
    {
        _numSupportFlags = numSupportFlags;
    }

}