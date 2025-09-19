namespace Monero.Daemon.Common;

public class MoneroRpcPaymentInfo
{
    protected ulong? _credits;
    protected string? _topBlockHash;

    public ulong? GetCredits()
    {
        return _credits;
    }

    public void SetCredits(ulong? credits)
    {
        this._credits = credits;
    }

    public string? GetTopBlockHash()
    {
        return _topBlockHash;
    }

    public void SetTopBlockHash(string? topBlockHash)
    {
        this._topBlockHash = topBlockHash;
    }
}