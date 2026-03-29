using System.Text.Json.Serialization;

using Monero.Wallet.Common;

namespace Monero.Wallet.Rpc;

public class TransferResponse
{
    [JsonPropertyName("amount")]
    public ulong Amount { get; set; }

    [JsonPropertyName("amounts_by_dest")]
    public AmountsByDestination AmountsByDestination { get; set; } = new();

    [JsonPropertyName("fee")]
    public ulong Fee { get; set; }

    [JsonPropertyName("tx_hash")]
    public string TxHash { get; set; } = string.Empty;

    [JsonPropertyName("tx_key")]
    public string? TxKey { get; set; }

    [JsonPropertyName("tx_blob")]
    public string? TxBlob { get; set; }

    [JsonPropertyName("tx_metadata")]
    public string? TxMetadata { get; set; }

    [JsonPropertyName("multisig_txset")]
    public string? MultisigTxset { get; set; }

    [JsonPropertyName("unsigned_txset")]
    public string? UnsignedTxset { get; set; }

    [JsonPropertyName("spent_key_images")]
    public SpentKeyImages SpentKeyImages { get; set; } = new();

    [JsonPropertyName("weight")]
    public ulong Weight { get; set; }
}