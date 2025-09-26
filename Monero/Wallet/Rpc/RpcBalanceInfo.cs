using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class RpcBalanceInfo
{
    [JsonPropertyName("account_index")]
    public uint AccountIndex { get; set; }

    [JsonPropertyName("address_index")] public uint AddressIndex { get; set; }

    [JsonPropertyName("balance")]
    public ulong Balance { get; set; }
    [JsonPropertyName("unlocked_balance")]
    public ulong UnlockedBalance { get; set; }

    [JsonPropertyName("address")] public string Address { get; set; } = "";
    [JsonPropertyName("num_unspent_outputs")] public ulong NumUnspentOutputs { get; set; }
    [JsonPropertyName("label")] public string Label { get; set; } = "";
    [JsonPropertyName("used")] public bool Used { get; set; }
    [JsonPropertyName("blocks_to_unlock")] public ulong BlocksToUnlock { get; set; }
}