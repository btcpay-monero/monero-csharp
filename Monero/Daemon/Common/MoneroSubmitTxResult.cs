
namespace Monero.Daemon.Common;

public class MoneroSubmitTxResult
{
    private bool? isGood;
    private bool? isRelayed;
    private bool? isDoubleSpend;
    private bool? isFeeTooLow;
    private bool? isMixinTooLow;
    private bool? hasInvalidInput;
    private bool? hasInvalidOutput;
    private bool? hasTooFewOutputs;
    private bool? isOverspend;
    private bool? isTooBig;
    private bool? sanityCheckFailed;
    private string? reason;
    private ulong? credits;
    private string? topBlockHash;
    private bool? isTxExtraTooBig;
    private bool? isNonzeroUnlockTime;

    public bool? IsGood()
    {
        return isGood;
    }

    public MoneroSubmitTxResult SetIsGood(bool? isGood)
    {
        this.isGood = isGood;
        return this;
    }

    public bool? IsRelayed()
    {
        return isRelayed;
    }

    public MoneroSubmitTxResult SetIsRelayed(bool? isRelayed)
    {
        this.isRelayed = isRelayed;
        return this;
    }

    public bool? IsDoubleSpend()
    {
        return isDoubleSpend;
    }

    public MoneroSubmitTxResult SetIsDoubleSpend(bool? isDoubleSpend)
    {
        this.isDoubleSpend = isDoubleSpend;
        return this;
    }

    public bool? IsFeeTooLow()
    {
        return isFeeTooLow;
    }

    public MoneroSubmitTxResult SetIsFeeTooLow(bool? isFeeTooLow)
    {
        this.isFeeTooLow = isFeeTooLow;
        return this;
    }

    public bool? IsMixinTooLow()
    {
        return isMixinTooLow;
    }

    public MoneroSubmitTxResult SetIsMixinTooLow(bool? isMixinTooLow)
    {
        this.isMixinTooLow = isMixinTooLow;
        return this;
    }

    public bool? HasInvalidInput()
    {
        return hasInvalidInput;
    }

    public MoneroSubmitTxResult SetHasInvalidInput(bool? hasInvalidInput)
    {
        this.hasInvalidInput = hasInvalidInput;
        return this;
    }

    public bool? HasInvalidOutput()
    {
        return hasInvalidOutput;
    }

    public MoneroSubmitTxResult SetHasInvalidOutput(bool? hasInvalidOutput)
    {
        this.hasInvalidOutput = hasInvalidOutput;
        return this;
    }

    public bool? HasTooFewOutputs()
    {
        return hasTooFewOutputs;
    }

    public MoneroSubmitTxResult SetHasTooFewOutputs(bool? hasTooFewOutputs)
    {
        this.hasTooFewOutputs = hasTooFewOutputs;
        return this;
    }

    public bool? IsOverspend()
    {
        return isOverspend;
    }

    public MoneroSubmitTxResult SetIsOverspend(bool? isOverspend)
    {
        this.isOverspend = isOverspend;
        return this;
    }

    public bool? IsTooBig()
    {
        return isTooBig;
    }

    public MoneroSubmitTxResult SetIsTooBig(bool? isTooBig)
    {
        this.isTooBig = isTooBig;
        return this;
    }

    public bool? GetSanityCheckFailed()
    {
        return sanityCheckFailed;
    }

    public MoneroSubmitTxResult SetSanityCheckFailed(bool? sanityCheckFailed)
    {
        this.sanityCheckFailed = sanityCheckFailed;
        return this;
    }

    public string? GetReason()
    {
        return reason;
    }

    public MoneroSubmitTxResult SetReason(string? reason)
    {
        this.reason = reason;
        return this;
    }

    public ulong? GetCredits()
    {
        return credits;
    }

    public MoneroSubmitTxResult SetCredits(ulong? credits)
    {
        this.credits = credits;
        return this;
    }

    public string? GetTopBlockHash()
    {
        return topBlockHash;
    }

    public MoneroSubmitTxResult SetTopBlockHash(string? topBlockHash)
    {
        this.topBlockHash = topBlockHash;
        return this;
    }

    public bool? IsTxExtraTooBig()
    {
        return isTxExtraTooBig;
    }

    public MoneroSubmitTxResult SetIsTxExtraTooBig(bool? isTxExtraTooBig)
    {
        this.isTxExtraTooBig = isTxExtraTooBig;
        return this;
    }

    public bool? IsNonzeroUnlockTime()
    {
        return isNonzeroUnlockTime;
    }

    public MoneroSubmitTxResult SetIsNonzeroUnlockTime(bool? isNonzeroUnlockTime)
    {
        this.isNonzeroUnlockTime = isNonzeroUnlockTime;
        return this;
    }
}