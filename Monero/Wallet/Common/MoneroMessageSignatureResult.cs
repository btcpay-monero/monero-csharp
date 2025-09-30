using System.Text.Json.Serialization;

namespace Monero.Wallet.Common;

public class MoneroMessageSignatureResult
{
    [JsonPropertyName("good")]
    public bool IsGood { get; set; }

    [JsonPropertyName("old")]
    public bool? IsOld { get; set; }

    [JsonPropertyName("signature_type")]
    public string? SignatureType { get; set; }

    [JsonPropertyName("version")]
    public int? Version { get; set; }
}