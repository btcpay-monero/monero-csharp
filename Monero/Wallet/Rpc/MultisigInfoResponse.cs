using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class MultisigInfoResponse
{
    [JsonPropertyName("tx_data_hex")]
    public required string TxDataHex { get; set; }
    [JsonPropertyName("tx_hash_list")]
    public required List<string> TxHashList { get; set; }
    [JsonPropertyName("n_outputs")] public int Outputs { get; set; }
    [JsonPropertyName("info")]
    public required string Hex { get; set; }
    [JsonPropertyName("address")]
    public required string Address { get; set; }
    [JsonPropertyName("multisig_info")]
    public required string Info { get; set; }
}