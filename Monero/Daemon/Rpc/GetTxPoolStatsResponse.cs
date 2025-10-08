using System.Text.Json.Serialization;

using Monero.Daemon.Common;

namespace Monero.Daemon.Rpc;

public class GetTxPoolStatsResponse : MoneroRpcResponse
{
    [JsonPropertyName("pool_stats")]
    public required MoneroTxPoolStats TxPoolStats { get; set; }
}