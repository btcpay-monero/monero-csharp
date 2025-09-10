namespace Monero.Daemon.Common;

public class MoneroFeeEstimate
{
    private ulong? fee;
    private List<ulong> fees;
    private ulong? quantizationMask;

    public MoneroFeeEstimate()
    {
        fees = [];
    }

    public MoneroFeeEstimate(ulong? fee, List<ulong> fees, ulong? quantizationMask)
    {
        this.fee = fee;
        this.fees = fees;
        this.quantizationMask = quantizationMask;
    }

    public MoneroFeeEstimate(MoneroFeeEstimate feeEstimate)
    {
        fee = feeEstimate.fee;
        fees = new List<ulong>(feeEstimate.fees);
        quantizationMask = feeEstimate.quantizationMask;
    }

    public ulong? GetFee()
    {
        return fee;
    }

    public MoneroFeeEstimate SetFee(ulong? fee)
    {
        this.fee = fee;
        return this;
    }

    public List<ulong> GetFees()
    {
        return fees;
    }

    public MoneroFeeEstimate SetFees(List<ulong> fees)
    {
        this.fees = fees;
        return this;
    }

    public ulong? GetQuantizationMask()
    {
        return quantizationMask;
    }

    public MoneroFeeEstimate SetQuantizationMask(ulong? quantizationMask)
    {
        this.quantizationMask = quantizationMask;
        return this;
    }

    public MoneroFeeEstimate Clone()
    {
        return new MoneroFeeEstimate(this);
    }
}