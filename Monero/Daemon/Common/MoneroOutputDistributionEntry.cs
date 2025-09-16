namespace Monero.Daemon.Common;

public class MoneroOutputDistributionEntry
{
    private uint? _base;
    private ulong? _amount;
    private List<uint>? _distribution;
    private ulong? _startHeight;

    public ulong? GetAmount()
    {
        return _amount;
    }

    public void SetAmount(ulong? amount)
    {
        this._amount = amount;
    }

    public uint? GetBase()
    {
        return _base;
    }

    public void SetBase(uint? entryBase)
    {
        this._base = entryBase;
    }

    public List<uint>? GetDistribution()
    {
        return _distribution;
    }

    public void SetDistribution(List<uint> distribution)
    {
        this._distribution = distribution;
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