
namespace Monero.Daemon.Common;

public class MoneroPruneResult
{
    private bool? isPruned;
    private int? pruningSeed;

    public MoneroPruneResult()
    {
        // nothing to construct
    }

    public bool? IsPruned()
    {
        return isPruned;
    }

    public void SetIsPruned(bool? isPruned)
    {
        this.isPruned = isPruned;
    }

    public int? GetPruningSeed()
    {
        return pruningSeed;
    }

    public void SetPruningSeed(int? pruningSeed)
    {
        this.pruningSeed = pruningSeed;
    }
}