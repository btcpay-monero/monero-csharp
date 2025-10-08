using System.Text.Json.Serialization;

using Monero.Wallet.Common;

namespace Monero.Wallet.Rpc;

public class GetAccountsResponse
{
    [JsonPropertyName("subaddress_accounts")]
    public List<MoneroAccount> Accounts { get; set; } = [];
}