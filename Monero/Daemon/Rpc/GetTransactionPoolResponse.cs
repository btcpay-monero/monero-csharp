using System.Text.Json.Serialization;

using Monero.Common;
using Monero.Daemon.Common;

namespace Monero.Daemon.Rpc;

public class GetTransactionPoolResponse : MoneroRpcResponse
{
    [JsonPropertyName("tx_hashes")] public List<string> TxHashes { get; set; } = [];
    [JsonPropertyName("transactions")] public List<MoneroTx> Txs { get; set; } = [];
}