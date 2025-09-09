
namespace Monero.Daemon.Common;

public class MoneroDaemonSyncInfo
{
    private ulong? height;
    private List<MoneroPeer> peers = [];
    private List<MoneroConnectionSpan> spans = [];
    private ulong? targetHeight;
    private uint? nextNeededPruningSeed;
    private string? overview;
    private ulong? credits;
    private string? topBlockHash;

    public ulong? GetHeight()
    {
        return height;
    }

    public void SetHeight(ulong? height)
    {
        this.height = height;
    }

    public List<MoneroPeer> GetPeers()
    {
        return peers;
    }

    public void SetPeers(List<MoneroPeer> peers)
    {
        this.peers = peers;
    }

    public List<MoneroConnectionSpan> GetSpans()
    {
        return spans;
    }

    public void SetSpans(List<MoneroConnectionSpan> spans)
    {
        this.spans = spans;
    }

    public ulong? GetTargetHeight()
    {
        return targetHeight;
    }

    public void SetTargetHeight(ulong? targetHeight)
    {
        this.targetHeight = targetHeight;
    }

    public uint? GetNextNeededPruningSeed()
    {
        return nextNeededPruningSeed;
    }

    public void SetNextNeededPruningSeed(uint? nextNeededPruningSeed)
    {
        this.nextNeededPruningSeed = nextNeededPruningSeed;
    }

    public string? GetOverview()
    {
        return overview;
    }

    public void SetOverview(string? overview)
    {
        this.overview = overview;
    }

    public ulong? GetCredits()
    {
        return credits;
    }

    public void SetCredits(ulong? credits)
    {
        this.credits = credits;
    }

    public string? GetTopBlockHash()
    {
        return topBlockHash;
    }

    public void SetTopBlockHash(string? topBlockHash)
    {
        this.topBlockHash = topBlockHash;
    }
}