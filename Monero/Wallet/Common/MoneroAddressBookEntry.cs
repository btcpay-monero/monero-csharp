using System.Text.Json.Serialization;

namespace Monero.Wallet.Common;

public class MoneroAddressBookEntry
{
    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("index")]
    public uint? Index { get; set; }

    [JsonPropertyName("payment_id")]
    public string? PaymentId { get; set; }
}