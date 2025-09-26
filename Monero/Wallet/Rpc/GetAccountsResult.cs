using System.Text.Json.Serialization;

using Monero.Wallet.Common;

namespace Monero.Wallet.Rpc;

public class GetAccountsResult
{
    [JsonPropertyName("subaddress_accounts")]
    public List<MoneroAccount> Accounts { get; set; } = [];
}