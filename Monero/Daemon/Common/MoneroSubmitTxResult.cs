namespace Monero.Daemon.Common;

public class MoneroSubmitTxResult
{
    private ulong? _credits;
    private bool? _hasInvalidInput;
    private bool? _hasInvalidOutput;
    private bool? _hasTooFewOutputs;
    private bool? _isDoubleSpend;
    private bool? _isFeeTooLow;
    private bool? _isGood;
    private bool? _isMixinTooLow;
    private bool? _isNonzeroUnlockTime;
    private bool? _isOverspend;
    private bool? _isRelayed;
    private bool? _isTooBig;
    private bool? _isTxExtraTooBig;
    private string? _reason;
    private bool? _sanityCheckFailed;
    private string? _topBlockHash;

    public bool? IsGood()
    {
        return _isGood;
    }

    public MoneroSubmitTxResult SetIsGood(bool? isGood)
    {
        this._isGood = isGood;
        return this;
    }

    public bool? IsRelayed()
    {
        return _isRelayed;
    }

    public MoneroSubmitTxResult SetIsRelayed(bool? isRelayed)
    {
        this._isRelayed = isRelayed;
        return this;
    }

    public bool? IsDoubleSpend()
    {
        return _isDoubleSpend;
    }

    public MoneroSubmitTxResult SetIsDoubleSpend(bool? isDoubleSpend)
    {
        this._isDoubleSpend = isDoubleSpend;
        return this;
    }

    public bool? IsFeeTooLow()
    {
        return _isFeeTooLow;
    }

    public MoneroSubmitTxResult SetIsFeeTooLow(bool? isFeeTooLow)
    {
        this._isFeeTooLow = isFeeTooLow;
        return this;
    }

    public bool? IsMixinTooLow()
    {
        return _isMixinTooLow;
    }

    public MoneroSubmitTxResult SetIsMixinTooLow(bool? isMixinTooLow)
    {
        this._isMixinTooLow = isMixinTooLow;
        return this;
    }

    public bool? HasInvalidInput()
    {
        return _hasInvalidInput;
    }

    public MoneroSubmitTxResult SetHasInvalidInput(bool? hasInvalidInput)
    {
        this._hasInvalidInput = hasInvalidInput;
        return this;
    }

    public bool? HasInvalidOutput()
    {
        return _hasInvalidOutput;
    }

    public MoneroSubmitTxResult SetHasInvalidOutput(bool? hasInvalidOutput)
    {
        this._hasInvalidOutput = hasInvalidOutput;
        return this;
    }

    public bool? HasTooFewOutputs()
    {
        return _hasTooFewOutputs;
    }

    public MoneroSubmitTxResult SetHasTooFewOutputs(bool? hasTooFewOutputs)
    {
        this._hasTooFewOutputs = hasTooFewOutputs;
        return this;
    }

    public bool? IsOverspend()
    {
        return _isOverspend;
    }

    public MoneroSubmitTxResult SetIsOverspend(bool? isOverspend)
    {
        this._isOverspend = isOverspend;
        return this;
    }

    public bool? IsTooBig()
    {
        return _isTooBig;
    }

    public MoneroSubmitTxResult SetIsTooBig(bool? isTooBig)
    {
        this._isTooBig = isTooBig;
        return this;
    }

    public bool? GetSanityCheckFailed()
    {
        return _sanityCheckFailed;
    }

    public MoneroSubmitTxResult SetSanityCheckFailed(bool? sanityCheckFailed)
    {
        this._sanityCheckFailed = sanityCheckFailed;
        return this;
    }

    public string? GetReason()
    {
        return _reason;
    }

    public MoneroSubmitTxResult SetReason(string? reason)
    {
        this._reason = reason;
        return this;
    }

    public ulong? GetCredits()
    {
        return _credits;
    }

    public MoneroSubmitTxResult SetCredits(ulong? credits)
    {
        this._credits = credits;
        return this;
    }

    public string? GetTopBlockHash()
    {
        return _topBlockHash;
    }

    public MoneroSubmitTxResult SetTopBlockHash(string? topBlockHash)
    {
        this._topBlockHash = topBlockHash;
        return this;
    }

    public bool? IsTxExtraTooBig()
    {
        return _isTxExtraTooBig;
    }

    public MoneroSubmitTxResult SetIsTxExtraTooBig(bool? isTxExtraTooBig)
    {
        this._isTxExtraTooBig = isTxExtraTooBig;
        return this;
    }

    public bool? IsNonzeroUnlockTime()
    {
        return _isNonzeroUnlockTime;
    }

    public MoneroSubmitTxResult SetIsNonzeroUnlockTime(bool? isNonzeroUnlockTime)
    {
        this._isNonzeroUnlockTime = isNonzeroUnlockTime;
        return this;
    }
}