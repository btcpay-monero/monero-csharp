using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class CheckSpendProofResponse
{
    [JsonPropertyName("good")] public bool Good { get; set; } = false;
}