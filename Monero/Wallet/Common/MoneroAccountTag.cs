using System.Text.Json.Serialization;

namespace Monero.Wallet.Common;

public class MoneroAccountTag
{
    [JsonPropertyName("accounts")]
    public List<uint>? AccountIndices { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("tag")]
    public string? Tag { get; set; }
}