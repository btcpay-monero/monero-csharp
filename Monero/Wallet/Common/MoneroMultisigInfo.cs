using System.Text.Json.Serialization;

namespace Monero.Wallet.Common;

public class MoneroMultisigInfo
{
    [JsonPropertyName("multisig")]
    public bool? IsMultisig { get; set; }

    [JsonPropertyName("ready")]
    public bool? IsReady { get; set; }

    [JsonPropertyName("total")]
    public int? NumParticipants { get; set; }

    [JsonPropertyName("threshold")]
    public int? Threshold { get; set; }

    public MoneroMultisigInfo(MoneroMultisigInfo multisigInfo)
    {
        IsMultisig = multisigInfo.IsMultisig;
        IsReady = multisigInfo.IsReady;
        Threshold = multisigInfo.Threshold;
        NumParticipants = multisigInfo.NumParticipants;
    }
}