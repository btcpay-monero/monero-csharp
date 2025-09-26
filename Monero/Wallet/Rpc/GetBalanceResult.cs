using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;



public class GetBalanceResult
{
    [JsonPropertyName("multisig_import_needed")]
    public bool MultisigImportNeeded { get; set; }
    [JsonPropertyName("balance")]
    public ulong Balance { get; set; }

    [JsonPropertyName("unlocked_balance")] public ulong UnlockedBalance { get; set; }
    [JsonPropertyName("per_subaddress")] public List<RpcBalanceInfo> PerSubaddress { get; set; } = [];
}