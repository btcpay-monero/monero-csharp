namespace Monero.Daemon.Common;

public class MoneroOutputHistogramEntry
{
    private ulong? amount;
    private ulong? numInstances;
    private ulong? numRecentInstances;
    private ulong? numUnlockedInstances;

    public ulong? GetAmount()
    {
        return amount;
    }

    public void SetAmount(ulong? amount)
    {
        this.amount = amount;
    }

    public ulong? GetNumInstances()
    {
        return numInstances;
    }

    public void SetNumInstances(ulong? numInstances)
    {
        this.numInstances = numInstances;
    }

    public ulong? GetNumUnlockedInstances()
    {
        return numUnlockedInstances;
    }

    public void SetNumUnlockedInstances(ulong? numUnlockedInstances)
    {
        this.numUnlockedInstances = numUnlockedInstances;
    }

    public ulong? GetNumRecentInstances()
    {
        return numRecentInstances;
    }

    public void SetNumRecentInstances(ulong? numRecentInstances)
    {
        this.numRecentInstances = numRecentInstances;
    }
}