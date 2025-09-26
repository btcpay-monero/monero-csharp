using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class QueryKeyResult
{
    [JsonPropertyName("key")]
    public required string Key { get; set; }
}