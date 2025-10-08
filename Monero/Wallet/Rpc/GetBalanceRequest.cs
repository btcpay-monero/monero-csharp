using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class GetBalanceRequest
{
    [JsonPropertyName("all_accounts")] public bool AllAccounts { get; set; }
}