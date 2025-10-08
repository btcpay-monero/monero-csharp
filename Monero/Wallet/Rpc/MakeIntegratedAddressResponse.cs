using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class MakeIntegratedAddressResponse
{
    [JsonPropertyName("integrated_address")]
    public string IntegratedAddress { get; set; } = "";
}