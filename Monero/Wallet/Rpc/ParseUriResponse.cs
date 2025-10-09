using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class ParsedUri
{
    [JsonPropertyName("address")] public string Address { get; set; } = "";
    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("payment_id")] public string PaymentId { get; set; } = "";
    [JsonPropertyName("recipient_name")] public string RecipientName { get; set; } = "";
    [JsonPropertyName("tx_description")] public string TxDescription { get; set; } = "";
}

public class ParseUriResponse
{

    [JsonPropertyName("uri")] public required ParsedUri Uri;
}