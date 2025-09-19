namespace Monero.Daemon.Common;

public class MoneroOutputHistogramEntry
{
    private ulong? _amount;
    private ulong? _numInstances;
    private ulong? _numRecentInstances;
    private ulong? _numUnlockedInstances;

    public ulong? GetAmount()
    {
        return _amount;
    }

    public void SetAmount(ulong? amount)
    {
        _amount = amount;
    }

    public ulong? GetNumInstances()
    {
        return _numInstances;
    }

    public void SetNumInstances(ulong? numInstances)
    {
        _numInstances = numInstances;
    }

    public ulong? GetNumUnlockedInstances()
    {
        return _numUnlockedInstances;
    }

    public void SetNumUnlockedInstances(ulong? numUnlockedInstances)
    {
        _numUnlockedInstances = numUnlockedInstances;
    }

    public ulong? GetNumRecentInstances()
    {
        return _numRecentInstances;
    }

    public void SetNumRecentInstances(ulong? numRecentInstances)
    {
        _numRecentInstances = numRecentInstances;
    }
}