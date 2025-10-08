using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class GetAttributeResponse
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}