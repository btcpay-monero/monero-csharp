using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class FrozenResult
{
    [JsonPropertyName("frozen")] public bool Frozen { get; set; } = false;
}