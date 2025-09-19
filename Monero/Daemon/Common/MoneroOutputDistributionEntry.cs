namespace Monero.Daemon.Common;

public class MoneroOutputDistributionEntry
{
    private ulong? _amount;
    private uint? _base;
    private List<uint>? _distribution;
    private ulong? _startHeight;

    public ulong? GetAmount()
    {
        return _amount;
    }

    public void SetAmount(ulong? amount)
    {
        _amount = amount;
    }

    public uint? GetBase()
    {
        return _base;
    }

    public void SetBase(uint? entryBase)
    {
        _base = entryBase;
    }

    public List<uint>? GetDistribution()
    {
        return _distribution;
    }

    public void SetDistribution(List<uint> distribution)
    {
        _distribution = distribution;
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