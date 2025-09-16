using Monero.Common;

namespace Monero.Daemon.Common;

public class MoneroPeer
{
    private string? _address;
    private ulong? _avgDownload;
    private ulong? _avgUpload;
    private ulong? _currentDownload;
    private ulong? _currentUpload;
    private string? _hash;
    private ulong? _height;
    private string? _host;
    private string? _id;
    private bool _isIncoming;
    private bool _isLocalHost;
    private bool _isLocalIp;
    private bool _isOnline;
    private ulong? _lastSeenTimestamp;
    private ulong? _liveTime;
    private int? _numReceives;
    private int? _numSends;
    private int? _numSupportFlags;
    private int? _port;
    private int? _pruningSeed;
    private ulong? _receiveIdleTime;
    private ulong? _rpcCreditsPerHash;
    private int? _rpcPort;
    private ulong? _sendIdleTime;
    private string? _state;
    private MoneroConnectionType? _type;

    public string? GetId()
    {
        return _id;
    }

    public MoneroPeer SetId(string? id)
    {
        this._id = id;
        return this;
    }

    public string? GetAddress()
    {
        return _address;
    }

    public MoneroPeer SetAddress(string? address)
    {
        this._address = address;
        return this;
    }

    public string? GetHost()
    {
        return _host;
    }

    public MoneroPeer SetHost(string? host)
    {
        this._host = host;
        return this;
    }

    public int? GetPort()
    {
        return _port;
    }

    public MoneroPeer SetPort(int? port)
    {
        this._port = port;
        return this;
    }

    public bool IsOnline()
    {
        return _isOnline;
    }

    public MoneroPeer SetIsOnline(bool isOnline)
    {
        this._isOnline = isOnline;
        return this;
    }

    public ulong? GetLastSeenTimestamp()
    {
        return _lastSeenTimestamp;
    }

    public MoneroPeer SetLastSeenTimestamp(ulong? lastSeenTimestamp)
    {
        this._lastSeenTimestamp = lastSeenTimestamp;
        return this;
    }

    public int? GetPruningSeed()
    {
        return _pruningSeed;
    }

    public MoneroPeer SetPruningSeed(int? pruningSeed)
    {
        this._pruningSeed = pruningSeed;
        return this;
    }

    public int? GetRpcPort()
    {
        return _rpcPort;
    }

    public MoneroPeer SetRpcPort(int? rpcPort)
    {
        this._rpcPort = rpcPort;
        return this;
    }

    public ulong? GetRpcCreditsPerHash()
    {
        return _rpcCreditsPerHash;
    }

    public MoneroPeer SetRpcCreditsPerHash(ulong? rpcCreditsPerHash)
    {
        this._rpcCreditsPerHash = rpcCreditsPerHash;
        return this;
    }

    public string? GetHash()
    {
        return _hash;
    }

    public void SetHash(string? hash)
    {
        this._hash = hash;
    }

    public ulong? GetAvgDownload()
    {
        return _avgDownload;
    }

    public void SetAvgDownload(ulong? avgDownload)
    {
        this._avgDownload = avgDownload;
    }

    public ulong? GetAvgUpload()
    {
        return _avgUpload;
    }

    public void SetAvgUpload(ulong? avgUpload)
    {
        this._avgUpload = avgUpload;
    }

    public ulong? GetCurrentDownload()
    {
        return _currentDownload;
    }

    public void SetCurrentDownload(ulong? currentDownload)
    {
        this._currentDownload = currentDownload;
    }

    public ulong? GetCurrentUpload()
    {
        return _currentUpload;
    }

    public void SetCurrentUpload(ulong? currentUpload)
    {
        this._currentUpload = currentUpload;
    }

    public ulong? GetHeight()
    {
        return _height;
    }

    public void SetHeight(ulong? height)
    {
        this._height = height;
    }

    public bool IsIncoming()
    {
        return _isIncoming;
    }

    public void SetIsIncoming(bool isIncoming)
    {
        this._isIncoming = isIncoming;
    }

    public ulong? GetLiveTime()
    {
        return _liveTime;
    }

    public void SetLiveTime(ulong? liveTime)
    {
        this._liveTime = liveTime;
    }

    public bool IsLocalIp()
    {
        return _isLocalIp;
    }

    public void SetIsLocalIp(bool isLocalIp)
    {
        this._isLocalIp = isLocalIp;
    }

    public bool IsLocalHost()
    {
        return _isLocalHost;
    }

    public void SetIsLocalHost(bool isLocalHost)
    {
        this._isLocalHost = isLocalHost;
    }

    public int? GetNumReceives()
    {
        return _numReceives;
    }

    public void SetNumReceives(int? numReceives)
    {
        this._numReceives = numReceives;
    }

    public int? GetNumSends()
    {
        return _numSends;
    }

    public void SetNumSends(int? numSends)
    {
        this._numSends = numSends;
    }

    public ulong? GetReceiveIdleTime()
    {
        return _receiveIdleTime;
    }

    public void SetReceiveIdleTime(ulong? receiveIdleTime)
    {
        this._receiveIdleTime = receiveIdleTime;
    }

    public ulong? GetSendIdleTime()
    {
        return _sendIdleTime;
    }

    public void SetSendIdleTime(ulong? sendIdleTime)
    {
        this._sendIdleTime = sendIdleTime;
    }

    public string? GetState()
    {
        return _state;
    }

    public void SetState(string? state)
    {
        this._state = state;
    }

    public int? GetNumSupportFlags()
    {
        return _numSupportFlags;
    }

    public void SetNumSupportFlags(int? numSupportFlags)
    {
        this._numSupportFlags = numSupportFlags;
    }

    public MoneroConnectionType? GetConnectionType()
    {
        return _type;
    }

    public void SetConnectionType(MoneroConnectionType? type)
    {
        this._type = type;
    }
}