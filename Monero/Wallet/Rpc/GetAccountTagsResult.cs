using System.Text.Json.Serialization;

using Monero.Wallet.Common;

namespace Monero.Wallet.Rpc;

public class GetAccountTagsResult
{
    [JsonPropertyName("account_tags")] public List<MoneroAccountTag> AccountTags { get; set; } = [];
}