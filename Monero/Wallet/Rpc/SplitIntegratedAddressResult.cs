using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class SplitIntegratedAddressResult
{
    [JsonPropertyName("standard_address")] public string Address { get; set; } = "";
    [JsonPropertyName("payment_id")] public string PaymentId { get; set; } = "";
}