namespace Monero.Daemon.Common;

public class MoneroConnectionSpan
{
    private string? _connectionId;
    private ulong? _numBlocks;
    private ulong? _rate;
    private string? _remoteAddress;
    private ulong? _size;
    private ulong? _speed;
    private ulong? _startHeight;

    public string? GetConnectionId()
    {
        return _connectionId;
    }

    public void SetConnectionId(string? connectionId)
    {
        this._connectionId = connectionId;
    }

    public ulong? GetNumBlocks()
    {
        return _numBlocks;
    }

    public void SetNumBlocks(ulong? numBlocks)
    {
        this._numBlocks = numBlocks;
    }

    public string? GetRemoteAddress()
    {
        return _remoteAddress;
    }

    public void SetRemoteAddress(string? remoteAddress)
    {
        this._remoteAddress = remoteAddress;
    }

    public ulong? GetRate()
    {
        return _rate;
    }

    public void SetRate(ulong? rate)
    {
        this._rate = rate;
    }

    public ulong? GetSpeed()
    {
        return _speed;
    }

    public void SetSpeed(ulong? speed)
    {
        this._speed = speed;
    }

    public ulong? GetSize()
    {
        return _size;
    }

    public void SetSize(ulong? size)
    {
        this._size = size;
    }

    public ulong? GetStartHeight()
    {
        return _startHeight;
    }

    public void SetStartHeight(ulong? startHeight)
    {
        this._startHeight = startHeight;
    }
}