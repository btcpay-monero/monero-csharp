using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class GenerateFromKeysResponse
{
    [JsonPropertyName("address")] public string? ViewWalletAddress { get; set; }
    [JsonPropertyName("info")] public string? CreationInfo { get; set; }
}