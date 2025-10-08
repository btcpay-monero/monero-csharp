using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class GetTransfersRequest
{
    [JsonPropertyName("in")] public bool In { get; set; }
    [JsonPropertyName("out")] public bool Out { get; set; }
    [JsonPropertyName("pending")] public bool Pending { get; set; }
    [JsonPropertyName("failed")] public bool Failed { get; set; }
    [JsonPropertyName("pool")] public bool Pool { get; set; }
    [JsonPropertyName("filter_by_height ")] public bool? FilterByHeight { get; set; }
    [JsonPropertyName("min_height")] public uint? MinHeight { get; set; }
    [JsonPropertyName("max_height")] public uint? MaxHeight { get; set; }
    [JsonPropertyName("account_index")] public uint? AccountIndex { get; set; }
    [JsonPropertyName("subaddr_indices")] public IEnumerable<uint>? SubaddrIndices { get; set; }
    [JsonPropertyName("all_accounts")] public bool? AllAccounts { get; set; }
}