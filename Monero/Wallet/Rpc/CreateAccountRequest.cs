using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class CreateAccountRequest
{
    [JsonPropertyName("label")] public string? Label { get; set; }
}