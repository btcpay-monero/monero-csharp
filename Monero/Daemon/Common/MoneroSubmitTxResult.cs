using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroSubmitTxResult : MoneroRpcPaymentInfo
{
    [JsonPropertyName("invalid_input")]
    public bool? HasInvalidInput;

    [JsonPropertyName("invalid_output")]
    public bool? HasInvalidOutput;

    [JsonPropertyName("too_few_outputs")]
    public bool? HasTooFewOutputs;

    [JsonPropertyName("double_spend")]
    public bool? IsDoubleSpend;

    [JsonPropertyName("fee_too_low")]
    public bool? IsFeeTooLow;

    public bool? IsGood;

    [JsonPropertyName("low_mixin")]
    public bool? IsMixinTooLow;

    [JsonPropertyName("nonzero_unlock_time")]
    public bool? IsNonzeroUnlockTime;

    [JsonPropertyName("overspend")]
    public bool? IsOverspend;

    [JsonPropertyName("not_relayed")]
    public bool? NotRelayed;

    [JsonPropertyName("too_big")]
    public bool? IsTooBig;

    [JsonPropertyName("tx_extra_too_big")]
    public bool? IsTxExtraTooBig;

    [JsonPropertyName("reason")]
    public string? Reason;

    [JsonPropertyName("sanity_check_failed")]
    public bool? SanityCheckFailed;

    [JsonPropertyName("top_hash")]
    public new string? TopBlockHash;
}