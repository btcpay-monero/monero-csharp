using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class SubmitTransferResult
{
    [JsonPropertyName("tx_hash_list")] public List<string> TxHashList { get; set; } = [];
}