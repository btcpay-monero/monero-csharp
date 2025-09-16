namespace Monero.Common;

public abstract class MoneroConnection
{
    public static readonly ulong DefaultTimeout = 20000;
    protected readonly Dictionary<string, string?> _attributes = [];
    protected bool? _isOnline;
    protected int _priority;
    protected string? _proxyUri;
    protected ulong? _responseTime;
    protected ulong _timeoutMs;
    protected string? _uri;

    protected MoneroConnection()
    {
        _priority = 0;
        _timeoutMs = 20000;
    }

    protected MoneroConnection(string? uri)
    {
        _uri = uri;
        _priority = 0;
        _timeoutMs = 20000;
    }

    protected MoneroConnection(string? uri, string? proxyUri)
    {
        _uri = uri;
        _proxyUri = proxyUri;
        _priority = 0;
        _timeoutMs = 20000;
    }

    protected MoneroConnection(string? uri, string? proxyUri, int priority)
    {
        _uri = uri;
        _proxyUri = proxyUri;
        _priority = priority;
        _timeoutMs = 20000;
    }

    protected MoneroConnection(string? uri, string? proxyUri, int priority, ulong timeoutMs)
    {
        _uri = uri;
        _proxyUri = proxyUri;
        _priority = priority;
        _timeoutMs = timeoutMs;
    }

    protected MoneroConnection(MoneroConnection other)
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