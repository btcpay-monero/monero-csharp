namespace Monero.Daemon.Common;

public class MoneroDaemonSyncInfo
{
    private ulong? _credits;
    private ulong? _height;
    private uint? _nextNeededPruningSeed;
    private string? _overview;
    private List<MoneroPeer>? _peers;
    private List<MoneroConnectionSpan>? _spans;
    private ulong? _targetHeight;
    private string? _topBlockHash;

    public ulong? GetHeight()
    {
        return _height;
    }

    public void SetHeight(ulong? height)
    {
        this._height = height;
    }

    public List<MoneroPeer>? GetPeers()
    {
        return _peers;
    }

    public void SetPeers(List<MoneroPeer>? peers)
    {
        this._peers = peers;
    }

    public List<MoneroConnectionSpan>? GetSpans()
    {
        return _spans;
    }

    public void SetSpans(List<MoneroConnectionSpan>? spans)
    {
        this._spans = spans;
    }

    public ulong? GetTargetHeight()
    {
        return _targetHeight;
    }

    public void SetTargetHeight(ulong? targetHeight)
    {
        this._targetHeight = targetHeight;
    }

    public uint? GetNextNeededPruningSeed()
    {
        return _nextNeededPruningSeed;
    }

    public void SetNextNeededPruningSeed(uint? nextNeededPruningSeed)
    {
        this._nextNeededPruningSeed = nextNeededPruningSeed;
    }

    public string? GetOverview()
    {
        return _overview;
    }

    public void SetOverview(string? overview)
    {
        this._overview = overview;
    }

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