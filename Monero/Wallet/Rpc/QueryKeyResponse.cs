using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class QueryKeyResponse
{
    [JsonPropertyName("key")]
    public required string Key { get; set; }
}