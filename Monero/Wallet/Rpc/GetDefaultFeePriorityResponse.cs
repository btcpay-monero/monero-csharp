using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class GetDefaultFeePriorityResponse
{
    [JsonPropertyName("priority")]
    public int Priority { get; set; }
}