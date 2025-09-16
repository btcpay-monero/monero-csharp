namespace Monero.Daemon.Common;

public class MoneroMinerTxSum
{
    private ulong? _emissionSum;
    private ulong? _feeSum;

    public ulong? GetEmissionSum()
    {
        return _emissionSum;
    }

    public void SetEmissionSum(ulong? emissionSum)
    {
        this._emissionSum = emissionSum;
    }

    public ulong? GetFeeSum()
    {
        return _feeSum;
    }

    public void SetFeeSum(ulong? feeSum)
    {
        this._feeSum = feeSum;
    }
}