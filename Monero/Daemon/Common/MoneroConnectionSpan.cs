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
        _connectionId = connectionId;
    }

    public ulong? GetNumBlocks()
    {
        return _numBlocks;
    }

    public void SetNumBlocks(ulong? numBlocks)
    {
        _numBlocks = numBlocks;
    }

    public string? GetRemoteAddress()
    {
        return _remoteAddress;
    }

    public void SetRemoteAddress(string? remoteAddress)
    {
        _remoteAddress = remoteAddress;
    }

    public ulong? GetRate()
    {
        return _rate;
    }

    public void SetRate(ulong? rate)
    {
        _rate = rate;
    }

    public ulong? GetSpeed()
    {
        return _speed;
    }

    public void SetSpeed(ulong? speed)
    {
        _speed = speed;
    }

    public ulong? GetSize()
    {
        return _size;
    }

    public void SetSize(ulong? size)
    {
        _size = size;
    }

    public ulong? GetStartHeight()
    {
        return _startHeight;
    }

    public void SetStartHeight(ulong? startHeight)
    {
        _startHeight = startHeight;
    }
}