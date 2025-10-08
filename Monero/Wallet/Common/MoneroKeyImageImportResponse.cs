using System.Text.Json.Serialization;

namespace Monero.Wallet.Common;

public class MoneroKeyImageImportResponse
{
    [JsonPropertyName("height")]
    public ulong? Height { get; set; }

    [JsonPropertyName("spent")]
    public ulong? SpentAmount { get; set; }

    [JsonPropertyName("unspent")]
    public ulong? UnspentAmount { get; set; }
}