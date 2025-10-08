using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class GetAccountsRequest
{
    [JsonPropertyName("tag")] public string? Tag { get; set; }
}