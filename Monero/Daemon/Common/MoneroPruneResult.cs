namespace Monero.Daemon.Common;

public class MoneroPruneResult
{
    private bool? _isPruned;
    private int? _pruningSeed;

    public bool? IsPruned()
    {
        return _isPruned;
    }

    public void SetIsPruned(bool? isPruned)
    {
        _isPruned = isPruned;
    }

    public int? GetPruningSeed()
    {
        return _pruningSeed;
    }

    public void SetPruningSeed(int? pruningSeed)
    {
        _pruningSeed = pruningSeed;
    }
}