using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class GetDefaultFeePriorityResult
{
    [JsonPropertyName("priority")]
    public int Priority { get; set; }
}