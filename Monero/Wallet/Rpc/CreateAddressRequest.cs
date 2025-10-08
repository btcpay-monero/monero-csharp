using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class CreateAddressRequest
{
    [JsonPropertyName("account_index")] public long AccountIndex { get; set; }

    [JsonPropertyName("label")] public string? Label { get; set; }
}