using System.Text.Json.Serialization;

using Monero.Wallet.Common;

namespace Monero.Wallet.Rpc;

public class GetTransfersResponse
{
    [JsonPropertyName("in")]
    public List<TransferItem> IncomingTransfers { get; set; } = [];
    [JsonPropertyName("out")]
    public List<TransferItem> OutgoingTransfers { get; set; } = [];
    [JsonPropertyName("pending")]
    public List<TransferItem> PendingTransfers { get; set; } = [];
    [JsonPropertyName("failed")]
    public List<TransferItem> FailedTransfers { get; set; } = [];
    [JsonPropertyName("pool")]
    public List<TransferItem> PooledTransfers { get; set; } = [];
}