namespace Monero.Daemon.Common;

public class MoneroConnectionSpan
{
    private string? connectionId;
    private ulong? numBlocks;
    private ulong? rate;
    private string? remoteAddress;
    private ulong? size;
    private ulong? speed;
    private ulong? startHeight;

    public string? GetConnectionId()
    {
        return connectionId;
    }

    public void SetConnectionId(string? connectionId)
    {
        this.connectionId = connectionId;
    }

    public ulong? GetNumBlocks()
    {
        return numBlocks;
    }

    public void SetNumBlocks(ulong? numBlocks)
    {
        this.numBlocks = numBlocks;
    }

    public string? GetRemoteAddress()
    {
        return remoteAddress;
    }

    public void SetRemoteAddress(string? remoteAddress)
    {
        this.remoteAddress = remoteAddress;
    }

    public ulong? GetRate()
    {
        return rate;
    }

    public void SetRate(ulong? rate)
    {
        this.rate = rate;
    }

    public ulong? GetSpeed()
    {
        return speed;
    }

    public void SetSpeed(ulong? speed)
    {
        this.speed = speed;
    }

    public ulong? GetSize()
    {
        return size;
    }

    public void SetSize(ulong? size)
    {
        this.size = size;
    }

    public ulong? GetStartHeight()
    {
        return startHeight;
    }

    public void SetStartHeight(ulong? startHeight)
    {
        this.startHeight = startHeight;
    }
}