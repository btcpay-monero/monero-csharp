using System.Text.Json.Serialization;

using Monero.Common;
using Monero.Daemon.Common;

namespace Monero.Daemon.Rpc;

public class GetTransactionsResult : MoneroRpcResponse
{
    [JsonPropertyName("txs")] public List<MoneroTx> Txs { get; set; } = [];
}