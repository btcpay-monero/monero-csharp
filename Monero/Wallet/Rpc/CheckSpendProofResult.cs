using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class CheckSpendProofResult
{
    [JsonPropertyName("good")] public bool Good { get; set; } = false;
}