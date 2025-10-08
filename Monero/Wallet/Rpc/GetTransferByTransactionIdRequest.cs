using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class GetTransferByTransactionIdRequest
{
    [JsonPropertyName("txid")]
    public required string TransactionId { get; set; }

    [JsonPropertyName("account_index")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long? AccountIndex { get; set; }
}