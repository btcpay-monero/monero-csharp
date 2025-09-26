using System.Text.Json.Serialization;

using Monero.Wallet.Common;

namespace Monero.Wallet.Rpc;

public class GetTransfersResult
{
    [JsonPropertyName("in")]
    public List<MoneroTransfer> IncomingTransfers { get; set; } = [];
    [JsonPropertyName("out")]
    public List<MoneroTransfer> OutgoingTransfers { get; set; } = [];
    [JsonPropertyName("pending")]
    public List<MoneroTransfer> PendingTransfers { get; set; } = [];
    [JsonPropertyName("failed")]
    public List<MoneroTransfer> FailedTransfers { get; set; } = [];
    [JsonPropertyName("pool")]
    public List<MoneroTransfer> PooledTransfers { get; set; } = [];
}