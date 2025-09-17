namespace Monero.Daemon.Common;

public class MoneroFeeEstimate
{
    private ulong? _fee;
    private List<ulong> _fees;
    private ulong? _quantizationMask;

    public MoneroFeeEstimate()
    {
        _fees = [];
    }

    public MoneroFeeEstimate(ulong? fee, List<ulong> fees, ulong? quantizationMask)
    {
        this._fee = fee;
        this._fees = fees;
        this._quantizationMask = quantizationMask;
    }

    public MoneroFeeEstimate(MoneroFeeEstimate feeEstimate)
    {
        _fee = feeEstimate._fee;
        _fees = feeEstimate._fees;
        _quantizationMask = feeEstimate._quantizationMask;
    }

    public ulong? GetFee()
    {
        return _fee;
    }

    public MoneroFeeEstimate SetFee(ulong? fee)
    {
        this._fee = fee;
        return this;
    }

    public List<ulong> GetFees()
    {
        return _fees;
    }

    public MoneroFeeEstimate SetFees(List<ulong> fees)
    {
        this._fees = fees;
        return this;
    }

    public ulong? GetQuantizationMask()
    {
        return _quantizationMask;
    }

    public MoneroFeeEstimate SetQuantizationMask(ulong? quantizationMask)
    {
        this._quantizationMask = quantizationMask;
        return this;
    }

    public MoneroFeeEstimate Clone()
    {
        return new MoneroFeeEstimate(this);
    }
}