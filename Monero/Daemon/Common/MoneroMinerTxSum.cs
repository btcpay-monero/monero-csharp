namespace Monero.Daemon.Common;

public class MoneroMinerTxSum
{
    private ulong? emissionSum;
    private ulong? feeSum;

    public ulong? GetEmissionSum()
    {
        return emissionSum;
    }

    public void SetEmissionSum(ulong? emissionSum)
    {
        this.emissionSum = emissionSum;
    }

    public ulong? GetFeeSum()
    {
        return feeSum;
    }

    public void SetFeeSum(ulong? feeSum)
    {
        this.feeSum = feeSum;
    }
}