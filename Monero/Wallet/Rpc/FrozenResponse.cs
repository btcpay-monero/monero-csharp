using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class FrozenResponse
{
    [JsonPropertyName("frozen")] public bool Frozen { get; set; } = false;
}