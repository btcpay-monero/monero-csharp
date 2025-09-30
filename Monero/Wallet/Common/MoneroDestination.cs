using System.Text.Json.Serialization;

namespace Monero.Wallet.Common;

public class MoneroDestination
{
    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("amount")]
    public ulong? Amount { get; set; }
}