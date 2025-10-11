using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class ValidateAddressResponse
{
    [JsonPropertyName("integrated")] public bool Integrated { get; set; } = false;
    [JsonPropertyName("nettype")] public string Nettype { get; set; } = "";
    [JsonPropertyName("openalias_address")] public string OpenAliasAddress { get; set; } = "";
    [JsonPropertyName("subaddress")] public bool Subaddress { get; set; } = false;
    [JsonPropertyName("valid")] public bool Valid { get; set; } = false;
}