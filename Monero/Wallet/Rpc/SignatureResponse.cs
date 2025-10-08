using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class SignatureResponse
{
    [JsonPropertyName("signature")] public string Signature { get; set; } = "";
}