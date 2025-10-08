using System.Text.Json.Serialization;

using Monero.Wallet.Common;

namespace Monero.Wallet.Rpc;

public class GetAccountTagsResponse
{
    [JsonPropertyName("account_tags")] public List<MoneroAccountTag> AccountTags { get; set; } = [];
}