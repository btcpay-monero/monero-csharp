using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class RelayTxResult
{
    [JsonPropertyName("tx_hash")] public string TxHash { get; set; } = "";
}