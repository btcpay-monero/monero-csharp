using System.Text.Json.Serialization;


namespace Monero.Wallet.Common;

public abstract class MoneroTransfer
{
    [JsonPropertyName("address")]
    public required string Address { get; set; }
    [JsonPropertyName("amount")]
    public ulong Amount { get; set; }
    [JsonPropertyName("amounts")]
    public List<ulong> Amounts { get; set; } = [];
    [JsonPropertyName("destinations")]
    public List<MoneroDestination> Destinations { get; set; } = [];
    [JsonPropertyName("confirmations")]
    public ulong Confirmations { get; set; }
    [JsonPropertyName("fee")]
    public ulong Fee { get; set; }
    [JsonPropertyName("double_spend_seen")]
    public bool IsDoubleSpendSeen { get; set; }
    [JsonPropertyName("height")]
    public ulong Height { get; set; }
    [JsonPropertyName("locked")]
    public bool IsLocked { get; set; }
    [JsonPropertyName("note")]
    public required string Note { get; set; }
    [JsonPropertyName("payment_id")]
    public required string PaymentID { get; set; }
    [JsonPropertyName("subaddr_index")]
    public required MoneroSubaddress SubaddressIndex { get; set; }
    [JsonPropertyName("subaddr_indices")]
    public List<MoneroSubaddress> SubaddressIndices { get; set; } = [];
    [JsonPropertyName("suggested_confirmations_threshold")]
    public ulong SuggestedConfirmationsThreshold { get; set; }
    [JsonPropertyName("timestamp")]
    public ulong Timestamp { get; set; }
    [JsonPropertyName("txid")]
    public required string TransactionID { get; set; }
    [JsonPropertyName("type")]
    public required string Type { get; set; }
    [JsonPropertyName("unlock_time")]
    public ulong UnlockTime { get; set; }
}