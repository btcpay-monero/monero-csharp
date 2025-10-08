using System.Text.Json.Serialization;

using Monero.Wallet.Common;

namespace Monero.Wallet.Rpc;

public class GetTransferByTransactionIdResponse
{
    [JsonPropertyName("transfer")]
    public required TransferItem Transfer { get; set; }

    [JsonPropertyName("transfers")]
    public IEnumerable<TransferItem> Transfers { get; set; } = [];
}