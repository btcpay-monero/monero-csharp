using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class RelayTxResponse
{
    [JsonPropertyName("tx_hash")] public string TxHash { get; set; } = "";
}