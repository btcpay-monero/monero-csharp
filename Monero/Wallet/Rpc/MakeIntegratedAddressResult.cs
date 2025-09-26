using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class MakeIntegratedAddressResult
{
    [JsonPropertyName("integrated_address")]
    public string IntegratedAddress { get; set; } = "";
}