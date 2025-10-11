using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class ValidateAddressRequest
{
    [JsonPropertyName("address")] public string Address { get; set; } = "";
    [JsonPropertyName("any_net_type")] public bool AnyNetType { get; set; } = false;
    [JsonPropertyName("allow_openalias")] public bool AllowOpenAlias { get; set; } = false;
}