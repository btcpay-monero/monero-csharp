using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroSubmitTxResult : MoneroRpcPaymentInfo
{
    [JsonPropertyName("invalid_input")]
    private bool? _hasInvalidInput;
    [JsonPropertyName("invalid_output")]
    private bool? _hasInvalidOutput;
    [JsonPropertyName("too_few_outputs")]
    private bool? _hasTooFewOutputs;
    [JsonPropertyName("double_spend")]
    private bool? _isDoubleSpend;
    [JsonPropertyName("fee_too_low")]
    private bool? _isFeeTooLow;
    private bool? _isGood;
    [JsonPropertyName("low_mixin")]
    private bool? _isMixinTooLow;
    [JsonPropertyName("nonzero_unlock_time")]
    private bool? _isNonzeroUnlockTime;
    [JsonPropertyName("overspend")]
    private bool? _isOverspend;
    [JsonPropertyName("not_relayed")]
    private bool? _notRelayed;
    [JsonPropertyName("too_big")]
    private bool? _isTooBig;
    [JsonPropertyName("tx_extra_too_big")]
    private bool? _isTxExtraTooBig;
    [JsonPropertyName("reason")]
    private string? _reason;
    [JsonPropertyName("sanity_check_failed")]
    private bool? _sanityCheckFailed;
    [JsonPropertyName("top_hash")]
    private new string? _topBlockHash;

    public bool? IsGood()
    {
        return _isGood;
    }

    public MoneroSubmitTxResult SetIsGood(bool? isGood)
    {
        _isGood = isGood;
        return this;
    }

    public bool? IsRelayed()
    {
        return !_notRelayed;
    }

    public MoneroSubmitTxResult SetIsRelayed(bool? isRelayed)
    {
        if (isRelayed != null)
        {
            _notRelayed = !isRelayed;
        }
        else
        {
            _notRelayed = null;
        }

        return this;
    }

    public bool? IsDoubleSpend()
    {
        return _isDoubleSpend;
    }

    public MoneroSubmitTxResult SetIsDoubleSpend(bool? isDoubleSpend)
    {
        _isDoubleSpend = isDoubleSpend;
        return this;
    }

    public bool? IsFeeTooLow()
    {
        return _isFeeTooLow;
    }

    public MoneroSubmitTxResult SetIsFeeTooLow(bool? isFeeTooLow)
    {
        _isFeeTooLow = isFeeTooLow;
        return this;
    }

    public bool? IsMixinTooLow()
    {
        return _isMixinTooLow;
    }

    public MoneroSubmitTxResult SetIsMixinTooLow(bool? isMixinTooLow)
    {
        _isMixinTooLow = isMixinTooLow;
        return this;
    }

    public bool? HasInvalidInput()
    {
        return _hasInvalidInput;
    }

    public MoneroSubmitTxResult SetHasInvalidInput(bool? hasInvalidInput)
    {
        _hasInvalidInput = hasInvalidInput;
        return this;
    }

    public bool? HasInvalidOutput()
    {
        return _hasInvalidOutput;
    }

    public MoneroSubmitTxResult SetHasInvalidOutput(bool? hasInvalidOutput)
    {
        _hasInvalidOutput = hasInvalidOutput;
        return this;
    }

    public bool? HasTooFewOutputs()
    {
        return _hasTooFewOutputs;
    }

    public MoneroSubmitTxResult SetHasTooFewOutputs(bool? hasTooFewOutputs)
    {
        _hasTooFewOutputs = hasTooFewOutputs;
        return this;
    }

    public bool? IsOverspend()
    {
        return _isOverspend;
    }

    public MoneroSubmitTxResult SetIsOverspend(bool? isOverspend)
    {
        _isOverspend = isOverspend;
        return this;
    }

    public bool? IsTooBig()
    {
        return _isTooBig;
    }

    public MoneroSubmitTxResult SetIsTooBig(bool? isTooBig)
    {
        _isTooBig = isTooBig;
        return this;
    }

    public bool? GetSanityCheckFailed()
    {
        return _sanityCheckFailed;
    }

    public MoneroSubmitTxResult SetSanityCheckFailed(bool? sanityCheckFailed)
    {
        _sanityCheckFailed = sanityCheckFailed;
        return this;
    }

    public string? GetReason()
    {
        return _reason;
    }

    public MoneroSubmitTxResult SetReason(string? reason)
    {
        _reason = reason;
        return this;
    }

    public new ulong? GetCredits()
    {
        return _credits;
    }

    public new MoneroSubmitTxResult SetCredits(ulong? credits)
    {
        _credits = credits;
        return this;
    }

    public new string? GetTopBlockHash()
    {
        return _topBlockHash;
    }

    public new MoneroSubmitTxResult SetTopBlockHash(string? topBlockHash)
    {
        _topBlockHash = topBlockHash;
        return this;
    }

    public bool? IsTxExtraTooBig()
    {
        return _isTxExtraTooBig;
    }

    public MoneroSubmitTxResult SetIsTxExtraTooBig(bool? isTxExtraTooBig)
    {
        _isTxExtraTooBig = isTxExtraTooBig;
        return this;
    }

    public bool? IsNonzeroUnlockTime()
    {
        return _isNonzeroUnlockTime;
    }

    public MoneroSubmitTxResult SetIsNonzeroUnlockTime(bool? isNonzeroUnlockTime)
    {
        _isNonzeroUnlockTime = isNonzeroUnlockTime;
        return this;
    }
}