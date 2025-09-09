using Monero.Common;

namespace Monero.Daemon.Common;

public class MoneroPeer
{
    private string? id;
    private string? address;
    private string? host;
    private int? port;
    private bool isOnline;
    private ulong? lastSeenTimestamp;
    private int? pruningSeed;
    private int? rpcPort;
    private ulong? rpcCreditsPerHash;
    private string? hash;
    private ulong? avgDownload;
    private ulong? avgUpload;
    private ulong? currentDownload;
    private ulong? currentUpload;
    private ulong? height;
    private bool isIncoming;
    private ulong? liveTime;
    private bool isLocalIp;
    private bool isLocalHost;
    private int? numReceives;
    private int? numSends;
    private ulong? receiveIdleTime;
    private ulong? sendIdleTime;
    private string? state;
    private int? numSupportFlags;
    private MoneroConnectionType? type;

    public string? GetId()
    {
        return id;
    }

    public MoneroPeer SetId(string? id)
    {
        this.id = id;
        return this;
    }

    public string? GetAddress()
    {
        return address;
    }

    public MoneroPeer SetAddress(string? address)
    {
        this.address = address;
        return this;
    }

    public string? GetHost()
    {
        return host;
    }

    public MoneroPeer SetHost(string? host)
    {
        this.host = host;
        return this;
    }

    public int? GetPort()
    {
        return port;
    }

    public MoneroPeer SetPort(int? port)
    {
        this.port = port;
        return this;
    }

    public bool IsOnline()
    {
        return isOnline;
    }

    public MoneroPeer SetIsOnline(bool isOnline)
    {
        this.isOnline = isOnline;
        return this;
    }

    public ulong? GetLastSeenTimestamp()
    {
        return lastSeenTimestamp;
    }

    public MoneroPeer SetLastSeenTimestamp(ulong? lastSeenTimestamp)
    {
        this.lastSeenTimestamp = lastSeenTimestamp;
        return this;
    }

    public int? GetPruningSeed()
    {
        return pruningSeed;
    }

    public MoneroPeer SetPruningSeed(int? pruningSeed)
    {
        this.pruningSeed = pruningSeed;
        return this;
    }

    public int? GetRpcPort()
    {
        return rpcPort;
    }

    public MoneroPeer SetRpcPort(int? rpcPort)
    {
        this.rpcPort = rpcPort;
        return this;
    }

    public ulong? GetRpcCreditsPerHash()
    {
        return this.rpcCreditsPerHash;
    }

    public MoneroPeer SetRpcCreditsPerHash(ulong? rpcCreditsPerHash)
    {
        this.rpcCreditsPerHash = rpcCreditsPerHash;
        return this;
    }

    public string? GetHash()
    {
        return hash;
    }

    public void SetHash(string? hash)
    {
        this.hash = hash;
    }

    public ulong? GetAvgDownload()
    {
        return avgDownload;
    }

    public void SetAvgDownload(ulong? avgDownload)
    {
        this.avgDownload = avgDownload;
    }

    public ulong? GetAvgUpload()
    {
        return avgUpload;
    }

    public void SetAvgUpload(ulong? avgUpload)
    {
        this.avgUpload = avgUpload;
    }

    public ulong? GetCurrentDownload()
    {
        return currentDownload;
    }

    public void SetCurrentDownload(ulong? currentDownload)
    {
        this.currentDownload = currentDownload;
    }

    public ulong? GetCurrentUpload()
    {
        return currentUpload;
    }

    public void SetCurrentUpload(ulong? currentUpload)
    {
        this.currentUpload = currentUpload;
    }

    public ulong? GetHeight()
    {
        return height;
    }

    public void SetHeight(ulong? height)
    {
        this.height = height;
    }

    public bool IsIncoming()
    {
        return isIncoming;
    }

    public void SetIsIncoming(bool isIncoming)
    {
        this.isIncoming = isIncoming;
    }

    public ulong? GetLiveTime()
    {
        return liveTime;
    }

    public void SetLiveTime(ulong? liveTime)
    {
        this.liveTime = liveTime;
    }

    public bool IsLocalIp()
    {
        return isLocalIp;
    }

    public void SetIsLocalIp(bool isLocalIp)
    {
        this.isLocalIp = isLocalIp;
    }

    public bool IsLocalHost()
    {
        return isLocalHost;
    }

    public void SetIsLocalHost(bool isLocalHost)
    {
        this.isLocalHost = isLocalHost;
    }

    public int? GetNumReceives()
    {
        return numReceives;
    }

    public void SetNumReceives(int? numReceives)
    {
        this.numReceives = numReceives;
    }

    public int? GetNumSends()
    {
        return numSends;
    }

    public void SetNumSends(int? numSends)
    {
        this.numSends = numSends;
    }

    public ulong? GetReceiveIdleTime()
    {
        return receiveIdleTime;
    }

    public void SetReceiveIdleTime(ulong? receiveIdleTime)
    {
        this.receiveIdleTime = receiveIdleTime;
    }

    public ulong? GetSendIdleTime()
    {
        return sendIdleTime;
    }

    public void SetSendIdleTime(ulong? sendIdleTime)
    {
        this.sendIdleTime = sendIdleTime;
    }

    public string? GetState()
    {
        return state;
    }

    public void SetState(string? state)
    {
        this.state = state;
    }

    public int? GetNumSupportFlags()
    {
        return numSupportFlags;
    }

    public void SetNumSupportFlags(int? numSupportFlags)
    {
        this.numSupportFlags = numSupportFlags;
    }

    public MoneroConnectionType? GetType()
    {
        return type;
    }

    public void SetType(MoneroConnectionType? type)
    {
        this.type = type;
    }
}