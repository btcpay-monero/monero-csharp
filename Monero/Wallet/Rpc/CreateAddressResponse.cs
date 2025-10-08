using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class CreateAddressResponse
{
    [JsonPropertyName("address")] public string Address { get; set; } = "";
    [JsonPropertyName("address_index")] public uint Index { get; set; }
}