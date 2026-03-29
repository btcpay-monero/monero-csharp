using System.Text.Json.Serialization;

using Monero.Wallet.Common;

namespace Monero.Wallet.Rpc;

public class TransferRequest
{
    [JsonPropertyName("destinations")]
    public List<TransferDestination> Destinations { get; set; } = [];

    [JsonPropertyName("account_index")]
    public uint AccountIndex { get; set; } = 0;

    [JsonPropertyName("subaddr_indices")]
    public List<uint>? SubaddressIndices { get; set; }

    [JsonPropertyName("subtract_fee_from_outputs")]
    public List<uint>? SubtractFeeFromOutputs { get; set; }

    [JsonPropertyName("priority")]
    public uint Priority { get; set; } = 0;

    [JsonPropertyName("unlock_time")]
    public ulong UnlockTime { get; set; } = 0;

    [JsonPropertyName("get_tx_key")]
    public bool? GetTxKey { get; set; }

    [JsonPropertyName("get_tx_hex")]
    public bool? GetTxHex { get; set; }

    [JsonPropertyName("get_tx_metadata")]
    public bool? GetTxMetadata { get; set; }

    [JsonPropertyName("do_not_relay")]
    public bool? DoNotRelay { get; set; }
}