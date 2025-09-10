namespace Monero.Daemon.Common;

public class MoneroOutputDistributionEntry
{
    private uint? _base;
    private ulong? amount;
    private List<uint>? distribution;
    private ulong? startHeight;

    public ulong? GetAmount()
    {
        return amount;
    }

    public void SetAmount(ulong? amount)
    {
        this.amount = amount;
    }

    public uint? GetBase()
    {
        return _base;
    }

    public void SetBase(uint? _base)
    {
        this._base = _base;
    }

    public List<uint>? GetDistribution()
    {
        return distribution;
    }

    public void SetDistribution(List<uint> distribution)
    {
        this.distribution = distribution;
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