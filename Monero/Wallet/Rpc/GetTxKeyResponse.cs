using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class GetTxKeyResponse
{
    [JsonPropertyName("tx_key")] public string TxKey { get; set; } = "";
}