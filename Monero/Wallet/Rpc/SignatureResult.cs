using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class SignatureResult
{
    [JsonPropertyName("signature")] public string Signature { get; set; } = "";
}