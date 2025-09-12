namespace Monero.Common;

public abstract class MoneroConnection
{
    public static readonly ulong DefaultTimeout = 2000;
    protected Dictionary<string, string?> _attributes = [];
    protected bool? _isOnline;
    protected int _priority;
    protected string? _proxyUri;
    protected ulong? _responseTime;
    protected ulong _timeoutMs;
    protected string? _uri;

    public MoneroConnection(string? uri = null, string? proxyUri = null, int priority = 0, ulong timeoutMs = 2000)
    {
        _uri = uri;
        _proxyUri = proxyUri;
        _priority = priority;
        _timeoutMs = timeoutMs;
    }

    public MoneroConnection(MoneroConnection other)
    {
        _uri = other._uri;
        _proxyUri = other._proxyUri;
        _priority = other._priority;
        _timeoutMs = other._timeoutMs;
        _isOnline = other._isOnline;
        _responseTime = other._responseTime;
    }

    public abstract bool? IsAuthenticated();

    public virtual bool? IsOnline()
    {
        return _isOnline;
    }

    public string? GetAttribute(string key)
    {
        return _attributes.GetValueOrDefault(key, null);
    }

    public virtual MoneroConnection SetAttribute(string key, string value)
    {
        _attributes[key] = value;
        return this;
    }

    public virtual string? GetUri()
    {
        return _uri;
    }

    public virtual MoneroConnection SetUri(string? uri)
    {
        _uri = uri;
        return this;
    }

    public string? GetProxyUri()
    {
        return _proxyUri;
    }

    public virtual MoneroConnection SetProxyUri(string? uri)
    {
        _proxyUri = uri;
        return this;
    }

    public int GetPriority()
    {
        return _priority;
    }

    public virtual MoneroConnection SetPriority(int priority)
    {
        _priority = priority;
        return this;
    }

    public ulong GetTimeout() { return _timeoutMs; }

    public MoneroConnection SetTimeout(ulong timeoutMs)
    {
        _timeoutMs = timeoutMs;
        return this;
    }

    public ulong? GetResponseTime()
    {
        return _responseTime;
    }

    public abstract bool? IsConnected();

    public abstract bool CheckConnection(ulong timeoutMs);

    public abstract MoneroConnection Clone();
}